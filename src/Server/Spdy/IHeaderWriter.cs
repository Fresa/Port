
using System;

namespace Port.Server.Spdy
{
    public interface IHeaderWriter : IFrameWriter, IAsyncDisposable
    {
    }
}