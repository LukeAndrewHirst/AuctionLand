using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Entities;
using MongoDB.Entities;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer(IMapper mapper) : IConsumer<AuctionCreated>
    {
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("Consuming AuctionCreated:" + context.Message.Id);

            var item = mapper.Map<Item>(context.Message);

            if(item.Model == "Foo") throw new ArgumentException("No cars with the name of foo can be auctioned");

            await DB.Default.SaveAsync(item);
        }
    }
}