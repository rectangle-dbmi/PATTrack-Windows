using System.Collections.Generic;

namespace PATTrack.PATAPI.POCO
{
    public struct PatternResponse : Response
    {
        public List<Pattern> patterns { get; set; }
    }

    public struct Pattern
    {
        public string pid { get; set; }
        public IEnumerable<Pt> pts { get; set; }
    }

    public struct Pt
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
