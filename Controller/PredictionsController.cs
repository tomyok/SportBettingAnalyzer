using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsBettingAnalyzer.Services;
using System.Text.Json;
using SportsBettingAnalyzer.Models;

namespace SportsBettingAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PredictionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiFootballService _apiFootball;

        public PredictionsController(AppDbContext context, ApiFootballService apiFootball)
        {
            _context = context;
            _apiFootball = apiFootball;
        }

        [HttpGet("team-matches")]
        public async Task<IActionResult> GetTeamMatches(int teamId)
        {
            var json = await _apiFootball.GetLastMatches(teamId);

            var matches = JsonSerializer.Deserialize<ApiFootballResponse>(json);

            return Ok(matches.response);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrediction(
            int homeTeamId,
            int awayTeamId,
            double homeOdd,
            double drawOdd,
            double awayOdd)
        {

            var homeJson = await _apiFootball.GetLastMatches(homeTeamId);
            var awayJson = await _apiFootball.GetLastMatches(awayTeamId);

            // 1) Partidos de cada equipo
            var homeMatches = JsonSerializer.Deserialize<ApiFootballResponse>(homeJson);
            var awayMatches = JsonSerializer.Deserialize<ApiFootballResponse>(awayJson);

            if (homeMatches?.response == null || awayMatches?.response == null)
            {
                return BadRequest("Could not retrieve matches from API-Football");
            }

            // 2) Goles a favor / en contra
            double homeGoalsFor = 0;
            double homeGoalsAgainst = 0;

            foreach (var match in homeMatches.response.Take(10))
            {
                if (match.teams.home.id == homeTeamId)
                {
                    homeGoalsFor += match.goals.home ?? 0;
                    homeGoalsAgainst += match.goals.away ?? 0;
                }
                else
                {
                    homeGoalsFor += match.goals.away ?? 0;
                    homeGoalsAgainst += match.goals.home ?? 0;
                }
            }

            double awayGoalsFor = 0;
            double awayGoalsAgainst = 0;

            foreach (var match in awayMatches.response.Take(10))
            {
                if (match.teams.home.id == awayTeamId)
                {
                    awayGoalsFor += match.goals.home ?? 0;
                    awayGoalsAgainst += match.goals.away ?? 0;
                }
                else
                {
                    awayGoalsFor += match.goals.away ?? 0;
                    awayGoalsAgainst += match.goals.home ?? 0;
                }
            }

            int homeMatchCount = homeMatches.response.Take(10).Count();
            int awayMatchCount = awayMatches.response.Take(10).Count();

            // 3) Promedios
            double avgGoalsForHome = homeGoalsFor / homeMatchCount;
            double avgGoalsAgainstHome = homeGoalsAgainst / homeMatchCount;

            double avgGoalsForAway = awayGoalsFor / awayMatchCount;
            double avgGoalsAgainstAway = awayGoalsAgainst / awayMatchCount;

            double expectedGoalsHome = (avgGoalsForHome + avgGoalsAgainstAway) / 2;
            double expectedGoalsAway = (avgGoalsForAway + avgGoalsAgainstHome) / 2;

            // 4) Expected goals
            double[] homeGoalProb = new double[6];
            double[] awayGoalProb = new double[6];

            for (int i = 0; i <= 5; i++)
            {
                homeGoalProb[i] = Poisson(expectedGoalsHome, i);
                awayGoalProb[i] = Poisson(expectedGoalsAway, i);
            }

            // 6) Probabilidades de resultado
            double homeWinProbability = 0;
            double drawProbability = 0;
            double awayWinProbability = 0;

            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    double probability = homeGoalProb[i] * awayGoalProb[j];

                    if (i > j)
                        homeWinProbability += probability;
                    else if (i == j)
                        drawProbability += probability;
                    else
                        awayWinProbability += probability;
                }
            }

            // 7) Value calculation
            double homeValue = (homeWinProbability * homeOdd) - 1;
            double drawValue = (drawProbability * drawOdd) - 1;
            double awayValue = (awayWinProbability * awayOdd) - 1;

            string recommendation = "No value bet";

            if (homeValue > drawValue && homeValue > awayValue && homeValue > 0)
                recommendation = "Bet Home";
            else if (drawValue > homeValue && drawValue > awayValue && drawValue > 0)
                recommendation = "Bet Draw";
            else if (awayValue > homeValue && awayValue > drawValue && awayValue > 0)
                recommendation = "Bet Away";

            // Redondeamos
            homeWinProbability = Math.Round(homeWinProbability, 3);
            drawProbability = Math.Round(drawProbability, 3);
            awayWinProbability = Math.Round(awayWinProbability, 3);

            //nombres de los equipos de los ultimos partidos
            var firstHomeMatch = homeMatches.response.First();

            string homeTeamName =
                firstHomeMatch.teams.home.id == homeTeamId
                ? firstHomeMatch.teams.home.name
                : firstHomeMatch.teams.away.name;

            var firstAwayMatch = awayMatches.response.First();

            string awayTeamName =
                firstAwayMatch.teams.home.id == awayTeamId
                ? firstAwayMatch.teams.home.name
                : firstAwayMatch.teams.away.name;

            return Ok(new
            {
                homeTeam = homeTeamName,
                awayTeam = awayTeamName,

                expectedGoalsHome,
                expectedGoalsAway,

                homeWinProbability,
                drawProbability,
                awayWinProbability,

                homeOdd,
                drawOdd,
                awayOdd,

                homeValue,
                drawValue,
                awayValue,

                recommendation
            });
        }

        [HttpGet("league-matches")]
        public async Task<IActionResult> GetLeagueMatches(int league)
        {
            var json = await _apiFootball.GetLeagueFixtures(league);

            var fixtures = JsonSerializer.Deserialize<ApiFootballResponse>(json);

            if (fixtures?.response == null)
                return BadRequest("No fixtures found");

            return Ok(fixtures.response);
        }


        private double Poisson(double lambda, int k)
        {
            return Math.Pow(lambda, k) * Math.Exp(-lambda) / Factorial(k);
        }

        private double Factorial(int n)
        {
            double result = 1;

            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }

        return result;
        }
    }
}
