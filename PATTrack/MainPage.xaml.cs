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
    using Windows.Phone.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Maps;

    public sealed partial class MainPage : Page
    {
        private const string baseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";
        private static IDisposable subscription;

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
            Setup();
        }

        private void OnAppSuspending(object sender, SuspendingEventArgs e)
        {
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
                                  ,onError: ex => { } 
                                  ,onCompleted: () => { } );
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
            string requestUrl = CreateRequest(String.Format("getvehicles?key={0}&rt={1}",
                api_key,
                routes.Aggregate((a,b)=> a + "," + b)));
            var responseStream = MakeRequest(requestUrl);
            XmlSerializer serializer = new XmlSerializer(typeof(bustimeresponse));
            return serializer.Deserialize(responseStream) as bustimeresponse;
        }

        public static string CreateRequest(string queryString)
        {
            string UrlRequest = baseUrl + queryString + @"&format=xml";
            return (UrlRequest);
        }

        public static Stream MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                WebResponse response = request.GetResponseAsync().Result;

                XmlDocument xmlDoc = new XmlDocument();
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
