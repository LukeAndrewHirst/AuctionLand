using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class Auctioncontroller(DataConext conext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
        {
            var query = conext.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

            if(!string.IsNullOrEmpty(date)) query = query.Where(a => a.CreatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) >0);

            return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await conext.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);
            if(auction == null) return BadRequest("Auction not found");

            return mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = mapper.Map<Auction>(createAuctionDto);

            //Add current user as seller - to be replaced with auth later
            auction.Seller = "Test";

            conext.Auctions.Add(auction);
            var result = await conext.SaveChangesAsync() > 0;

            if(!result) return BadRequest("Failed to create auction");

            return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await conext.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);
            if(auction == null) return BadRequest("Auction not found");

            // Check seller is current user - to be replaced with auth later

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.UpdatedAt = DateTime.UtcNow;

            var result = await conext.SaveChangesAsync() > 0;
            if(!result) return BadRequest("Failed to update auction");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await conext.Auctions.FindAsync(id);
            if(auction == null) return NotFound("Auction not found");

            // Check seller is current user - to be replaced with auth later
            conext.Auctions.Remove(auction);

            var result = await conext.SaveChangesAsync() > 0;
            if(!result) return BadRequest("Failed to delete auction");

            return Ok();
        }
    }
}