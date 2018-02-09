$(function () {
    $("#DataGrid").hide();
});

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
    }, 150);
}