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

function onEdit(e) {
    $('.k-edit-form-container').css('width', '100%');
    $('.editor-label').css('width', '15%');
    $('.editor-field').css('width', '75%');

    var searchComboBox = $("#search").data("kendoComboBox");
    var contestsDropDown = $("#contests").data("kendoDropDownList");

    var contestId = searchComboBox.value() || contestsDropDown.value();
    var contestName = searchComboBox.text() || contestsDropDown.text();

    $('#ContestId').val(contestId).change();
    $('#ContestName').val(contestName).change();


    //If id is missing (the action is Create), populate OrderBy field with the next value
    if (!e.model.id) {
        autoFillNextOrderBy();
    };

    function autoFillNextOrderBy() {
        var lastOrderBy = parseInt($('#DataGrid').find('tbody tr:last [data-order]').text());
        var newOrderBy = lastOrderBy + 1;

        var orderByInput = $('#OrderBy');
        var orderByKendoNumericTextBox = orderByInput.data('kendoNumericTextBox');

        orderByKendoNumericTextBox._text.val(newOrderBy);
        orderByKendoNumericTextBox.element.val(newOrderBy);

        setTimeout(function () {
            orderByInput.prev().trigger('focus');
        }, 180);
    }
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

function refreshGrid(e) {
    if(e.type !== "read" && !e.response.Errors){
        e.sender.read();
    }
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