using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SportsBettingAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Team>> CreateTeam(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team);
        }
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetTeamStats(int id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
                return NotFound();

            var matches = await _context.Matches
                .Where(m => m.HomeTeamId == id || m.AwayTeamId == id)
                .ToListAsync();

            int played = matches.Count;
            int wins = 0;
            int draws = 0;
            int losses = 0;
            int goalsFor = 0;
            int goalsAgainst = 0;

            foreach (var match in matches)
            {
                int gf;
                int ga;

                if (match.HomeTeamId == id)
                {
                    gf = match.HomeGoals ?? 0;
                    ga = match.AwayGoals ?? 0;
                }
                else
                {
                    gf = match.AwayGoals ?? 0;
                    ga = match.HomeGoals ?? 0;
                }

                goalsFor += gf;
                goalsAgainst += ga;

                if (gf > ga)
                    wins++;
                else if (gf == ga)
                    draws++;
                else
                    losses++;
            }

            double avgGoalsFor = played > 0 ? (double)goalsFor / played : 0;
            double avgGoalsAgainst = played > 0 ? (double)goalsAgainst / played : 0;

            return Ok(new
            {
                team = team.Name,
                played,
                wins,
                draws,
                losses,
                goalsFor,
                goalsAgainst,
                avgGoalsFor,
                avgGoalsAgainst
            });
        }
    }
}