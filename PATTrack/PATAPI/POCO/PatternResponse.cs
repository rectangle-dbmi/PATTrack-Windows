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
                                select new Pattern()
                                {
                                    Pid = ptr.Element("pid")?.Value,
                                    Pts = from pt in ptr?.Descendants("pt")
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

            public IEnumerable<Pt> Pts { get; set; }
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
