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

var bulkAddUsersManager = new BulkAddUsersManager();

function BulkAddUsersManager() {
    var examGroupId;
    var popUpWindowSelector;
    var popUpWindow;

    function init(id) {
        examGroupId = id;
        popUpWindowSelector = $('#BulkAddUsersPopUpWindow_' + examGroupId);
        popUpWindow = popUpWindowSelector.data('kendoWindow');

        popUpWindowSelector.children('form').submit(function () {
            if ($(this).find("textarea").val() === '') {
                return false;
            }

            $('#bulk-add-users-loading-mask').show();
        });

        popUpWindow.open().center();

        $('.kendo-window-cancel-btn').click(function () {
            popUpWindow.close();
        });
    }

    function onBulkAddUsersSuccess(response, status) {
        popUpWindow.close().refresh();
        displayAlertMessage(response, status, $('.main-container'));
        $('#UsersInExamGroup_' + examGroupId).data('kendoGrid').dataSource.fetch();
    }

    function onBulkAddUsersFailure(response, status) {
        $('#bulk-add-users-loading-mask').hide();
        displayAlertMessage(response.responseJSON.errorMessages, status, $(this).parent());
    }

    return {
        init: init,
        onFormSuccess: onBulkAddUsersSuccess,
        onFormFailure: onBulkAddUsersFailure
    }
}