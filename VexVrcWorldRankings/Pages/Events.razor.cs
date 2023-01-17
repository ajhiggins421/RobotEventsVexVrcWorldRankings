using CsvHelper;
using EloEngine.Vex.Api;
using EloEngine.Vex.Endpoints;
using Newtonsoft.Json;
using System.Globalization;
using static MudBlazor.CategoryTypes;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using VexVrcWorldRankings.Services;

namespace VexVrcWorldRankings.Pages
{
    public partial class Events
    {
        public int[] pageSizeOptions = new[] { 10, 100, 1000 };
        public List<Event> Elements = null!;

        [Inject]
        IEventsProvider eventsProvider { get; set; } = null!;
        protected override async Task OnInitializedAsync()
        {
            if (Elements == null)
            {
                await LoadEvents();
            }
        }

        async Task LoadEvents()
        {
            //eventsProvider.Events
            //var eventsJsonFullPath = Path.Combine(DataDirectory, "events.json");
            //var eventJson = await File.ReadAllTextAsync(eventsJsonFullPath);
            // var events = JsonConvert.DeserializeObject<List<Event>>(eventJson) ?? new List<Event>();
            //Elements = events.Where(x => x.Ongoing == false && x.Awards_finalized == true).ToList();
            var events = eventsProvider.Events;
            Elements = events;

        }

        private bool dense = true;
        private bool hover = true;
        private bool striped = true;
        private bool bordered = true;
        private string searchString1 = "";
        private Event selectedItem1 = null;
        private HashSet<Event> selectedItems = new HashSet<Event>();





        private bool FilterFunc1(Event element) => FilterFunc(element, searchString1);

        private bool FilterFunc(Event element, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Sku.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
