using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AuctionService.UnitTests
{
    public class AuctionControllerTests
    {
        private readonly Mock<IAuctionRepository> repository;
        private readonly Mock<IPublishEndpoint> publishEndpoint;
        private readonly Fixture fixture;
        private readonly AuctionsController controller;
        private readonly IMapper mapper;

        public AuctionControllerTests()
        {
            fixture = new Fixture();
            repository = new Mock<IAuctionRepository>();
            publishEndpoint = new Mock<IPublishEndpoint>();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfiles>();
            });

            var provider = services.BuildServiceProvider();
            mapper = provider.GetRequiredService<IMapper>();

            controller = new AuctionsController(repository.Object, mapper, publishEndpoint.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = Helpers.GetClaimsPrincipal()
                    }
                }
            };
        }

        [Fact]
        public async Task GetAuctions_WithNoParams_Returns10Auctions()
        {
            var auctions = fixture.CreateMany<AuctionDto>(10).ToList();
            repository.Setup(repository => repository.GetAuctionsAsync(null)).ReturnsAsync(auctions);

            var result = await controller.GetAllAuctions(null);

            Assert.NotNull(result.Value);
            Assert.Equal(10, result.Value.Count);
            Assert.IsType<ActionResult<List<AuctionDto>>>(result);
        }

        [Fact]
        public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
        {
            var auction = fixture.Create<AuctionDto>();
            repository.Setup(repository => repository.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

            var result = await controller.GetAuctionById(auction.Id);

            Assert.NotNull(result.Value);
            Assert.Equal(auction.Make, result.Value.Make);
            Assert.IsType<ActionResult<AuctionDto>>(result);
        }

        [Fact]
        public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
        {
            repository.Setup(repository => repository.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AuctionDto?) null);

            var result = await controller.GetAuctionById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateAuction_WithValidCreateAuctioDto_CreatedAtActionResult()
        {
            var auction = fixture.Create<CreateAuctionDto>();
            repository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
            repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            var result = await controller.CreateAuction(auction);
            var createdResult = result.Result as CreatedAtActionResult;

            Assert.NotNull(createdResult);
            Assert.Equal("GetAuctionById", createdResult.ActionName);
            Assert.IsType<AuctionDto>(createdResult.Value);
        }

        [Fact]
        public async Task CreateAuction_FailedSave_Returns400BadRequest()
        {
            var auctionDto = fixture.Create<CreateAuctionDto>();
            repository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
            repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

            var result = await controller.CreateAuction(auctionDto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();
            auction.Item = fixture.Build<Item>().Without(a => a.Auction).Create();
            auction.Seller = "test";

            var updateDto = fixture.Create<UpdateAuctionDto>();
            repository.Setup(repo => repo.GetAuctionByEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            var result = await controller.UpdateAuction(auction.Id, updateDto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();
            auction.Seller = "not-test";

            var updateDto = fixture.Create<UpdateAuctionDto>();
            repository.Setup(repo => repo.GetAuctionByEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

            var result = await controller.UpdateAuction(auction.Id, updateDto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();

            var updateDto = fixture.Create<UpdateAuctionDto>();
            repository.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

            var result = await controller.UpdateAuction(auction.Id, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();
            auction.Seller = "test";

            repository.Setup(repo => repo.GetAuctionByEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
            repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

            var result = await controller.DeleteAuction(auction.Id);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();

            repository.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

            var result = await controller.DeleteAuction(auction.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAuction_WithInvalidUser_Returns403Response()
        {
            var auction = fixture.Build<Auction>().Without(i => i.Item).Create();
            auction.Seller = "not-test";

            repository.Setup(repo => repo.GetAuctionByEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

            var result = await controller.DeleteAuction(auction.Id);

            Assert.IsType<ForbidResult>(result);
        }
    }
}