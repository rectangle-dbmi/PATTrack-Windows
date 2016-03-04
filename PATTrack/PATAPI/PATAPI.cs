namespace PATTrack.PATAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using POCO;
    using Windows.Foundation.Diagnostics;

    internal class PatApi
    {
        private const string BaseUrl = @"http://truetime.portauthority.org/bustime/api/v2/";

        public static Func<string, string, Task<PatternResponse>> GetPatternsMemo()
        {
            Dictionary<string, PatternResponse> dict = new Dictionary<string, PatternResponse>();

            return async (route, apiKey) =>
            {
                if (!dict.ContainsKey(route))
                {
                    dict[route] = await GetPatterns(route, apiKey);
                }

                return dict[route];
            };
        }

        public static async Task<VehicleResponse> GetBustimeResponse(string[] routes, string apiKey)
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
                apiKey,
                routes.Aggregate((a, b) => a + "," + b));
                return await VehicleResponse.ParseResponse(requestUrl);
        }

        internal static async Task<PatternResponse> GetPatterns(string route, string apiKey)
        {
            string requestUrl = string.Format(
            "{0}getpatterns?key={1}&rt={2}&format=xml",
            BaseUrl,
            apiKey,
            route);
            return await PatternResponse.ParseResponse(requestUrl);
        }

        internal static async Task<Stream> MakeRequest(string requestUrl)
        {
            try
            {
                var request = WebRequest.Create(requestUrl) as HttpWebRequest;
                var response = await request.GetResponseAsync();
                LoggingSingleton.Instance.Channel.LogMessage("request made", LoggingLevel.Critical);
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                LoggingSingleton.Instance.Channel.LogMessage(e.Message, LoggingLevel.Critical);
                throw;
            }
        }
    }
}
