var rowNumber = 0;

function renderNumber(data) {
    if (data._events.change.length <= 1) {
        return ++rowNumber;
    } else {
        return $('#' + data.Id).text();
    }
}

function onDataBound() {
    rowNumber = 0;
}

function onSearchSelect(e) {
    var contestId = this.dataItem(e.item.index()).Id;
    populateDropDowns(contestId);
    filterByContest(contestId);
}

function onContestSelect() {
    var contestId = $('#contests').val();
    $('#search').data('kendoComboBox').value(null);
    filterByContest(contestId);
}

function onDataBinding(e) {
    var page = e.sender.dataSource.page();
    var pageSize = e.sender.dataSource.pageSize();
    rowNumber = (page - 1) * pageSize;
}

function fillContestData() {
    $('.k-edit-form-container').css('width', '100%');
    $('.editor-label').css('width', '10%');
    $('.editor-field').css('width', '80%');

    var searchComboBox = $("#search").data("kendoComboBox");
    var contestsDropDown = $("#contests").data("kendoDropDownList");

    var contestId = searchComboBox.value() || contestsDropDown.value();
    var contestName = searchComboBox.text() || contestsDropDown.text();

    $('#ContestId').val(contestId).change();
    $('#ContestName').val(contestName).change();
}

function filterByContest(contestId) {
    var grid = $("#DataGrid");

    grid.data("kendoGrid").dataSource.filter({
        "field": "ContestId",
        "operator": "eq",
        "value": contestId
    });

    setTimeout(function () {
        grid.show();
    }, 200);
}

$(function () {
    var contestId = $("#search").data("kendoComboBox").value() ||
        $("#contests").data("kendoDropDownList").value() ||
        $('#contestId-input').val();

    if (contestId) {
        populateDropDowns(contestId);
        filterByContest(contestId);
    } else {
        $("#DataGrid").hide();
    }
});