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
        var result = ToonConverter.ToCompactToon(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToToon_WithString_ReturnsFormattedString()
    {
        var result = ToonConverter.ToCompactToon("hello");
        result.Should().Be("hello");
    }

    [Fact]
    public void ToToon_WithInteger_ReturnsFormattedInteger()
    {
        var result = ToonConverter.ToCompactToon(42);
        result.Should().Be("42");
    }

    [Fact]
    public void ToToon_WithBoolean_ReturnsLowercaseBoolean()
    {
        ToonConverter.ToCompactToon(true).Should().Be("true");
        ToonConverter.ToCompactToon(false).Should().Be("false");
    }

    [Fact]
    public void ToToon_WithDecimal_ReturnsFormattedDecimal()
    {
        var result = ToonConverter.ToCompactToon(99.99m);
        result.Should().Be("99.99");
    }

    #endregion

    #region ToToon Tests - Arrays

    [Fact]
    public void ToToon_WithEmptyArray_ReturnsEmptyArrayFormat()
    {
        var result = ToonConverter.ToCompactToon(Array.Empty<int>());
        result.Should().Be("~[]");
    }

    [Fact]
    public void ToToon_WithIntArray_ReturnsFormattedArray()
    {
        var result = ToonConverter.ToCompactToon(new[] { 1, 2, 3 });
        result.Should().Be("~[1,2,3]");
    }

    [Fact]
    public void ToToon_WithStringArray_ReturnsFormattedArray()
    {
        var result = ToonConverter.ToCompactToon(new[] { "a", "b", "c" });
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

        var result = ToonConverter.ToCompactToon(users);
        
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
        var result = ToonConverter.ToCompactToon(list);
        result.Should().Be("~[1,2,3]");
    }

    #endregion

    #region ToToon Tests - Objects

    [Fact]
    public void ToToon_WithSimpleObject_ReturnsFormattedObject()
    {
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        var result = ToonConverter.ToCompactToon(user);

        result.Should().StartWith("~");
        result.Should().Contain("Name|Alice");
        result.Should().Contain("Age|30");
        result.Should().Contain("City|NYC");
    }

    [Fact]
    public void ToToon_WithAnonymousType_ReturnsFormattedObject()
    {
        var obj = new { Name = "Test", Value = 123 };
        var result = ToonConverter.ToCompactToon(obj);

        result.Should().StartWith("~");
        result.Should().Contain("Name|Test");
        result.Should().Contain("Value|123");
    }

    [Fact]
    public void ToToon_WithEnumProperty_ReturnsEnumName()
    {
        var entity = new EntityWithEnum { Name = "Test", Status = Status.Active };
        var result = ToonConverter.ToCompactToon(entity);

        result.Should().Contain("Status|Active");
    }

    #endregion

    #region ToToon Tests - Options

    [Fact]
    public void ToToon_WithCustomDelimiter_UsesCustomDelimiter()
    {
        var options = new ToonOptions { Delimiter = ';' };
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        
        var result = ToonConverter.ToCompactToon(user, options);
        
        result.Should().Contain("Name;Alice");
    }

    [Fact]
    public void ToToon_WithCustomArrayDelimiter_UsesCustomArrayDelimiter()
    {
        var options = new ToonOptions { ArrayDelimiter = ';' };
        var array = new[] { 1, 2, 3 };
        
        var result = ToonConverter.ToCompactToon(array, options);
        
        result.Should().Be("~[1;2;3]");
    }

    [Fact]
    public void ToToon_WithCustomPrefix_UsesCustomPrefix()
    {
        var options = new ToonOptions { Prefix = '#' };
        var array = new[] { 1, 2, 3 };
        
        var result = ToonConverter.ToCompactToon(array, options);
        
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
        var result = ToonConverter.ToCompactToon(user);

        // Should escape the pipe and comma in values
        result.Should().Contain("O'Brien");
    }

    [Fact]
    public void ToToon_WithDateTime_FormatsCorrectly()
    {
        var date = new DateTime(2025, 1, 15, 10, 30, 0);
        var result = ToonConverter.ToCompactToon(date);

        result.Should().Contain("2025");
        result.Should().Contain("01");
        result.Should().Contain("15");
    }

    [Fact]
    public void ToToon_WithGuid_FormatsCorrectly()
    {
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = ToonConverter.ToCompactToon(guid);

        result.Should().Contain("12345678");
    }

    [Fact]
    public void ToToon_ExceedsMaxDepth_ThrowsException()
    {
        // MaxDepth = 0 means only the root object is allowed, no nested objects
        var options = new ToonOptions { MaxDepth = 0 };
        var nested = new NestedObject 
        { 
            Id = "1", 
            User = new User { Name = "Test", Age = 25, City = "NYC" } 
        };

        var action = () => ToonConverter.ToCompactToon(nested, options);
        
        action.Should().Throw<ToonSerializationException>();
    }

    [Fact]
    public void ToToon_WithNullableInt_HandlesNull()
    {
        int? value = null;
        var result = ToonConverter.ToCompactToon(value);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToToon_WithNullableInt_HandlesValue()
    {
        int? value = 42;
        var result = ToonConverter.ToCompactToon(value);
        result.Should().Be("42");
    }

    [Fact]
    public void ToToon_WithEmptyString_ReturnsEmptyString()
    {
        var result = ToonConverter.ToCompactToon("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToToon_WithWhitespaceString_ReturnsWhitespace()
    {
        var result = ToonConverter.ToCompactToon("   ");
        result.Should().Be("   ");
    }

    [Fact]
    public void ToToon_WithNegativeNumbers_FormatsCorrectly()
    {
        ToonConverter.ToCompactToon(-42).Should().Be("-42");
        ToonConverter.ToCompactToon(-99.99m).Should().Be("-99.99");
        ToonConverter.ToCompactToon(-3.14).Should().Contain("-3.14");
    }

    [Fact]
    public void ToToon_WithZero_FormatsCorrectly()
    {
        ToonConverter.ToCompactToon(0).Should().Be("0");
        ToonConverter.ToCompactToon(0.0).Should().Be("0");
        ToonConverter.ToCompactToon(0m).Should().Be("0");
    }

    [Fact]
    public void ToToon_WithLargeNumbers_FormatsCorrectly()
    {
        ToonConverter.ToCompactToon(long.MaxValue).Should().Be("9223372036854775807");
        ToonConverter.ToCompactToon(long.MinValue).Should().Be("-9223372036854775808");
    }

    [Fact]
    public void ToToon_WithFloat_FormatsCorrectly()
    {
        var result = ToonConverter.ToCompactToon(3.14159f);
        result.Should().Contain("3.14");
    }

    [Fact]
    public void ToToon_WithDateTimeOffset_FormatsCorrectly()
    {
        var dto = new DateTimeOffset(2025, 6, 15, 14, 30, 0, TimeSpan.FromHours(-5));
        var result = ToonConverter.ToCompactToon(dto);
        result.Should().Contain("2025");
        result.Should().Contain("06");
        result.Should().Contain("15");
    }

    #endregion

    #region Additional Array Edge Cases

    [Fact]
    public void ToToon_WithSingleElementArray_FormatsCorrectly()
    {
        var result = ToonConverter.ToCompactToon(new[] { 42 });
        result.Should().Be("~[42]");
    }

    [Fact]
    public void ToToon_WithMixedNullsInArray_HandlesCorrectly()
    {
        var users = new User?[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            null,
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        var result = ToonConverter.ToCompactToon(users);
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
    }

    [Fact]
    public void ToToon_WithEmptyStringInArray_FormatsCorrectly()
    {
        var result = ToonConverter.ToCompactToon(new[] { "a", "", "c" });
        result.Should().Be("~[a,,c]");
    }

    [Fact]
    public void ToToon_WithBoolArray_FormatsCorrectly()
    {
        var result = ToonConverter.ToCompactToon(new[] { true, false, true });
        result.Should().Be("~[true,false,true]");
    }

    [Fact]
    public void ToToon_WithDecimalArray_FormatsCorrectly()
    {
        var result = ToonConverter.ToCompactToon(new[] { 1.1m, 2.2m, 3.3m });
        result.Should().Be("~[1.1,2.2,3.3]");
    }

    #endregion

    #region Object Edge Cases

    [Fact]
    public void ToToon_WithAllNullProperties_ExcludesNulls()
    {
        var nested = new NestedObject { Id = "1", User = null };
        var options = new ToonOptions { IncludeNulls = false };
        
        var result = ToonConverter.ToCompactToon(nested, options);
        
        result.Should().Contain("Id|1");
        result.Should().NotContain("User|");
    }

    [Fact]
    public void ToToon_WithIncludeNullsTrue_IncludesNulls()
    {
        var nested = new NestedObject { Id = "1", User = null };
        var options = new ToonOptions { IncludeNulls = true };
        
        var result = ToonConverter.ToCompactToon(nested, options);
        
        result.Should().Contain("Id|1");
        result.Should().Contain("User|");
    }

    [Fact]
    public void ToToon_WithEmptyObject_ReturnsEmptyObjectFormat()
    {
        var obj = new EmptyClass();
        var result = ToonConverter.ToCompactToon(obj);
        result.Should().Be("~{}");
    }

    public class EmptyClass { }

    [Fact]
    public void ToToon_WithAllEnumValues_FormatsCorrectly()
    {
        ToonConverter.ToCompactToon(Status.Active).Should().Be("Active");
        ToonConverter.ToCompactToon(Status.Inactive).Should().Be("Inactive");
        ToonConverter.ToCompactToon(Status.Pending).Should().Be("Pending");
    }

    #endregion

    #region FromToon Edge Cases

    [Fact]
    public void FromToon_WithWhitespace_ReturnsNull()
    {
        var result = ToonConverter.FromToon<User>("   ");
        result.Should().BeNull();
    }

    [Fact]
    public void FromToon_WithNullString_ReturnsNull()
    {
        string? nullString = null;
        var result = ToonConverter.FromToon<User>(nullString!);
        result.Should().BeNull();
    }

    [Fact]
    public void FromToon_WithStringArray_ReturnsStringArray()
    {
        var result = ToonConverter.FromToon<string[]>("~[hello,world,test]");
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "hello", "world", "test" });
    }

    [Fact]
    public void FromToon_WithBoolArray_ReturnsBoolArray()
    {
        var result = ToonConverter.FromToon<bool[]>("~[true,false,true]");
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { true, false, true });
    }

    [Fact]
    public void FromToon_WithDecimalArray_ReturnsDecimalArray()
    {
        var result = ToonConverter.FromToon<decimal[]>("~[1.1,2.2,3.3]");
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { 1.1m, 2.2m, 3.3m });
    }

    [Fact]
    public void FromToon_WithList_ReturnsList()
    {
        var result = ToonConverter.FromToon<List<int>>("~[1,2,3]");
        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public void FromToon_CaseInsensitivePropertyMatch()
    {
        var toon = "~age|30,city|NYC,name|Alice";  // lowercase property names
        var result = ToonConverter.FromToon<User>(toon);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void FromToon_WithEnumValue_ParsesCorrectly()
    {
        var toon = "~Name|Test,Status|Pending";
        var result = ToonConverter.FromToon<EntityWithEnum>(toon);

        result.Should().NotBeNull();
        result!.Status.Should().Be(Status.Pending);
    }

    #endregion

    #region Token Reduction Edge Cases

    [Fact]
    public void GetTokenReduction_WithEmptyArray_ReturnsStats()
    {
        var stats = ToonConverter.GetTokenReduction(Array.Empty<int>());

        stats.Should().NotBeNull();
        stats.ToonOutput.Should().Be("~[]");
    }

    [Fact]
    public void GetTokenReduction_WithSingleObject_ReturnsStats()
    {
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        var stats = ToonConverter.GetTokenReduction(user);

        stats.Should().NotBeNull();
        stats.JsonTokens.Should().BeGreaterThan(0);
        stats.ToonTokens.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetTokenReduction_StatsToString_FormatsCorrectly()
    {
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        var stats = ToonConverter.GetTokenReduction(user);

        var str = stats.ToString();
        str.Should().Contain("JSON:");
        str.Should().Contain("TOON:");
        str.Should().Contain("Saved:");
    }

    #endregion

    #region Options Edge Cases

    [Fact]
    public void ToonOptions_DefaultValues_AreCorrect()
    {
        var options = ToonOptions.Default;

        options.Delimiter.Should().Be('|');
        options.ArrayDelimiter.Should().Be(',');
        options.Prefix.Should().Be('~');
        options.IncludeNulls.Should().BeFalse();
        options.MaxDepth.Should().Be(10);
        options.UseHeaderRow.Should().BeTrue();
        options.PropertyNameCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public void ToToon_WithUseHeaderRowFalse_OmitsHeader()
    {
        var options = new ToonOptions { UseHeaderRow = false };
        var users = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" }
        };

        var result = ToonConverter.ToCompactToon(users, options);
        
        // Without header row, should not have the [Header] format
        result.Should().StartWith("~");
    }

    [Fact]
    public void ToToon_WithCustomDateTimeFormat_UsesFormat()
    {
        var options = new ToonOptions { DateTimeFormat = "yyyy-MM-dd" };
        var date = new DateTime(2025, 6, 15, 14, 30, 45);

        var result = ToonConverter.ToCompactToon(date, options);
        
        result.Should().Be("2025-06-15");
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_WithSimpleArray_PreservesData()
    {
        var original = new[] { 1, 2, 3, 4, 5 };
        var toon = ToonConverter.ToCompactToon(original);
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

        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<User[]>(toon);

        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Alice");
        result[0].Age.Should().Be(30);
        result[1].Name.Should().Be("Bob");
        result[1].City.Should().Be("LA");
    }

    [Fact]
    public void RoundTrip_WithBoolArray_PreservesData()
    {
        var original = new[] { true, false, true, false };
        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<bool[]>(toon);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_WithStringArray_PreservesData()
    {
        var original = new[] { "hello", "world", "test" };
        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<string[]>(toon);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_WithDecimalArray_PreservesData()
    {
        var original = new[] { 1.5m, 2.75m, 3.125m };
        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<decimal[]>(toon);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_WithLargeArray_PreservesData()
    {
        var original = Enumerable.Range(1, 1000).ToArray();
        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<int[]>(toon);

        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void RoundTrip_WithProductArray_PreservesData()
    {
        var original = new[]
        {
            new Product { Sku = "SKU-001", Name = "Widget", Price = 9.99m, Quantity = 100 },
            new Product { Sku = "SKU-002", Name = "Gadget", Price = 19.99m, Quantity = 50 }
        };

        var toon = ToonConverter.ToCompactToon(original);
        var result = ToonConverter.FromToon<Product[]>(toon);

        result.Should().HaveCount(2);
        result![0].Sku.Should().Be("SKU-001");
        result[0].Price.Should().Be(9.99m);
        result[1].Name.Should().Be("Gadget");
        result[1].Quantity.Should().Be(50);
    }

    #endregion

    #region Exception Tests

    [Fact]
    public void ToonException_CanBeCreatedWithMessage()
    {
        var ex = new ToonException("Test message");
        ex.Message.Should().Be("Test message");
    }

    [Fact]
    public void ToonException_CanBeCreatedWithInnerException()
    {
        var inner = new InvalidOperationException("Inner");
        var ex = new ToonException("Outer", inner);
        
        ex.Message.Should().Be("Outer");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void ToonParseException_IncludesPosition()
    {
        var ex = new ToonParseException("Parse error", 42);
        
        ex.Message.Should().Be("Parse error");
        ex.Position.Should().Be(42);
    }

    [Fact]
    public void ToonSerializationException_IncludesTargetType()
    {
        var ex = new ToonSerializationException("Serialize error", typeof(User));
        
        ex.Message.Should().Be("Serialize error");
        ex.TargetType.Should().Be(typeof(User));
    }

    #endregion

    #region TryFromToon Tests

    [Fact]
    public void TryFromToon_WithValidInput_ReturnsTrue()
    {
        var toon = "~[1,2,3]";
        var success = ToonConverter.TryFromToon<int[]>(toon, out var result);

        success.Should().BeTrue();
        result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void TryFromToon_WithInvalidInput_ReturnsFalse()
    {
        var success = ToonConverter.TryFromToon<int[]>("invalid data", out var result);

        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromToon_WithEmptyString_ReturnsTrueWithNull()
    {
        var success = ToonConverter.TryFromToon<User>("", out var result);

        success.Should().BeTrue();
        result.Should().BeNull();
    }

    #endregion

    #region IsValidToon Tests

    [Fact]
    public void IsValidToon_WithValidArray_ReturnsTrue()
    {
        ToonConverter.IsValidToon("~[1,2,3]").Should().BeTrue();
    }

    [Fact]
    public void IsValidToon_WithValidObject_ReturnsTrue()
    {
        ToonConverter.IsValidToon("~Name|Alice,Age|30").Should().BeTrue();
    }

    [Fact]
    public void IsValidToon_WithInvalidPrefix_ReturnsFalse()
    {
        ToonConverter.IsValidToon("[1,2,3]").Should().BeFalse();
    }

    [Fact]
    public void IsValidToon_WithEmptyString_ReturnsFalse()
    {
        ToonConverter.IsValidToon("").Should().BeFalse();
        ToonConverter.IsValidToon("   ").Should().BeFalse();
    }

    [Fact]
    public void IsValidToon_WithCustomPrefix_ValidatesCorrectly()
    {
        var options = new ToonOptions { Prefix = '#' };
        ToonConverter.IsValidToon("#[1,2,3]", options).Should().BeTrue();
        ToonConverter.IsValidToon("~[1,2,3]", options).Should().BeFalse();
    }

    #endregion

    #region Async Tests

    [Fact]
    public async Task ToCompactToonAsync_WritesToStream()
    {
        var data = new[] { 1, 2, 3 };
        using var stream = new MemoryStream();

        await ToonConverter.ToCompactToonAsync(stream, data);

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();

        result.Should().Be("~[1,2,3]");
    }

    [Fact]
    public async Task FromToonAsync_ReadsFromStream()
    {
        var toon = "~[1,2,3]";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(toon));

        var result = await ToonConverter.FromToonAsync<int[]>(stream);

        result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task RoundTrip_Async_PreservesData()
    {
        var original = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        using var stream = new MemoryStream();
        await ToonConverter.ToCompactToonAsync(stream, original);

        stream.Position = 0;
        var result = await ToonConverter.FromToonAsync<User[]>(stream);

        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
    }

    #endregion

    #region TextReader/TextWriter Tests

    [Fact]
    public void ToToon_WithTextWriter_WritesCorrectly()
    {
        var data = new[] { 1, 2, 3 };
        using var writer = new StringWriter();

        ToonConverter.ToCompactToon(writer, data);

        writer.ToString().Should().Be("~[1,2,3]");
    }

    [Fact]
    public void FromToon_WithTextReader_ReadsCorrectly()
    {
        var toon = "~[1,2,3]";
        using var reader = new StringReader(toon);

        var result = ToonConverter.FromToon<int[]>(reader);

        result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    #endregion

    #region ToonOptions Static Factories

    [Fact]
    public void ToonOptions_MinimalTokens_HasCorrectDefaults()
    {
        var options = ToonOptions.MinimalTokens;

        options.Delimiter.Should().Be('|');
        options.ArrayDelimiter.Should().Be(',');
        options.Prefix.Should().Be('~');
        options.IncludeNulls.Should().BeFalse();
        options.UseHeaderRow.Should().BeTrue();
    }

    [Fact]
    public void ToonOptions_StrictMode_HasCorrectDefaults()
    {
        var options = ToonOptions.StrictMode;

        options.Strict.Should().BeTrue();
        options.PropertyNameCaseInsensitive.Should().BeFalse();
    }

    [Fact]
    public void ToonOptions_NewProperties_HaveCorrectDefaults()
    {
        var options = new ToonOptions();

        options.Strict.Should().BeFalse();
        options.EscapeCharacter.Should().Be('\\');
        options.IncludeTypeInfo.Should().BeFalse();
        options.MaxOutputLength.Should().Be(0);
    }

    #endregion

    #region Standard TOON Format Tests

    [Fact]
    public void ToToon_WithArray_ReturnsStandardFormat()
    {
        var users = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        var result = ToonConverter.ToToon(users);

        // Standard format uses indentation and tabular headers
        result.Should().Contain("{");  // Field list in braces
        result.Should().Contain("}:");
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
    }

    [Fact]
    public void ToToon_WithObject_ReturnsStandardFormat()
    {
        var user = new User { Name = "Alice", Age = 30, City = "NYC" };
        var result = ToonConverter.ToToon(user);

        // Standard format uses key: value with proper spacing
        result.Should().Contain("Name: Alice");
        result.Should().Contain("Age: 30");
        result.Should().Contain("City: NYC");
    }

    [Fact]
    public void ToToon_WithPrimitiveArray_ReturnsInlineFormat()
    {
        var result = ToonConverter.ToToon(new[] { 1, 2, 3 });

        result.Should().Contain("[3]:");
        result.Should().Contain("1,2,3");
    }

    [Fact]
    public void Serialize_WithStandardFormat_ReturnsStandardToon()
    {
        var data = new[] { 1, 2, 3 };
        var result = ToonConverter.Serialize(data, ToonFormat.Standard);

        result.Should().Contain("[3]:");
    }

    [Fact]
    public void Serialize_WithCompactFormat_ReturnsCompactToon()
    {
        var data = new[] { 1, 2, 3 };
        var result = ToonConverter.Serialize(data, ToonFormat.Compact);

        result.Should().StartWith("~");
    }

    #endregion

    #region Format Comparison Tests

    [Fact]
    public void CompareFormats_ReturnsAllThreeFormats()
    {
        var users = new[]
        {
            new User { Name = "Alice", Age = 30, City = "NYC" },
            new User { Name = "Bob", Age = 25, City = "LA" }
        };

        var stats = ToonConverter.CompareFormats(users);

        stats.JsonTokens.Should().BeGreaterThan(0);
        stats.StandardToonTokens.Should().BeGreaterThan(0);
        stats.CompactToonTokens.Should().BeGreaterThan(0);

        // Compact should be most efficient
        stats.CompactToonTokens.Should().BeLessThanOrEqualTo(stats.StandardToonTokens);
        stats.StandardToonTokens.Should().BeLessThan(stats.JsonTokens);
    }

    [Fact]
    public void CompareFormats_WithNull_ReturnsZeroStats()
    {
        var stats = ToonConverter.CompareFormats(null);

        stats.JsonTokens.Should().Be(0);
        stats.StandardToonTokens.Should().Be(0);
        stats.CompactToonTokens.Should().Be(0);
    }

    [Fact]
    public void CompareFormats_ShowsBothFormatsSaveTokens()
    {
        var products = Enumerable.Range(1, 50).Select(i => new Product
        {
            Sku = $"SKU-{i:D5}",
            Name = $"Product {i}",
            Price = 9.99m + i,
            Quantity = i * 10
        }).ToArray();

        var stats = ToonConverter.CompareFormats(products);

        // Both formats should save significant tokens vs JSON
        stats.StandardToonSaved.Should().BeGreaterThan(0);
        stats.CompactToonSaved.Should().BeGreaterThan(0);
        stats.StandardToonReductionPercent.Should().BeGreaterThan(30);
        stats.CompactToonReductionPercent.Should().BeGreaterThan(30);
        
        // Both outputs should be non-empty and different from JSON
        stats.StandardToonOutput.Should().NotBeEmpty();
        stats.CompactToonOutput.Should().NotBeEmpty();
        stats.StandardToonOutput.Should().NotBe(stats.JsonOutput);
        stats.CompactToonOutput.Should().NotBe(stats.JsonOutput);
    }

    [Fact]
    public void CompareFormats_ToString_FormatsCorrectly()
    {
        var data = new[] { new User { Name = "Test", Age = 30, City = "NYC" } };
        var stats = ToonConverter.CompareFormats(data);

        var str = stats.ToString();
        str.Should().Contain("JSON:");
        str.Should().Contain("Standard TOON:");
        str.Should().Contain("Compact TOON:");
        str.Should().Contain("saved");
    }

    [Fact]
    public void GetTokenReduction_WithFormat_UsesCorrectFormat()
    {
        var data = new[] { 1, 2, 3 };
        
        var standardStats = ToonConverter.GetTokenReduction(data, ToonFormat.Standard);
        var compactStats = ToonConverter.GetTokenReduction(data, ToonFormat.Compact);

        // Outputs should be different
        standardStats.ToonOutput.Should().NotBe(compactStats.ToonOutput);
        
        // Compact should have prefix
        compactStats.ToonOutput.Should().StartWith("~");
    }

    #endregion

    #region ToonFormat Enum Tests

    [Fact]
    public void ToonFormat_HasExpectedValues()
    {
        Enum.GetValues<ToonFormat>().Should().HaveCount(2);
        Enum.IsDefined(ToonFormat.Standard).Should().BeTrue();
        Enum.IsDefined(ToonFormat.Compact).Should().BeTrue();
    }

    #endregion

    #region Performance Sanity Tests

    [Fact]
    public void ToToon_With1000Items_CompletesQuickly()
    {
        var users = Enumerable.Range(1, 1000).Select(i => new User
        {
            Name = $"User{i}",
            Age = 20 + (i % 50),
            City = $"City{i % 100}"
        }).ToArray();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = ToonConverter.ToCompactToon(users);
        stopwatch.Stop();

        result.Should().NotBeEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in under 1 second
    }

    [Fact]
    public void GetTokenReduction_With1000Items_ShowsSignificantSavings()
    {
        var products = Enumerable.Range(1, 1000).Select(i => new Product
        {
            Sku = $"SKU-{i:D6}",
            Name = $"Product Number {i}",
            Price = 9.99m + (i * 0.01m),
            Quantity = i * 10
        }).ToArray();

        var stats = ToonConverter.GetTokenReduction(products);

        // With 1000 items, should see at least 40% reduction
        stats.ReductionPercent.Should().BeGreaterThan(40);
        stats.TokensSaved.Should().BeGreaterThan(1000);
    }

    #endregion
}
