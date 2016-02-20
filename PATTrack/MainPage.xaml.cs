namespace PATTrack
{
    using PATTrack.PATAPI;
    using PATTrack.PATAPI.POCO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Resources;
    using Windows.Devices.Geolocation;
    using Windows.Foundation.Diagnostics;
    using Windows.Phone.UI.Input;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Maps;
    using Windows.UI.Xaml.Input;
    public sealed partial class MainPage : Page
    {

        private static IDisposable vehicleSubscription;
        BusMap busmap = new BusMap(); 
        private static readonly LoggingChannel log = LoggingSingleton.Instance.channel;
        private static readonly LoggingSession logSession = LoggingSingleton.Instance.session;

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += new SuspendingEventHandler(OnAppSuspending);
            Application.Current.Resuming += new EventHandler<object>(OnAppResuming);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed; 
            }
            Setup();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnAppResuming(object sender, object e)
        {
            log.LogMessage("Resuming");
            Setup();
        }

        private async void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
            log.LogMessage("Suspending");
            await logSession.SaveToFileAsync(ApplicationData.Current.LocalFolder, "logFile.etl");
            vehicleSubscription.Dispose();
            busmap.ClearMap();
        }

        private void Setup()
        {
            busmap.map = map;
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
                                              clicked = args.AddedItems.Select(x => new vehicleselected() {rt = (x as ListViewItem).Content.ToString(),
                                                                                         selected = (x as ListViewItem).IsSelected})
                                                                       .Concat(from x in args.RemovedItems
                                                                               select new vehicleselected() {rt = (x as ListViewItem).Content.ToString(),
                                                                                           selected = (x as ListViewItem).IsSelected} )
                                                                       .Single()
                                          };
                                      })
                                      .Throttle(new TimeSpan(days: 0
                                                             , hours: 0
                                                             , minutes: 0
                                                             , seconds: 0
                                                             , milliseconds: 300))
                                      .Publish()
                                      .RefCount();

            var mapChanges = from item in listState
                             select item.clicked;
            var mapSubscription = mapChanges.SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(
                onNext: async x =>
                {
                    await busmap.AddPolylines(x, api_key);
                }
                , onError: ex =>
                {
                }
                , onCompleted: () =>
                {
                } );

            var vehicles = listState.Select(x => from t in Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
                                                 select x)
                                    .Switch()
                                    .Publish()
                                    .RefCount();

            vehicleSubscription = vehicles.Select(async xs => (await PAT_API.GetBustimeResponse(xs.selected, api_key)).vehicle)
                                   .SubscribeOn(NewThreadScheduler.Default)
                                   .ObserveOnDispatcher()
                                   .Subscribe(
                                       onNext: async x =>
                                       {
                                           var xs = await x;
                                           busmap.UpdateBuses(xs);
                                       }
                                       , onError: ex => 
                                       {
                                           log.LogMessage(ex.Message);
                                       } 
                                       ,onCompleted: () =>
                                       {
                                           log.LogMessage("Vehicle observable completed");
                                       } );
        }
    }
}
