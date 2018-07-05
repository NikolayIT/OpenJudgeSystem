/* exported countdownTimer */
var countdownTimer = function (endingTime) {
    'use strict';

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
    var countdownTimerContainer = $('#countdown-timer');

    function updateCountdown() {
        hoursContainer.text(ts.hours);
        minutesContainer.text(ts.minutes);
        secondsContainer.text(ts.seconds);
        countdownTimerContainer.show();
    }

    var start = function () {
        updateCountdown();

        var timerId = window.setInterval(function () {
            if (ts.hours === 0 && ts.minutes <= 4) {
                if (!countdownTimerContainer.hasClass('countdown-warning')) {
                    countdownTimerContainer.addClass('countdown-warning');
                }

                if (ts.start > ts.end) {
                    window.clearInterval(timerId);

                    // TODO: Handle contest over.
                }
            }

            updateCountdown();
            ts = countdown(null, endTime, countdown.HOURS | countdown.MINUTES | countdown.SECONDS);
        }, 1000);
    };

    return {
        start: start
    };
};

function Notifier() {
    'use strict';

    function showMessage(data) {
        var container = $("div[id^='notify-container']").filter(':visible');

        var notification = $('<div/>', {
            text: data.message,
            'class': data.cssClass,
            style: 'display: none'
        }).appendTo(container);

        notification.show({
            duration: 500
        });

        if (data.response) {
            var grid = $('#Submissions_' + data.response).getKendoGrid();
            if (grid) {
                grid.dataSource.read();
            }
        }

        setTimeout(function () {
            var dropdown = $('[id^=SubmissionsTabStrip-]').filter(':visible').find('input[id^="dropdown_"]').getKendoDropDownList();
            dropdown.close();
            notification.hide(500, function () {
                notification.remove();
            });
        }, 3500);
    }

    function notifySuccess(response) {
        var codeMirrorInstance = getCodeMirrorInstance();
        codeMirrorInstance.setValue('');

        showMessage({
            message: 'Успешно изпратено!',
            response: response,
            cssClass: 'alert alert-success'
        });
    }

    function notifyFailure(error) {
        showMessage({
            message: error.statusText,
            cssClass: 'alert alert-danger'
        });
    }

    function notifyWarning(warning) {
        showMessage({
            message: warning.statusText,
            cssClass: 'alert alert-warning'
        });
    }

    return {
        showMessage: showMessage,
        notifySuccess: notifySuccess,
        notifyFailure: notifyFailure,
        notifyWarning: notifyWarning
    };
}

//  update the code mirror textarea to display the submission content - not used
//  $("#SubmissionsTabStrip").on("click", ".view-source-button", function () {
//      var submissionId = $(this).data("submission-id");
//
//      $.get("/Contests/Compete/GetSubmissionContent/" + submissionId, function (response) {
//          var codeMirrorInstance = getCodeMirrorInstance();
//          codeMirrorInstance.setValue(response);
//      }).fail(function (err) {
//          notifyFailure(err);
//      });
//  });

function getCodeMirrorInstance() {
    'use strict';

    var codeMirrorContainer = $('.CodeMirror:visible').siblings('textarea')[0];
    var codeMirrorInstance = $.data(codeMirrorContainer, 'CodeMirrorInstance');
    return codeMirrorInstance;
}

/* exported displayMaximumValues */
var displayMaximumValues = function (maxMemory, maxTime, memoryString, timeString) {
    'use strict';

    var memoryInMb = (maxMemory / 1024 / 1024).toFixed(2);
    var maxTimeInSeconds = (maxTime / 1000).toFixed(3);
    var result = memoryString + ': ' + memoryInMb + ' MB <br />' + timeString + ': ' + maxTimeInSeconds + ' s';
    return result;
};

/* exported validateSubmissionContent */
function validateSubmissionContent() {
    'use strict';

    var codeMirrorInstance = getCodeMirrorInstance();
    var codeMirrorText = codeMirrorInstance.getValue();

    if (!codeMirrorText || codeMirrorText.length < 5) {
        messageNotifier.showMessage({
            message: 'Решението трябва да съдържа поне 5 символа!',
            cssClass: 'alert alert-warning'
        });

        return false;
    }

    return true;
}

/* exported validateBinaryFileExists */
function validateBinaryFileExists(fileInput) {
    'use strict';

    if (!fileInput.files[0]) {
        messageNotifier.notifyWarning({
            statusText: 'Моля изберете файл, който да изпратите.'
        });
        return false;
    }

    return true;
}

/* exported validateBinaryFileSize */
function validateBinaryFileSize(fileInput, size) {
    'use strict';

    if (!size) {
        return true;
    }

    var file = fileInput.files[0];
    if (file && file.size > size) {
        messageNotifier.notifyWarning({
            statusText: 'Избраният файл е твърде голям. Моля, изберете файл с по-малък размер.'
        });

        return false;
    }

    return true;
}

/* exported validateBinaryFileAllowedExtensions */
function validateBinaryFileAllowedExtensions(fileInput, extensions) {
    'use strict';

    var fileName = fileInput.files[0].name;
    var fileExtension = fileName.split('.')[fileName.split('.').length - 1].toLowerCase();

    if (!extensions || extensions.length === 0) {
        return true;
    }

    if ($.inArray(fileExtension, extensions) < 0) {
        messageNotifier.notifyWarning({
            statusText: 'Избраният тип файл не е позволен. Разрешените формати са: ' + extensions.join(',') + '.'
        });

        return false;
    }

    return true;
}

var messageNotifier = new Notifier();

/* exported submissionTimeValidator */
var submissionTimeValidator = function (initialServerTime) {
    'use strict';

    var lastSubmissionTime;
    var currentServerTime = initialServerTime;

    if (currentServerTime) {
        setInterval(function () {
            currentServerTime = new Date(currentServerTime.getTime() + 1000);
        }, 1000);
    }

    function validate(lastSubmit, limitBetweenSubmissions, serverTime) {
        if (!lastSubmissionTime) {
            lastSubmissionTime = lastSubmit;
        }

        if (!currentServerTime) {
            currentServerTime = serverTime;
            setInterval(function () {
                currentServerTime = new Date(currentServerTime.getTime() + 1000);
            }, 1000);
        }

        if (lastSubmissionTime) {
            var secondsForLastSubmission = (currentServerTime - lastSubmissionTime) / 1000;

            var differenceBetweenSubmissionAndLimit = parseInt(limitBetweenSubmissions - secondsForLastSubmission);

            if (differenceBetweenSubmissionAndLimit > 0) {
                messageNotifier.showMessage({
                    message: 'Моля, изчакайте още ' + differenceBetweenSubmissionAndLimit + ' секунди преди да изпратите решение.',
                    cssClass: 'alert alert-warning'
                });

                return false;
            }
        }

        lastSubmissionTime = currentServerTime;
        return true;
    }

    return {
        validate: validate
    };
};

var tabStripManager = new TabStripManager();

function TabStripManager() {
    'use strict';

    var tabStrip;
    var index = 0;

    function selectTabWithIndex(ind) {
        tabStrip.select(ind);
        index = ind;
    }

    function init(tabstrip) {
        tabStrip = tabstrip;

        tabStrip = $('#SubmissionsTabStrip').data('kendoTabStrip');
        if (tabstrip) {
            var hashIndex = getSelectedIndexFromHashtag();
            if (!hashIndex) {
                hashIndex = 0;
            }

            selectTabWithIndex(hashIndex);
            markEcludedFromHomeworkTabs();
        }
    }

    function tabSelected() {
        if (tabStrip) {
            var selectedIndex = tabStrip.select().index();
            window.location.hash = selectedIndex;
        }
    }

    function createCodeMirrorForTextBox() {
        var element = $('.code-for-problem:visible')[0];

        if (!$(element).data('CodeMirrorInstance')) {
            /* eslint new-cap: 0 */
            var editor = new CodeMirror.fromTextArea(element, {
                lineNumbers: true,
                matchBrackets: true,
                mode: 'text/x-csharp',
                theme: 'the-matrix',
                showCursorWhenSelecting: true,
                undoDepth: 100,
                lineWrapping: true
            });

            $.data(element, 'CodeMirrorInstance', editor);
        }
    }

    function markEcludedFromHomeworkTabs() {
        var marker = $('<span class="right-buffer">&#10038;</span>').css('color', '#b88700');

        $('.excluded-from-homework-tab').append(marker);
    }

    function onContentLoad() {
        createCodeMirrorForTextBox();
        var hashTag = getSelectedIndexFromHashtag();
        selectTabWithIndex(hashTag);
    }

    function currentIndex() {
        return index;
    }

    return {
        selectTabWithIndex: selectTabWithIndex,
        tabSelected: tabSelected,
        onContentLoad: onContentLoad,
        currentIndex: currentIndex,
        init: init
    };
}

function getSelectedIndexFromHashtag() {
    'use strict';

    return parseInt(window.location.hash.substr(1) || '0');
}

$(document).ready(function () {
    'use strict';

    $(window).on('hashchange', function () {
        var hashIndex = getSelectedIndexFromHashtag();
        if (hashIndex !== tabStripManager.currentIndex()) {
            tabStripManager.selectTabWithIndex(hashIndex);
        }
    });

    var tabStrip = $('#SubmissionsTabStrip').data('kendoTabStrip');
    tabStripManager.init(tabStrip);
});

/* exported cloneSubmissionsGridPager */
function cloneSubmissionsGridPager() {
    'use strict';

    var self = this; // submission grid
    if (self.dataSource.total() && typeof(self.pagerTop) === 'undefined') {
        var wrapper = $('<div class="k-pager-wrap k-grid-pager pagerTop"/>').insertAfter(self.element.find('.k-toolbar'));
        self.pagerTop = new kendo.ui.Pager(
            wrapper,
            $.extend(
                {},
                self.options.pageable,
                { dataSource: self.dataSource }));

        self.element.height('').find('.pagerTop').css('border-width', '0 0 1px 0');
        self.element.find('.k-toolbar').css('border-width', '1px 0');
    }
}
