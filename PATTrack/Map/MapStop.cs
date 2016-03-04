namespace PATTrack.Map
{
    using PATAPI.POCO;
    using Windows.UI.Xaml.Controls.Maps;

    internal class MapStop
    {
        private ushort refcount;

        internal MapIcon Icon { get; set; }

        internal PatternResponse.Stop Stop { get; set; }

        internal void AddRoute(string rt)
        {
            this.refcount += 1;
            this.Icon.Visible = true;
        }

        internal void RemoveRoute(string rt)
        {
            this.refcount -= 1;
            checked
            {
                this.Icon.Visible = this.refcount != 0;
            }
        }
    }
}