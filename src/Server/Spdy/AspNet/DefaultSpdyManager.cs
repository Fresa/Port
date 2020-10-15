using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Port.Server.Spdy.AspNet
{
    internal sealed class DefaultSpdyManager : ISpdyManager
    {
        private static readonly Func<IFeatureCollection, ISpdyFeature?>
            NullWebSocketFeature = featureCollection => null;

        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultSpdyManager(
            IFeatureCollection features) => _features =
            new FeatureReferences<FeatureInterfaces>(features);

        private bool TryFetchSpdyFeature(
            [NotNullWhen(true)] out ISpdyFeature? feature)
        {
            feature = _features.Fetch(
                ref _features.Cache.Spdy, NullWebSocketFeature);
            return feature != null;
        }

        public bool IsSpdyRequest =>
            TryFetchSpdyFeature(out var feature) && feature.IsSpdyRequest;

        public Task<SpdySession> AcceptSpdyAsync()
        {
            if (TryFetchSpdyFeature(out var feature))
            {
                return feature.AcceptAsync();
            }

            throw new NotSupportedException("Spdy is not supported");
        }

        private struct FeatureInterfaces
        {
            public IHttpRequestFeature Request;
            public ISpdyFeature? Spdy;
        }
    }
}