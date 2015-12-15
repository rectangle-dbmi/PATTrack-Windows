using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation.Diagnostics;

namespace PATTrack
{
    class PATAPI
    {
        internal static Stream MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                WebResponse response = request.GetResponseAsync().Result;

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
    }
}
