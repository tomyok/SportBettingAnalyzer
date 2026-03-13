using System.Text.Json.Serialization;

namespace SportsBettingAnalyzer.Models
{
    public class ApiFootballResponse
    {
        public List<FixtureResponse> response { get; set; }
    }

    public class FixtureResponse
    {
        public Goals goals { get; set; }
        public Teams teams { get; set; }
        public League league { get; set; }
        public Fixture fixture { get; set; }
    }

    public class Goals
    {
        public int? home { get; set; }
        public int? away { get; set; }
    }

    public class Teams
    {
        public Team home { get; set; }
        public Team away { get; set; }
    }

    public class Team
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class League
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Fixture
    {
        public int id { get; set; }
    }
}
