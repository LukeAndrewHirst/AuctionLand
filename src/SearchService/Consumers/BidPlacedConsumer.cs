using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("Consuming placed bid.");

            var auction = await DB.Default.Find<Item>().OneAsync(context.Message.AuctionId);

            if(context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction?.CurrentHighBid)
            {
                auction.CurrentHighBid = context.Message.Amount;

                await DB.Default.SaveAsync(auction);
            }
        }
    }
}