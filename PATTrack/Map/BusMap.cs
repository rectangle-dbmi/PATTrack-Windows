namespace PATTrack.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PATAPI.POCO;
    using Windows.Devices.Geolocation;
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml.Controls.Maps;
    using Stop = PATAPI.POCO.PatternResponse.Stop;
    
    public struct VehicleSelected
    {
        public string Rt { get; set; }

        public bool Selected { get; set; }
    }

    internal sealed class BusMap
    {
        private Dictionary<string, Route> routes = new Dictionary<string, Route>();

        private Dictionary<string, MapIcon> busIcons = new Dictionary<string, MapIcon>();

        private Dictionary<string, MapStop> busStops = new Dictionary<string, MapStop>();

        internal MapControl Map { get; set; } = null;

        internal void ClearMap()
        {
            this.routes.Clear();
            this.busIcons.Clear();
            this.busStops.Clear();
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

        internal void UpdatePolylines(VehicleSelected selection, PatternResponse patternResponse, string apiKey)
        {
            Route route;
            var previouslySelected = this.routes.TryGetValue(selection.Rt, out route);

            // prevent attempting to select an already selected route
            if (selection.Selected && (route?.IsSelected ?? false))
            {
                return;
            }

            // if this is new, get the polylines and initialize route
            if (!previouslySelected)
            {
                if (selection.Selected == false)
                {
                    return;
                }

                // this is useless, fix it
                if (patternResponse.IsError)
                {
                    return;
                }

                route = new Route()
                {
                    Polylines = this.GetPolylines(patternResponse),
                    Stops = (from pattern in patternResponse.Patterns
                             from stop in pattern.Stops
                             select stop).Distinct().ToList()
                };
                this.routes[selection.Rt] = route;
                foreach (var stop in route.Stops)
                {
                    if (!this.busStops.ContainsKey(stop.Stpid))
                    {
                        this.AddStopToMap(stop);
                    }
                }
            }

            this.ToggleBusStops(selection, route);
            this.TogglePolylines(selection, route);

            route.IsSelected = selection.Selected;
        }

        private void ToggleBusStops(VehicleSelected selection, Route route)
        {
            var stops = route.Stops;
            if (selection.Selected)
            {
                foreach (var stop in stops)
                {
                    var mapStop = this.busStops[stop.Stpid];
                    mapStop.AddRoute(selection.Rt);
                }
            }
            else
            {
                foreach (var stop in stops) 
                {
                    var mapStop = this.busStops[stop.Stpid];
                    mapStop.RemoveRoute(selection.Rt);
                }
            }
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

        private void AddStopToMap(Stop stop)
        {
            var icon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/bus_stop.png")),
                Location = new Geopoint(new BasicGeoposition() { Latitude = stop.Lat, Longitude = stop.Lon }),
                NormalizedAnchorPoint = new Point(0.5, 0.5),
                MapTabIndex = int.Parse(stop.Stpid),
                Title = stop.Stpid,
                ZIndex = 10
            };
            this.Map.MapElements.Add(icon);
            var mapStop = new MapStop()
            {
                Stop = stop,
                Icon = icon
            };
            this.busStops.Add(stop.Stpid, mapStop);
        }

        private List<MapPolyline> GetPolylines(PatternResponse patternResponse)
        {
            if (patternResponse.ResponseError != null)
            {
                return new List<MapPolyline>();
            }

            return patternResponse.Patterns.Select(pat =>
            {
                var points = from pt in pat.Pts
                             select new BasicGeoposition()
                             {
                                 Latitude = pt.Lat,
                                 Longitude = pt.Lon
                             };
                var polyline = new MapPolyline
                {
                    Path = new Geopath(points.ToList()),
                    Visible = true,
                    StrokeThickness = 3
                };
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
            var mi = new MapIcon
            {
                Location = new Geopoint(new BasicGeoposition() { Latitude = v.Lat, Longitude = v.Lon }),
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = v.Rt
            };
            this.busIcons[v.Vid] = mi;
            this.Map.MapElements.Add(mi);
        }
    }
}
