using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities
{
    [Table("Items")]
    public class Item
    {
        public Guid Id { get; set; }

        [Column("Auction_Id")]
        public Guid AuctionId { get; set; }

        public Auction Auction { get; set; } = null!;
        
        public required string Make { get; set; }

        public required string Model { get; set; }

        public required string Color { get; set; }

        public required int Year { get; set; }

        public required int Mileage { get; set; }

        [Column("Image_Url")]
        public required string ImageUrl { get; set; }
    }
}