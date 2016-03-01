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

    internal class PAT_API
    {
        private const string BaseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";

        public static async Task<VehicleResponse> GetBustimeResponse(string[] routes, string api_key)
        {
            if (routes.Length == 0 || routes.Length > 10)
            {
                return new VehicleResponse()
                {
                    IsError = true,
                    ResponseError = new Exception("Routes array must contain between 1 and 10 elements")
                };
            }

            string requestUrl = string.Format(
                "{0}getvehicles?key={1}&rt={2}&format=xml",
                BaseUrl,
                api_key,
                routes.Aggregate((a, b) => a + "," + b));
                return await VehicleResponse.ParseResponse(requestUrl);
        }

        internal static async Task<PatternResponse> GetPatterns(string route, string api_key)
        {
            string requestUrl = string.Format(
            "{0}getpatterns?key={1}&rt={2}&format=xml",
            BaseUrl,
            api_key,
            route);
            return await PatternResponse.ParseResponse(requestUrl);
        }

        internal static async Task<Stream> MakeRequest(string requestUrl)
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
                throw e;
            }
        }
    }
}
