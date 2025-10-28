using System.Reflection;
using MediaOrganizer.Infrastructure.ApiClients;

namespace MediaOrganizer.Tests.Configuration;

public class TmdbApiConfigTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("INVALID_KEY!", false)]
    [InlineData("abc123", true)]
    public void ApiKey_Setter_ValidatesInput(string? apiKey, bool expectedIsValid)
    {
        // Arrange
        var config = new TmdbApiConfig();

        // Act & Assert
        if (expectedIsValid)
        {
            config.ApiKey = apiKey!;
            Assert.Equal(apiKey, config.ApiKey);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => config.ApiKey = apiKey!);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("INVALID_KEY!", false)]
    [InlineData("abc123", true)]
    public void ApiKey_Getter_ValidatesStoredValue(string? apiKey, bool expectedIsValid)
    {
        // Arrange
        var config = new TmdbApiConfig();
        var field = typeof(TmdbApiConfig).GetField("_apiKey", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(config, apiKey);

        // Act & Assert
        if (expectedIsValid)
        {
            var retrieved = config.ApiKey;
            Assert.Equal(apiKey, retrieved);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => _ = config.ApiKey);
        }
    }
}