using System.Text;
using System.Text.Json;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using Enver.API.Http;

namespace Enver.API.Features.Ratings;

public sealed record RatingInput(int Score, string? Comment)
{
    public static RatingInput? Parse(JsonElement json)
    {
        if (json.ValueKind != JsonValueKind.Object) return null;
        int? score = null;
        string? comment = null;
        foreach (var field in json.EnumerateObject())
        {
            switch (field.Name)
            {
                case "score" when field.Value.ValueKind == JsonValueKind.Number &&
                    field.Value.TryGetInt32(out var value) && value is >= 1 and <= 5:
                    score = value;
                    break;
                case "comment" when field.Value.ValueKind is JsonValueKind.String or JsonValueKind.Null:
                    var trimmed = field.Value.GetString()?.Trim();
                    if (trimmed is { Length: > 0 })
                    {
                        if (trimmed.EnumerateRunes().Count() > 500) return null;
                        comment = trimmed;
                    }
                    break;
                default: return null;
            }
        }
        return score is { } valid ? new(valid, comment) : null;
    }
}

public sealed record Rating(Guid Id, Guid OrderId, Guid AuthorId, Guid RecipientId, int Score, string? Comment, DateTime CreatedAt);
public enum RatingInsert { Confirmed, Duplicate, Unavailable }

public interface IRatingStore
{
    Task<RatingInsert> InsertAsync(Rating rating, RequestBudget budget);
    Task<Rating?> FindByRelationAsync(Guid order, Guid author, Guid recipient, RequestBudget budget);
}

public enum RatingOutcome { Created, Replay, Forbidden, NotFound, StateConflict, AlreadyExists, Unavailable }

public sealed class RatingService(OrderService orders, IRatingStore ratings, TimeProvider clock)
{
    public async Task<(RatingOutcome Outcome, Rating? Rating)> SubmitAsync(Guid orderId, Guid author, RatingInput input, RequestBudget budget)
    {
        var (accountOutcome, account) = await orders.AuthenticateAccountAsync(author, budget);
        if (account is null)
            return (accountOutcome == OrderOutcome.Forbidden ? RatingOutcome.Forbidden : RatingOutcome.Unavailable, null);

        var read = await orders.DetailAsync(orderId, author, budget);
        if (budget.Cause != BudgetCancellationCause.None) return (RatingOutcome.Unavailable, null);
        if (read.Outcome == OrderOutcome.NotFound) return (RatingOutcome.NotFound, null);
        if (read.Order is not { } order) return (RatingOutcome.Unavailable, null);
        if (order.OwnerId != author) return (RatingOutcome.Forbidden, null);
        if (order.Status != OrderStatus.Completed || order.RescuerId is not { } recipient || recipient == Guid.Empty || recipient == author)
            return (RatingOutcome.StateConflict, null);

        // Order completion and its assignment are terminal; no cross-collection write is needed.
        var now = DateTimeOffset.FromUnixTimeMilliseconds(clock.GetUtcNow().ToUnixTimeMilliseconds()).UtcDateTime;
        var rating = new Rating(Guid.NewGuid(), orderId, author, recipient, input.Score, input.Comment, now);
        var inserted = await ratings.InsertAsync(rating, budget);
        if (budget.Cause != BudgetCancellationCause.None) return (RatingOutcome.Unavailable, null);
        if (inserted == RatingInsert.Confirmed) return (RatingOutcome.Created, rating);
        // An uncertain write must not claim success. An identical client retry can recover the winner.
        if (inserted != RatingInsert.Duplicate) return (RatingOutcome.Unavailable, null);

        var previous = await ratings.FindByRelationAsync(orderId, author, recipient, budget);
        if (previous is null || budget.Cause != BudgetCancellationCause.None) return (RatingOutcome.Unavailable, null);
        return previous.Score == input.Score && previous.Comment == input.Comment
            ? (RatingOutcome.Replay, previous) : (RatingOutcome.AlreadyExists, null);
    }
}
