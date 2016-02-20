namespace PATTrack.PATAPI.POCO
{
    public struct BustimeVehicleResponse : Response
    {
        public error[] error { get; set; }

        public vehicle[] vehicle { get; set; }
    }

    public struct error {
        
        public string vid { get; set; }
        
        public string rt { get; set; }
        
        public string msg { get; set; }
    }

    public struct vehicle {
        
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
