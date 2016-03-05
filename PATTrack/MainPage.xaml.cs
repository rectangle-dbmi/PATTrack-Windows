namespace PATTrack
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Map;
    using PATAPI;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Resources;
    using Windows.Devices.Geolocation;
    using Windows.Foundation.Diagnostics;
    using Windows.Phone.UI.Input;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Maps;

    public sealed partial class MainPage : Page
    {
        private static readonly LoggingChannel Log = LoggingSingleton.Instance.Channel;
        private static readonly LoggingSession LogSession = LoggingSingleton.Instance.Session;
        private static IDisposable vehicleSubscription;
        private static IDisposable mapSubscription;
        private readonly BusMap busmap = new BusMap(); 

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += OnAppSuspending;
            Application.Current.Resuming += OnAppResuming;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed; 
            }

            Listview.SelectionChanged += Listview_SelectionChanged;
            this.Setup();
        }

        private void Listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Listview.SelectedItems.Count > 10)
            {
                Listview.SelectedItems.RemoveAt(10);
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void OnAppResuming(object sender, object e)
        {
            Log.LogMessage("Resuming");
            Setup();
        }

        private async void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Log.LogMessage("Suspending");
            vehicleSubscription.Dispose();
            mapSubscription.Dispose();
            await LogSession.SaveToFileAsync(ApplicationData.Current.LocalFolder, "logFile.etl");
            deferral.Complete();
        }

        private async void Setup()
        {
            this.busmap.Map = Map;
            var pitt = new Geopoint(new BasicGeoposition()
            {
                Latitude = 40.4397,
                Longitude = -79.9764
            });
            await Map.TrySetViewAsync(pitt, 12);

            var loader = new ResourceLoader();
            var apiKey = loader.GetString("PAT_KEY");

            var listState = Observable.FromEventPattern(Listview, "SelectionChanged")
                .Select(k =>
                {
                    var args = (SelectionChangedEventArgs)k.EventArgs;
                    return new
                    {
                        selected = Listview.SelectedItems
                            .Select(i => (i as ListViewItem).Content.ToString())
                            .ToArray(),
                        clicked = args.AddedItems.Select(x => new VehicleSelected()
                        {
                            Rt = (x as ListViewItem).Content.ToString(),
                            Selected = (x as ListViewItem).IsSelected
                        })
                            .Concat(from x in args.RemovedItems
                                select new VehicleSelected()
                                {
                                    Rt = (x as ListViewItem).Content.ToString(),
                                    Selected = (x as ListViewItem).IsSelected
                                })
                    };
                })
                .Where(x => x.selected.Length < 10 || (x.selected.Length == 10 && x.clicked.All(s => s.Selected)))
                .Publish()
                .RefCount();

            var getPatterns = PatApi.GetPatternsMemo();

            var mapChanges =
                from item in listState
                from clicked in item.clicked
                from patternResponse in Observable.FromAsync(
                    () => getPatterns(clicked.Rt, apiKey))
                select new
                {
                    Patterns = patternResponse,
                    Vh = clicked
                };

            mapSubscription = mapChanges
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(
                onNext: x =>
                {
                    busmap.UpdatePolylines(x.Vh, x.Patterns, apiKey);
                    Debug.WriteLine("updating polylines");
                },
                onError: ex =>
                {
                },
                onCompleted: () =>
                {
                });

            var busListState = listState.Throttle(new TimeSpan(
                days: 0,
                hours: 0,
                minutes: 0,
                seconds: 0,
                milliseconds: 300));

            var bustimeResponses =
                (from item in busListState
                select from t in Observable.Timer(
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(10))
                    select item)
                .Switch()
                .Select(v => Observable.FromAsync(
                    () => PatApi.GetBustimeResponse(v.selected, apiKey)))
                .Concat();

            vehicleSubscription = bustimeResponses
                                   .SubscribeOn(NewThreadScheduler.Default)
                                   .ObserveOnDispatcher()
                                   .Subscribe(
                                       onNext: x =>
                                       {
                                           busmap.UpdateBuses(x.Vehicles);
                                           Debug.WriteLine("updating buses");
                                       },
                                       onError: ex =>
                                       {
                                           Log.LogMessage(ex.Message);
                                       },
                                       onCompleted: () =>
                                       {
                                           Log.LogMessage("Vehicle observable completed");
                                       });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.View.IsPaneOpen = !this.View.IsPaneOpen;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            this.Listview.SelectedItems.Clear();
        }
        
        private void Map_ZoomLevelChanged(MapControl sender, object args)
        {
        }
    }
}
