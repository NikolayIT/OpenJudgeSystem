function getTypeName(type) {
    switch (type) {
        case 0: return "Default"; break;
        case 1: return "Drop-down list"; break;
        case 2: return "Single line text"; break;
        case 3: return "Multi-line text"; break;
    }
}

$(document).ready(function () {
    $('table').on('click', "#delete-all-answers", function (e) {
        e.preventDefault();
        var confirmation = confirm("Are you sure you want to delete all answers?");
        if (confirmation) {
            var questionId = $(e.target).attr('data-question-id');

            $.get('/Administration/ContestQuestionAnswers/DeleteAllAnswers/' + questionId, function () {
                var grid = $('#DetailAnswerGrid_' + questionId).data("kendoGrid");
                grid.dataSource.read();
                grid.refresh();
            })
        }
    })
})