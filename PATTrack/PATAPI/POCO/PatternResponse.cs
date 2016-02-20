namespace PATTrack.PATAPI.POCO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Windows.Foundation.Diagnostics;
    public class PatternResponse : Response
    {
        public List<Pattern> patterns { get; set; }

        public Exception ResponseError { get; set; }

        public static PatternResponse ParseResponse(XDocument doc)
        {
            try
            {
                return new PatternResponse()
                {
                    patterns = (from ptr in doc.Descendants("ptr")
                                select new Pattern()
                                {
                                    pid = ptr.Element("pid")?.Value,
                                    pts = from pt in ptr?.Descendants("pt")
                                          select new Pt()
                                          {
                                              seq = int.Parse(pt.Element("seq")?.Value),
                                              lat = double.Parse(pt.Element("lat")?.Value),
                                              lon = double.Parse(pt.Element("lon")?.Value),
                                              typ = pt.Element("typ")?.Value,
                                              stpid = pt.Element("stpid")?.Value,
                                              stpnm = pt.Element("stpnm")?.Value,
                                              pdist = pt.Element("pdist")?.Value
                                          }
                                }).ToList()
                };
            }
            catch (Exception ex)
            {
                LoggingSingleton.Instance.channel.LogMessage("Exception in Response.ParseResponse", LoggingLevel.Error);
                LoggingSingleton.Instance.channel.LogMessage(ex.StackTrace, LoggingLevel.Verbose);
                LoggingSingleton.Instance.channel.LogMessage(doc.ToString(), LoggingLevel.Verbose);
                ex.Data["XmlResponse"] = doc.ToString();
                return new PatternResponse() { ResponseError = ex };
            }
        }
    }

    public class Pattern
    {
        public string pid { get; set; }
        public IEnumerable<Pt> pts { get; set; }
    }

    public class Pt
    {
          public int seq{ get; set; }
          public double lat{ get; set; }
          public double lon{ get; set; }
          public string typ{ get; set; }
          public string stpid{ get; set; }
          public string stpnm{ get; set; }
          public string pdist{ get; set; }
    }
}
