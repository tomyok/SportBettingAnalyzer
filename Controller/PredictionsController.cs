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

        //1 buscar equipo local
        //2 buscar equipo visitante
        //3 obtener sus partidos
        //4 calcular promedio de goles
        //5 calcular goles esperados
        //6 devolver resultado
        [HttpGet]
        public async Task<IActionResult> GetPrediction(int homeTeamId, int awayTeamId)
        {
            var homeTeam = await _context.Teams.FindAsync(homeTeamId);
            var awayTeam = await _context.Teams.FindAsync(awayTeamId);

            if (homeTeam == null || awayTeam == null)
                return NotFound("One or both teams not found");

            var homeMatches = await _context.Matches
                .Where(m => m.HomeTeamId == homeTeamId || m.AwayTeamId == homeTeamId)
                .ToListAsync();

            var awayMatches = await _context.Matches
                .Where(m => m.HomeTeamId == awayTeamId || m.AwayTeamId == awayTeamId)
                .ToListAsync();

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

            double avgGoalsForHome = homeMatches.Count > 0 ? homeGoalsFor / homeMatches.Count : 0;
            double avgGoalsAgainstHome = homeMatches.Count > 0 ? homeGoalsAgainst / homeMatches.Count : 0;

            double avgGoalsForAway = awayMatches.Count > 0 ? awayGoalsFor / awayMatches.Count : 0;
            double avgGoalsAgainstAway = awayMatches.Count > 0 ? awayGoalsAgainst / awayMatches.Count : 0;

            double expectedGoalsHome = (avgGoalsForHome + avgGoalsAgainstAway) / 2;
            double expectedGoalsAway = (avgGoalsForAway + avgGoalsAgainstHome) / 2;

            return Ok(new
            {
                homeTeam = homeTeam.Name,
                awayTeam = awayTeam.Name,
                expectedGoalsHome,
                expectedGoalsAway
            });
        }

    }
}
