using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities
{
    [Table("Auctions")]
    public class Auction
    {
        public Guid Id { get; set; }

        public required string Seller { get; set; }

        public string? Winner { get; set; }

        [Column("Reserve_Price")]
        public int ReservePrice { get; set; }

        [Column("Sold_Amount")]
        public int? SoldAmount { get; set; }

        [Column("Current_High_Bid")]
        public int? CurrentHighBid { get; set; }

        public Status Status { get; set; } = Status.Live;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("Auction_End")]
        public DateTime AuctionEnd { get; set; }

        public Item Item { get; set; } = null!;
    }
}