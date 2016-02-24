namespace PATTrack.PATAPI.POCO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Windows.Foundation.Diagnostics;

    public class PatternResponse : IResponse
    {
        public List<Pattern> Patterns { get; set; }

        public Exception ResponseError { get; set; }

        public bool IsError { get; set; } = false;

        public static PatternResponse ParseResponse(XDocument doc)
        {
            try
            {
                return new PatternResponse()
                {
                    Patterns = (from ptr in doc.Descendants("ptr")
                                let pid = ptr.Element("pid")?.Value
                                let rtdir = ptr.Element("rtdir")?.Value
                                let pts = from pt in ptr?.Descendants("pt")
                                          select new Pt()
                                          {
                                              Seq = int.Parse(pt.Element("seq")?.Value),
                                              Lat = double.Parse(pt.Element("lat")?.Value),
                                              Lon = double.Parse(pt.Element("lon")?.Value),
                                              Typ = pt.Element("typ")?.Value,
                                              Stpid = pt.Element("stpid")?.Value,
                                              Stpnm = pt.Element("stpnm")?.Value,
                                              Pdist = pt.Element("pdist")?.Value
                                          }
                                let stops = from pt in ptr?.Descendants("pt")
                                            where pt?.Element("typ")?.Value == "S"
                                            select new Stop()
                                            {
                                                Stpid = pt.Element("stpid")?.Value,
                                                Stpnm = pt.Element("stpnm")?.Value,
                                                Lat = double.Parse(pt.Element("lat")?.Value),
                                                Lon = double.Parse(pt.Element("lon")?.Value),
                                                Rtdir = ptr?.Element("rtdir")?.Value
                                            }
                                select new Pattern()
                                {
                                    Pid = pid,
                                    Rtdir = rtdir,
                                    Pts = pts,
                                    Stops = stops
                                }).ToList()
                };
            }
            catch (Exception ex)
            {
                LoggingSingleton.Instance.Channel.LogMessage("Exception in Response.ParseResponse", LoggingLevel.Error);
                LoggingSingleton.Instance.Channel.LogMessage(ex.StackTrace, LoggingLevel.Verbose);
                LoggingSingleton.Instance.Channel.LogMessage(doc.ToString(), LoggingLevel.Verbose);
                ex.Data["XmlResponse"] = doc.ToString();
                return new PatternResponse() { ResponseError = ex, IsError = true };
            }
        }

        public class Pattern
        {
            public string Pid { get; set; }

            public string Rtdir { get; set; }

            public IEnumerable<Pt> Pts { get; set; }

            public IEnumerable<Stop> Stops { get; set; }
        }

        public class Stop
        {
            public double Lat { get; set; }

            public double Lon { get; set; }

            public string Rtdir { get; set; }

            public string Stpnm { get; set; }

            public string Stpid { get; set; }
        }

        public class Pt
        {
            public int Seq { get; set; }

            public double Lat { get; set; }

            public double Lon { get; set; }

            public string Typ { get; set; }

            public string Stpid { get; set; }

            public string Stpnm { get; set; }

            public string Pdist { get; set; }
        }
    }
}
