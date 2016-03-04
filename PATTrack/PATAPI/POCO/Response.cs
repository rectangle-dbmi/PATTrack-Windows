namespace PATTrack.PATAPI.POCO
{
    using System;

    public interface IResponse
    {
        Exception ResponseError { get; set; }

        bool IsError { get; set; }
    }
}
