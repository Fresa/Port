using System.Threading.Tasks;

namespace Port.Server.Spdy.AspNet
{
    public interface ISpdyManager
    {
        bool IsSpdyRequest { get; }
        Task<SpdySession> AcceptSpdyAsync();
    }
}