using CsvHelper;
using EloEngine.Vex.Api;
using EloEngine.Vex.Endpoints;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace EloEngine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            await TestApi();
            await RunVex(iterations: 2, eloK: 20, false);

        }
        static async Task TestApi()
        {
            var client = new WebClient();
            var url = "https://www.robotevents.com/api/v2/events?season%5B%5D=173&season%5B%5D=0&myEvents=false";

            client.Headers.Add("Authorization", Authorization.BearerToken);
            client.Headers.Add("Accept", "application/json");

            var result = client.DownloadString(url);

        }
        static async Task RunVex(int iterations = 1, int eloK = 40, bool refresh = false)
        {
            IEventClient eventClient = new EventClient();
            var teamsClient = new TeamClient();
            var events = new Dictionary<string, Event>();
            var page = 1;

            if (refresh || !File.Exists("events.json"))
            {
                Console.WriteLine($"[{DateTime.Now}] Retrieving events.");
                var allEvents = await eventClient.GetAllEventsAsync(season: new[] { 173 });
                Console.WriteLine($"[{DateTime.Now}] Retrieved {allEvents.Count} events.");
                events = allEvents.ToDictionary(x => x.Sku, x => x);
                var eventJson = JsonConvert.SerializeObject(events, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                });
                File.WriteAllText("events.json", eventJson);

                eventJson = File.ReadAllText("events.json");
                events = JsonConvert.DeserializeObject<Dictionary<string, Event>>(eventJson) ?? new();
            }
            else
            {
                var eventJson = File.ReadAllText("events.json");
                events = JsonConvert.DeserializeObject<Dictionary<string, Event>>(eventJson) ?? new();
            }


            var eventMatches = new Dictionary<string, List<MatchObj>>();
            if (refresh || !File.Exists("eventMatches.json"))
            {
                int processedEvents = 0;
                foreach (var kvp in events)
                {
                    var evt = kvp.Value;
                    var matches = await eventClient.GetAllMatchesForEventAsync(id: evt.Id, div: evt.Divisions.First().Id);
                    eventMatches.Add(evt.Sku, matches);
                    processedEvents++;
                    Console.WriteLine($"[{DateTime.Now}] Retrieved matches for event #{evt.Id} - {processedEvents} of {events.Count}");
                }
                {
                    var matchJson = JsonConvert.SerializeObject(eventMatches, new JsonSerializerSettings { Formatting = Formatting.Indented });
                    File.WriteAllText("eventMatches.json", matchJson);

                    matchJson = File.ReadAllText("eventMatches.json");
                    eventMatches = JsonConvert.DeserializeObject<Dictionary<string, List<MatchObj>>>(matchJson) ?? new();
                }
            }
            else
            {
                var eventMatchesJson = File.ReadAllText("eventMatches.json");
                eventMatches = JsonConvert.DeserializeObject<Dictionary<string, List<MatchObj>>>(eventMatchesJson) ?? new();
            }


            var eventTeams = new Dictionary<string, List<Vex.Api.Team>>();
            var allTeams = new Dictionary<int, Vex.Api.Team>();
            if (refresh || !File.Exists("eventTeams.json"))
            {
                int processedEvents = 0;

                foreach (var kvp in events)
                {
                    var evt = kvp.Value;
                    List<Vex.Api.Team> allEventTeams = await eventClient.GetAllTeamsForEventAsync(id: evt.Id);
                    eventTeams.Add(evt.Sku, allEventTeams);
                    foreach (var team in allEventTeams)
                    {
                        if (!allTeams.ContainsKey(team.Id))
                        {
                            allTeams.Add(team.Id, team);
                        }
                    }
                    processedEvents++;
                    Console.WriteLine($"[{DateTime.Now}] Retrieved {allEventTeams.Count} teams for event #{evt.Id} - {processedEvents} of {events.Count}");
                }
                {
                    var eventTeamsJson = JsonConvert.SerializeObject(eventTeams, new JsonSerializerSettings { Formatting = Formatting.Indented });
                    File.WriteAllText("eventTeams.json", eventTeamsJson);

                    var allTeamsJson = JsonConvert.SerializeObject(allTeams, new JsonSerializerSettings { Formatting = Formatting.Indented });
                    File.WriteAllText("allTeams.json", allTeamsJson);
                }
            }
            else
            {
                var eventTeamsJson = File.ReadAllText("eventTeams.json");
                eventTeams = JsonConvert.DeserializeObject<Dictionary<string, List<Vex.Api.Team>>>(eventTeamsJson);

                var allTeamsJson = File.ReadAllText("allTeams.json");
                allTeams = JsonConvert.DeserializeObject<Dictionary<int, Vex.Api.Team>>(allTeamsJson) ?? new();
            }

            bool allTeamsDirty = false;
            Dictionary<int, int> elos = null;
            if (refresh || !File.Exists($"team-elos-by-id-x{iterations}-k{eloK}.json"))
            {
                elos = new Dictionary<int, int>();
                var engine = new Elo();
                engine.K = eloK;

                for (var k = 0; k < iterations; k++)
                {

                    Console.Title = $"Elos Loop {k} of 10";
                    foreach (var kvp in eventMatches)
                    {
                        var sku = kvp.Key;
                        var evt = events[sku];
                        //skip scrimages
                        if (evt.Name.IndexOf("scrim", StringComparison.OrdinalIgnoreCase) > -1
                            || evt.Name.IndexOf("test", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            continue;
                        }
                        var matches = kvp.Value;
                        eventMatches[sku] = matches = matches.OrderBy(x => x.Started).ToList();
                        foreach (var match in matches)
                        {
                            if (match.Round <= 1 || match.Name.Contains("practice", StringComparison.OrdinalIgnoreCase)
                                || match.Started == DateTimeOffset.MinValue)
                            {
                                continue;
                            }
                            var team1 = match.Alliances.First();
                            if (team1.Teams.Count == 0)
                                continue;

                            var team2 = match.Alliances.Last();
                            if (team2.Teams.Count == 0)
                                continue;

                            var player1A = team1.Teams.First().Team;
                            if (!elos.ContainsKey(player1A.Id))
                                elos.Add(player1A.Id, 1200);

                            var player1B = team1.Teams.Last().Team;
                            if (!elos.ContainsKey(player1B.Id))
                                elos.Add(player1B.Id, 1200);

                            var player2A = team2.Teams.First().Team;
                            if (!elos.ContainsKey(player2A.Id))
                                elos.Add(player2A.Id, 1200);

                            var player2B = team2.Teams.Last().Team;
                            if (!elos.ContainsKey(player2B.Id))
                                elos.Add(player2B.Id, 1200);

                            var playerIds = new[] { player1A.Id, player1B.Id, player2A.Id, player2B.Id };
                            foreach (var playerId in playerIds)
                            {
                                if (!allTeams.ContainsKey(playerId))
                                {
                                    allTeamsDirty = true;
                                    Console.WriteLine($"Looking up missing team {playerId}");
                                    var teamResults = await teamsClient.GetTeamsAsync(id: new[] { playerId },
                                        registered: null, program: new[] { 1 }, grade: new[] { GradeLevel.High_School, GradeLevel.Middle_School }, page: 1);
                                    if (teamResults.Data.Count > 0)
                                    {
                                        var missingTeam = teamResults.Data.First();
                                        allTeams.Add(playerId, missingTeam);
                                    }
                                    else
                                    {
                                        string bp = "Team not found";
                                    }

                                }
                            }

                            if (team1.Score == team2.Score && team1.Score == 0)
                            {
                                continue;
                            }

                            var rating1a = elos[player1A.Id];
                            var rating1b = elos[player1B.Id];
                            var rating2a = elos[player2A.Id];
                            var rating2b = elos[player2B.Id];
                            var rating1 = rating1a + rating1b;
                            var rating2 = rating2a + rating2b;
                            var pct1a = (double)rating1a / rating1;
                            var pct1b = (double)rating1b / rating1;
                            var pct2a = (double)rating2a / rating2;
                            var pct2b = (double)rating2b / rating2;

                            var result = team1.Score > team2.Score ? GameOutcome.Player1Win : team1.Score == team2.Score ? GameOutcome.Player1Tie : GameOutcome.Player1Loss;
                            var eloUpdate = engine.CalculateELO(rating1, rating2, result);
                            var rating1Offset = eloUpdate.Player1Rating - rating1;
                            var rating2Offset = eloUpdate.Player2Rating - rating2;
                            var player1AOffset = (int)(rating1Offset * pct1a);
                            var player1BOffset = (int)(rating1Offset * pct1b);
                            var player2AOffset = (int)(rating2Offset * pct2a);
                            var player2BOffset = (int)(rating2Offset * pct2b);

                            elos[player1A.Id] += player1AOffset;
                            elos[player1B.Id] += player1BOffset;
                            elos[player2A.Id] += player2AOffset;
                            elos[player2B.Id] += player2BOffset;

                        }


                    }
                }
                var s = JsonConvert.SerializeObject(elos, Formatting.Indented);
                File.WriteAllText($"team-elos-by-id-x{iterations}-k{eloK}.json", s);


            }
            else
            {
                elos = JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText($"team-elos-by-id-x{iterations}-k{eloK}.json")) ?? new Dictionary<int, int>();
            }

            if (allTeamsDirty)
            {
                var allTeamsJson = JsonConvert.SerializeObject(allTeams, new JsonSerializerSettings { Formatting = Formatting.Indented });
                File.WriteAllText("allTeams.json", allTeamsJson);
            }


            List<Vex.Api.Team> RankedTeams = null;
            if (refresh || !File.Exists($"vex-teams-with-elos-x{iterations}-k{eloK}.json"))
            {
                foreach (var team in allTeams)
                {
                    if (elos.ContainsKey(team.Key))
                    {
                        team.Value.Rating = elos[team.Key];
                    }
                }
                var rankedTeams = allTeams.ToLookup(x => x.Value.Rating, x => x).OrderByDescending(x => x.Key).ToList();
                var worldRanking = 1;
                foreach (var rankedTeam in rankedTeams)
                {
                    foreach (var team in rankedTeam)
                    {
                        team.Value.AdditionalProperties["worldrank"] = worldRanking;
                    }
                    worldRanking += rankedTeam.Count();
                }

                RankedTeams = rankedTeams.SelectMany(x => x.Select(kvp => kvp.Value)).OrderByDescending(
                    t => int.Parse(t.AdditionalProperties["worldrank"].ToString())).ToList();

                var teamsJson = JsonConvert.SerializeObject(RankedTeams, Formatting.Indented);
                File.WriteAllText($"vex-teams-with-elos-x{iterations}-k{eloK}.json", teamsJson);
            }
            else
            {
                var teamsJon = File.ReadAllText($"vex-teams-with-elos-x{iterations}-k{eloK}.json");
                RankedTeams = JsonConvert.DeserializeObject<List<Vex.Api.Team>>(teamsJon) ?? new();
            }


        }
        static async Task RunNFL()
        {
            var engine = new GameEngine();
            engine.Merge();
            engine.Run();
        }
    }

    public class Authorization
    {
        public static string BearerToken = null;
        static Authorization()
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetFullPath("."));
            var driveRoot = assemblyDirectory.Root;
            var authFolder = Path.Combine(driveRoot.FullName, "invisettings");
            var authFile = Path.Combine(authFolder, "vex-api-bear-token.txt");
            var fi = new FileInfo(authFile);
            if (fi.Directory == null || fi.Directory.Exists || !fi.Exists)
            {
                fi.Directory?.Create();
                File.WriteAllText(fi.FullName, "bearer xxx");
            }
            BearerToken = File.ReadAllText(fi.FullName);
        }
    }
    public class TeamRankInfo
    {
        public TeamRankInfo(int Rank, string number, string Team, int Rating, int id, string organization, string city, string region, string country)
        {
            this.Rank = Rank;
            Number = number;
            this.Team = Team;
            this.Rating = Rating;
            Id = id;
            Organization = organization;
            City = city;
            Region = region;
            Country = country;
        }

        public int Rank { get; set; }
        public string Number { get; set; }
        public string Team { get; set; }
        public int Rating { get; set; }
        public int Id { get; set; }
        public string Organization { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }

    public enum GameOutcome
    {
        Player1Win = 2,
        Player1Tie = 1,
        Player1Loss = 0
    }
    public struct EloRanking
    {
        public int Player1Rating;
        public int Player2Rating;
        public int PreviousPlayer1Rating;
        public int PreviousPlayer2Rating;
        public double Player1WinProbability;
        public double Player2WinProbability;
    }
    public class Elo
    {
        /// <summary>
        /// Finding The Perfect “K”
        /// 
        /// Lower k allows Elo to converge more rapidly, higher k gives more stable numbers and requires more matches to converge
        /// 
        /// K can be static or it can slide from lower to higher as more matches are conducted.
        /// </summary>
        public static int EloK = 32;

        public int K { get; internal set; } = EloK;

        //https://dotnetcoretutorials.com/2018/09/18/calculating-elo-in-c/
        //https://www.omnicalculator.com/sports/elo
        //https://www.geeksforgeeks.org/elo-rating-algorithm/
        public double ExpectationToWin(int playerOneRating, int playerTwoRating)
        {
            return 1 / (1 + Math.Pow(10, (playerTwoRating - playerOneRating) / 400.0));
        }


        public EloRanking CalculateELO(int playerOneRating, int playerTwoRating, GameOutcome outcome)
        {

            double d = ((int)outcome / 2.0);
            var player1WinProbability = ExpectationToWin(playerOneRating, playerTwoRating);
            int delta = (int)(K * (d - player1WinProbability));

            return new EloRanking
            {
                PreviousPlayer1Rating = playerOneRating,
                PreviousPlayer2Rating = playerTwoRating,
                Player1Rating = playerOneRating + delta,
                Player2Rating = playerTwoRating - delta,
                Player1WinProbability = player1WinProbability,
                Player2WinProbability = 1 - player1WinProbability
            };
        }
    }
}