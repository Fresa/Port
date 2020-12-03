using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace Kubernetes.Test.API.Server
{
    internal sealed class UpgradeTestHandshake : IHttpUpgradeFeature,
        IHttpDuplexStreamFeature
    {
        private readonly HttpContext _context;
        private const string ConnectionUpgrade = "Upgrade";
        private bool? _isSpdyRequest;

        private static readonly string[] NeededHeaders =
        {
            HeaderNames.Connection,
            HeaderNames.Upgrade
        };

        public UpgradeTestHandshake(
            HttpContext context) => _context = context;

        public Task<Stream> UpgradeAsync()
        {
            if (!IsUpgradableRequest)
            {
                throw new InvalidOperationException("Not an upgrade request.");
            }

            if (_context.Response.HasStarted)
            {
                throw new InvalidOperationException(
                    "The response has already started");
            }

            var client = new CrossWiredStream();
            var server = client.CreateReverseStream();

            _context.Response.StatusCode =
                StatusCodes.Status101SwitchingProtocols;
            Body = client;
            OnUpgraded.Invoke();
            return Task.FromResult(server);
        }

        internal event Action OnUpgraded = () => { };

        public Stream Body { get; private set; } = Stream.Null;

        public bool IsUpgradableRequest
        {
            get
            {
                if (_isSpdyRequest == null)
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
                                            string>(headerName, value)));
                    }

                    _isSpdyRequest = CheckSupportedUpgradeRequest(headers);
                }

                return _isSpdyRequest.Value;
            }
        }

        private static bool CheckSupportedUpgradeRequest(
            IEnumerable<KeyValuePair<string, string>> headers)
        {
            bool validUpgrade = false, validConnection = false;

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
                    validUpgrade = true;
                }
            }

            return validConnection && validUpgrade;
        }
    }
}