namespace PATTrack.Extensions
{
    using Windows.Devices.Geolocation;
    using Windows.Foundation;
    using Windows.UI.Xaml.Controls.Maps;
    using PATAPI.POCO;
    public static class MapExtensions
    {
        internal static void AddBusIcons(this MapControl map, vehicle[] vehicles)
        {
            if (vehicles == null)
            {
                return;
            }

            foreach (var v in vehicles)
            {
                MapIcon mi = new MapIcon();
                mi.Location = new Geopoint(new BasicGeoposition() { Latitude = v.lat, Longitude = v.lon });
                mi.NormalizedAnchorPoint = new Point(0.5, 1.0);
                mi.Title = v.rt;
                map.MapElements.Add(mi);
            }
        }
    }
}