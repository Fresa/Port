using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.Test.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/namespaces/{namespace}/pods")]
    public class PodController : ControllerBase
    {
        private readonly TestFramework _testFramework;

        public PodController(
            TestFramework testFramework)
        {
            _testFramework = testFramework;
        }

        [HttpPost("{name}/portforward")]
        public async Task<ActionResult<string>> PortForward(
            string @namespace,
            string name,
            [FromQuery] int[] ports)
        {
            return await _testFramework.PodSubscriptions.PortForward(
                new PortForward(@namespace, name, ports));
        }
    }

    public abstract class Workload
    {
        protected internal Workload(
            string @namespace)
        {
            Namespace = @namespace;
        }

        public string Namespace { get; }
    }

    public abstract class Pod : Workload
    {
        protected internal Pod(
            string @namespace,
            string name)
            : base(@namespace)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public sealed class PortForward : Pod
    {
        internal PortForward(
            string @namespace,
            string name,
            int[] ports)
            : base(@namespace, name)
        {
            Ports = ports;
        }

        public int[] Ports { get; }
    }
}