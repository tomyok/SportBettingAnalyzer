using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SportsBettingAnalyzer.Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class PredictionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrediction(
            int homeTeamId,
            int awayTeamId,
            double homeOdd,
            double drawOdd,
            double awayOdd)
        {
            var homeTeam = await _context.Teams.FindAsync(homeTeamId);
            var awayTeam = await _context.Teams.FindAsync(awayTeamId);

            if (homeTeam == null || awayTeam == null)
                return NotFound("One or both teams not found");

            // 1) Partidos de cada equipo
            var homeMatches = await _context.Matches
                .Where(m => m.HomeTeamId == homeTeamId || m.AwayTeamId == homeTeamId)
                .ToListAsync();

            var awayMatches = await _context.Matches
                .Where(m => m.HomeTeamId == awayTeamId || m.AwayTeamId == awayTeamId)
                .ToListAsync();

            // 2) Goles a favor / en contra
            double homeGoalsFor = 0;
            double homeGoalsAgainst = 0;

            foreach (var match in homeMatches)
            {
                if (match.HomeTeamId == homeTeamId)
                {
                    homeGoalsFor += match.HomeGoals ?? 0;
                    homeGoalsAgainst += match.AwayGoals ?? 0;
                }
                else
                {
                    homeGoalsFor += match.AwayGoals ?? 0;
                    homeGoalsAgainst += match.HomeGoals ?? 0;
                }
            }

            double awayGoalsFor = 0;
            double awayGoalsAgainst = 0;

            foreach (var match in awayMatches)
            {
                if (match.HomeTeamId == awayTeamId)
                {
                    awayGoalsFor += match.HomeGoals ?? 0;
                    awayGoalsAgainst += match.AwayGoals ?? 0;
                }
                else
                {
                    awayGoalsFor += match.AwayGoals ?? 0;
                    awayGoalsAgainst += match.HomeGoals ?? 0;
                }
            }

            // 3) Promedios
            double avgGoalsForHome = homeMatches.Count > 0 ? homeGoalsFor / homeMatches.Count : 0;
            double avgGoalsAgainstHome = homeMatches.Count > 0 ? homeGoalsAgainst / homeMatches.Count : 0;

            double avgGoalsForAway = awayMatches.Count > 0 ? awayGoalsFor / awayMatches.Count : 0;
            double avgGoalsAgainstAway = awayMatches.Count > 0 ? awayGoalsAgainst / awayMatches.Count : 0;

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

            return Ok(new
            {
                homeTeam = homeTeam.Name,
                awayTeam = awayTeam.Name,

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
