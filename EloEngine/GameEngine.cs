using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EloEngine
{
    public class GameData
    {
        // https://www.footballdb.com/games/index.html
        public static List<GameScore> LoadData()
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.Delimiter = "\t";
            csvConfig.HeaderValidated = null;
            csvConfig.MissingFieldFound = null;
            using (var reader = new StreamReader("2022-2023_results.txt", Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecords<GameScore>();
                return records.ToList();
            }
        }

        internal static Dictionary<string, TeamRecord> LoadRecords()
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfig.Delimiter = "\t";
            csvConfig.HeaderValidated = null;
            csvConfig.MissingFieldFound = null;
            using (var reader = new StreamReader("2022-2023_records.txt", Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecords<TeamRecord>();
                return records.ToDictionary(x => x.Abr, x => x);
            }
        }
    }
    public class GameEngine
    {
        public async void Merge()
        {
            var di = new DirectoryInfo(Path.GetFullPath(@"..\EloEngine\bin\Debug\net6.0"));
            var files = di.GetFiles("teamElos-k-*-converged-*.csv");

            var data = new Dictionary<int, Dictionary<string, SeasonElosRecord>>();
            var output = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
                //csvConfig.Delimiter = ,";
                csvConfig.HeaderValidated = null;
                csvConfig.MissingFieldFound = null;
                using (var reader = new StreamReader(file.FullName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var records = csv.GetRecords<SeasonElosRecord>()
                        .ToDictionary(x => x.Abr, x => x);
                    var k = int.Parse(file.Name.Substring("teamElos-k-".Length).Split('-')[0]);
                    if (data.ContainsKey(k))
                    {
                        string bp = $"Duplicate file for {k}";
                    }
                    data.Add(k, records);
                    if (k == 10)
                    {
                        var outputLines = File.ReadAllLines(file.FullName).ToList();
                        output = outputLines.ToDictionary(x => x.Split(",")[0], x => x);
                    }

                }
            }
            var teams = data.First().Value.Keys.ToList();

            foreach (var kvp in data)
            {
                var k = kvp.Key;
                var records = kvp.Value.ToList();
                output["Key"] += $",Rank {k},Rating {k}";

                for (var i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    var team = record.Key;
                    var rating = record.Value.CurrentRating;
                    output[team] += $",{i + 1},{rating}";
                }
            }

            File.WriteAllLines("Merged-Elos.csv", output.Values);

        }
        public async Task Run()
        {
            var data = GameData.LoadData();
            var teams = data.Select(x => new Team(x.Visitor, x.VisitorAbr))
                .Concat(data.Select(x => new Team(x.Home, x.HomeAbr)).Distinct())
                .Distinct().ToDictionary(x => x.Abr, x => x);
            var engine = new Elo();
            engine.K = 80;
            var minK = 10;
            int numPasses = 10000;
            var lastResults = "";
            int step = 0;
            while (engine.K >= minK)
            {

                for (step = 0; step < numPasses; step++)
                {

                    if (step % 10000 == 0)
                    {
                        Console.Title = $"Pass {step} of {numPasses}";
                    }
                    foreach (var game in data)
                    {
                        if (game.Date > DateTime.Now)
                            break;

                        var home = teams[game.HomeAbr];
                        var away = teams[game.VisitorAbr];

                        GameOutcome outcome = GameOutcome.Player1Tie;
                        switch (game.GameResult)
                        {
                            case GameResult.HomeWin:
                                outcome = GameOutcome.Player1Win;
                                break;
                            case GameResult.VisitorWin:
                                outcome = GameOutcome.Player1Loss;
                                break;
                            case GameResult.Tie:
                                outcome = GameOutcome.Player1Tie;
                                break;

                            default: break;
                        }


                        var result = engine.CalculateELO(home.CurrentRating, away.CurrentRating, outcome);
                        game.UpdateElo(result, home, away);
                    }

                    if (step % 1000 == 0)
                    {
                        var sb = new StringBuilder();
                        using (TextWriter writer = new StringWriter(sb))
                        {
                            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                            config.Delimiter = "\t";
                            using (var csv = new CsvWriter(writer, config, false))
                            {

                                csv.WriteRecords(teams.OrderByDescending(x => x.Value.CurrentRating)); // where values implements IEnumerable
                                csv.Flush();
                            }
                        }

                        var current = sb.ToString();
                        if (current == lastResults)
                        {
                            Console.WriteLine($"K={engine.K} converged at step {step}");
                            break;
                        }
                        else
                        {
                            var lastLines = lastResults.Split("\n");
                            var newLines = current.Split("\n");
                            if (newLines.Length == lastLines.Length)
                            {
                                var joinedLines = Enumerable.Range(0, lastLines.Length)
                                    .Select(i => $"{lastLines[i].Trim()}\t\t{newLines[i].Trim()}").ToList();
                                var joined = string.Join("\n", joinedLines);
                                bool writeToFile = false;
                                if (writeToFile)
                                {
                                    File.WriteAllText("Progress.txt", joined);
                                }
                            }
                        }
                        lastResults = current;
                    }


                    if (step < numPasses - 1)
                    {
                        foreach (var team in teams)
                        {
                            var latest = team.Value.Elos.Last();
                            team.Value.Elos.Clear();
                            team.Value.Elos.Add(latest);
                        }
                    }

                }
                if (step == numPasses)
                {
                    Console.WriteLine($"K={engine.K} failed to converge at step {step}");
                }
                var records = GameData.LoadRecords();
                foreach (var kvp in records)
                {
                    var team = teams[kvp.Key];
                    var record = kvp.Value;
                    team.W = record.W;
                    team.L = record.L;
                    team.T = record.T;
                    team.Mascot = record.Team;
                }

                using (TextWriter writer = new StreamWriter($@"results-k-{engine.K}-converged-{step}.csv", false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false);
                    csv.WriteRecords(data); // where values implements IEnumerable
                }



                using (TextWriter writer = new StreamWriter($@"teamElos-k-{engine.K}-converged-{step}.csv", false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false);
                    //var teamElos= teams.Values.Select(x => new TeamEloRecord(x))
                    //    .OrderByDescending(x=> x.Elo).ToList();

                    csv.WriteRecords(teams.OrderByDescending(x => x.Value.CurrentRating)); // where values implements IEnumerable
                }
                engine.K--;
            }
            await Task.CompletedTask;
        }
    }


    public class SeasonElosRecord
    {
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Abr { get; set; } = null!;
        public int CurrentRating { get; set; }
        public string Mascot { get; set; } = null!;
        public int W { get; set; }
        public int L { get; set; }
        public int T { get; set; }

    }
    public class TeamRecord
    {
        public string Abr { get; set; } = null!;
        public string Team { get; set; } = null!;
        public int W { get; set; }
        public int L { get; set; }
        public int T { get; set; }
    }
    public class TeamEloRecord
    {
        public TeamEloRecord(Team team)
        {
            Name = team.Name;
            Abr = team.Abr;
            Elo = team.CurrentRating;
        }

        public string Name { get; set; } = null!;
        public string Abr { get; set; } = null!;
        public int Elo { get; set; }
    }
    public class Team : IEquatable<Team?>
    {
        public Team(string name, string abr)
        {
            this.Name = name;
            this.Abr = abr;
        }

        public string Name { get; set; } = null!;
        public string Abr { get; set; } = null!;

        public int CurrentRating => Elos.Last().UpdatedElo;
        public string Mascot { get; set; } = null!;
        public int W { get; internal set; }
        public int L { get; internal set; }
        public int T { get; internal set; }


        public override bool Equals(object? obj)
        {
            return Equals(obj as Team);
        }

        public bool Equals(Team? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Abr == other.Abr;
        }

        public static bool operator ==(Team? left, Team? right)
        {
            return EqualityComparer<Team>.Default.Equals(left, right);
        }

        public static bool operator !=(Team? left, Team? right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Abr.GetHashCode();
        }
        public override string ToString()
        {
            return Name;
        }

        internal void UpdateElo(int newRating, GameScore game)
        {
            var latestElo = Elos.Last();
            var newElo = new EloHistory();
            newElo.UpdatedElo = newElo.InitialElo = latestElo.UpdatedElo = newRating;
            latestElo.GameScore = game;
            Elos.Add(newElo);
        }

        public List<EloHistory> Elos { get; private set; } = new List<EloHistory> { new EloHistory() };

    }

    public class EloHistory
    {
        public int InitialElo { get; set; } = 1200;
        public int UpdatedElo { get; set; } = 1200;
        public GameScore GameScore { get; set; } = null!;

    }

    public class GameScore
    {
        public DateTime Date { get; set; }
        public string Visitor { get; set; } = null!;
        public string VisitorAbr { get; set; } = null!;
        public int VisitorScore { get; set; }
        public string Home { get; set; } = null!;
        public string HomeAbr { get; set; } = null!;
        public int HomeScore { get; set; }

        public int PreEloHome { get; set; }
        public int PreEloAway { get; set; }
        public double HomeWinProbability { get; set; }
        public double AwayWinProbability { get; set; }
        public int PostEloHome { get; set; }
        public int PostEloAway { get; set; }

        public GameResult GameResult => VisitorScore > HomeScore ? GameResult.VisitorWin : (VisitorScore < HomeScore ? GameResult.HomeWin : GameResult.Tie);

        public void UpdateElo(EloRanking result, Team home, Team away)
        {
            PreEloHome = result.PreviousPlayer1Rating;
            PreEloAway = result.PreviousPlayer2Rating;
            PostEloHome = result.Player1Rating;
            PostEloAway = result.Player2Rating;
            HomeWinProbability = result.Player1WinProbability;
            AwayWinProbability = result.Player2WinProbability;
            home.UpdateElo(result.Player1Rating, this);
            away.UpdateElo(result.Player2Rating, this);
        }
    }
    public enum GameResult
    {
        Tie = 0,
        HomeWin = 1,
        VisitorWin = 2
    }
}