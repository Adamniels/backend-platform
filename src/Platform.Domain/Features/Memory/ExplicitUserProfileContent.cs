using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory;

/// <summary>Validation and JSON helpers for the explicit <c>memory_explicit_profile</c> row. User input only — inference pipelines must not use this to overwrite.</summary>
public static class ExplicitUserProfileContent
{
    public const int MaxStringLength = 256;
    public const int MaxListSize = 64;
    public const int MaxKeyLength = 128;
    public const int MaxValueLength = 512;
    public const int MaxNameLength = 256;
    public const int MaxProjectExternalIdLength = 256;
    public const int MaxJsonChars = 128_000;

    public const double ExplicitUserAuthorityValue = 1.0d;

    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static void ThrowIfStringListInvalid(
        IReadOnlyList<string> items,
        string field,
        int max,
        int maxStringLen = MaxStringLength)
    {
        if (items.Count > max)
        {
            throw new MemoryDomainException($"{field} has too many items (max {max}).");
        }

        for (var i = 0; i < items.Count; i++)
        {
            var s = items[i]?.Trim() ?? "";
            if (s.Length == 0)
            {
                throw new MemoryDomainException($"{field} entries must be non-empty.");
            }

            if (s.Length > maxStringLen)
            {
                throw new MemoryDomainException($"{field} item exceeds {maxStringLen} characters.");
            }
        }
    }

    public static IReadOnlyList<ProfileMemoryPreference> ParseAndValidatePreferencesJson(
        string json, string field)
    {
        ThrowIfJsonSize(json, field);
        try
        {
            var list = JsonSerializer.Deserialize<List<ProfileMemoryPreference>>(json, JsonReadOptions)
                ?? new List<ProfileMemoryPreference>();
            if (list.Count > MaxListSize)
            {
                throw new MemoryDomainException($"{field} has too many entries (max {MaxListSize}).");
            }

            foreach (var p in list)
            {
                var k = p.Key?.Trim() ?? "";
                var v = p.Value?.Trim() ?? "";
                if (k.Length == 0)
                {
                    throw new MemoryDomainException($"{field} key is required for each entry.");
                }

                if (k.Length > MaxKeyLength || v.Length > MaxValueLength)
                {
                    throw new MemoryDomainException($"{field} key/value length is out of range.");
                }
            }

            return list;
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid {field} JSON: {ex.Message}.");
        }
    }

    public static IReadOnlyList<ProfileMemoryProject> ParseAndValidateActiveProjectsJson(
        string json, string field)
    {
        ThrowIfJsonSize(json, field);
        try
        {
            var list = JsonSerializer.Deserialize<List<ProfileMemoryProject>>(json, JsonReadOptions)
                ?? new List<ProfileMemoryProject>();
            if (list.Count > MaxListSize)
            {
                throw new MemoryDomainException($"{field} has too many entries (max {MaxListSize}).");
            }

            foreach (var p in list)
            {
                var name = p.Name?.Trim() ?? "";
                if (name.Length == 0)
                {
                    throw new MemoryDomainException("Each active project must have a name.");
                }

                if (name.Length > MaxNameLength)
                {
                    throw new MemoryDomainException("Active project name is too long.");
                }

                var ex = p.ExternalId?.Trim();
                if (ex is { Length: > 0 } && ex.Length > MaxProjectExternalIdLength)
                {
                    throw new MemoryDomainException("Active project externalId is too long.");
                }
            }

            return list;
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid {field} JSON: {ex.Message}.");
        }
    }

    public static IReadOnlyList<ProfileMemorySkillLevel> ParseAndValidateSkillLevelsJson(
        string json, string field)
    {
        ThrowIfJsonSize(json, field);
        try
        {
            var list = JsonSerializer.Deserialize<List<ProfileMemorySkillLevel>>(json, JsonReadOptions)
                ?? new List<ProfileMemorySkillLevel>();
            if (list.Count > MaxListSize)
            {
                throw new MemoryDomainException($"{field} has too many entries (max {MaxListSize}).");
            }

            foreach (var s in list)
            {
                var n = s.Name?.Trim() ?? "";
                if (n.Length == 0)
                {
                    throw new MemoryDomainException("Each skill level must have a name.");
                }

                if (n.Length > MaxNameLength)
                {
                    throw new MemoryDomainException("Skill name is too long.");
                }

                MemoryValueConstraints.ThrowIfOutOf01(nameof(s.Level), s.Level);
            }

            return list;
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid {field} JSON: {ex.Message}.");
        }
    }

    public static string SerialisePreferencesJson(IReadOnlyList<ProfileMemoryPreference> list) =>
        JsonSerializer.Serialize(list, JsonWriteOptions);

    public static string SerialiseProjectsJson(IReadOnlyList<ProfileMemoryProject> list) =>
        JsonSerializer.Serialize(list, JsonWriteOptions);

    public static string SerialiseSkillLevelsJson(IReadOnlyList<ProfileMemorySkillLevel> list) =>
        JsonSerializer.Serialize(list, JsonWriteOptions);

    private static void ThrowIfJsonSize(string json, string field)
    {
        if (json is null)
        {
            throw new MemoryDomainException($"{field} is required.");
        }

        if (json.Length > MaxJsonChars)
        {
            throw new MemoryDomainException($"{field} is too large.");
        }
    }
}

public sealed record ProfileMemoryPreference(
    [property: JsonPropertyName("key")]
    string Key,
    [property: JsonPropertyName("value")]
    string Value);

public sealed record ProfileMemoryProject(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("externalId")]
    string? ExternalId = null);

public sealed record ProfileMemorySkillLevel(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("level")]
    double Level);
