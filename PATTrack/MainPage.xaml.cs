namespace PATTrack
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Resources;
    using Windows.Foundation.Diagnostics;
    using Windows.Phone.UI.Input;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using PATTrack.Extensions;
    using Windows.Devices.Geolocation;
    using Windows.UI.Xaml.Controls.Maps;
    using Windows.UI;
    public sealed partial class MainPage : Page
    {

        private static IDisposable subscription;
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
            subscription.Dispose();
            map.MapElements.Clear();
        }

        private void Setup()
        {

            //placeholder keyboard event until there's a listview of buses with a click event
            var userInput = Observable.FromEventPattern(this, "KeyDown")
                                      .Select(k =>
                                      {
                                          if (((KeyRoutedEventArgs)k.EventArgs).Key == Windows.System.VirtualKey.A)
                                              return new string[] { "61A", "61B", "61C", "61D" };
                                          else
                                              return new string[] { "28X", "54" };
                                      })
                                      .Throttle(new TimeSpan(days: 0
                                                             , hours: 0
                                                             , minutes: 0
                                                             , seconds: 0
                                                             , milliseconds: 300));

            var timed = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10));

            var vehicles = userInput.CombineLatest(timed, (ui,time) => ui)
                                .Select(async xs => (await PATAPI.GetBustimeResponse(xs)).vehicle)
                                .Publish()
                                .RefCount();

            subscription = vehicles.SubscribeOn(NewThreadScheduler.Default)
                                   .ObserveOnDispatcher()
                                   .Subscribe(
                                       onNext: async x =>
                                       {
                                           map.MapElements.Clear();
                                           map.AddBusIcons(await x);
                                       }
                                       , onError: ex => 
                                       {
                                           log.LogMessage(ex.Message);
                                       } 
                                       , onCompleted: () =>
                                       {
                                           log.LogMessage("Vehicle observable completed");
                                       } );
        }

        private async System.Threading.Tasks.Task AddPolyline(string route)
        {
            var paths = from ptr in (await PATAPI.GetPatterns(route)).Items
                        select from pt in ptr.pt
                               select new BasicGeoposition() { Latitude = Double.Parse(pt.lat), Longitude = Double.Parse(pt.lon) };
            foreach (var path in paths)
            {
                var polyline = new MapPolyline();
                polyline.Path = new Geopath(path);
                polyline.StrokeColor = Colors.Blue;
                polyline.Visible = true;
                map.MapElements.Add(polyline);
            }
        }
    }
}