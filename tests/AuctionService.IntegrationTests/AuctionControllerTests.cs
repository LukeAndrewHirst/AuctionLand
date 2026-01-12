using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests
{
   [Collection("Sharedcollection")]
    public class AuctionControllerTests(CustomWebAppFactory factory) : IAsyncLifetime
    {
        private readonly HttpClient _client = factory.CreateClient();
        private const string TEST_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

        [Fact]
        public async Task GetAuctions_ShouldReturn3Auctions()
        {
           var responseMessage = await _client.GetAsync("/api/auctions");
           responseMessage.EnsureSuccessStatusCode();

           var response = await responseMessage.Content.ReadFromJsonAsync<List<Auction>>();
           Assert.NotNull(response);
           Assert.Equal(3, response.Count);
        }

        [Fact]
        public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
        {
           var response = await _client.GetFromJsonAsync<AuctionDto>($"/api/auctions/{TEST_ID}");

           Assert.NotNull(response);
           Assert.Equal("GT", response.Model);
        }

        [Fact]
        public async Task GetAuctionById_WithInValidId_ShouldInternalServer()
        {
           var response = await _client.GetAsync($"/api/auctions/{Guid.NewGuid()}");

           Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuction_NoAuth_ShouldReturn401()
        {
           var auction = new CreateAuctionDto{Make = "Test", Model = "Test Model", Color = "Test Color", ImageUrl="Test", ReservePrice = 1000};

           var response = await _client.PostAsJsonAsync($"/api/auctions/", auction);

           Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuction_WithAuth_ShouldReturn201()
        {
           var auction = GetAuctionForCreate();
           _client.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("testuser"));

           var response = await _client.PostAsJsonAsync($"/api/auctions/", auction);

           response.EnsureSuccessStatusCode();

           Assert.Equal(HttpStatusCode.Created, response.StatusCode); 
           Assert.Equal(HttpStatusCode.Created, response.StatusCode); 

           var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
           Assert.Equal("testuser", createdAuction?.Seller);
         }

         [Fact]
         public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
         {
            var auction = GetAuctionForCreate();
            auction.Make = string.Empty;

            _client.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("testuser"));

            var response = await _client.PostAsJsonAsync($"/api/auctions/", auction);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
         }

         [Fact]
         public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
         {
            var auctionUpdated = new UpdateAuctionDto{Make = "Updated Make"};
            _client.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            var response = await _client.PutAsJsonAsync($"/api/auctions/{TEST_ID}", auctionUpdated);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
         }

         [Fact]
         public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
         {
            var auctionUpdated = new UpdateAuctionDto{Make = "Updated Make"};
            _client.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("notbob"));

            var response = await _client.PutAsJsonAsync($"/api/auctions/{TEST_ID}", auctionUpdated);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
         }

         public Task InitializeAsync() => Task.CompletedTask;

         public async Task DisposeAsync()
         {
               using var scope = factory.Services.CreateAsyncScope();
               var db = scope.ServiceProvider.GetRequiredService<DataContext>();
               await DbHelper.ReinitDbForTestsAsync(db);

               await Task.CompletedTask;
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