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
            var loader = new ResourceLoader();
            var api_key = loader.GetString("PAT_KEY");

            //placeholder keyboard event until there's a listview of buses with a click event
            var userInput = Observable.FromEventPattern(listview, "SelectionChanged")
                                      .Select(k =>
                                      {
                                          var list = listview.SelectedItems
                                                             .Select(i => ((ListViewItem)i).Content.ToString())
                                                             .ToArray();
                                          return Observable.Timer(TimeSpan.Zero,TimeSpan.FromSeconds(10))
                                                           .Select(l => list);
                                     })
                                      .Throttle(new TimeSpan(days: 0
                                                             , hours: 0
                                                             , minutes: 0
                                                             , seconds: 0
                                                             , milliseconds: 300))
                                      .Switch();

            var vehicles = userInput.Select(async xs => (await PATAPI.GetBustimeResponse(xs, api_key)).vehicle)
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
                                       ,onError: ex => 
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
