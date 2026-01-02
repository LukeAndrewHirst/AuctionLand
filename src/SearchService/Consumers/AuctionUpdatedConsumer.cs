using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
    {
        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("Consuming updated auction:" + context.Message.Id);

            var item = mapper.Map<Item>(context.Message);

            var result = await DB.Default.Update<Item>().Match(a => a.ID == context.Message.Id)
                .ModifyOnly(m => new
                {
                   m.Color,
                   m.Make,
                   m.Model,
                   m.Year,
                   m.Mileage 
                }, item).ExecuteAsync();
            if(!result.IsAcknowledged) throw new MessageException(typeof(AuctionUpdated), "Failed to update auction");
        }
    }
}