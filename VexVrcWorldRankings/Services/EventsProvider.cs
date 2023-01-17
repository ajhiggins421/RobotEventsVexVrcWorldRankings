using EloEngine.Vex.Api;
using Newtonsoft.Json;

namespace VexVrcWorldRankings.Services
{
    public interface IEventsProvider
    {
        Dictionary<string, List<MatchObj>> EventMatches { get; }
        List<Event> Events { get; }
        Dictionary<string, Event> EventsDict { get; }
    }

    public class DataProvider
    {
        public static string DataDirectory = null!;
        static DataProvider()
        {
     
            DataDirectory = Path.GetFullPath("data");
        }
        public DataProvider(IWebHostEnvironment env)
        {
            var fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var di = fi.Directory;
            var directoryPath = di.FullName;

            DataDirectory = Path.Combine(directoryPath, "data");
        }
    }
    public class EventsProvider : IEventsProvider
    {
        public static string DataDirectory = DataProvider.DataDirectory;
        public static void LoadCache()
        {
            var appDirectory = Path.GetFullPath(".");
            var eventsJsonFullPath = Path.Combine(DataDirectory, "events.json");
            var eventJson = File.ReadAllText(eventsJsonFullPath);
            var events = JsonConvert.DeserializeObject<Dictionary<string, Event>>(eventJson) ?? new();
            EventsCache = events.Select(x=> x.Value).ToList();
            EventsDictCache = events;

            var matchesJsonFullPath = Path.Combine(DataDirectory, "eventMatches.json");
            var matchesJson = File.ReadAllText(matchesJsonFullPath);
            EventMatchesCache = JsonConvert.DeserializeObject<Dictionary<string, List<MatchObj>>>(matchesJson) ?? new();
        }
        public static List<Event> EventsCache = null!;
        public static Dictionary<string, Event> EventsDictCache = null!;
        public static Dictionary<string, List<MatchObj>> EventMatchesCache  = null!;
        public List<Event> Events { get => EventsCache; } 
        public Dictionary<string, List<MatchObj>> EventMatches { get => EventMatchesCache; }
        public Dictionary<string, Event> EventsDict { get => EventsDictCache; }
    }
}
