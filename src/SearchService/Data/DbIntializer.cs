using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbIntializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("AuctionLandSearchDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));
            await DB.Default.Index<Item>().Key(k => k.Make, KeyType.Text).Key(k => k.Model, KeyType.Text).Key(k => k.Color, KeyType.Text).CreateAsync();

            var count = await DB.Default.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine($"Items to sync: {items.Count}");

            if(items.Count > 0) await DB.Default.SaveAsync(items);
        }
    }
}