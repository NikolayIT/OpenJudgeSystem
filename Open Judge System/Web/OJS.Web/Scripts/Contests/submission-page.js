var countdownTimer = function (endingTime) {
    "use strict";

    var endTime = new Date(
        endingTime.year,
        endingTime.month,
        endingTime.day,
        endingTime.hour,
        endingTime.minute,
        endingTime.second
    );

    var ts = countdown(null, endTime, countdown.HOURS | countdown.MINUTES | countdown.SECONDS);

    var hoursContainer = $('#hours-remaining');
    var minutesContainer = $('#minutes-remaining');
    var secondsContainer = $('#seconds-remaining');
    var countdownTimer = $('#countdown-timer');

    var start = function () {
        updateCountdown();

        var timerId = window.setInterval(function () {
            if (ts.hours === 0 && ts.minutes <= 4) {
                if (!countdownTimer.hasClass('countdown-warning')) {
                    countdownTimer.addClass('countdown-warning');
                }

                if (ts.start > ts.end) {
                    window.clearInterval(timerId);
                    // TODO: Handle contest over.
                }
            }

            updateCountdown();
            ts = countdown(null, endTime, countdown.HOURS | countdown.MINUTES | countdown.SECONDS);
        }, 1000);
    }

    function updateCountdown() {
        hoursContainer.text(ts.hours);
        minutesContainer.text(ts.minutes);
        secondsContainer.text(ts.seconds);
        countdownTimer.show();
    }

    return {
        start: start
    }
}

function createCodeMirrorForTextBox() {
    var element = $('.code-for-problem:visible')[0];

    if (!$(element).data('CodeMirrorInstance')) {
        var editor = new CodeMirror.fromTextArea(element, {
            lineNumbers: true,
            matchBrackets: true,
            mode: "text/x-csharp",
            theme: "the-matrix",
            showCursorWhenSelecting: true,
            undoDepth: 100,
            lineWrapping: true,
        });

        $.data(element, 'CodeMirrorInstance', editor);
    }
};

function notifySuccess(response) {
    var codeMirrorInstance = getCodeMirrorInstance();
    codeMirrorInstance.setValue("");

    showMessage({
        message: "Успешно изпратено!",
        response: response,
        cssClass: "alert alert-success"
    });
}

function notifyFailure(error) {
    showMessage({
        message: error.statusText,
        cssClass: "alert alert-danger"
    });
}

function getCodeMirrorInstance() {
    var codeMirrorContainer = $(".CodeMirror:visible").siblings('textarea')[0];
    var codeMirrorInstance = $.data(codeMirrorContainer, 'CodeMirrorInstance');
    return codeMirrorInstance;
}

function showMessage(data) {
    container = $("div[id^='notify-container']");

    var notification = $('<div/>', {
        text: data.message,
        "class": data.cssClass,
        style: "display: none"
    }).appendTo(container);

    notification.show({
        duration: 500
    });

    if (data.response) {
        var grid = $('#Submissions_' + data.response).getKendoGrid();
        grid.dataSource.read();
    }

    setTimeout(function () {
        notification.hide(500, function () {
            notification.remove();
        });
    }, 3500);
}

// update the code mirror textarea to display the submission content
$("#SubmissionsTabStrip").on("click", ".view-source-button", function () {
    var submissionId = $(this).data("submission-id");

    $.get("/Contests/Compete/GetSubmissionContent/" + submissionId, function (response) {
        var codeMirrorInstance = getCodeMirrorInstance();
        codeMirrorInstance.setValue(response);
    }).fail(function (err) {
        notifyFailure(err);
    });
});

// Previously viewing a submission was loading the submission content in the 
// code mirror box.
// $(document).on('click', '.view-submission-button', function () {
//     var submissionId = $(this).data("submission-id");
   
//     $.get("/Contests/Compete/GetSubmissionContent/" + submissionId, function (response) {
//         var codeMirrorInstance = getCodeMirrorInstance();
//         codeMirrorInstance.setValue(response);
//     }).fail(function (err) {
//         notifyFailure(err);
//     });
// });

var displayMaximumValues = function (maxMemory, maxTime) {
    var result = "Памет: " + maxMemory + "<br />" + "Време: " + maxTime;
    return result;
}

// validate the submission content
function validateSubmissionContent() {
    var codeMirrorInstance = getCodeMirrorInstance();
    var codeMirrorText = codeMirrorInstance.getValue();

    if (!codeMirrorText || codeMirrorText.length < 5) {
        showMessage({
            message: "Решението трябва да съдържа поне 5 символа!",
            cssClass: "alert alert-warning"
        });

        return false;
    }

    return true;
}

var lastSubmissionTime;

// validate the submission time
var submissionTimeValidator = function (lastSubmit, limitBetweenSubmissions) {

    if (!lastSubmissionTime) {
        lastSubmissionTime = lastSubmit;
    }

    var limitBetweenSubmissions = limitBetweenSubmissions;

    function validate() {
        var currentTime = new Date();
        var secondsForLastSubmission = (currentTime - lastSubmissionTime) / 1000;

        if (!lastSubmissionTime) {
            lastSubmissionTime = new Date();
            return true;
        }

        var differenceBetweenSubmissionAndLimit = parseInt(limitBetweenSubmissions - secondsForLastSubmission);

        if (differenceBetweenSubmissionAndLimit > 0) {
            showMessage({
                message: "Моля изчакайте още " + differenceBetweenSubmissionAndLimit + " секунди преди да изпратите решение.",
                cssClass: "alert alert-warning"
            });

            return false;
        }

        lastSubmissionTime = new Date();
        return true;
    }

    return {
        validate: validate
    }
}