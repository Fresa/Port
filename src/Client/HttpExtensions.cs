using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Port.Client
{
    public static class HttpExtensions
    {
        public static async Task<T> GetFromNewtonsoftJsonAsync<T>(
            this HttpClient httpClient,
            string url,
            CancellationToken cancellationToken = default)
        {
            var message = await httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);
            message.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(
                await message.Content.ReadAsStringAsync()
                    .ConfigureAwait(false));
        }
    }
}