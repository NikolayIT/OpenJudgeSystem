$(function () {
    $(window).on('popstate', function () {
        location.reload(true);
    });
});

function OnPageChanged() {
    var currentPageValue = $('.pagination').find('span').first().text();

    var pageRegex = new RegExp(/page=(\d+)/);
    var currentPageHref = window.location.href;
    if (!currentPageHref.match(pageRegex)) {
        currentPageHref = currentPageHref + '?page=1';
    }

    var newPageQuery = currentPageHref.replace(pageRegex, function (pageQuery, pageNumber) {
        return pageQuery.replace(pageNumber, currentPageValue);
    });

    window.history.pushState(null, '', newPageQuery);
}