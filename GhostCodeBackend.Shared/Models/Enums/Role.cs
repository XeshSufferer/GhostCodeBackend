using System.Text.Json.Serialization;

namespace GhostCodeBackend.Shared.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    User,
    Admin
}