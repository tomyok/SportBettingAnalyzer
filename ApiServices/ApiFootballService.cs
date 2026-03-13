using System.Net.Http;

namespace SportsBettingAnalyzer.Services
{
    public class ApiFootballService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ApiFootballService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiFootball:ApiKey"];
        }

        public async Task<string> GetLastMatches(int teamId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://v3.football.api-sports.io/fixtures?team={teamId}&season=2024"
            );

            request.Headers.Add("x-apisports-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLeagueFixtures(int leagueId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://v3.football.api-sports.io/fixtures?league={leagueId}&season=2024"
            );

            request.Headers.Add("x-apisports-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            return json;
        }

        public async Task<string> GetOdds(int fixtureId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://v3.football.api-sports.io/odds?fixture={fixtureId}"
            );

            request.Headers.Add("x-apisports-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetFixture(int fixtureId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://v3.football.api-sports.io/fixtures?id={fixtureId}"
            );

            request.Headers.Add("x-apisports-key", _apiKey);

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}