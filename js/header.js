function getNavbar(league, season, division) {
  var html = '<div class="container-fluid">'
    + '<button class="btn navbar-brand px-0 mx-0 border-0" onclick="loadHome()">Home</button>';
  + '<svg xmlns="http://www.w3.org/2000/svg" width="30" height="30" fill="currentColor" class="bi bi-chevron-compact-right" viewBox="0 0 16 16">'
    + '<path fill-rule="evenodd" d="M6.776 1.553a.5.5 0 0 1 .671.223l3 6a.5.5 0 0 1 0 .448l-3 6a.5.5 0 1 1-.894-.448L9.44 8 6.553 2.224a.5.5 0 0 1 .223-.671z" />'
    + '</svg>'
    + '<button id="league-name" class="btn navbar-brand px-0 mx-0 border-0">League</button>'
    + '<button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">'
    + '<span class="navbar-toggler-icon"></span>'
    + '</button>'
    + '<div class="collapse navbar-collapse" id="navbarCollapse">'
    + '<ul class="navbar-nav me-auto mb-2 mb-md-0">'
    + '<li class="nav-item">'
    + '<button class="btn nav-link border-0">Summaries</button>'
    + '</li>'
    + '<li class="nav-item">'
    + '<button class="btn nav-link border-0">Players</button>'
    + '</li>'
    + '<li class="nav-item dropdown">'
    + '<button class="btn nav-link dropdown-toggle border-0" data-bs-toggle="dropdown" aria-expanded="false">Seasons</button>'
    + '<ul class="dropdown-menu dropdown-menu-dark">'
    + '<li><button class="btn dropdown-item border-0" href="#">Season 3</button></li>'
    + '<li><button class="btn dropdown-item border-0" href="#">Season 2</button></li>'
    + '<li><button class="btn dropdown-item border-0" href="#">Season 1</button></li>'
    + '</ul>'
    + '</li>'
    + '</ul>'
    + '</div>'
    + '</div>'
  if (league) {
    
    if (season) {

    }
  }
  return html;
}