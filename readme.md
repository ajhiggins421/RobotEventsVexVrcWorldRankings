VEX VRC Competition World Rankings
==================================

This website contains world rankings for teams participating int 2022-2023 VEX VRC Robotics Spin-Up competition.

Teams are ranked based on their performance in tournaments throughout the year using a hybrid variation of ELO rating algorithm.

The math behind the ELO rating system is explained in detail [here](https://www.omnicalculator.com/sports/elo).

Code implementations of the ELO rating system in various languages can be found [here](https://www.geeksforgeeks.org/elo-rating-algorithm/).

While ELO is traditionally used for team verse team or player verse player competition, the implementation here uses the combined ELO of each team on a VEX alliance to form a single ELO for each alliance .

The combined ELO is they used to predict each alliances proability of winning the match.

Then an elo rating change is calculuated for the combined ELO of each alliance based on their performance.

Finally, the ELO for each team on the alliance is updated based on their percentage of the the combined ELO.

The data for teams and matches is pulled directly from the REC Foundations [Public Robot Events API](https://www.robotevents.com/api/v2). 

To seed the data, create a text file `C:\invisettings\vex-api-bear-token.txt` containing your Vex API bearer token, eg `bearer xxxx`, and run `eloengine.exe` after build the <a href='eloengine'>EloEngine</a> project.

This application was developed by members of Cherry Hill East High School robotics team in NJ, is open source and available on [AJ Higgins Github](https://github.com/ajhiggins421).

If you are interested in taking control and maintaing the project after this year please let us know. AJ is a senior and will likely not be continuing development, maintanence or hosting of this code after this year.

World Rankings
--------------

The [World Rankings](/rankings) page shows the latest world ranking for VEX VRC Robotics Competition teams. You can search for teams by Team Number, Name or Location.

![](../blob/master/EloWeb/wwwroot/images/world-rankings.png?raw=true "VEX VRC Robotics Team World Rankings")

Events
------

The [events](/events) page shows a list of events for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season.

![](../blob/master/EloWeb/wwwroot/images/events.png?raw=true "Events for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season")

You can browse through the events or search for events by name or sku.

![](../blob/master/EloWeb/wwwroot/images/wave-at-wpi-event.png?raw=true "Event Teams for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season")

Event Details
-------------

Clicking on an event to view the event details which displays a list of teams for the event along with a list of matches for the event.

### Event Details - Teams

On the top of the event details page is a searchable list of teams for the event, their ranking in the event, world ranking, elo and more.

![](../blob/master/EloWeb/wwwroot/images/wave-at-wpi-teams.png?raw=true "Event Details for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season")

### Event Details - Matches

Following the teams is a searchable list of matches listing the color-coded alliance for each match, combined team and invidiual elo for the alliance, the probability each team would win, and the result.

The corresponding blue win probality column or red win probality column is highlighted with it's respective color if that alliance was expected to win based on the alliance's combinded elo.

The result column is bordered with the color of the alliance that was expected to win and highlighted with the team that actually won. This makes upset victories stick out, that is when a lower rated team defeated a higher rated team.

When matches are first scheduled the result of the match will be a tie until the match is completed, but the win probablities of the match will still be visible.

![](../blob/master/EloWeb/wwwroot/images/wave-at-wpi-event-matches.png?raw=true "Event Matches for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season")

Match Caculator
---------------

The match caclulator allows you to enter two team codes for the blue alliance and two team codes for the red alliance and calculates the probality of each team winning in a match.

After entering the teams for each alliance, the teams will be displayed along with their rankings and records in the Teams table and the match with probablity will be displayed in the matches table.

![](../blob/master/EloWeb/wwwroot/images/vex-vrc-robotics-match-calculator.png?raw=true "Event Matches for the 2022-2023 VEX VRC Robotics Competition Spin-Up Season")