using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util
{
    public static class ServiceCollectionExtensions
    {
        public static void RemoveDbConext<T>(this IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));

            if(descriptor != null) services.Remove(descriptor);
        }
        
        public static async void EnsureCreated<T>(this IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DataContext>();

            await db.Database.MigrateAsync();

            // Seed data
            await DbHelper.InitDbForTestsAsync(db);
        }
    }
}