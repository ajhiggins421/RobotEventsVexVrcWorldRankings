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
    public partial class EventDetails
    {
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
            if (Events == null)
            {
                await LoadEvents();

            }
        }

        async Task LoadEvents()
        {
            Events = eventsProvider.Events;
            if (Sku == null)
            {

            }
            else
            {
                var eventDict = eventsProvider.EventsDict;
                if (eventDict.ContainsKey(Sku))
                {
                    Event = eventDict[Sku];
                    if (Event != null)
                    {
                        Elements = eventsProvider.EventMatches[Sku].OrderBy(x => x.Scheduled).ToList();
                    }
                    //var teams = eventTeamsProvider.TeamsDict[Event.Id];
                    var teams = Elements.SelectMany(x => x.Alliances.SelectMany(a => a.Teams.Select(t => t.Team.Id))).Distinct().ToList();

                    var teamsList = new List<EloEngine.Vex.Api.Team>();
                    if (teams.Count == 0)
                    {
                        var eventTeams = eventTeamsProvider.TeamsDict[Event.Sku];
                        var eventTeamsDict = eventTeams.ToDictionary(x => x.Id, x => x);

                        foreach (var eventTeam in eventTeamsDict)
                        {
                            var team = teamsProvider.TeamsDict[eventTeam.Value.Id];
                            teamsList.Add(team);
                        }
                    }
                    else
                    {

                        foreach (var teamId in teams)
                        {
                            var team = teamsProvider.TeamsDict[teamId];
                            teamsList.Add(team);
                        }
                    }


                    this.Teams = teamsList.OrderBy(x => x.WorldRank).Select(x => new RankedTeam(x)
                    {
                        WorldRank = x.WorldRank,
                    }).ToList();
                    for (var i = 0; i < Teams.Count; i++)
                    {
                        Teams[i].Rank = i + 1;
                    }
                }


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
            var result = "Tie";
            if (alliances.First().Score > alliances.Last().Score)
            {
                result = "Blue";
            }
            else if (alliances.First().Score < alliances.Last().Score)
            {
                result = "Red";
            }
            result = $"{result}";
            return result;
        }
    }
}
