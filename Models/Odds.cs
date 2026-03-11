using System;

public class Odds
{
    public int Id { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; }

    public double HomeWin { get; set; }
    public double Draw { get; set; }
    public double AwayWin { get; set; }

    public string Bookmaker { get; set; }
}
