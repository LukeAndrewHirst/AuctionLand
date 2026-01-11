using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuctionService.IntegrationTests.Util
{
    public static class TestJsonExtensions
    {
        private static readonly JsonSerializerOptions TestJsonOptions = new()
        {
            Converters = {new JsonStringEnumConverter()},
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T?> ReadFromJsonAsync<T>(this HttpContent content)
        {
            return await content.ReadFromJsonAsync<T>(TestJsonOptions);
        }
    }
}