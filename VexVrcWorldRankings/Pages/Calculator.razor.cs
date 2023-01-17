using CsvHelper;
using EloEngine.Vex.Api;
using EloEngine.Vex.Endpoints;
using Newtonsoft.Json;
using System.Globalization;
using static MudBlazor.CategoryTypes;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Text;
using System.Linq;
using VexVrcWorldRankings.Services;
using MudBlazor;
using EloEngine;
using Team = EloEngine.Vex.Api.RankedTeam;

namespace VexVrcWorldRankings.Pages
{
    public partial class Calculator
    {

        private string? blue1 = null;
        private string? blue2 = null;
        private string? red1 = null;
        private string? red2 = null;
        public string? Blue1
        {
            get => blue1;
            set
            {
                if (blue1 == value) return;
                blue1 = (value ?? "").Trim();
                SetPlayer(blue1, ref BluePlayer1);
                InvokeAsync(StateHasChanged);
            }
        }


        public string? Blue2
        {
            get => blue2;
            set
            {
                if (blue2 == value) return;
                blue2 = (value ?? "").Trim();
                SetPlayer(blue2, ref BluePlayer2);
            }
        }

        public string Red1
        {
            get => red1;
            set
            {
                if (red1 == value) return;
                red1 = (value ?? "").Trim();
                SetPlayer(red1, ref RedPlayer1);
            }
        }
        public string Red2
        {
            get => red2;
            set
            {
                if (red2 == value) return;
                red2 = (value ?? "").Trim();
                SetPlayer(red2, ref RedPlayer2);

            }
        }



        public Team? BluePlayer1;
        public Team? BluePlayer2;

        public Team? RedPlayer1;
        public Team? RedPlayer2;

        private void SetPlayer(string? teamNumber, ref Team? target)
        {
            if (string.IsNullOrEmpty(teamNumber))
                target = null;
            else
            {
                var team = teamsProvider.Teams.FirstOrDefault(x => string.Compare(x.Number, teamNumber, true) == 0);
                if (team is null)
                    target = null;
                else
                    target = new RankedTeam(team);
            }
            if (teamsAreSet())
            {
                InvokeAsync(LoadEvents);
            }
        }

        bool teamsAreSet() => (BluePlayer1 != null && BluePlayer2 != null && RedPlayer1 != null && RedPlayer2 != null);

        public List<Event> Events = null!;
        public List<MatchObj> Elements = null!;
        public Event Event = null!;
        public List<Team> Teams = null!;

        public int[] pageSizeOptions = new[] { 10, 100, 1000 };
        [Inject]
        public IEventsProvider eventsProvider { get; set; } = null!;
        [Inject]
        public IEventTeamsProvider eventTeamsProvider { get; set; } = null!;
        [Inject]
        public ITeamsProvider teamsProvider { get; set; } = null!;

        [Inject]
        public Elo engine { get; set; } = null!;
        [Parameter]
        public string? Sku { get; set; } = null!;

        private bool dense = true;
        private bool hover = true;
        private bool striped = true;
        private bool bordered = true;
        private string searchString1 = "";
        private MatchObj? selectedItem1 = null;

        private HashSet<Event> selectedItems = new HashSet<Event>();

        private string searchStringTeams1 = "";
        private Team? selectedTeam = null;
        private HashSet<Team> selectedTeams = new HashSet<Team>();

        public string DataDirectory = DataProvider.DataDirectory;

        protected override async Task OnInitializedAsync()
        {
            Blue1 = "515R";
            Blue2 = "4082B";
            Red1 = "9364C";
            Red2 = "229V";
            if (Events == null)
            {
                await LoadEvents();

            }
        }

        async Task LoadEvents()
        {
            if ((blue1 is null || blue2 is null || red1 is null || red2 is null))
                return;

            if ((BluePlayer1 is null || BluePlayer2 is null || RedPlayer1 is null || RedPlayer2 is null))
                return;

            var match = new MatchObj()
            {
                Name = "What-if",
                Started = DateTimeOffset.Now,
                Scheduled = DateTimeOffset.Now,
                Alliances = new List<Alliance>()
                {
                     new Alliance()
                     {
                         Color = AllianceColor.Blue,
                         Score=0,
                         Teams= new List<AllianceTeam>()
                         {
                            new AllianceTeam() { Team=new IdInfo() { Code=blue1, Id=BluePlayer1.Team.Id, Name= BluePlayer1.Team.Team_name } },
                            new AllianceTeam() { Team=new IdInfo() { Code=blue2, Id=BluePlayer2.Team.Id, Name= BluePlayer2.Team.Team_name } },
                         }
                     },
                     new Alliance()
                     {
                         Color = AllianceColor.Red,
                         Score=0,
                         Teams= new List<AllianceTeam>()
                         {
                            new AllianceTeam() { Team=new IdInfo() { Code=red1, Id=RedPlayer1.Team.Id, Name= RedPlayer1.Team.Team_name } },
                            new AllianceTeam() { Team=new IdInfo() { Code=red2, Id=RedPlayer2.Team.Id, Name= RedPlayer2.Team.Team_name } },
                         }
                     },

                }

            };
            Elements = new List<MatchObj> { match };


            // eventsProvider.EventMatches[Sku].OrderBy(x => x.Scheduled).ToList();

            //var teams = eventTeamsProvider.TeamsDict[Event.Id];

            var teams = Elements.SelectMany(x => x.Alliances.SelectMany(a => a.Teams.Select(t => t.Team.Id))).Distinct().ToList();

            var teamsList = new List<EloEngine.Vex.Api.Team>()
            {
                teamsProvider.TeamsDict[BluePlayer1.Team.Id],
                teamsProvider.TeamsDict[BluePlayer2.Team.Id],
                teamsProvider.TeamsDict[RedPlayer1.Team.Id],
                teamsProvider.TeamsDict[RedPlayer2.Team.Id],
            };



            this.Teams = teamsList.OrderBy(x => x.WorldRank).Select(x => new RankedTeam(x)
            {
                WorldRank = x.WorldRank,
            }).ToList();
            for (var i = 0; i < Teams.Count; i++)
            {
                Teams[i].Rank = i + 1;
            }

            await Task.CompletedTask;
        }




        private bool FilterTeamFunc1(Team element) => FilterTeamFunc(element, searchStringTeams1);

        private bool FilterTeamFunc(Team element, string searchString)
        {

            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            searchString = searchString.ToLower();
            var x = element;
            var matched = x.Team.Number.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1
                || x.Team.Team_name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1
                || x.Team.Organization.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1;

            return matched;
        }


        private bool FilterFunc1(MatchObj element) => FilterFunc(element, searchString1);

        private bool FilterFunc(MatchObj element, string searchString)
        {

            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            searchString = searchString.ToLower();

            var matchTeams = element
                .Alliances
                .SelectMany(a => a.Teams.Where(t => t.Team.Name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1)
                ).ToList();
            return matchTeams.Count > 0;
        }

        public string GetRanking(int teamNumber)
        {
            if (!this.teamsProvider.TeamsDict.ContainsKey(teamNumber))
                return "1200";

            var team = this.teamsProvider.TeamsDict[teamNumber];
            var rating = team.AdditionalProperties["rating"]?.ToString() ?? "1200";
            return rating;
        }

        public double ExpectationToWinRed(ICollection<Alliance> alliances)
        {
            var blue1 = GetRating(alliances.First().Teams.First().Team);
            var blue2 = GetRating(alliances.First().Teams.Last().Team);

            int red1 = GetRating(alliances.Last().Teams.First().Team);
            int red2 = GetRating(alliances.Last().Teams.Last().Team);

            return engine.ExpectationToWin(red1 + red2, blue1 + blue2);
        }

        private int GetRating(IdInfo teamInfo)
        {
            if (teamInfo.Name != null && TeamsProvider.TeamsDictCache.ContainsKey(teamInfo.Id))
            {
                var team = TeamsProvider.TeamsDictCache[teamInfo.Id];
                return team.Rating;
            }
            return 1200;
        }

        public double ExpectationToWinBlue(ICollection<Alliance> alliances)
        {

            var blue1 = GetRating(alliances.First().Teams.First().Team);
            var blue2 = GetRating(alliances.First().Teams.Last().Team);

            int red1 = GetRating(alliances.Last().Teams.First().Team);
            int red2 = GetRating(alliances.Last().Teams.Last().Team);


            return engine.ExpectationToWin(blue1 + blue2, red1 + red2);
        }
        public int EloBlue(ICollection<Alliance> alliances)
        {
            var blue1 = GetRating(alliances.First().Teams.First().Team);
            var blue2 = GetRating(alliances.First().Teams.Last().Team);
            return blue1 + blue2;
        }
        public int EloRed(ICollection<Alliance> alliances)
        {
            int red1 = GetRating(alliances.Last().Teams.First().Team);
            int red2 = GetRating(alliances.Last().Teams.Last().Team);
            return red1 + red2;
        }
        public string WinLabelClass(ICollection<Alliance> alliances)
        {
            var result = WinLabel(alliances).ToLower();
            var expected = "";
            int blueElo = EloBlue(alliances);
            int redElo = EloRed(alliances);
            if (blueElo > redElo)
            {
                expected = "blue";
            }
            else if (redElo > blueElo)
            {
                expected = "red";
            }
            else
            {
                expected = "tie";
            }
            if (result == expected)
            {
                return result;
            }
            return $"{result} border-{expected}";
        }
        public string WinLabel(ICollection<Alliance> alliances)
        {
            int blueElo = EloBlue(alliances);
            int redElo = EloRed(alliances);
            var result = "Tie";
            if (blueElo > redElo)
            {
                result = "Blue";
            }
            else if (redElo > blueElo)
            {
                result = "Red";
            }
            
            result = $"{result}";
            return result;
        }
    }
}
