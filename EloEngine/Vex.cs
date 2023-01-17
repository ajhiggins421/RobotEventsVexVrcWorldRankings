using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EloEngine.Vex
{

    namespace Endpoints
    {
        using static Constants;
        public class Constants
        {
            public const string RootUrl = "https://www.robotevents.com/api/v2/";
        }


        public class Events
        {
            //48071/teams?page=6'
            public static string Get => $"{RootUrl}/events";
            public static string GetEvent(int id)
                => $"{RootUrl}{Get}/{id}";
            public static string GetEventTeams(int id)
                => $"{RootUrl}{Get}/{id}/teams";
            public static string GetEventSkills(int id)
               => $"{RootUrl}{Get}/{id}/skills";
            public static string GetEventAwards(int id)
                => $"{RootUrl}{Get}/{id}/awards";
            public static string GetDivsionMatches(int id, int div)
                => $"{RootUrl}{Get}/{id}/divisions/{div}/matches";
            public static string GetDivsionFinalists(int id, int div)
                => $"{RootUrl}{Get}/{id}/divisions/{div}/finalistRankings";
            public static string GetDivsionRankings(int id, int div)
                => $"{RootUrl}{Get}/{id}/divisions/{div}/rankings";
        }

        public class Teams
        {
            public static string Get => $"{RootUrl}/teams";
            public static string GetTeam(string id)
                => $"{RootUrl}{Get}/{id}";
            public static string GetTeamEvents(string id)
                => $"{RootUrl}{Get}/{id}/events";
            public static string GetTeamMatches(string id)
                => $"{RootUrl}{Get}/{id}/matches";
            public static string GetTeamRankings(string id)
                => $"{RootUrl}{Get}/{id}/rankings";
            public static string GetTeamSkills(string id)
                => $"{RootUrl}{Get}/{id}/skills";
            public static string GetTeamAwards(string id)
                => $"{RootUrl}{Get}/{id}/awards";
        }
    }
    public class Client
    {
        private readonly HttpClient client;

        public Client()
        {
            this.client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", Authorization.BearerToken);
        }

        public async Task<string> GetTeams()
        {
            var result = await client.GetStringAsync(Endpoints.Events.Get);
            return result;
        }
    }

    namespace ModelsManual
    {

        public class Paginated<T>
        {
            public Meta Meta { get; set; }
            public T[] Data { get; set; }
        }

        public class Meta
        {
            public int current_page { get; set; }
            public string first_page_url { get; set; }
            public int from { get; set; }
            public int last_page { get; set; }
            public string last_page_url { get; set; }
            public string next_page_url { get; set; }
            public string path { get; set; }
            public int per_page { get; set; }
            public string prev_page_url { get; set; }
            public int to { get; set; }
            public int total { get; set; }

            public int code { get; set; }
            public string message { get; set; }
        }
        public class PaginatedProgram : Paginated<Program>
        {

        }

        public class PaginatedTeam : Paginated<Team>
        {

        }

        public class PaginatedEvent : Paginated<Event>
        {

        }
        public class PaginatedAward : Paginated<Award>
        {

        }

        public class Award
        {

        }

        public class Event
        {
            public int id { get; set; }
            public string sku { get; set; }
            public string name { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public Season season { get; set; }
            public Program program { get; set; }
            public Location location { get; set; }
            public Division[] divisions { get; set; }
            public string level { get; set; }
            public bool ongoing { get; set; }
            public bool awards_finalized { get; set; }
            public string event_type { get; set; }
        }

        public class Season
        {
            public int id { get; set; }
            public string name { get; set; }
            public string code { get; set; }
        }

        public class Program
        {
            public int id { get; set; }
            public string name { get; set; }
            public string code { get; set; }
        }

        public class Location
        {
            public string venue { get; set; }
            public string address_1 { get; set; }
            public string address_2 { get; set; }
            public string city { get; set; }
            public string region { get; set; }
            public string postcode { get; set; }
            public string country { get; set; }
            public Coordinates coordinates { get; set; }
        }

        public class Coordinates
        {
            public int lat { get; set; }
            public int lon { get; set; }
        }

        public class Division
        {
            public int id { get; set; }
            public string name { get; set; }
            public int order { get; set; }
        }



        public class Team
        {
            public int id { get; set; }
            public string number { get; set; }
            public string team_name { get; set; }
            public string robot_name { get; set; }
            public string organization { get; set; }
            public Location location { get; set; }
            public bool registered { get; set; }
            public Program program { get; set; }
            public string grade { get; set; }
        }






    }
}
