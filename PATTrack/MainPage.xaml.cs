namespace PATTrack
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using PATTrack.Map;
    using PATTrack.PATAPI;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Resources;
    using Windows.Foundation.Diagnostics;
    using Windows.Phone.UI.Input;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using System.Collections.Generic;
    using Windows.Devices.Geolocation;
    public sealed partial class MainPage : Page
    {
        private static readonly LoggingChannel Log = LoggingSingleton.Instance.Channel;
        private static readonly LoggingSession LogSession = LoggingSingleton.Instance.Session;
        private static IDisposable vehicleSubscription;
        private static IDisposable mapSubscription;
        private BusMap busmap = new BusMap(); 

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += new SuspendingEventHandler(OnAppSuspending);
            Application.Current.Resuming += new EventHandler<object>(OnAppResuming);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed; 
            }

            listview.SelectionChanged += Listview_SelectionChanged;

            this.Setup();
        }

        private void Listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview.SelectedItems.Count > 10)
            {
                listview.SelectedItems.RemoveAt(10);
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
            Log.LogMessage("Suspending");
            vehicleSubscription.Dispose();
            mapSubscription.Dispose();
            var deferral = e.SuspendingOperation.GetDeferral();
            await LogSession.SaveToFileAsync(ApplicationData.Current.LocalFolder, "logFile.etl");
            deferral.Complete();
        }

        private async void Setup()
        {
            this.busmap.Map = map;
            var pitt = new Geopoint(new BasicGeoposition()
            {
                Latitude = 40.4397, Longitude = -79.9764
            });
            await map.TrySetViewAsync(pitt, 12);

            var loader = new ResourceLoader();
            var api_key = loader.GetString("PAT_KEY");

            var listState = Observable.FromEventPattern(listview, "SelectionChanged")
                                      .Select(k =>
                                      {
                                          var args = k.EventArgs as SelectionChangedEventArgs;
                                          return new
                                          {
                                              selected = listview.SelectedItems
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
                                                                       .Single()
                                          };
                                      })
                                      .Throttle(new TimeSpan(
                                          days: 0
                                          , hours: 0
                                          , minutes: 0
                                          , seconds: 0
                                          , milliseconds: 300))
                                      .Where(x => x.selected.Length < 10 || (x.selected.Length == 10 && x.clicked.Selected == true))
                                      .Publish()
                                      .RefCount();

            var mapChanges = from item in listState
                             select item.clicked;

            mapSubscription = mapChanges
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(
                onNext: async x =>
                {
                    await busmap.UpdatePolylines(x, api_key);
                }

                , onError: ex =>
                {
                }

                , onCompleted: () =>
                {
                });

            var vehicles = listState.Select(x => from t in Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
                                                 select x)
                                    .Switch()
                                    .Publish()
                                    .RefCount();

            vehicleSubscription = vehicles.Select(async xs => (await PAT_API.GetBustimeResponse(xs.selected, api_key)))
                                   .SubscribeOn(NewThreadScheduler.Default)
                                   .ObserveOnDispatcher()
                                   .Subscribe(
                                       onNext: async x =>
                                       {
                                           var xs = await x;
                                           busmap.UpdateBuses(xs.Vehicles);
                                       }

                                       , onError: ex => 
                                       {
                                           Log.LogMessage(ex.Message);
                                       } 

                                       , onCompleted: () =>
                                       {
                                           Log.LogMessage("Vehicle observable completed");
                                       });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.View.IsPaneOpen = !this.View.IsPaneOpen;
        }

        private void map_ZoomLevelChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {

        }
    }
}
