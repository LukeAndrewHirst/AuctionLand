using Microsoft.AspNetCore.Components.Forms;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var lastUpdated = await DB.Default.Find<Item, string>().Sort(i => i.Descending(i => i.UpdatedAt)).Project(i => i.UpdatedAt.ToString()).ExecuteFirstAsync();

            return await httpClient.GetFromJsonAsync<List<Item>>(configuration["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated) ?? [];
        }
    }
}