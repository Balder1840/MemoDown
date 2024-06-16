using MemoDown.Models;
using MemoDown.Options;
using Microsoft.Extensions.Options;
using Radzen;
using System.Net;

namespace MemoDown.Services
{
    public class CloudflareTurnstileService
    {
        private readonly IOptions<MemoDownOptions> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string CloudflareTurnstileVerifyEndpoint = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
        public CloudflareTurnstileService(IOptions<MemoDownOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CloudflareTurnstileVerifyResult> Verify(string token, string? idempotencyKey = null,
                IPAddress? userIpAddress = null, CancellationToken ct = default)
        {
            CloudflareTurnstileVerifyRequestModel requestModel = new(_options.Value.CloudflareTurnstile.SecretKey!, token, userIpAddress?.ToString(), idempotencyKey);

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(CloudflareTurnstileVerifyEndpoint, requestModel);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CloudflareTurnstileVerifyResult>();

            //var result = await response.ReadAsync<CloudflareTurnstileVerifyResult>();

            return result!;
        }
    }
}
