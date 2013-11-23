var displayTestRuns = function (testRuns) {
    "use strict";

    var result = "";
    var i;

    for (i = 0; i < testRuns.length; i++) {
        if (testRuns[i].IsTrialTest) {
            continue;
        }

        switch (testRuns[i].ExecutionResult) {
            case 0:
                result += '<span class="glyphicon glyphicon-ok text-success" title="Правилен отговор"></span>';
                break;
            case 1:
                result += '<span class="glyphicon glyphicon-remove text-danger" title="Грешен отговор"></span>';
                break;
            case 2:
                result += '<span class="glyphicon glyphicon-time" title="Лимит време"></span>';
                break;
            case 3:
                result += '<span class="glyphicon glyphicon-hdd text-danger" title="Лимит памет"></span>';
                break;
            case 4:
                result += '<span class="glyphicon glyphicon-asterisk text-danger" title="Грешка при изпълнение"></span>';
                break;
        }
    }

    result += " ";
    return result;
};