namespace PATTrack.Map
{
    using PATAPI.POCO;
    using Windows.UI.Xaml.Controls.Maps;

    internal class MapStop
    {
        internal MapIcon Icon { get; set; }

        internal PatternResponse.Stop Stop { get; set; }

        private int RouteCount { get; set; } = 0;

        internal void IncrementRouteCount()
        {
            this.RouteCount += 1;
            this.Icon.Visible = true;
        }

        internal void DecrementRouteCount()
        {
            this.RouteCount -= 1;
            if (this.RouteCount == 0)
            {
                this.Icon.Visible = false;
            }
        }
    }
}