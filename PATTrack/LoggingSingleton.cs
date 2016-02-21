namespace PATTrack
{
    using Windows.Foundation.Diagnostics;

    internal sealed class LoggingSingleton
    {
        private static readonly LoggingSingleton Inst = new LoggingSingleton();

        private LoggingSingleton()
        {
            this.Session = new LoggingSession("SessionName");
            this.Channel = new LoggingChannel("ChannelName", null);
            this.Session.AddLoggingChannel(this.Channel);
        }

        public LoggingSession Session { get; private set; }

        public LoggingChannel Channel { get; private set; }

        internal static LoggingSingleton Instance => Inst;
    }
}