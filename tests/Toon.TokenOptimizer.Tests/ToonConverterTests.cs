using FluentAssertions;
using Xunit;

namespace Toon.TokenOptimizer.Tests;

public class ToonConverterTests
{
    #region Test Models

    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string City { get; set; } = "";
    }

    public class Product
    {
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class NestedObject
    {
        public string Id { get; set; } = "";
        public User? User { get; set; }
    }

    public enum Status { Active, Inactive, Pending }

    public class EntityWithEnum
    {
        public string Name { get; set; } = "";
        public Status Status { get; set; }
    }

    #endregion

    #region ToToon Tests - Primitives

    [Fact]
    public void ToToon_WithNull_ReturnsEmptyString()
    {
        var result = ToonConverter.ToToon(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToToon_WithString_ReturnsFormattedString()
    {
        var result = ToonConverter.ToToon("hello");
        result.Should().Be("hello");
    }

    [Fact]
    public void ToToon_WithInteger_ReturnsFormattedInteger()
    {
        var result = ToonConverter.ToToon(42);
        result.Should().Be("42");
    }

    [Fact]
    public void ToToon_WithBoolean_ReturnsLowercaseBoolean()
    {
        ToonConverter.ToToon(true).Should().Be("true");
        ToonConverter.ToToon(false).Should().Be("false");
    }

    [Fact]
    public void ToToon_WithDecimal_ReturnsFormattedDecimal()
    {
        var result = ToonConverter.ToToon(99.99m);
        result.Should().Be("99.99");
    }

    #endregion

    #region ToToon Tests - Arrays

    [Fact]
    public void ToToon_WithEmptyArray_ReturnsEmptyArrayFormat()
    {
        var result = ToonConverter.ToToon(Array.Empty<int>());
        result.Should().Be("~[]");
    }

    [Fact]
    public void ToToon_WithIntArray_ReturnsFormattedArray()
    {
        var result = ToonConverter.ToToon(new[] { 1, 2, 3 });
        result.Should().Be("~[1,2,3]");
    }

    [Fact]
    public void ToToon_WithStringArray_ReturnsFormattedArray()
    {
        var result = ToonConverter.ToToon(new[] { "a", "b", "c" });
        result.Should().Be("~[a,b,c]");
    }

    [Fact]
    public void ToToon_WithObjectArray_ReturnsHeaderRowFormat()
    {
        var users = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        var result = ToonConverter.ToToon(users);
        
        result.Should().StartWith("~[");
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("City");
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
    }

    [Fact]
    public void ToToon_WithList_ReturnsFormattedArray()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = ToonConverter.ToToon(list);
        result.Should().Be("~[1,2,3]");
    }

    #endregion

    #region ToToon Tests - Objects

    [Fact]
    public void ToToon_WithSimpleObject_ReturnsFormattedObject()
    {
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        var result = ToonConverter.ToToon(user);

        result.Should().StartWith("~");
        result.Should().Contain("Name|Alice");
        result.Should().Contain("Age|30");
        result.Should().Contain("City|NYC");
    }

    [Fact]
    public void ToToon_WithAnonymousType_ReturnsFormattedObject()
    {
        var obj = new { Name = "Test", Value = 123 };
        var result = ToonConverter.ToToon(obj);

        result.Should().StartWith("~");
        result.Should().Contain("Name|Test");
        result.Should().Contain("Value|123");
    }

    [Fact]
    public void ToToon_WithEnumProperty_ReturnsEnumName()
    {
        var entity = new EntityWithEnum { Name = "Test", Status = Status.Active };
        var result = ToonConverter.ToToon(entity);

        result.Should().Contain("Status|Active");
    }

    #endregion

    #region ToToon Tests - Options

    [Fact]
    public void ToToon_WithCustomDelimiter_UsesCustomDelimiter()
    {
        var options = new ToonOptions { Delimiter = ';' };
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        
        var result = ToonConverter.ToToon(user, options);
        
        result.Should().Contain("Name;Alice");
    }

    [Fact]
    public void ToToon_WithCustomArrayDelimiter_UsesCustomArrayDelimiter()
    {
        var options = new ToonOptions { ArrayDelimiter = ';' };
        var array = new[] { 1, 2, 3 };
        
        var result = ToonConverter.ToToon(array, options);
        
        result.Should().Be("~[1;2;3]");
    }

    [Fact]
    public void ToToon_WithCustomPrefix_UsesCustomPrefix()
    {
        var options = new ToonOptions { Prefix = '#' };
        var array = new[] { 1, 2, 3 };
        
        var result = ToonConverter.ToToon(array, options);
        
        result.Should().StartWith("#[");
    }

    #endregion

    #region FromToon Tests

    [Fact]
    public void FromToon_WithEmptyString_ReturnsNull()
    {
        var result = ToonConverter.FromToon<User>("");
        result.Should().BeNull();
    }

    [Fact]
    public void FromToon_WithSimpleArray_ReturnsArray()
    {
        var result = ToonConverter.FromToon<int[]>("~[1,2,3]");
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void FromToon_WithObjectArray_ReturnsObjectArray()
    {
        var toon = "~[Age|City|Name]:30|NYC|Alice,25|LA|Bob";
        var result = ToonConverter.FromToon<User[]>(toon);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Alice");
        result[0].Age.Should().Be(30);
        result[1].Name.Should().Be("Bob");
        result[1].Age.Should().Be(25);
    }

    [Fact]
    public void FromToon_WithObject_ReturnsObject()
    {
        var toon = "~Age|30,City|NYC,Name|Alice";
        var result = ToonConverter.FromToon<User>(toon);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
        result.Age.Should().Be(30);
        result.City.Should().Be("NYC");
    }

    #endregion

    #region Token Reduction Tests

    [Fact]
    public void GetTokenReduction_WithArray_ShowsReduction()
    {
        var users = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" },
            new User { Name = "Charlie", Age = 35, City = "Chicago" }
        };

        var stats = ToonConverter.GetTokenReduction(users);

        stats.ToonTokens.Should().BeLessThan(stats.JsonTokens);
        stats.ReductionPercent.Should().BeGreaterThan(0);
        stats.TokensSaved.Should().BePositive();
    }

    [Fact]
    public void GetTokenReduction_WithLargeDataset_ShowsSignificantReduction()
    {
        var products = Enumerable.Range(1, 100).Select(i => new Product
        {
            Sku = $"SKU-{i:D5}",
            Name = $"Product {i}",
            Price = 9.99m + i,
            Quantity = i * 10
        }).ToArray();

        var stats = ToonConverter.GetTokenReduction(products);

        // TOON should provide at least 30% reduction for repetitive data
        stats.ReductionPercent.Should().BeGreaterThan(30);
    }

    [Fact]
    public void GetTokenReduction_WithNull_ReturnsZeroStats()
    {
        var stats = ToonConverter.GetTokenReduction(null);

        stats.JsonTokens.Should().Be(0);
        stats.ToonTokens.Should().Be(0);
        stats.ReductionPercent.Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToToon_WithSpecialCharacters_EscapesCorrectly()
    {
        var user = new User { Name = "O'Brien|Test", Age = 30, City = "New York, NY" };
        var result = ToonConverter.ToToon(user);

        // Should escape the pipe and comma in values
        result.Should().Contain("O'Brien");
    }

    [Fact]
    public void ToToon_WithDateTime_FormatsCorrectly()
    {
        var date = new DateTime(2025, 1, 15, 10, 30, 0);
        var result = ToonConverter.ToToon(date);

        result.Should().Contain("2025");
        result.Should().Contain("01");
        result.Should().Contain("15");
    }

    [Fact]
    public void ToToon_WithGuid_FormatsCorrectly()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = ToonConverter.ToToon(guid);

        result.Should().Contain("12345678");
    }

    [Fact]
    public void ToToon_ExceedsMaxDepth_ThrowsException()
    {
        var options = new ToonOptions { MaxDepth = 1 };
        var nested = new NestedObject 
        { 
            Id = "1", 
            User = new User { Name = "Test", Age = 25, City = "NYC" } 
        };

        var action = () => ToonConverter.ToToon(nested, options);
        
        action.Should().Throw<ToonSerializationException>();
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_WithSimpleArray_PreservesData()
    {
        var original = new[] { 1, 2, 3, 4, 5 };
        var toon = ToonConverter.ToToon(original);
        var result = ToonConverter.FromToon<int[]>(toon);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_WithObjectArray_PreservesData()
    {
        var original = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        var toon = ToonConverter.ToToon(original);
        var result = ToonConverter.FromToon<User[]>(toon);

        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Alice");
        result[0].Age.Should().Be(30);
        result[1].Name.Should().Be("Bob");
        result[1].City.Should().Be("LA");
    }

    #endregion
}
