namespace PATTrack.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PATTrack.PATAPI;
    using PATTrack.PATAPI.POCO;
    using Windows.Devices.Geolocation;
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml.Controls.Maps;
    using Stop = PATTrack.PATAPI.POCO.PatternResponse.Stop;

    public struct VehicleSelected
    {
        public string Rt { get; set; }

        public bool Selected { get; set; }
    }

    internal sealed class BusMap
    {
        private Dictionary<string, Route> routes = new Dictionary<string, Route>() { };

        private Dictionary<string, MapIcon> busIcons = new Dictionary<string, MapIcon>() { };

        private Dictionary<string, MapStop> busStops = new Dictionary<string, MapStop>() { };

        internal MapControl Map { get; set; } = null;

        internal void ClearMap()
        {
            routes.Clear();
            busIcons.Clear();
            busStops.Clear();
            this.Map.MapElements.Clear();
        }

        internal void UpdateBuses(VehicleResponse.Vehicle[] vehicles)
        {
            foreach (var icon in this.busIcons)
            {
                this.Map.MapElements.Remove(icon.Value);
            }

            this.AddBusIcons(vehicles);
        }

        internal async Task UpdatePolylines(VehicleSelected selection, string api_key)
        {
            Route route;
            var previouslySelected = this.routes.TryGetValue(selection.Rt, out route);

            //prevent attempting to select an already selected route
            if (selection.Selected && (route?.IsSelected ?? false))
            {
                return;
            }

            //if this is new, get the polylines and initialize route
            if (!previouslySelected)
            {
                if (selection.Selected == false)
                {
                    return;
                }

                PatternResponse patternResponse = await PAT_API.GetPatterns(selection.Rt, api_key);

                // this is useless, fix it
                if (patternResponse.IsError)
                {
                    return;
                }

                this.routes[selection.Rt] = new Route()
                {
                    Polylines = this.GetPolylines(selection.Rt, patternResponse),
                    Stops = (from pattern in patternResponse.Patterns
                            from stop in pattern.Stops
                            select stop).Distinct().ToList()
                };
            }

            route = this.routes[selection.Rt];
            this.TogglePolylines(selection, route);
            foreach (var stop in route.Stops)
            {
                MapStop mapStop;
                if (!this.busStops.ContainsKey(stop.Stpid))
                {
                    mapStop = this.AddStopToMap(stop);
                }
                else
                {
                    mapStop = this.busStops[stop.Stpid];
                }

                if (selection.Selected)
                {
                    mapStop.AddRoute(selection.Rt);
                }
                else
                {
                    mapStop.RemoveRoute(selection.Rt);
                }
            }

            route.IsSelected = selection.Selected;
        }

        private void TogglePolylines(VehicleSelected selection, Route route)
        {
            if (selection.Selected)
            {
                foreach (var line in route.Polylines)
                {
                    line.Visible = true;
                    this.Map.MapElements.Add(line);
                }
            }
            else
            {
                foreach (var line in route.Polylines)
                {
                    line.Visible = false;
                    this.Map.MapElements.Remove(line);
                }
            }
        }

        private MapStop AddStopToMap(Stop stop)
        {
            var icon = new MapIcon();
            icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/bus_stop.png"));
            icon.Location = new Geopoint(new BasicGeoposition() { Latitude = stop.Lat, Longitude = stop.Lon });
            icon.NormalizedAnchorPoint = new Point(0.5, 0.5);
            icon.MapTabIndex = int.Parse(stop.Stpid);
            icon.Title = stop.Stpid;
            icon.ZIndex = 10;
            this.Map.MapElements.Add(icon);
            var mapStop = new MapStop()
            {
                Stop = stop,
                Icon = icon
            };
            this.busStops.Add(stop.Stpid, mapStop);
            return mapStop;
        }

        private List<MapPolyline> GetPolylines(string rt, PatternResponse patternResponse)
        {
            if (patternResponse.ResponseError != null)
            {
                return new List<MapPolyline>() { };
            }

            return patternResponse.Patterns.Select(pat =>
            {
                var points = from pt in pat.Pts
                             select new BasicGeoposition()
                             {
                                 Latitude = pt.Lat,
                                 Longitude = pt.Lon
                             };
                MapPolyline polyline = new MapPolyline();
                polyline.Path = new Geopath(points.ToList());
                polyline.Visible = true;
                polyline.StrokeThickness = 3;
                return polyline;
            }).ToList();
        }

        private void AddBusIcons(VehicleResponse.Vehicle[] vehicles)
        {
            if (vehicles == null)
            {
                return;
            }

            foreach (var v in vehicles)
            {
                this.AddBusIcon(v);
            }
        }

        private void AddBusIcon(VehicleResponse.Vehicle v)
        {
            MapIcon mi = new MapIcon();
            mi.Location = new Geopoint(new BasicGeoposition() { Latitude = v.Lat, Longitude = v.Lon });
            mi.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mi.Title = v.Rt;
            this.busIcons[v.Vid] = mi;
            this.Map.MapElements.Add(mi);
        }
    }
}
