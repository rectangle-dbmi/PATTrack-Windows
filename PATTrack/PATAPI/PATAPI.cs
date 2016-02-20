using PATTrack.PATAPI.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation.Diagnostics;
using Windows.UI.Xaml.Controls.Maps;

namespace PATTrack.PATAPI
{
    public class PAT_API
    {
        private const string baseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";
        private async static Task<Stream> MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                WebResponse response = await request.GetResponseAsync();

                XmlDocument xmlDoc = new XmlDocument();
                var f = LoggingSingleton.Instance;
                LoggingSingleton.Instance.channel.LogMessage("request made",LoggingLevel.Critical);
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                LoggingSingleton.Instance.channel.LogMessage(e.Message, LoggingLevel.Critical);
                return null;
            }
        }

        private async static Task<PatternResponse> GetPatterns(string route, string api_key)
        {
            string requestUrl = String.Format("{0}getpatterns?key={1}&rt={2}&format=xml",
                baseUrl,
                api_key,
                route);
            var responseStream = await PAT_API.MakeRequest(requestUrl);
            return (PatternResponse) Parser.ParseResponse(XDocument.Load(responseStream),
                typeof(PatternResponse));
        }

        public async static Task<BustimeVehicleResponse> GetBustimeResponse(string[] routes, string api_key)
        {
            if (routes.Length == 0)
            {
                return new BustimeVehicleResponse();
            }
            string requestUrl = String.Format("{0}getvehicles?key={1}&rt={2}&format=xml",
                baseUrl,
                api_key,
                routes.Aggregate((a, b) => a + "," + b));
            var responseStream = await PAT_API.MakeRequest(requestUrl);
            return (BustimeVehicleResponse) Parser.ParseResponse(XDocument.Load(responseStream),
                typeof(BustimeVehicleResponse));
        }

        public static async Task<List<MapPolyline>> GetPolylines(string rt, string api_key) {
            PatternResponse patternResponse = await PAT_API.GetPatterns(rt, api_key);
            return patternResponse.patterns.Select(pat =>
            {
                var points = from pt in pat.pts
                             select new BasicGeoposition() { Latitude = pt.lat, Longitude = pt.lon };
                MapPolyline polyline = new MapPolyline();
                polyline.Path = new Geopath(points);
                polyline.Visible = true;
                return polyline;
            }).ToList();
        }
    }
}
