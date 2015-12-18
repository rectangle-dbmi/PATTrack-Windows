using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Foundation.Diagnostics;

namespace PATTrack
{
    class PATAPI
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

        public async static Task<bustimeresponse> GetBustimeResponse(string[] routes, string api_key)
        {
            string requestUrl = String.Format("{0}getvehicles?key={1}&rt={2}&format=xml",
                baseUrl,
                api_key,
                routes.Aggregate((a, b) => a + "," + b));
            var responseStream = await PATAPI.MakeRequest(requestUrl);
            XmlSerializer serializer = new XmlSerializer(typeof(bustimeresponse));
            return serializer.Deserialize(responseStream) as bustimeresponse;
        }
    }
}
