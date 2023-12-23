# A guide for league judges at the end of a season

## Contribution

To change league data files you need to contribute a project with your GitHub account. If you don't have one, let's create it [here](https://github.com/signup).
Then, send your email/GitHub login to Imrihil so he can add you as a collaborator of the project.

## End season procedure

To add penalties, replacements and mark division as finished (to show its winner in the league main page), the division file needs to be updated. For example [this](/src/TMLeague/wwwroot/data/leagues/sl/seasons/s10/divisions/q.json). Data file is stored as [JSON](https://www.json.org/) and looks like below:
```
{
  "name": "",
  "judge": "",
  "players": [],
  "games": [],
  "penalties": [],
  "replacements": [],
  "isFinished": false,
  "winnerTitle": "",
  "promotions": 3,
  "relegations": 3
}
```
During modification, it's essential to close all brackets and separate items in arrays with a comma `,`, but without a comma `,` after the last item. In case of making an error in the JSON file, division results wouldn't be displayed in a web page until errors are fixed.

At the end of a season a judge needs to fill `penalties`, `replacements`, `isFinished` and for D1 also `winnerTitle` properties.

To modify a file, if you are a contributor of a project, just click an edit button in a browser:  
![Edit file](/docs/img/edit.png)

### Penalties

Penalties is an array of objects `[ ... ]`, so all items needs to be surrounded by brackets `{ ... }` and separated by comma `,` like this: `[ { ... }, { ... } ]`. A single penalty item looks like below:
```
{
  "player": "Figueira",
  "points": 10,
  "game": 296311,
  "details": "KM"
}
```
A `player` must be a player name exactly the same like in `players` array (must be surrounded by quotes: `"..."`).  
A `points` must be a number.  
A `game` must be an integer id of a game existing in `games` array.  
A `details` is a string description shown in a tooltip in the division table (must be surrounded by quotes: `"..."`).  
![penalty tooltip](/docs/img/penalty-tooltip.png)

### Replacements

Replacements are quite similar to penalties, but an object looks differently:
```
{
  "from": "Figueira",
  "to": "damrej",
  "game": 296391
}
```
A `from` is a name of player that is replaced. Must be the same like in `players` array.  
A `to` is a name of replacing player (it's case and white space sensitive so the name must be exactly the same like in the Thronemaster page).  
A `game` must be an integer id of a game existing in `games` array.

### Finishing
When all games are finished and all penalties and replacements are entered to the file, property `isFinished` should be changed to `true`:  
```
"isFinished": true,
```

> [!CAUTION]
> If you mark division as finished in the same pull request to master in which you added any penalties, replacements or winner title, you need to run an "import game" action that update division results despite the fact its finished (normally finished divisions' results are not updated anymore).

### Winner Title
For the D1 also a winner title can be set in the `winnerTitle` property:
```
"winnerTitle": "Puppet Master"
```

## Commit your changes and create a pull request
When all changes are applied, it needs to be saved. Because it's easy to make a mistake updating a file (to forget a comma or closing bracket) and also sometimes some code development is done in the same repository, it's forbidden to change files directly in a master branch. A good practice is to ask someone else for the review of your changes before merging them to master unless you are sure that you made no mistake.
All changes in a master branch are always published after a short time by the configured github action.

To save your changes you need to:
1. commit them to a new branch and then  
   
   ![commit changes](/docs/img/commit-changes.png)  
   ![propose changes](/docs/img/propose-changes.png)  
2. create a pull request to a master,
     
   ![create a pull request](img/create-pull-request.png)  
3. (optional) import games,  
   
   ![import games](img/import-games.png)  
4. (optional) ask someone else for a review of your changes  
   
   ![review a pull request](img/review-pull-request.png)  
5. merge a pull request.  
   
   ![merge a pull request](/docs/img/merge-pull-request.png)  