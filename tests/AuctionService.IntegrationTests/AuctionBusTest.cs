using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests
{
    [Collection("Sharedcollection")]
    public class AuctionBusTest(CustomWebAppFactory factory) : IAsyncLifetime
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly ITestHarness _harness = factory.Services.GetTestHarness();

        [Fact]
        public async Task CreateAuction_WithValidObject_PublishAuctionCreated()
        {
            var auction = GetAuctionForCreate();
            _client.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            var response = await _client.PostAsJsonAsync($"/api/auctions/", auction);

            response.EnsureSuccessStatusCode();
            Assert.True(await _harness.Published.Any<AuctionCreated>());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            await DbHelper.ReinitDbForTestsAsync(db);
        }

        private CreateAuctionDto GetAuctionForCreate()
         {
               return new CreateAuctionDto
               {
                  Make = "Test Make",
                  Model = "Test Model",
                  Color = "Test Color",
                  ImageUrl = "http://testimage.com/image.jpg",
                  ReservePrice = 5000,
                  Mileage = 10000,
                  Year = 2020
               };
        }
    }
}