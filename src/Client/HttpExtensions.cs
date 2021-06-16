using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
}