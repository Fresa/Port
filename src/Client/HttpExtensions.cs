using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Port.Shared;

namespace Port.Client
{
    internal static class HttpExtensions
    {
        internal static async Task<T> GetFromNewtonsoftJsonAsync<T>(
            this HttpClient httpClient,
            string url,
            CancellationToken cancellationToken = default)
        {
            var message = await httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);
            message.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(
                await message.Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false));
        }
    }

    internal static class PodExtensions
    {
        internal static IEnumerable<Pod> WhereServiceIsHosted(
            this IEnumerable<Pod> pods,
            Service service)
        {
            return pods.Where(
                pod =>
                    service.Selectors.Any(
                        pair =>
                            pod.Labels.Any(
                                valuePair =>
                                    valuePair.Key == pair.Key &&
                                    valuePair.Value == pair.Value)));
        }
    }
}