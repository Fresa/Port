using System.IO;

namespace Kubernetes.Test.API.Server
{
    internal interface IHttpDuplexStreamFeature
    {
        public Stream Body { get; }
    }
}