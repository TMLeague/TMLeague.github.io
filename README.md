# Overview
This is a scoreboard for Thronemaster leagues.

# Page content
The application contains following pages:
* Home page with the league selection
* League home page presents current champion, all seasons champions, hall of fame and status of the current season.
* Summary page shows total achievements of all players (best, average or summed results). It can be queried by all games or by division.
* Seasons page contains links to all division results of all seasons of the league.
* Judge panel is a form to generate division's drafts for the new season.
* All players, games and divisions are hyperlinked to the Thronemaster or internal results pages.
* All results are automatically updated every day according to the cron schedule configured in [deploy.yml](.github/workflows/deploy.yml) (remember that the configuration is in UTC!).

# Contribution
To add new league data or update existing, files should be created and modified in a new branch and pull request to master must be created.
They will be publicly visible after an approval from the code owner.
If you have an idea for the page improvement of found an error, please do not hesitete to create a new issue on GitHub.

## Main
Main application configuration is in [home.json](src/TMLeague/wwwroot/data/home.json).
To add a new league it is necessary to:
* add a new identifier in `leagues` array
* add a new dictionary with name equal to the identifier to [leagues](/src/TMLeague/wwwroot/data/leagues) directory.
* add a new `.json` file with the name equal to the identifier e.g. [leagues/sl/sl.json](/src/TMLeague/wwwroot/data/leagues/sl/sl.json) for identifier `sl`.

## League
