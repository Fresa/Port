using System.Threading.Tasks;

namespace Spdy.AspNetCore
{
    public interface ISpdyManager
    {
        bool IsSpdyRequest { get; }
        Task<SpdySession> AcceptSpdyAsync();
    }
}