namespace PATTrack.PATAPI.POCO
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Windows.Foundation.Diagnostics;

    public class VehicleResponse : IResponse
    {
        public Error[] Errors { get; set; }

        public Vehicle[] Vehicles { get; set; }

        public Exception ResponseError { get; set; }

        public bool IsError { get; set; } = false;

        public static VehicleResponse ParseResponse(XDocument doc)
        {
            try
            {
                return new VehicleResponse()
                {
                    Vehicles = (from d in doc.Descendants("vehicle")
                               select new Vehicle
                               {
                                   Lat = double.Parse(d.Element("lat")?.Value),
                                   Lon = double.Parse(d.Element("lon")?.Value),
                                   Rt = d.Element("rt")?.Value,
                                   Pid = int.Parse(d.Element("pid")?.Value),
                                   Vid = d.Element("vid")?.Value
                               }).ToArray(),
                    Errors = (from d in doc.Descendants("error")
                             select new Error
                             {
                                 Rt = d.Element("rt")?.Value,
                                 Vid = d.Element("vid")?.Value,
                                 Msg = d.Element("msg")?.Value
                             }).ToArray()
                };
            }
            catch (Exception ex)
            {
                LoggingSingleton.Instance.Channel.LogMessage("Exception in Response.ParseResponse", LoggingLevel.Error);
                LoggingSingleton.Instance.Channel.LogMessage(ex.StackTrace, LoggingLevel.Verbose);
                LoggingSingleton.Instance.Channel.LogMessage(doc.ToString(), LoggingLevel.Verbose);
                ex.Data["XmlResponse"] = doc.ToString();
                return new VehicleResponse() { ResponseError = ex, IsError = true };
            }
        }

        public class Error
        {
            public string Vid { get; set; }
            
            public string Rt { get; set; }
            
            public string Msg { get; set; }
        }

        public class Vehicle
        {
            public string Vid { get; set; }
            
            public string Tmpstmp { get; set; }
            
            public double Lat { get; set; }
            
            public double Lon { get; set; }
            
            public int Hdg { get; set; }
            
            public int Pid { get; set; }
            
            public int Pdist { get; set; }
            
            public string Rt { get; set; }
            
            public string Des { get; set; }
            
            public bool Dly { get; set; }
            
            public string Srvtmstmp { get; set; }
            
            public int Spd { get; set; }
            
            public int Blk { get; set; }
            
            public bool BlkFieldSpecified { get; set; }
            
            public string Tablockid { get; set; }
            
            public string Tatripid { get; set; }
            
            public string Zone { get; set; }
            
            public bool IsSelected { get; set; }
        }
    }
}
