using System.Text.Json;
using System.Text.Json.Nodes;

namespace Platform.Application.Features.SideLearning;

public static class SideLearningSessionContentHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static IReadOnlyList<string> ReadSectionIds(string sessionContentJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(sessionContentJson) ? "{}" : sessionContentJson);
            if (!doc.RootElement.TryGetProperty("sections", out var arr) || arr.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<string>();
            }

            var list = new List<string>();
            foreach (var s in arr.EnumerateArray())
            {
                if (s.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                {
                    var t = id.GetString();
                    if (!string.IsNullOrEmpty(t))
                    {
                        list.Add(t);
                    }
                }
            }

            return list;
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    public static bool AllSectionsComplete(string sessionContentJson, string sectionsProgressJson)
    {
        var ids = ReadSectionIds(sessionContentJson);
        if (ids.Count == 0)
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(sectionsProgressJson) ? "{}" : sectionsProgressJson);
            var root = doc.RootElement;
            foreach (var id in ids)
            {
                if (!root.TryGetProperty(id, out var el) || el.ValueKind != JsonValueKind.True)
                {
                    return false;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string SetSectionProgress(string sectionsProgressJson, string sectionId, bool completed)
    {
        try
        {
            var node = string.IsNullOrWhiteSpace(sectionsProgressJson)
                ? new JsonObject()
                : JsonNode.Parse(sectionsProgressJson)?.AsObject() ?? new JsonObject();
            node[sectionId] = completed;
            return node.ToJsonString(JsonOptions);
        }
        catch (JsonException)
        {
            var node = new JsonObject { [sectionId] = completed };
            return node.ToJsonString(JsonOptions);
        }
    }
}
