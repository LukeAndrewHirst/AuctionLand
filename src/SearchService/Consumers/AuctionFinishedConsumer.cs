using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            var auction = await DB.Default.Find<Item>().OneAsync(context.Message.AuctionId) ?? throw new InvalidOperationException("Auction was not found");
            
            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                if (context.Message.Amount.HasValue) auction.SoldAmount = (int)context.Message.Amount;
            }

            auction.Status = "Finished";

            await DB.Default.SaveAsync(auction);
        }
    }
}