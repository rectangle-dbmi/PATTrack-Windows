namespace PATTrack
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Resources;
    using Windows.Devices.Geolocation;
    using Windows.Foundation;
    using Windows.Foundation.Diagnostics;
    using Windows.Phone.UI.Input;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Maps;

    public sealed partial class MainPage : Page
    {
        private const string baseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";
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
            var loader = new ResourceLoader();
            var api_key = loader.GetString("PAT_KEY");
            var userInput = Observable.FromEventPattern(this, "KeyDown")
                                      .Throttle(new TimeSpan(days: 0
                                                             ,hours: 0
                                                             ,minutes: 0
                                                             ,seconds: 0
                                                             ,milliseconds: 300))
                                      .Select(l => new string[] { "58", "71C" });

            var timed = Observable.Interval(TimeSpan.FromSeconds(10))
                                  .Select(l => new string[] { "28X", "61B", "54C" });

            var vehicles = timed.Merge(userInput)
                                .Select(x => GetBustimeResponse(x, api_key).vehicle)
                                .Publish()
                                .RefCount();

            subscription = vehicles.SubscribeOn(NewThreadScheduler.Default)
                                  .ObserveOnDispatcher()
                                  .Subscribe(onNext: x =>
                                  {
                                      map.MapElements.Clear();
                                      AddMapIcons(x);
                                  }
                                  ,onError: ex => 
                                  {
                                      log.LogMessage(ex.Message);
                                  } 
                                  ,onCompleted: () =>
                                  {
                                      log.LogMessage("Vehicle observable completed");
                                  } );
        }

        private void AddMapIcons(vehicle[] vehicles)
        {
            foreach (var v in vehicles)
            {
                MapIcon mi = new MapIcon();
                mi.Location = new Geopoint(new BasicGeoposition() { Latitude = v.lat, Longitude = v.lon });
                mi.NormalizedAnchorPoint = new Point(0.5, 1.0);
                mi.Title = v.rt;
                map.MapElements.Add(mi);
            }
        }

        private static bustimeresponse GetBustimeResponse(string[] routes, string api_key)
        {
            string requestUrl = String.Format("{0}getvehicles?key={1}&rt={2}&format=xml",
                baseUrl,
                api_key,
                routes.Aggregate((a,b)=> a + "," + b));
            var responseStream = PATAPI.MakeRequest(requestUrl);
            XmlSerializer serializer = new XmlSerializer(typeof(bustimeresponse));
            return serializer.Deserialize(responseStream) as bustimeresponse;
        }

    }
}
