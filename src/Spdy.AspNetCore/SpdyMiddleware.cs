using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Spdy.Network;

namespace Spdy.AspNetCore
{
    public sealed class SpdyMiddleware : IMiddleware
    {
        public Task InvokeAsync(
            HttpContext context,
            RequestDelegate next)
        {
            // Detect if an opaque upgrade is available. If so, add a spdy upgrade.
            var upgradeFeature = context.Features.Get<IHttpUpgradeFeature>();
            if (upgradeFeature != null &&
                context.Features.Get<ISpdyFeature>() == null)
            {
                var spdyFeature = new UpgradeHandshake(context, upgradeFeature);
                context.Features.Set<ISpdyFeature>(spdyFeature);
            }

            return next.Invoke(context);
        }

        private class UpgradeHandshake : ISpdyFeature
        {
            private readonly HttpContext _context;
            private readonly IHttpUpgradeFeature _upgradeFeature;
            private bool? _isSpdyRequest;

            private static readonly string[] NeededHeaders =
            {
                HeaderNames.Connection,
                HeaderNames.Upgrade
            };

            private const string UpgradeSpdy = "SPDY/3.1";
            private const string ConnectionUpgrade = "Upgrade";

            public UpgradeHandshake(
                HttpContext context,
                IHttpUpgradeFeature upgradeFeature)
            {
                _context = context;
                _upgradeFeature = upgradeFeature;
            }

            public bool IsSpdyRequest
            {
                get
                {
                    if (_isSpdyRequest == null)
                    {
                        if (!_upgradeFeature.IsUpgradableRequest)
                        {
                            _isSpdyRequest = false;
                        }
                        else
                        {
                            var headers =
                                new List<KeyValuePair<string, string>>();
                            foreach (var headerName in NeededHeaders)
                            {
                                headers.AddRange(
                                    _context
                                        .Request.Headers
                                        .GetCommaSeparatedValues(headerName)
                                        .Select(
                                            value
                                                => new KeyValuePair<string,
                                                    string>(
                                                    headerName, value)));
                            }

                            _isSpdyRequest = CheckSupportedSpdyRequest(
                                _context.Request.Method, headers);
                        }
                    }

                    return _isSpdyRequest.Value;
                }
            }

            public async Task<SpdySession> AcceptAsync()
            {
                if (!IsSpdyRequest)
                {
                    throw new InvalidOperationException("Not a spdy request.");
                }

                GenerateResponseHeaders(_context.Response.Headers);

                var transport = await _upgradeFeature.UpgradeAsync()
                    .ConfigureAwait(false); // Sets status code to 101

                return SpdySession.CreateServer(new StreamingNetworkClient(transport));
            }

            private static bool CheckSupportedSpdyRequest(
                string method,
                IEnumerable<KeyValuePair<string, string>> headers)
            {
                bool validUpgrade = false, validConnection = false;

                if (!string.Equals(
                    "GET", method, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                foreach (var (key, value) in headers)
                {
                    if (string.Equals(
                        HeaderNames.Connection, key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(
                            ConnectionUpgrade, value,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            validConnection = true;
                        }
                    }
                    else if (string.Equals(
                        HeaderNames.Upgrade, key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(
                            UpgradeSpdy, value,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            validUpgrade = true;
                        }
                    }
                }

                return validConnection && validUpgrade;
            }

            private static void GenerateResponseHeaders(
                IHeaderDictionary headers)
            {
                headers[HeaderNames.Connection] = ConnectionUpgrade;
                headers[HeaderNames.Upgrade] = UpgradeSpdy;
            }
        }
    }
}