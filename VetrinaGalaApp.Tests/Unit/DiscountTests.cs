﻿using VetrinaGalaApp.ApiService.Domain.Discounts;
using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.Tests.Unit;

public class DiscountTests
{
    [Fact]
    public void SingleDiscount_AppliesCorrectPercentage()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var discount = new Discount(0.1m); // 10% discount

        // Act
        var result = discount.GetAppliedDiscounts(price).Single();
        var priceAfterDiscount = price - result.DiscountedAmount;

        // Assert
        Assert.Equal(10m, result.DiscountedAmount.Amount);
        Assert.Equal(new Price(90, Currency.Dollars), priceAfterDiscount);
        Assert.Equal(0.1m, result.DiscountPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Discount_ThrowsOnInvalidPercentage(decimal invalidPercentage)
    {
        // Assert
        Assert.Throws<InvalidOperationException>(() => new Discount(invalidPercentage));
    }

    [Fact]
    public void ChainedDiscounts_AppliesSequentially()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var discount1 = new Discount(0.1m); // 10% discount
        var discount2 = new Discount(0.2m); // 20% discount
        var multipleDiscounts = new ChainedDiscounts(discount1, discount2);

        // Act
        var results = multipleDiscounts.GetAppliedDiscounts(price).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(10m, results[0].DiscountedAmount.Amount);
        Assert.Equal(18m, results[1].DiscountedAmount.Amount); // 20% of 90
    }

    [Fact]
    public void PriceLimitedDiscounts_RespectsMinimumPrice()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var discount1 = new Discount(0.1m); // 10% discount
        var discount2 = new Discount(0.2m); // 20% discount
        var discount3 = new Discount(0.6m); // 60% discount
        var multipleDiscounts = new ChainedDiscounts(discount1, discount2);
        var applayableDiscounts = new PriceLimitedDiscounts(multipleDiscounts, 0.25m); // Minimum price 75

        // Act
        var results = applayableDiscounts.GetAppliedDiscounts(price).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        var totalDiscount = results.Sum(r => r.DiscountedAmount.Amount);

        Assert.True(price with { Amount = price.Amount - totalDiscount } == price * 0.75m);
    }

    [Fact]
    public void PriceLimitedDiscounts_StopsAtMinimumPrice()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var discount1 = new Discount(0.3m); // 30% discount
        var discount2 = new Discount(0.2m); // 20% discount
        var multipleDiscounts = new ChainedDiscounts(discount1, discount2);
        var applayableDiscounts = new PriceLimitedDiscounts(multipleDiscounts, 0.25m); // Minimum price 75

        // Act
        var results = applayableDiscounts.GetAppliedDiscounts(price).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(25m, results[0].DiscountedAmount.Amount);
    }

    [Fact]
    public void PriceLimitedDiscounts_HandlesZeroDiscounts()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var applayableDiscounts = new PriceLimitedDiscounts(new ChainedDiscounts(), 0.25m);

        // Act
        var results = applayableDiscounts.GetAppliedDiscounts(price).ToList();

        // Assert
        Assert.Empty(results);
    }    

    [Fact]
    public void PriceLimitedDiscounts_AllowsFullDiscount()
    {
        // Arrange
        var price = new Price(100, Currency.Dollars);
        var discount = new Discount(0.2m); // 20% discount
        var applayableDiscounts = new PriceLimitedDiscounts(discount, 0.3m); // Allows up to 30%

        // Act
        var results = applayableDiscounts.GetAppliedDiscounts(price).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(20m, results[0].DiscountedAmount.Amount);
    }
}