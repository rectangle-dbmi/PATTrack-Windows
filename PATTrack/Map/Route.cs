﻿namespace PATTrack.Map
{
    using System.Collections.Generic;
    using Windows.UI.Xaml.Controls.Maps;
    using Stop = PATTrack.PATAPI.POCO.PatternResponse.Stop;

    public class Route
    {
        public List<MapPolyline> Polylines { get; set; } = new List<MapPolyline>() { };

        public List<Stop> Stops { get; set; } = new List<Stop>() { };
    }
}
