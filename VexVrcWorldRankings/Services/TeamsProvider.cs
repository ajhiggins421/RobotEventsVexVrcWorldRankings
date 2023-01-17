
using EloEngine.Vex.Api;
using Newtonsoft.Json;
using System.Diagnostics;
using GameOutcome = EloEngine.GameOutcome;
namespace VexVrcWorldRankings.Services
{
    public interface ITeamsProvider
    {
        Dictionary<int, Team> TeamsDict { get; }
        List<Team> Teams { get; }
    }

    public class TeamsProvider : ITeamsProvider
    {
        public static string DataDirectory = DataProvider.DataDirectory;
        public static void LoadCache()
        {
            var eventsJsonFullPath = Path.Combine(DataDirectory, "vex-teams-with-elos-x2-k20.json");
            var json = File.ReadAllText(eventsJsonFullPath);
            var teams = (JsonConvert.DeserializeObject<List<Team>>(json) ?? new());
            TeamsDictCache = teams.ToDictionary(x => x.Id, x => x);
            TeamsCache = teams;

            var eventMatches = EventsProvider.EventMatchesCache;
            var events = EventsProvider.EventsDictCache;
            foreach (var matchList in eventMatches)
            {
                var key = matchList.Key;
                var matches = matchList.Value;
                var evt = events[key];

                if (evt.Name.IndexOf("scrim", StringComparison.OrdinalIgnoreCase) > -1
                                   || evt.Name.IndexOf("test", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    continue;
                }
                foreach (var match in matches)
                {
                    if (match.Round <= 1 || match.Name.Contains("practice", StringComparison.OrdinalIgnoreCase)
                               || match.Started == DateTimeOffset.MinValue)
                    {
                        continue;
                    }
                    var team1 = match.Alliances.First();
                    var team2 = match.Alliances.Last();
                    var player1A = team1.Teams.First().Team;
                    var player1B = team1.Teams.Last().Team;
                    var player2A = team2.Teams.First().Team;
                    var player2B = team2.Teams.Last().Team;

                    var matchResult = team1.Score > team2.Score ? GameOutcome.Player1Win : team1.Score == team2.Score ? GameOutcome.Player1Tie : GameOutcome.Player1Loss;

                    var isQualifier = match.Name.StartsWith("Q");
                    if (isQualifier)
                    {
                        switch (matchResult)
                        {
                            case GameOutcome.Player1Win:
                                TeamsDictCache[player1A.Id].TeamRecord.QualifierRecord.W++;
                                TeamsDictCache[player1B.Id].TeamRecord.QualifierRecord.W++;
                                TeamsDictCache[player2A.Id].TeamRecord.QualifierRecord.L++;
                                TeamsDictCache[player2B.Id].TeamRecord.QualifierRecord.L++;
                                break;
                            case GameOutcome.Player1Loss:
                                TeamsDictCache[player1A.Id].TeamRecord.QualifierRecord.L++;
                                TeamsDictCache[player1B.Id].TeamRecord.QualifierRecord.L++;
                                TeamsDictCache[player2A.Id].TeamRecord.QualifierRecord.W++;
                                TeamsDictCache[player2B.Id].TeamRecord.QualifierRecord.W++;
                                break;
                            case GameOutcome.Player1Tie:
                                if (team1.Score == 0)
                                {
                                    continue;
                                }
                                TeamsDictCache[player1A.Id].TeamRecord.QualifierRecord.T++;
                                TeamsDictCache[player1B.Id].TeamRecord.QualifierRecord.T++;
                                TeamsDictCache[player2A.Id].TeamRecord.QualifierRecord.T++;
                                TeamsDictCache[player2B.Id].TeamRecord.QualifierRecord.T++;
                                break;
                        }
                    }
                    else
                    {
                        switch (matchResult)
                        {
                            case GameOutcome.Player1Win:
                                TeamsDictCache[player1A.Id].TeamRecord.EliminationRecord.W++;
                                TeamsDictCache[player1B.Id].TeamRecord.EliminationRecord.W++;
                                TeamsDictCache[player2A.Id].TeamRecord.EliminationRecord.L++;
                                TeamsDictCache[player2B.Id].TeamRecord.EliminationRecord.L++;
                                break;
                            case GameOutcome.Player1Loss:
                                TeamsDictCache[player1A.Id].TeamRecord.EliminationRecord.L++;
                                TeamsDictCache[player1B.Id].TeamRecord.EliminationRecord.L++;
                                TeamsDictCache[player2A.Id].TeamRecord.EliminationRecord.W++;
                                TeamsDictCache[player2B.Id].TeamRecord.EliminationRecord.W++;
                                break;
                            case GameOutcome.Player1Tie:
                                if (team1.Score == 0)
                                {
                                    continue;
                                }
                                TeamsDictCache[player1A.Id].TeamRecord.EliminationRecord.T++;
                                TeamsDictCache[player1B.Id].TeamRecord.EliminationRecord.T++;
                                TeamsDictCache[player2A.Id].TeamRecord.EliminationRecord.T++;
                                TeamsDictCache[player2B.Id].TeamRecord.EliminationRecord.T++;
                                break;
                        }
                    }


                }
            }

        }
        public static List<Team> TeamsCache = null!;
        public static Dictionary<int, Team> TeamsDictCache = null!;

        public Dictionary<int, Team> TeamsDict { get => TeamsDictCache; }
        public List<Team> Teams { get => TeamsCache; }

    }

    public interface IEventTeamsProvider
    {
        Dictionary<string, List<Team>> TeamsDict { get; }

    }

    public class EventTeamsProvider : IEventTeamsProvider
    {
        public static string DataDirectory = DataProvider.DataDirectory;
        public static void LoadCache()
        {
            var eventsJsonFullPath = Path.Combine(DataDirectory, "eventTeams.json");
            var json = File.ReadAllText(eventsJsonFullPath);
            TeamsDictCache = (JsonConvert.DeserializeObject<Dictionary<string, List<Team>>>(json) ?? new())
                .ToDictionary(x => x.Key, x => x.Value.OrderByDescending(t => t.Rating).ToList());
        }

        public static Dictionary<string, List<Team>> TeamsDictCache = null!;

        public Dictionary<string, List<Team>> TeamsDict { get => TeamsDictCache; }

    }

}
