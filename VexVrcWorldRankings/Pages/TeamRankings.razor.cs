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
using Team = EloEngine.Vex.Api.Team;
using static MudBlazor.FilterOperator;

namespace VexVrcWorldRankings.Pages
{
    public partial class TeamRankings
    {
        private bool dense = true;
        private bool hover = true;
        private bool striped = true;
        private bool bordered = true;
        public int[] pageSizeOptions = new[] { 10, 100, 1000 };

        public List<Team> Teams = null!;




        [Inject]
        public ITeamsProvider teamsProvider { get; set; } = null!;






        private string searchStringTeams1 = "";
        private Team? selectedTeam = null;
        private HashSet<Team> selectedTeams = new HashSet<Team>();

        public string DataDirectory = DataProvider.DataDirectory;

        protected override async Task OnInitializedAsync()
        {
            if (Teams == null)
            {
                await LoadTeams();

            }
        }

        async Task LoadTeams()
        {

            Teams = teamsProvider.Teams.OrderBy(x=> x.WorldRank).ToList();

            //Teams = teams.Values.OrderByDescending(x=> int.Parse((x.AdditionalProperties["worldrank"].ToString() ?? "0"))).ToList();


            await Task.CompletedTask;
        }




        private bool FilterTeamFunc1(Team element) => FilterTeamFunc(element, searchStringTeams1);

        private bool FilterTeamFunc(Team element, string searchString)
        {

            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            searchString = searchString.ToLower();
            var x = element;
            var matched = x.Number.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1 ||
                x.Team_name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1 ||
                x.Organization.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1;
            //var matched = Teams.Any(x =>
            //    x.Number.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1 ||
            //    x.Team_name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1 ||
            //    x.Organization.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) > -1
            //    );

            return matched;
        }




    }
}
