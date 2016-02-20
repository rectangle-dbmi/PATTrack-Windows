namespace PATTrack.PATAPI
{
    using PATTrack.PATAPI.POCO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Windows.Foundation.Diagnostics;

    public interface Response
    {
        Exception ResponseError { get; set; }
    }
}
