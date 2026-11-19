using Enver.API.Features.Authentication.Core;
using Microsoft.AspNetCore.Identity;

namespace Enver.API.Security;

/// <summary>Una inicialización por contenedor de proceso/host, nunca por request. No representa una cuenta.</summary>
public sealed class DummyPasswordHash
{
    private readonly Lazy<User> user;

    public DummyPasswordHash(IPasswordHasher<User> hasher)
    {
        user = new(() =>
        {
            var identity = User.Create("", "", "", DateTime.UtcNow);
            // Secreto exclusivamente sintético y sin relación con usuarios; no se conserva como estado.
            var hash = hasher.HashPassword(identity, Guid.NewGuid().ToString("N"));
            return new User(identity.Id, "", "", hash, identity.Status, identity.CreatedAt, identity.UpdatedAt);
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal User User => user.Value;
    public bool IsInitialized => user.IsValueCreated;
    public void Initialize() => _ = user.Value;
    public override string ToString() => nameof(DummyPasswordHash);
}
