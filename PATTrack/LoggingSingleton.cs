namespace PATTrack
{
    using Windows.Foundation.Diagnostics;

    internal sealed class LoggingSingleton
    {
        private static readonly LoggingSingleton instance = new LoggingSingleton();
        public LoggingSession session { get; private set; }
        public LoggingChannel channel { get; private set; }

        private LoggingSingleton()
        {
            session = new LoggingSession("SessionName");
            channel = new LoggingChannel("ChannelName", null);
            session.AddLoggingChannel(channel);
        }

        internal static LoggingSingleton Instance => instance;
    }
}