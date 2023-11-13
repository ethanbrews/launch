using System.Text.Json.Serialization;

namespace Launch;

public class Shortcut
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("aliases")]
    public List<string> Aliases { get; init; } = new();
}