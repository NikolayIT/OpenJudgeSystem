/* exported onAdditionalData */
function onAdditionalData() {
    'use strict';

    return {
        text: $('#search').val()
    };
}

/* exported onSearchSelect */
function onSearchSelect(e) {
    'use strict';

    var problemId = this.dataItem(e.item.index()).Id;
    $('#problemId').val(problemId);
    populateDropDowns(problemId);
}

/* exported filterContests */
function filterContests() {
    'use strict';

    return {
        categoryId: $('#categories').val()
    };
}

/* exported filterProblems */
function filterProblems() {
    'use strict';

    return {
        contestId: $('#contests').val()
    };
}

/* exported onProblemSelect */
function onProblemSelect() {
    'use strict';

    var problemId = $('#problems').val();

    if (problemId) {
        $('#controls').show();
        $('#problemId').val(problemId);
        $('#exportFile').attr('href', '/Administration/Tests/Export/' + problemId);

        initializeGrid(parseInt(problemId), parseInt($('#contests').val()));
        $('#grid').show();
    } else {
        $('#controls').hide();
        $('#grid').hide();
    }
}

/* exported getTestTypeString */
function getTestTypeString(type) {
    'use strict';

    if (type === 1) {
        return 'Contest';
    } else if (type === 2) {
        return 'Open';
    } else if (type === 3) {
        return 'Practice';
    }

    return type;
}

function populateDropDowns(problemIdAsString) {
    'use strict';

    $('#controls').show();
    $('#exportFile').attr('href', '/Administration/Tests/Export/' + problemIdAsString);

    var response;

    $.get('/Administration/Tests/GetProblemInformation/' + problemIdAsString, function (data) {
        response = data;

        var categoryId = response.Category;
        var contestId = response.Contest;
        var problemId = parseInt(problemIdAsString);

        var categories = $('#categories').data('kendoDropDownList');
        var contests = $('#contests').data('kendoDropDownList');
        var problems = $('#problems').data('kendoDropDownList');

        var categoriesData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/LecturersKendoRemoteData/GetAvailableCategories',
                    dataType: 'json'
                }
            }
        });

        function categoriesCascade() {
            contests.dataSource.fetch(function () {
                contests.dataSource.read();

                contests.select(function (dataItem) {
                    return dataItem.Id === contestId;
                });
            });
        }

        function contestsCascade() {
            problems.dataSource.fetch(function () {
                problems.dataSource.read();

                problems.select(function (dataItem) {
                    return dataItem.Id === problemId;
                });
            });
        }

        categories.bind('cascade', categoriesCascade);
        contests.bind('cascade', contestsCascade);

        categoriesData.fetch(function () {
            categories.dataSource.data(categoriesData);
            categories.setDataSource(categoriesData);
            categories.refresh();
        });

        categories.select(function (dataItem) {
            return dataItem.Id === categoryId;
        });

        initializeGrid(problemId, contestId);
    });
}

function initializeGrid(problemId, contestId) {
    'use strict';

    var response;

    $('#status').show();

    $.get('/Administration/Tests/ProblemTests/' + problemId, function (data) {
        response = data;
    }).then(function () {
        $('#status').hide();
        $('#grid').html('');

        $('#grid').kendoGrid({
            dataSource: new kendo.data.DataSource({
                data: response
            }),
            scrollable: false,
            toolbar: [{
                template: '<a href="/Administration/Tests/Create/' + problemId +
                '" class="btn btn-sm btn-primary">Добавяне</a>' +
                ' <a href="/Administration/Tests/DeleteAll/' + problemId +
                '" class="btn btn-sm btn-primary">Изтриване на всички</a>' +
                ' <a href="/Administration/Problems/Contest/' + contestId +
                '" class="btn btn-sm btn-primary">Към задачите</a>' +
                ' <a href="/Administration/Tests/ExportToExcel?id=' + problemId +
                '" id="export" class="btn btn-sm btn-primary"><span></span>Експорт към Excel</a>' +
                ' <a href="/Administration/Tests/Export/' + problemId +
                '" class="btn btn-sm btn-primary" id="exportFile">Експортиране към ZIP файл</a>' +
                ' <a href="/Contests/Practice/Index/' + contestId +
                '" class="btn btn-sm btn-primary">Изпрати решение/я</a>'
            }],
            columns: [
                { field: 'Input', title: 'Вход' },
                { field: 'Output', title: 'Изход' },
                { field: 'TypeName', title: 'Вид тест', template: '#= getTestTypeString(Type) #', sortable: false },
                { field: 'OrderBy', title: 'Подредба' },
                { field: 'TestRunsCount', title: 'Изпълнения' },
                {
                    title: 'Операции', width: '25%', template: '<a href="/Administration/Tests/Details/#= Id #" ' +
                        'class="btn btn-sm btn-primary">Детайли</a>&nbsp;' +
                        '<a href="/Administration/Tests/Edit/#= Id #" class="btn btn-sm btn-primary">Промяна</a>&nbsp;' +
                        '<a href="/Administration/Tests/Delete/#= Id #" class="btn btn-sm btn-primary">Изтриване</a>'
                }
            ],
            sortable: true
        });
    });
}

$(document).ready(function () {
    'use strict';

    $('#status').hide();
    $('#controls').hide();
    $('#problemId').hide();

    if ($('#problemId').val()) {
        populateDropDowns($('#problemId').val());
    }
});
