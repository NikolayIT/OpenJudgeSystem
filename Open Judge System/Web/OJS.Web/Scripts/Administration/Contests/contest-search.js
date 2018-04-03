function populateDropDowns(contestIdAsString) {
    'use strict';

    $.get('/Administration/KendoRemoteData/GetContestInformation/' + contestIdAsString, function (data) {
        var categoryId = data.category;
        var contestId = data.contest;

        $('#categories').data('kendoDropDownList').select(function (dataItem) {
            return dataItem.Id === categoryId;
        });

        // TODO: Improve by using success callback or promises, not setTimeout - Cascade event on widgets might work too
        window.setTimeout(function () {
            $('#contests').data('kendoDropDownList').select(function (dataItem) {
                return dataItem.Id === contestId;
            });
        }, 500);
    });
}