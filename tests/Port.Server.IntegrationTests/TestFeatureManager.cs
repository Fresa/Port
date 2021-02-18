using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FeatureManagement;

namespace Port.Server.IntegrationTests
{
    internal sealed class TestFeatureManager : IFeatureManager
    {
        private readonly ConcurrentDictionary<string, bool> _features =
            new();

        public TestFeatureManager(
            params (string Feature, bool Enabled)[] features)
        {
            foreach (var (feature, enabled) in features)
            {
                _features.AddOrUpdate(
                    feature, _ => enabled, (
                        _,
                        __) => enabled);
            }
        }

        public IAsyncEnumerable<string> GetFeatureNamesAsync()
            => throw new System.NotImplementedException();

        public Task<bool> IsEnabledAsync(
            string feature)
            => Task.FromResult(
                _features.TryGetValue(feature, out var enabled) && enabled);

        public Task<bool> IsEnabledAsync<TContext>(
            string feature,
            TContext context)
            => IsEnabledAsync(feature);
    }
}