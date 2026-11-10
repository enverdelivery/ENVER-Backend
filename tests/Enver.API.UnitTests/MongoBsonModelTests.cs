using Enver.API.Features.Authentication.Core;
using Enver.API.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Enver.API.UnitTests;

public sealed class MongoBsonModelTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly string[] Fields = ["_id", "fullName", "normalizedEmail", "passwordHash", "status", "createdAt", "updatedAt"];

    [Fact]
    public void Document_has_exactly_seven_explicit_names_and_no_auxiliary_fields()
    {
        var bson = Document().ToBsonDocument();
        Assert.Equal(7, bson.ElementCount);
        Assert.Equal(Fields.Order(), bson.Names.Order());
        Assert.All(new[] { "fullName", "normalizedEmail", "passwordHash", "status" }, field =>
            Assert.Equal(BsonType.String, bson[field].BsonType));
        Assert.Equal(BsonType.DateTime, bson["createdAt"].BsonType);
        Assert.Equal(BsonType.DateTime, bson["updatedAt"].BsonType);
        Assert.Equal(new[] { "CreatedAt", "FullName", "Id", "NormalizedEmail", "PasswordHash", "Status", "UpdatedAt" },
            typeof(User).GetProperties().Select(p => p.Name).Order());
    }

    [Fact]
    public void Backend_creation_uses_uuid_v4_standard_subtype_4_and_active_equal_timestamps()
    {
        var user = User.Create("Synthetic", "synthetic@example.invalid", "synthetic-not-a-real-hash", Now.AddTicks(12345));
        var bson = UserDocument.FromUser(user).ToBsonDocument();
        var binary = bson["_id"].AsBsonBinaryData;
        Assert.Equal(4, user.Id.Version);
        Assert.Equal(0x80, user.Id.ToByteArray()[8] & 0xc0);
        Assert.Equal(BsonBinarySubType.UuidStandard, binary.SubType);
        Assert.Equal(user.Id, binary.ToGuid(GuidRepresentation.Standard));
        Assert.Equal(user.Id.ToByteArray(bigEndian: true), binary.Bytes);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(user.CreatedAt, user.UpdatedAt);
        Assert.Equal(0, user.CreatedAt.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(bson["createdAt"], bson["updatedAt"]);
    }

    [Theory]
    [InlineData(UserStatus.Active, "Active")]
    [InlineData(UserStatus.Disabled, "Disabled")]
    public void Only_approved_states_are_serialized_as_exact_strings(UserStatus status, string expected)
    {
        var bson = Document(status).ToBsonDocument();
        Assert.Equal(BsonType.String, bson["status"].BsonType);
        Assert.Equal(expected, bson["status"].AsString);
        Assert.Equal(status, BsonSerializer.Deserialize<UserDocument>(bson).Status);
        Assert.Equal(new[] { UserStatus.Active, UserStatus.Disabled }, Enum.GetValues<UserStatus>());
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("active")]
    [InlineData("0")]
    [InlineData("")]
    [InlineData("Active, Disabled")]
    public void Unknown_or_ordinal_string_status_fails_instead_of_defaulting(string value)
    {
        var bson = Document().ToBsonDocument();
        bson["status"] = value;
        Assert.Throws<FormatException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    public void Numeric_status_is_rejected_even_for_known_enum_ordinals(int value)
    {
        var bson = Document().ToBsonDocument();
        bson["status"] = value;
        Assert.Throws<FormatException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Fact]
    public void Null_status_is_not_active()
    {
        var bson = Document().ToBsonDocument();
        bson["status"] = BsonNull.Value;
        Assert.Throws<FormatException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Theory]
    [InlineData("_id")]
    [InlineData("fullName")]
    [InlineData("normalizedEmail")]
    [InlineData("passwordHash")]
    [InlineData("status")]
    [InlineData("createdAt")]
    [InlineData("updatedAt")]
    public void Every_field_is_required(string field)
    {
        var bson = Document().ToBsonDocument();
        bson.Remove(field);
        Assert.Throws<MongoDB.Bson.BsonSerializationException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Fact]
    public void Extra_fields_are_not_silently_accepted()
    {
        var bson = Document().ToBsonDocument();
        bson.Add("unexpected", true);
        Assert.Throws<FormatException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Fact]
    public void Binary_round_trip_preserves_every_field_and_millisecond_utc_precision()
    {
        var original = Document(UserStatus.Disabled, Now.AddTicks(1234567), Now.AddSeconds(1).AddTicks(7654321));
        var restored = BsonSerializer.Deserialize<UserDocument>(original.ToBson());
        Assert.Equal(original.ToBsonDocument(), restored.ToBsonDocument());
        Assert.Equal(original.Id, restored.ToUser().Id);
        Assert.Equal(original.FullName, restored.ToUser().FullName);
        Assert.Equal(original.NormalizedEmail, restored.ToUser().NormalizedEmail);
        Assert.Equal(original.PasswordHash, restored.ToUser().PasswordHash);
        Assert.Equal(UserStatus.Disabled, restored.ToUser().Status);
        Assert.Equal(DateTimeKind.Utc, restored.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, restored.UpdatedAt.Kind);
        Assert.Equal(Now.AddMilliseconds(123), restored.CreatedAt);
        Assert.Equal(Now.AddSeconds(1).AddMilliseconds(765), restored.UpdatedAt);
        Assert.Equal(original.CreatedAt, restored.ToUser().CreatedAt);
        Assert.Equal(original.UpdatedAt, restored.ToUser().UpdatedAt);
    }

    [Fact]
    public void Millisecond_conversion_also_rounds_down_before_unix_epoch()
    {
        var restored = BsonSerializer.Deserialize<UserDocument>(Document(created: DateTime.UnixEpoch.AddTicks(-1)).ToBson());
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(-1), restored.CreatedAt);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Non_utc_model_timestamps_fail(DateTimeKind kind)
    {
        var invalid = DateTime.SpecifyKind(Now, kind);
        Assert.Throws<ArgumentException>(() => Document(created: invalid));
        Assert.Throws<ArgumentException>(() => Document(updated: invalid));
    }

    [Fact]
    public void Invalid_uuid_or_status_cannot_enter_the_model()
    {
        Assert.Throws<ArgumentException>(() => new User(Guid.Empty, "Synthetic", "synthetic@example.invalid", "synthetic", UserStatus.Active, Now, Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => Document((UserStatus)99));
        var bson = Document().ToBsonDocument();
        bson["_id"] = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.CSharpLegacy);
        Assert.Throws<FormatException>(() => BsonSerializer.Deserialize<UserDocument>(bson));
    }

    [Fact]
    public void Default_diagnostics_do_not_print_sensitive_model_values()
    {
        var document = Document();
        Assert.DoesNotContain(document.PasswordHash, document.ToString()!);
        Assert.DoesNotContain(document.NormalizedEmail, document.ToString()!);
        Assert.DoesNotContain(document.PasswordHash, document.ToUser().ToString()!);
    }

    private static UserDocument Document(UserStatus status = UserStatus.Active, DateTime? created = null, DateTime? updated = null) =>
        new(Guid.NewGuid(), "Synthetic", "synthetic@example.invalid", "synthetic-not-a-real-hash", status, created ?? Now, updated ?? Now);
}
