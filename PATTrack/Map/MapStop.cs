namespace PATTrack.Map
{
    using PATAPI.POCO;
    using System.Collections.Generic;
    using Windows.UI.Xaml.Controls.Maps;

    internal class MapStop
    {
        internal MapIcon Icon { get; set; }

        internal PatternResponse.Stop Stop { get; set; }

        private HashSet<string> Routes { get; set; } = new HashSet<string>() { };

        internal void AddRoute(string rt)
        {
            Routes.Add(rt);
            this.Icon.Visible = true;
        }

        internal void RemoveRoute(string rt)
        {
            Routes.Remove(rt);
            this.Icon.Visible = this.Routes.Count != 0;
        }
    }
}