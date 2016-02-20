using PATTrack.PATAPI;
using PATTrack.PATAPI.POCO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;

namespace PATTrack
{
    public struct vehicleselected
    {
        public string rt { get; set; }
        public bool selected { get; set; }
    }

    sealed internal class BusMap
    {
        internal MapControl map { get; set; } = null;
        internal Dictionary<string,List<MapPolyline>> polylines { get; set; } = new Dictionary<string, List<MapPolyline>>();
        internal Dictionary<string,MapIcon> busIcons { get; set; } = new Dictionary<string, MapIcon>();

        internal void ClearMap()
        {
            map.MapElements.Clear();
        }

        internal void UpdateBuses(vehicle[] vehicles)
        {
            foreach (var icon in busIcons)
            {
                map.MapElements.Remove(icon.Value);
            }
            this.AddBusIcons(vehicles);
        }

        private void AddBusIcons(vehicle[] vehicles)
        {
            if (vehicles == null)
            {
                return;
            }

            foreach (var v in vehicles)
            {
                AddBusIcon(v);
            }
        }

        private void AddBusIcon(vehicle v)
        {
            MapIcon mi = new MapIcon();
            mi.Location = new Geopoint(new BasicGeoposition() { Latitude = v.lat, Longitude = v.lon });
            mi.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mi.Title = v.rt;
            busIcons[v.vid] = mi;
            map.MapElements.Add(mi);
        }

        internal async Task AddPolylines(vehicleselected selection, string api_key)
        {
            if (!polylines.ContainsKey(selection.rt))
            {
                polylines[selection.rt] = await PAT_API.GetPolylines(selection.rt, api_key);
            }
            foreach (var line in polylines[selection.rt])
            {
                if (selection.selected)
                {
                    line.Visible = true;
                    line.StrokeThickness = 3;
                    map.MapElements.Add(line);
                }
                else
                {
                    line.Visible = false;
                    map.MapElements.Remove(line);
                }
            }
        }
    }
}
