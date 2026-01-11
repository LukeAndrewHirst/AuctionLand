using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController(IAuctionRepository repository,IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
        {
            return await repository.GetAuctionsAsync(date);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await repository.GetAuctionByIdAsync(id);
            if(auction == null) return NotFound();

            return auction;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = mapper.Map<Auction>(createAuctionDto);

            var seller = User.Identity?.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
            auction.Seller = seller;

            repository.AddAuction(auction);

            var newAuction = mapper.Map<AuctionDto>(auction);
            await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));

            var result = await repository.SaveChangesAsync();
            if(!result) return BadRequest("Failed to create auction");

            return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, newAuction);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await repository.GetAuctionByEntityById(id);
            if(auction == null) return NotFound();

            if(auction.Seller != User.Identity?.Name) return Forbid();

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.UpdatedAt = DateTime.UtcNow;

            await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

            var result = await repository.SaveChangesAsync();
            if(!result) return BadRequest("Failed to update auction");

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await repository.GetAuctionByEntityById(id);
            if(auction == null) return NotFound();

            // Check seller is current user - to be replaced with auth later
            if(auction.Seller != User.Identity?.Name) return Forbid();
            
            repository.RemoveAuction(auction);

            await publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});

            var result = await repository.SaveChangesAsync();
            if(!result) return BadRequest("Failed to delete auction");

            return Ok();
        }
    }
}