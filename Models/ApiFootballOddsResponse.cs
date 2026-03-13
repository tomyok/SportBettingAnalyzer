namespace SportsBettingAnalyzer.Models
{
    public class ApiFootballOddsResponse
    {
        public List<OddsResponseItem> response { get; set; }
    }

    public class OddsResponseItem
    {
        public List<Bookmaker> bookmakers { get; set; }
    }

    public class Bookmaker
    {
        public string name { get; set; }
        public List<Bet> bets { get; set; }
    }

    public class Bet
    {
        public string name { get; set; }
        public List<BetValue> values { get; set; }
    }

    public class BetValue
    {
        public string value { get; set; }
        public string odd { get; set; }
    }
}