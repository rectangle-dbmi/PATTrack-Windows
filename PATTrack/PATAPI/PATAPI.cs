namespace PATTrack.PATAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using PATTrack.PATAPI.POCO;
    using Windows.Devices.Geolocation;
    using Windows.Foundation.Diagnostics;
    using Windows.UI.Xaml.Controls.Maps;

    public class PAT_API
    {
        private const string BaseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";

        public static async Task<VehicleResponse> GetBustimeResponse(string[] routes, string api_key)
        {
            if (routes.Length == 0 || routes.Length > 10)
            {
                return new VehicleResponse()
                {
                    ResponseError = new Exception("Routes array must contain between 1 and 10 elements")
                };
            }

            string requestUrl = string.Format(
                "{0}getvehicles?key={1}&rt={2}&format=xml",
                BaseUrl,
                api_key,
                routes.Aggregate((a, b) => a + "," + b));
            var responseStream = await PAT_API.MakeRequest(requestUrl);
            return VehicleResponse.ParseResponse(XDocument.Load(responseStream));
        }

        public static async Task<List<MapPolyline>> GetPolylines(string rt, string api_key)
        {
            PatternResponse patternResponse = await PAT_API.GetPatterns(rt, api_key);

            if (patternResponse.ResponseError != null)
            {
                return new List<MapPolyline>() { };
            }

            return patternResponse.Patterns.Select(pat =>
            {
                var points = (from pt in pat.Pts
                             select new BasicGeoposition() { Latitude = pt.Lat, Longitude = pt.Lon }).ToList();
                MapPolyline polyline = new MapPolyline();
                polyline.Path = new Geopath(points);
                polyline.Visible = true;
                return polyline;
            }).ToList();
        }

        private static async Task<Stream> MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                WebResponse response = await request.GetResponseAsync();
                XmlDocument xmlDoc = new XmlDocument();
                var f = LoggingSingleton.Instance;
                LoggingSingleton.Instance.Channel.LogMessage("request made", LoggingLevel.Critical);
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                LoggingSingleton.Instance.Channel.LogMessage(e.Message, LoggingLevel.Critical);
                return null;
            }
        }

        private static async Task<PatternResponse> GetPatterns(string route, string api_key)
        {
            string requestUrl = string.Format(
                "{0}getpatterns?key={1}&rt={2}&format=xml",
                BaseUrl,
                api_key,
                route);
            var responseStream = await PAT_API.MakeRequest(requestUrl);
            return PatternResponse.ParseResponse(XDocument.Load(responseStream));
        }
    }
}
