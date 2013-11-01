$(document).ready(function () {
    kendo.culture("en-GB");
});

function CreateExportToExcelButton(elementId) {
    elementId = typeof elementId !== 'undefined' ? elementId : "DataGrid";
    CreateKendoSubmitParamsButton("export", elementId);
}

function CreateKendoSubmitParamsButton(buttonId, elementId) {
    elementId = typeof elementId !== 'undefined' ? elementId : "DataGrid";
    var grid = $('#' + elementId).data('kendoGrid');
    // ask the parameterMap to create the request object for you
    var requestObject = (new kendo.data.transports["aspnetmvc-server"]({ prefix: "" }))
    .options.parameterMap({
        page: grid.dataSource.page(),
        sort: grid.dataSource.sort(),
        filter: grid.dataSource.filter()
    });

    // Get the export link as jQuery object
    var $exportLink = $('#' + buttonId);

    // Get its 'href' attribute - the URL where it would navigate to
    var href = $exportLink.attr('href');

    // Update the 'page' parameter with the grid's current page
    href = href.replace(/page=([^&]*)/, 'page=' + requestObject.page || '~');

    // Update the 'sort' parameter with the grid's current sort descriptor
    href = href.replace(/sort=([^&]*)/, 'sort=' + requestObject.sort || '~');

    // Update the 'pageSize' parameter with the grid's current pageSize
    href = href.replace(/pageSize=([^&]*)/, 'pageSize=' + grid.dataSource._pageSize);

    //update filter descriptor with the filters applied

    href = href.replace(/filter=([^&]*)/, 'filter=' + (requestObject.filter || '~'));

    // Update the 'href' attribute
    $exportLink.attr('href', href);
}