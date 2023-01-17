using EloEngine;
using VexVrcWorldRankings.Data;
using VexVrcWorldRankings.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;

namespace VexVrcWorldRankings
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
        
            builder.Services.AddSingleton<IEventsProvider, EventsProvider>();
          

            builder.Services.AddSingleton<ITeamsProvider, TeamsProvider>();
      
            builder.Services.AddSingleton<IEventTeamsProvider, EventTeamsProvider>();
            builder.Services.AddSingleton<DataProvider>();
            

            //var compTeams = EventTeamsProvider.TeamsDictCache.SelectMany(x => x.Value.Select(t=> t.Id)).Distinct().ToList();
            //var compHsTeams= EventTeamsProvider.TeamsDictCache.SelectMany(x => x.Value.Where(t=> t.Grade == EloEngine.Vex.Api.Grade.High_School)
            //    .Select(t => t.Id)).Distinct().ToList();
            //var compMsTeams = EventTeamsProvider.TeamsDictCache.SelectMany(x => x.Value.Where(t => t.Grade == EloEngine.Vex.Api.Grade.Middle_School)
            //    .Select(t => t.Id)).Distinct().ToList();

            builder.Services.AddSingleton<Elo>();
            builder.Services.AddMudServices();
            var app = builder.Build();

            DataProvider dp = app.Services.GetRequiredService<DataProvider>();
            EventsProvider.LoadCache();
            TeamsProvider.LoadCache();
            EventTeamsProvider.LoadCache();
           
           

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}