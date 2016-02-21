namespace PATTrack.PATAPI
{
    using System;

    public interface IResponse
    {
        Exception ResponseError { get; set; }

        bool IsError { get; set; }
    }
}
