using System.Threading.Tasks;

namespace Port.Server.Spdy.AspNet
{
    public interface ISpdyFeature
    {
        bool IsSpdyRequest { get; }
        Task<SpdySession> AcceptAsync();
    }
}