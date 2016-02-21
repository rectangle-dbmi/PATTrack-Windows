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
    using Windows.UI.Xaml.Controls.Maps;

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
            if (!this.routes.ContainsKey(selection.Rt))
            {
                PatternResponse patternResponse = await PAT_API.GetPatterns(selection.Rt, api_key);
                var stops = new List<PatternResponse.Stop>() { };
                foreach (var stop in from pattern in patternResponse.Patterns
                                     from stop in pattern.Stops
                                     select stop)
                {
                    stops.Add(stop);
                    if (!this.busStops.ContainsKey(stop.Stpid))
                    {
                        this.AddStopToMap(stop);
                    }
                }

                this.routes[selection.Rt] = new Route()
                {
                    Polylines = this.GetPolylines(selection.Rt, patternResponse),
                    Stops = (from pattern in patternResponse.Patterns
                            from stop in pattern.Stops
                            select stop).Distinct().ToList()
                };
            }

            if (selection.Selected)
            {
                foreach (var line in this.routes[selection.Rt].Polylines)
                {
                    line.Visible = true;
                    line.StrokeThickness = 3;
                    this.Map.MapElements.Add(line);
                }

                foreach (var stop in this.routes[selection.Rt].Stops)
                {
                    this.busStops[stop.Stpid]?.IncrementRouteCount();
                }
            }
            else
            {
                    foreach (var line in this.routes[selection.Rt].Polylines)
                    {
                        line.Visible = false;
                        this.Map.MapElements.Remove(line);
                    }

                    foreach (var stop in this.routes[selection.Rt].Stops)
                    {
                        this.busStops[stop.Stpid]?.DecrementRouteCount();
                    }
            }
        }

        private void AddStopToMap(PatternResponse.Stop stop)
        {
            var icon = new MapIcon();
            icon.Location = new Geopoint(new BasicGeoposition() { Latitude = stop.Lat, Longitude = stop.Lon });
            icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            icon.Title = stop.Stpid;
            this.Map.MapElements.Add(icon);
            this.busStops.Add(
                stop.Stpid
                , new MapStop()
                {
                    Stop = stop,
                    Icon = icon
                });
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
