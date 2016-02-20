using PATTrack.PATAPI.POCO;
using System;
using System.Linq;
using System.Xml.Linq;

namespace PATTrack.PATAPI
{
    public interface Response { }

    public class Parser
    {
        public static Response ParseResponse(XDocument doc, Type t)
        {
            try
            {
                if (t == typeof(BustimeVehicleResponse))
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
                else if (t == typeof(PatternResponse))
                {
                    return new PatternResponse()
                    {
                        patterns = (from ptr in doc.Descendants("ptr")
                                    select new Pattern()
                                    {
                                        pid = ptr.Element("pid")?.Value,
                                        pts = from pt in ptr.Descendants("pt")
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
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
