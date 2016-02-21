namespace PATTrack.Map
{
    using System.Collections.Generic;
    using PATTrack.PATAPI.POCO;
    using Windows.UI.Xaml.Controls.Maps;

    public class Route
    {
        public List<MapPolyline> Polylines { get; set; } = new List<MapPolyline>() { };

        public List<PatternResponse.Stop> Stops { get; set; } = new List<PatternResponse.Stop>() { };
    }
}
