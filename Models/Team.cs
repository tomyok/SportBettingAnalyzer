using System;
using System.Text.Json.Serialization;
// resolviendo "circular reference problem"
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    [JsonIgnore]
    public ICollection<Match>? HomeMatches { get; set; }
    [JsonIgnore]
    public ICollection<Match>? AwayMatches { get; set; }
}
