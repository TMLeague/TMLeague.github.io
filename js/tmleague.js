
$(function () {
  var params = getQueryParameters();
  if (!params.league) {
    loadHome();
    return;
  }
  if (!params.season) {
    loadLeague(params.league);
    return;
  }
  if (!params.division) {
    loadSeason(params.league, params.season);
    return;
  }
  loadDivision(params.league, params.season, params.division);
});

function getQueryParameters() {
  var parameters = {}, kv;
  var queryKvs = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
  for (var i = 0; i < queryKvs.length; i++) {
    kv = queryKvs[i].split('=');
    parameters[kv[0]] = kv[1];
  }
  return parameters;
}

function getDataUrl(league, season, division) {
  if (!season) {
    return 'leagues/' + league + "/" + league + ".json";
  }
  if (!division) {
    return 'leagues/' + league + "/seasons/" + season + "/" + season + ".json";
  }
  return 'leagues/' + league + "/seasons/" + season + "/divisions/" + division + ".json";
}

function loadHome() {
  $('main').load("home.html");
}

function loadLeague(league) {
  $('main').load("league.html", function () {
    $.getJSON(getDataUrl(league), function (data) {
      $('#league-name').html(data.name);
      $('#league-description').html(data.description);
      if (data.rules)
        $('#league-rules-link').attr("href", data.rules);
      else
        $('#league-rules').addClass("d-none");
      if (data.discord)
        $('#league-discord-link').attr("href", data.discord);
      else
        $('#league-discord').addClass("d-none");
      console.log(data);
    });
  });
}

function loadDivision(league, season, division) {
  $.getJSON(getDataUrl(league, season, division), function (data) {
    var html = '<h2>The Silent League of Faceless Men</h2>';
    html += '<table class="my-auto table table-striped table-bordered table-dark">'
      + '<thead>'
      + '<tr>'
      + '<th scope="col"></th>'
      + '<th scope="col">Points</th>'
      + '<th scope="col">Wins</th>'
      + '<th scope="col">Stark</th>'
      + '<th scope="col">Greyjoy</th>'
      + '<th scope="col">Lannister</th>'
      + '<th scope="col">Tyrell</th>'
      + '<th scope="col">Martell</th>'
      + '<th scope="col">Baratheon</th>'
      + '<th scope="col">Penalties</th>'
      + '<th scope="col">CLA</th>'
      + '<th scope="col">Supply</th>'
      + '</tr>'
      + '</thead>'
      + '<tbody>'
    for (var i = 0; i < data.results.length; i++) {
      html += '<tr>'
        + '<th scope="row">' + data.results[i].name + '</th>'
        + '<td>' + data.results[i].points + '</td>'
        + '<td>' + data.results[i].wins + '</td>'
        + '<td>' + data.results[i].stark + '</td>'
        + '<td>' + data.results[i].greyjoy + '</td>'
        + '<td>' + data.results[i].lannister + '</td>'
        + '<td>' + data.results[i].tyrell + '</td>'
        + '<td>' + data.results[i].martell + '</td>'
        + '<td>' + data.results[i].baratheon + '</td>'
        + '<td>' + data.results[i].penalties + '</td>'
        + '<td>' + data.results[i].cla + '</td>'
        + '<td>' + data.results[i].supply + '</td>'
        + '</tr>'
    }
    html += '</tbody>'
    html += '</table>';
    // Set all content
    $('main').html(html);
  });
}