using System.Text.Json;
using Enver.API.Features.Ratings;

namespace Enver.API.UnitTests;

public sealed class RatingInputTests
{
    [Theory]
    [InlineData("{\"score\":1}", 1, null)]
    [InlineData("{\"score\":5,\"comment\":null}", 5, null)]
    [InlineData("{\"score\":5,\"comment\":\"  \"}", 5, null)]
    [InlineData("{\"score\":5,\"comment\":\" Entrega rápida \"}", 5, "Entrega rápida")]
    public void Normalizes_contract(string json, int score, string? comment)
    {
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(new RatingInput(score, comment), RatingInput.Parse(doc.RootElement));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"score\":0}")]
    [InlineData("{\"score\":6}")]
    [InlineData("{\"score\":5.0}")]
    [InlineData("{\"score\":5e0}")]
    [InlineData("{\"score\":\"5\"}")]
    [InlineData("{\"score\":5,\"comment\":1}")]
    [InlineData("{\"score\":5,\"raterId\":null}")]
    [InlineData("{\"score\":5,\"authorId\":null}")]
    [InlineData("{\"score\":5,\"recipientId\":null}")]
    [InlineData("{\"score\":5,\"targetUserId\":null}")]
    [InlineData("{\"score\":5,\"ownerId\":null}")]
    [InlineData("{\"score\":5,\"rescuerId\":null}")]
    public void Rejects_invalid_contract(string json)
    {
        using var doc = JsonDocument.Parse(json);
        Assert.Null(RatingInput.Parse(doc.RootElement));
    }

    [Theory]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void Limits_trimmed_comment(int length, bool valid)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(new { score = 3, comment = $" {new string('a', length)} " }));
        Assert.Equal(valid, RatingInput.Parse(doc.RootElement) is not null);
    }
}
