function addTooltips() {
    $('[data-toggle="tooltip"]').tooltip({
        trigger: 'hover'
    });
    $('[data-toggle="tooltip"]').on('mouseleave', function () {
        $(this).tooltip('hide');
    });
    $('[data-toggle="tooltip"]').on('click', function () {
        $(this).tooltip('dispose');
    });
}

function copyClipboard(data) {
    navigator.clipboard.writeText(data);
}

function scrollToTop() {
    document.documentElement.scrollTop = 0;
}

function openInNewTab(url) {
    window.open(url, '_blank').focus();
}