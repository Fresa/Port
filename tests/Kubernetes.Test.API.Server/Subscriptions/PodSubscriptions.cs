using System;
using System.Threading.Tasks;
using Kubernetes.Test.API.Server.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Kubernetes.Test.API.Server.Subscriptions
{
    public sealed class PodSubscriptions
    {
        private Func<PortForward, Task<ActionResult<string>>> _subscription =
            forward => throw new ArgumentNullException();

        internal async Task<ActionResult<string>> PortForward(
            PortForward portForward)
        {
            return await _subscription.Invoke(portForward);
        }

        public PodSubscriptions OnPortForward(
            Func<PortForward, Task<ActionResult<string>>> subscription)
        {
            _subscription = subscription;
            return this;
        }
    }
}