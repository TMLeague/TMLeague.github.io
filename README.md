# Overview
This is a scoreboard for Thronemaster leagues. It can contains many leagues that are running in the www.thronemaster.net page. Few times a day results results of unfinished divisions are updated.

# Page content
The application contains following pages:
* **Main page** with the league selection
* **League home page** presents current champion, all seasons champions, hall of fame and status of the current season.
* **Summary page** shows total achievements of all players (best, average or summed results). It can be queried by all games or by division.
* **Seasons page** contains links to all division results of all seasons of the league.
* **Judge panel** is a form to generate division's drafts for the new season.
* **All players**, games and divisions are hyperlinked to the Thronemaster or internal results pages.
* **All results** are automatically updated every day according to the cron schedule configured in [deploy.yml](.github/workflows/deploy.yml) (remember that the configuration is in UTC!).

# Contribution
To add new league data or update existing, files should be created and modified in a new branch and pull request to master must be created.
They will be publicly visible after an approval from the code owner.
If you have an idea for the page improvement of found an error, please do not hesitate to create a new issue on GitHub.

## Main
Application configuration files are `.json` files located in appropriate directories.
Main application configuration is in [home.json](src/TMLeague/wwwroot/data/home.json).
To add a new league it is necessary to:
* add a new `<identifier>` in `leagues` array
* add a new dictionary with name equal to the `<identifier>` to [leagues](/src/TMLeague/wwwroot/data/leagues) directory.
* add a new `.json` file with the name equal to the `<identifier>.json` e.g. [leagues/sl/sl.json](/src/TMLeague/wwwroot/data/leagues/sl/sl.json) for identifier `sl`.
* add a new image file in the league directory e.g. [leagues/sl/sl.png](/src/TMLeague/wwwroot/data/leagues/sl/sl.png). It can be used as button background in the **Main page**.

League has seasons. Seasons have divisions. General structure looks like:
```
/src/TMLeague/wwwroot/data/leagues
+---<leagueId>
|   |   <leagueId>.json
|   |   <leagueId>.png
|   \---seasons
|       +---s1
|       |   |   s1.json
|       |   \---divisions
|       |       |   d1.json
|       |       |   d2.json
|       |       \   d3.json
|       +---s2
|       |   |   s2.json
|       |   \---divisions
|       |       |   d1.json
|       |       |   d2.json
|       |       \   d3.json
...     ...
```

## League
League configuration file contains following properties. Required properties are marked with the star `*`. Other are optional. Required arrays can be empty (`[]`).

To add a new season to the league, its index must be added to `seasons` or `trainingSeasons` array.

| Name | Description |
|---|---|
| name * | A full name of the league. |
| description | A short description of the league displayed on the **League home page** |
| backgroundImage | A path to the image that should be used as a background of the league button on the **Main page**. |
| rules | A link to the league rules. |
| discord | A link to the league discord server. |
| judgeTitle | A judge special title fot the league if any. |
| seasons * | An array of ids of the league seasons in the application. |
| trainingSeasons * | An additional array of ids of the training league seasons in the application that are skipped in all summaries, hall of fame etc. |
| mainDivisions * | An array of objects containing ids, names, number of promotions and relegations of league main divisions. E.g.: `{ "id": "d1", "name": "Division 1", "promotions": 3, "relegations": 3 }`. `Promotions` and `relegations` can be `null`. |
| scoring | A league scoring rules. Described below in details. |
| scoring:pointsPerStronghold | Number of points per each stronghold owned at the end of a game. |
| scoring:pointsPerCastle | Number of points per each castle at the end of a game. |
| scoring:pointsPerWin | Number of points per each won game. |
| scoring:pointsPerCleanWin | Number of extra points per each won game with more castles than opponents. |
| scoring:pointsPer2ndPlace | Number of points per games finished on 2nd place. |
| scoring:pointsPer3rdPlace | Number of points per games finished on 3rd place. |
| scoring:requiredBattlesBefore10thTurn | Number of battles required before 10th turn (can be 0 to skip that scoring). |
| scoring:tiebreakers | An ordered array of tiebreakers. Allowed values are: `wins`, `penalties`, `cla`, `supplies`, `powerTokens`, `minutesPerMove` |
| initialMessage | An object of initial message used for draft generation. Details are described below. |
| initialMessage:subject | A subject of the message with draft that should be send to players at the beginning of the season. It can contain following phrases that will be replaced with appropriate values: `{season}`, `{division}`, `{stark}`, `{greyjoy}`, `{lannister}`, `{tyrell}`, `{martell}`, `{baratheon}`, `{arryn}`, `{password}`, `{contact}`, `{judgeName}`. |
| initialMessage:body | An array of body lines format of the message with draft. It can contain the same phrases like the subject. |

## Season
Season configuration file contains following properties.

To add a new division to the league, its index must be added to `divisions` array.

| Name | Description |
|---|---|
| name * | A full name of the season. |
| startDate | A start date of the season in ISO 8601 format (e.g `"2023-02-23T17:44:02Z"`) |
| endDate | A deadline of the season in ISO 8601 format. |
| divisions * | An array of divisions identifiers. |

## Division
Before the season start, many division arrays is empty. They can be set later.

**It's very important to mark finished seasons to avoid generating unnecessary traffic to the Thronemaster's API.**

**It's also necessary to use correct data of Thronemaster's player names (also letter cases) and games IDs.**

Division configuration file contains following properties.

| Name | Description |
|---|---|
| name * | A full name of the division. |
| judge * | A Thronemaster's name of the division judge. |
| players * | An array of Thronemaster's names of players. |
| games * | An array of Thronemaster's ids of the division games. |
| penalties | An array of penalties. Penalty object is described in details below. |
| penalties[*]:player * | A Thronemaste's name of a player. |
| penalties[*]:game | A Thronemaster's game id. |
| penalties[*]:points * | A number of penalty points. |
| penalties[*]:details * | A description of a penalty shown in the tooltip in results table. |
| replacements | An array of player replacements. Replacement object is described in details below. |
| replacements[*]:from | A Thronemaster's name of a player that is replaced. |
| replacements[*]:to | A Thronemaster's name of a player that is replacing. |
| replacements[*]:game | A Thronemaster's game id. |
| isFinished * | If `true`, then every time when results are fetched from the Thronemasters, all games from that division will be updated (few times a day). Set to `false`, when the division is correctly fetched at least once and no more recalculation is needed. |
| winnerTitle | A winner title. |
| promotions | Number of players promoted to higher division. If `null`, value is taken from the league configuration. |
| relegations | Number of players relegated to lower division. If `null`, value is taken from the league configuration. |

# Importing games locally
To import games locally, it is needed to have installed [.NET6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). Then it is possible to execute following command from the root folder:
```
dotnet run --project src/TMGameImporter/TMGameImporter.csproj -c Release --BaseLocation src/TMLeague/wwwroot/data --FetchFinishedDivisions false --FetchFinishedGames false
```
where there are possible following parameters:
- `FetchFinishedDivisions` should be set to `true` when already fetched division results needs to be recalculated and
- `FetchFinishedGames` should be set to `true` when already fetched and finished games need to be recalculated,
- `League` - id of the league to import (when `null` or empty, all leagues are imported)
- `Season` - id of the season to import (when `null` or empty, all seasons are imported)
- `Division` - id of the division to import (when `null` or empty, all division are imported)
- `Games` - ids of games to import (when contains any ids other parameters are ignored)