/* exported getTypeName */
function getTypeName(type) {
    'use strict';

    switch (type) {
        case 0:
            return 'Default';
        case 1:
            return 'Drop-down list';
        case 2:
            return 'Single line text';
        case 3:
            return 'Multi-line text';
        default:
            throw new Error('Invalid type.');
    }
}

/* exported copyFromContest */
function copyFromContest(ev, contestId) {
    'use strict';

    ev.preventDefault();
    var copyWindow = $('#CopyFromContestWindow_' + contestId);
    copyWindow.data('kendoWindow').open().center();

    var form = copyWindow.children('form');

    form.one('submit', function () {
        $.post('/Administration/ContestQuestions/CopyTo/' + contestId, form.serialize(), function () {
            copyWindow.data('kendoWindow').close();
            var grid = $('#DetailQuestionsGrid_' + contestId).data('kendoGrid');
            grid.dataSource.read();
            grid.refresh();
        });

        return false;
    });
}

function onDataBound() {
    CreateExportToExcelButton();
}

function resetFormValues() {
    document.getElementById('lab-start-form').reset();
}

function showErrorMessage(errorData) {
    var errorObject = JSON.parse(errorData.responseText);
    if (errorObject.Message) {
        $('span[data-valmsg-for=Duration]')
            .removeClass('field-validation-valid')
            .addClass('field-validation-error')
            .html(errorObject.Message);
    } else {
        alert('Възникна неочаквана грешка');
    }
}

function showDownloadSubmissionsPopup(ev) {
    ev.preventDefault();

    clearDownloadContestSubmissionsForm();

    var contestId = this.dataItem($(ev.currentTarget).closest('tr')).get('Id');
    $('#download-contest-submissions-form #contest-id').val(contestId);

    var downloadContestSubmissionsWindow = $('#download-contest-submissions-popup').kendoWindow({
        height: 'auto',
        width: '30%',
        title: 'Изтегляне на решения',
        visible: false,
        modal: 'true',
        deactivate: clearDownloadContestSubmissionsForm
    }).data('kendoWindow');

    downloadContestSubmissionsWindow.center();
    downloadContestSubmissionsWindow.open();
}

function clearDownloadContestSubmissionsForm() {
    document.getElementById('download-contest-submissions-form').reset();
}

$(document).ready(function () {
    'use strict';

    $('table').on('click',
        '#delete-all-answers',
        function (e) {
            e.preventDefault();
            var confirmation = confirm('Are you sure you want to delete all answers?');
            if (confirmation) {
                var questionId = $(e.target).attr('data-question-id');

                $.get('/Administration/ContestQuestionAnswers/DeleteAllAnswers/' + questionId,
                    function () {
                        var grid = $('#DetailAnswerGrid_' + questionId).data('kendoGrid');
                        grid.dataSource.read();
                        grid.refresh();
                    });
            }
        });
});
