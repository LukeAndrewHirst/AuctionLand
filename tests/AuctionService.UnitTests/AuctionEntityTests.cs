using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        var auction = new Auction{Id = Guid.NewGuid(), Seller = "Test Seller", ReservePrice = 10};

        var result = auction.HasReservePrice();

        Assert.True(result);
    }
}
