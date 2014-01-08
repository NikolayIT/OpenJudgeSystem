function onAdditionalData() {
    return {
        text: $("#search").val()
    };
}

function onSearchSelect(e) {
    
    var problemId = this.dataItem(e.item.index()).Id;
    $('#problemId').val(problemId);
    populateDropDowns(problemId);
}

function filterContests() {
    return {
        id: $("#categories").val()
    };
}

function filterProblems() {
    return {
        id: $("#contests").val()
    };
}

function onProblemSelect(e) {
    var problemId = $('#problems').val();

    if (problemId != "") {
        $('#controls').show();
        $('#problemId').val(problemId);
        $('#export').attr('href', '/Administration/Tests/Export/' + problemId)

        initializeGrid(parseInt(problemId), parseInt($("#contests").val()));
        $('#grid').show();
    }
    else {
        $('#controls').hide();
        $('#grid').hide();
    }
}

function populateDropDowns(problemIdAsString) {

    $('#controls').show();
    $('#export').attr('href', '/Administration/Tests/Export/' + problemIdAsString)

    var response;

    $.get('/Administration/Tests/GetProblemInformation/' + problemIdAsString, function (data) {
        response = data;

        var categoryId = response.Category;
        var contestId = response.Contest;
        var problemId = parseInt(problemIdAsString);

        var categories = $("#categories").data("kendoDropDownList");
        var contests = $("#contests").data("kendoDropDownList");
        var problems = $("#problems").data("kendoDropDownList");

        var categoriesData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/Tests/GetCascadeCategories',
                    dataType: 'json'
                }
            }
        });

        var contestsData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/Tests/GetCascadeContests/' + categoryId.toString(),
                    dataType: 'json'
                }
            }
        });

        var problemsData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/Tests/GetCascadeProblems/' + contestId.toString(),
                    dataType: 'json'
                }
            }
        });

        function categoriesCascade() {
            contests.dataSource.fetch(function () {
                contests.dataSource.read();

                contests.select(function (dataItem) {
                    return dataItem.Id == contestId;
                });
            });
        }

        function contestsCascade() {
            problems.dataSource.fetch(function () {
                problems.dataSource.read();

                problems.select(function (dataItem) {
                    return dataItem.Id == problemId;
                })
            });
        }

        categories.bind("cascade", categoriesCascade);
        contests.bind("cascade", contestsCascade);

        categoriesData.fetch(function () {
            categories.dataSource.data(categoriesData);
            categories.setDataSource(categoriesData);
            categories.refresh();
        });

        categories.select(function (dataItem) {
            return dataItem.Id === categoryId;
        });

        initializeGrid(problemId, contestId);
    })
}

function initializeGrid(problemId, contestId) {

    var response;
    var grid;

    $('#status').show();

    var request = $.get('/Administration/Tests/ProblemTests/' + problemId, function (data) {
        response = data;
    })
    .then(function () {

        $('#status').hide();
        $('#grid').html('');

        $('#grid').kendoGrid({
            dataSource: new kendo.data.DataSource({
                data: response
            }),
            scrollable: false,
            toolbar: [{
                template: '<a href="/Administration/Tests/Create/' + problemId + '" class="btn btn-sm btn-primary">Добавяне</a>' +
                    ' <a href="/Administration/Tests/DeleteAll/' + problemId + '" class="btn btn-sm btn-primary">Изтриване на всички</a>' +
                    ' <a href="/Administration/Problems/Contest/' + contestId + '" class="btn btn-sm btn-primary">Към задачата</a>',
            }],
            columns: [
                { field: "Input", title: "Вход" },
                { field: "Output", title: "Изход" },
                { field: "TrialTestName", title: "Вид тест" },
                { field: "OrderBy", title: "Подредба" },
                { field: "TestRunsCount", title: "Изпълнения" },
                { title: "Операции", width: "25%", template: '<a href="/Administration/Tests/Details/#= Id #" class="btn btn-sm btn-primary">Детайли</a>&nbsp;<a href="/Administration/Tests/Edit/#= Id #" class="btn btn-sm btn-primary">Промяна</a>&nbsp;<a href="/Administration/Tests/Delete/#= Id #" class="btn btn-sm btn-primary">Изтриване</a>' },
            ],
            sortable: true
        });
    });
}

$(document).ready(function () {
    $('#status').hide();
    $('#controls').hide();
    $('#problemId').hide();

    if ($('#problemId').val() != '') {
        populateDropDowns($('#problemId').val());
    }
});