namespace Contracts
{
    public class AuctionUpdated
    {
        public required string Id { get; set; }
        public string Make { get; set; } = string.Empty;
        
        public string Model { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int Year { get; set; }

        public int Mileage { get; set; }
    }
}