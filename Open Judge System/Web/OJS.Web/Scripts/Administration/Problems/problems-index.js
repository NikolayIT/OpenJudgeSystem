/* exported onSearchSelect */
function onSearchSelect(e) {
    'use strict';

    var contestId = this.dataItem(e.item.index()).Id;
    populateDropDowns(contestId);
    initializeGrid(contestId);
}

function setContestIdToGetProblemGroups() {
    return {
        contestId: $('#ContestToCopyTo').val()
    }
}

function setContestFilterToCopySearch() {
    return {
        contestFilter: $('#ContestToCopyTo').data("kendoComboBox").input.val()
    }
}

/* exported onContestSelect */
function onContestSelect() {
    'use strict';

    var contestId = $('#contests').val();
    initializeGrid(contestId);
}

function initializeGrid(contestId) {
    'use strict';

    var response;
    var sendSubmissionsActionName;

    $('#status').show();

    $.get('/Administration/KendoRemoteData/GetContestCompeteOrPracticeActionName/' + contestId, function (data) {
        sendSubmissionsActionName = data;
    }).then(function() {
        $.get('/Administration/Problems/ByContest/' + contestId, function (data) {
            response = data;
        }).then(function () {
            $('#status').hide();
            $('#problems-grid').html('');
            $('#problems-grid').kendoGrid({
                dataSource: new kendo.data.DataSource({
                    data: response
                }),
                scrollable: false,
                toolbar: [{
                    template: '<a href="/Administration/Problems/Create/' + contestId +
                    '" class="btn btn-sm btn-primary">Добавяне</a>' +
                    ' <a href="/Administration/Problems/DeleteAll/' + contestId +
                    '" class="btn btn-sm btn-primary">Изтриване на всички</a>' +
                    ' <a href="/Administration/Problems/ExportToExcel?contestId=' + contestId +
                    '" id="export" class="btn btn-sm btn-primary"><span></span>Експорт към Excel</a>' +
                    ' <a href="/Contests/' + sendSubmissionsActionName + '/Index/' + contestId +
                    '" class="btn btn-sm btn-primary"><span></span>Изпрати решение/я</a>'
                }],
                columns: [
                    { field: 'Id', title: 'Номер' },
                    { field: 'Name', title: 'Име' },
                    { field: 'ProblemGroupOrderBy', title: 'Група' },
                    { field: 'ContestName', title: 'Състезание' },
                    { title: 'Тестове', template: '<div> Пробни: #= TrialTests # </div><div> Състезателни: #= CompeteTests # </div>' },
                    {
                        title: 'Операции', width: '50%', template: '<div class="text-center">' +
                        '<a href="/Administration/Problems/Details/#= Id #" class="btn btn-sm btn-primary">Детайли</a>&nbsp;' +
                        '<a href="/Administration/Tests/Problem/#= Id #" class="btn btn-sm btn-primary">Тестове</a>&nbsp;' +
                        '<button class="btn btn-sm btn-primary resource-btn" id="resource-btn-#= Id #">Ресурси</button>&nbsp;' +
                        '<a href="/Administration/Problems/Retest/#= Id #" class="btn btn-sm btn-primary">Ретест</a>&nbsp;' +
                        '<a href="/Administration/Problems/Edit/#= Id #" class="btn btn-sm btn-primary">Промяна</a>&nbsp;' +
                        '<a href="/Administration/Problems/Delete/#= Id #" class="btn btn-sm btn-primary">Изтриване</a>&nbsp;' +
                        '<a data-role="button" onclick="prepareCopyWindow(#=Id#, \'#=Name#\')" class="btn btn-sm btn-primary">Копиране</a></div>'
                    }
                ],
                detailInit: detailInit
            });

            function detailInit(e) {
                $('<div/>').appendTo(e.detailCell).kendoGrid({
                    dataSource: {
                        transport: {
                            read: '/Administration/Resources/GetAll/' + e.data.Id,
                            destroy: '/Administration/Resources/Delete'
                        },
                        type: 'aspnetmvc-ajax',
                        pageSize: 5,
                        schema: {
                            data: 'Data',
                            total: 'Total',
                            errors: 'Errors',
                            model: {
                                id: 'Id',
                                fields: {
                                    Id: { type: 'number', editable: false },
                                    Name: { type: 'string' },
                                    Type: { type: 'number' },
                                    TypeName: { type: 'string' },
                                    OrderBy: { type: 'number' },
                                    Link: { type: 'string' }
                                }
                            }
                        },
                        sort: { field: 'OrderBy', dir: 'asc' },
                        serverSorting: true,
                        serverPaging: true,
                        serverFiltering: true
                    },
                    editable: 'popup',
                    pagable: true,
                    sortable: true,
                    filterable: true,
                    scrollable: false,
                    toolbar: [{
                        template: '<a href="/Administration/Resources/Create/' +
                        e.data.Id +
                        '" class="btn btn-sm btn-primary">Добави ресурс</a>'
                    }],
                    columns: [
                        { field: 'Id', title: 'Номер' },
                        { field: 'Name', title: 'Име' },
                        { field: 'Type', title: 'Тип', template: '#= TypeName #' },
                        { field: 'OrderBy', title: 'Подредба' },
                        {
                            title: 'Линк', template: '# if(Type == 3) { ' +
                                '# <a href="#= Link #" class="btn btn-sm btn-primary" target="_blank">Линк</a> ' +
                                '# } else { # ' +
                                '<a href="/Administration/Resources/Download/#= Id #" class="btn btn-sm btn-primary">Свали</a> # } #'
                        },
                        {
                            title: 'Операции', template: "<a href='/Administration/Resources/Edit/#= Id #' " +
                            "class='btn btn-sm btn-primary'>Промяна</a> " +
                            "<a href='\\#' class='btn btn-sm btn-primary k-grid-delete'>Изтрий</a>"
                        }
                    ]
                });
            }

            $('.resource-btn').click(function (e) {
                var target = $(e.target);
                var tr = target.closest('tr');
                var grid = $('#problems-grid').data('kendoGrid');

                if (target.data('expanded')) {
                    grid.collapseRow(tr);
                    target.removeData('expanded');
                } else {
                    grid.expandRow(tr);
                    target.data('expanded', true);
                }
            });

            if ($('#problemId').val()) {
                $('#resource-btn-' + $('#problemId').val()).click();
            }
        });
    });
}

/* exported hideTheadFromGrid */
function hideTheadFromGrid() {
    'use strict';

    $('#future-grid thead').hide();

    $('[data-clickable="grid-click"]').click(function () {
        var contestId = $(this).data('id');
        populateDropDowns(contestId);
        initializeGrid(contestId);
    });
}

function prepareCopyWindow(problemId, problemName) {
    var copyWindowSelector = $('#copy-popup-window');
    var title = 'Копиране на задача ' + problemName;
    var url = '/Administration/Problems/CopyToContestPartial/' + problemId;

    var copyPopUp = copyWindowSelector.data('kendoWindow');

    if (typeof copyPopUp == typeof undefined) {
        (function () {
            copyWindowSelector.kendoWindow({
                width: '600px',
                modal: true,
                height: '200px',
                iframe: false,
                resizable: false,
                title: title,
                content: url,
                visible: false,
                refresh: onWindowLoaded,
                error: validateModelStateErrors
            });

            copyPopUp = copyWindowSelector.data('kendoWindow');

        })();
    } else {
        copyPopUp.title(title);
        copyPopUp.refresh(url);
    }

    copyPopUp.open();
    copyPopUp.center();

    function onWindowLoaded() {
        var form = copyWindowSelector.children('form');

        form.submit(function () {
            $.post('/Administration/Problems/CopyTo/' + problemId, form.serialize(), function (response) {
                displayStandardJsonResultMessage(response);
                copyPopUp.close();
            });

            return false;
        });
    }
}

$(document).ready(function () {
    'use strict';

    $('#status').hide();

    var contestId = $('#contestId').val();
    if (contestId) {
        populateDropDowns(contestId);
        initializeGrid(contestId);
    }
});