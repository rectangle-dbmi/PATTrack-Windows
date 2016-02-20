namespace PATTrack.PATAPI.POCO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Windows.Foundation.Diagnostics;
    public class BustimeVehicleResponse : Response
    {
        public error[] error { get; set; }

        public vehicle[] vehicle { get; set; }

        public Exception ResponseError { get; set; }

        public static BustimeVehicleResponse ParseResponse(XDocument doc)
        {
            try
            {
                return new BustimeVehicleResponse()
                {
                    vehicle = (from d in doc.Descendants("vehicle")
                               select new vehicle
                               {
                                   lat = double.Parse(d.Element("lat")?.Value),
                                   lon = double.Parse(d.Element("lon")?.Value),
                                   rt = d.Element("rt")?.Value,
                                   pid = int.Parse(d.Element("pid")?.Value),
                                   vid = d.Element("vid")?.Value
                               }).ToArray(),
                    error = (from d in doc.Descendants("error")
                             select new error
                             {
                                 rt = d.Element("rt")?.Value,
                                 vid = d.Element("vid")?.Value,
                                 msg = d.Element("msg")?.Value
                             }).ToArray()
                };
            }
            catch (Exception ex)
            {
                LoggingSingleton.Instance.channel.LogMessage("Exception in Response.ParseResponse", LoggingLevel.Error);
                LoggingSingleton.Instance.channel.LogMessage(ex.StackTrace, LoggingLevel.Verbose);
                LoggingSingleton.Instance.channel.LogMessage(doc.ToString(), LoggingLevel.Verbose);
                ex.Data["XmlResponse"] = doc.ToString();
                return new BustimeVehicleResponse() { ResponseError = ex };
            }
        }
    }

    public class error {
        
        public string vid { get; set; }
        
        public string rt { get; set; }
        
        public string msg { get; set; }
    }

    public class vehicle {
        
        public string vid { get; set; }
        
        public string tmpstmp { get; set; }
        
        public double lat { get; set; }
        
        public double lon { get; set; }
        
        public int hdg { get; set; }
        
        public int pid { get; set; }
        
        public int pdist { get; set; }
        
        public string rt { get; set; }
        
        public string des { get; set; }
        
        public bool dly { get; set; }
        
        public string srvtmstmp { get; set; }
        
        public int spd { get; set; }
        
        public int blk { get; set; }
        
        public bool blkFieldSpecified;
        
        public string tablockid { get; set; }
        
        public string tatripid { get; set; }
        
        public string zone { get; set; }
        
        public bool isSelected { get; set; }
    }
}
