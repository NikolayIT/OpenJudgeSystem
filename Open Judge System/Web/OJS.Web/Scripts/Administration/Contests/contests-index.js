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
