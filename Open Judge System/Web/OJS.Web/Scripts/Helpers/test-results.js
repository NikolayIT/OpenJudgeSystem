var displayTestRuns = function (testRuns) {
    'use strict';

    var result = '';
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
            default:
                throw new Error('Invalid execution result');
        }
    }

    result += ' ';
    return result;
};

/* eslint max-params: 0 */
// TODO: Refactor - too many params
/* exported testResult */
function testResult(
    tests,
    points,
    problemMaximumPoints,
    maxUsedMemory,
    maxUsedTime,
    processed,
    isCompiledSuccessfully,
    submissionType) {
    'use strict';

    var result = '';

    /* eslint no-negated-condition: 0 */
    if (!processed) {
        result += '<span class="glyphicon glyphicon-time text-primary" title="Loading..."></span>';
        result += '<strong class="text-primary"> Обработва се...</strong>';
    } else if (!isCompiledSuccessfully) {
        result += '<span class="glyphicon glyphicon-remove text-danger" title="Compilation failed"></span>';
        result += '<strong class="text-danger"> Грешка при компилация</strong>' + '<small> | ' + submissionType + '</small>';
    } else {
        result += '<div><strong class="text-primary"> ' + points +
            ' / ' + problemMaximumPoints + '</strong>' +
            '<small> ' + (maxUsedMemory / 1024 / 1024).toFixed(2) + ' MB | ' +
            (maxUsedTime / 1000).toFixed(3) + ' sec. | ' + submissionType + '</small></div> ';
        result += displayTestRuns(tests);
    }

    return result;
}
