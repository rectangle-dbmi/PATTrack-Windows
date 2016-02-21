namespace PATTrack
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
        internal MapControl Map { get; set; } = null;

        private Dictionary<string, List<MapPolyline>> Polylines { get; set; } = new Dictionary<string, List<MapPolyline>>() { };

        private Dictionary<string, MapIcon> BusIcons { get; set; } = new Dictionary<string, MapIcon>() { };

        internal void ClearMap()
        {
            this.Map.MapElements.Clear();
        }

        internal void UpdateBuses(VehicleResponse.Vehicle[] vehicles)
        {
            foreach (var icon in this.BusIcons)
            {
                this.Map.MapElements.Remove(icon.Value);
            }

            this.AddBusIcons(vehicles);
        }

        internal async Task UpdatePolylines(VehicleSelected selection, string api_key)
        {
            if (!this.Polylines.ContainsKey(selection.Rt))
            {
                PatternResponse patternResponse = await PAT_API.GetPatterns(selection.Rt, api_key);
                this.Polylines[selection.Rt] =  this.GetPolylines(selection.Rt, patternResponse);
            }

            foreach (var line in this.Polylines[selection.Rt])
            {
                if (selection.Selected)
                {
                    line.Visible = true;
                    line.StrokeThickness = 3;
                    this.Map.MapElements.Add(line);
                }
                else
                {
                    line.Visible = false;
                    this.Map.MapElements.Remove(line);
                }
            }
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
            this.BusIcons[v.Vid] = mi;
            this.Map.MapElements.Add(mi);
        }
    }
}
