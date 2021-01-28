using System.Threading.Tasks;

namespace Spdy.AspNetCore
{
    public interface ISpdyFeature
    {
        bool IsSpdyRequest { get; }
        Task<SpdySession> AcceptAsync();
    }
}