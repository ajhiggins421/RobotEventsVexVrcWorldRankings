VEX VRC Competition World Rankings - EloEngine Data Seeder
==================================

This project contains the logic to pull events, teams and matches from directly from the REC Foundations [Public Robot Events API](https://www.robotevents.com/api/v2). 

A live demo is available at [vexvrcworldrankings.azurewebsites.net](https://vexvrcworldrankings.azurewebsites.net/).

To seed the data, create a text file `C:\invisettings\vex-api-bear-token.txt` containing your Vex API bearer token, eg `bearer xxxx`, and run `eloengine.exe` after build the <a href='eloengine'>EloEngine</a> project.

The program will create JSON files creating the vex data and run all matches from non-practice events and non-practice matches creating the resulting teams, events and match data needed for the website.

