using AuctionService.DTOs;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AuctionService.Data
{
    public class AuctionRepository(DataContext conext , IMapper mapper) : IAuctionRepository
    {
        public void AddAuction(Auction auction)
        {
            conext.Auctions.Add(auction);
        }

        public async Task<Auction> GetAuctionByEntityById(Guid id)
        {
            var auction = await conext.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

            return auction ?? throw new KeyNotFoundException($"Auction {id} not found");
        }

        public async Task<AuctionDto?> GetAuctionByIdAsync(Guid id)
        {
            var auction = await conext.Auctions.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).FirstOrDefaultAsync(a => a.Id == id);

             return auction ?? throw new KeyNotFoundException($"Auction {id} not found");
        }

        public async Task<List<AuctionDto>> GetAuctionsAsync(string? date)
        {
            var query = conext.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

            if(!string.IsNullOrEmpty(date)) query = query.Where(a => a.CreatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) >0);

            return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveAuction(Auction auction)
        {
            conext.Auctions.Remove(auction);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await conext.SaveChangesAsync() > 0;
        }
    }
}