function onEdit() {
    $('.k-edit-form-container').css('width', '100%');
    $('.editor-label').css('width', '10%');
    $('.editor-field').css('width', '80%');

    $('#ContestId').change(function () {
        $('#ContestName').val($('#ContestId_option_selected').text()).change();
    });
}

function onDetailDataBound() {
    var detailsGrid = $('#' + this.element.attr("id"));
    var isAddButtonDisabled = detailsGrid.find('.k-grid-add.k-state-disabled').length >= 1;
    if (isAddButtonDisabled) {
        var deleteButtons = $('#' + this.element.attr("id")).find('.k-button.k-grid-delete');
        disableKendoButtons(deleteButtons);
    }
}

function OpenBulkAddUsersWindow(id, name) {
    var popUpWindowSelector = $("#BulkAddUsersPopUpWindow");
    var popUpWindow = popUpWindowSelector.data("kendoWindow");
    var url = '/Administration/ExamGroups/BulkAddUsersPartial/' + id + '?name=' + name;

    popUpWindow.refresh(url);

    popUpWindow.bind('refresh', function() {
        popUpWindow.center();
        popUpWindow.open();

        popUpWindowSelector.find('form').one('submit', function () {
            disableKendoButtons($(this).find('input[type="submit"]'));
            $('#bulk-add-users-loading-mask').show();
        });
    });
}

function onBulkAddUsersSuccess(response, status) {
    $('#BulkAddUsersPopUpWindow').data('kendoWindow').close();
    displayAlertMessage(response, status, $('.main-container'));
}

function onBulkAddUsersFailure(response, status) {
    displayAlertMessage(response.responseJSON.errorMessages, status, $(this).parent());
}