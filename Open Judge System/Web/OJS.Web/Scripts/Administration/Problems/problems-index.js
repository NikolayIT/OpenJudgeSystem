function onAdditionalData() {
    return {
        text: $("#search").val()
    };
}

function onSearchSelect(e) {
    var contestId = this.dataItem(e.item.index()).Id;
    populateDropDowns(contestId);
}

function onContestSelect() {
    initializeGrid(parseInt($('#contests').val()));
}

function filterContests() {
    return {
        categories: $("#categories").val()
    };
}

function populateDropDowns(contestIdAsString) {

    var response;

    $.get('/Administration/Problems/GetContestInformation/' + contestIdAsString, function(data) {
        response = data;

        var categoryId = response.category;
        var contestId = response.contest;

        var categories = $("#categories").data("kendoDropDownList");
        var contests = $("#contests").data("kendoDropDownList");

        var categoriesData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/Problems/GetCascadeCategories',
                    dataType: 'json'
                }
            }
        });

        var contestsData = new kendo.data.DataSource({
            transport: {
                read: {
                    url: '/Administration/Problems/GetCascadeContests/' + categoryId.toString(),
                    dataType: 'json'
                }
            }
        });

        categoriesData.fetch(function() {
            categories.dataSource.data(categoriesData);
            categories.setDataSource(categoriesData);
            categories.refresh();

            contestsData.fetch(function() {
                contests.dataSource.data(contestsData);
                contests.refresh();

                categories.select(function(dataItem) {
                    return dataItem.Id === categoryId;
                });

                // TODO: Improve by using success callback or promises, not setTimeout - Cascade event on widgets might work too

                window.setTimeout(function() {

                    contests.select(function(dataItem) {
                        return dataItem.Id === contestId;
                    });

                }, 500);

            });
        });

        initializeGrid(contestId);
    });
}

function initializeGrid(contestId) {

    var response;
    var grid;

    $('#status').show();

    var request = $.get('/Administration/Problems/ByContest/' + contestId, function (data) {
        response = data;
    })
    .then(function () {
        $('#status').hide();
        $('#problems-grid').html('');

        $('#problems-grid').kendoGrid({
            dataSource: new kendo.data.DataSource({
                data: response
            }),
            scrollable: false,
            toolbar: [{
                template: '<a href="/Administration/Problems/Create/' + contestId + '" class="btn btn-sm btn-primary">Добавяне</a>' +
                    ' <a href="/Administration/Problems/DeleteAll/' + contestId + '" class="btn btn-sm btn-primary">Изтриване на всички</a>' +
                    ' <a href="/Administration/Problems/ExportToExcel?contestId=' + contestId + '" id="export" class="btn btn-sm btn-primary"><span></span>Експорт към Excel</a>' +
                    ' <a href="/Contests/Practice/Index/' + contestId + '" class="btn btn-sm btn-primary"><span></span>Изпрати решение/я</a>',
            }],
            columns: [
                { field: "Id", title: "Номер" },
                { field: "Name", title: "Име" },
                { field: "ContestName", title: "Състезание" },
                { title: "Тестове", template: '<div> Пробни: #= TrialTests # </div><div> Състезателни: #= CompeteTests # </div>' },
                { title: "Операции", width: '50%', template: '<div class="text-center"><a href="/Administration/Problems/Details/#= Id #" class="btn btn-sm btn-primary">Детайли</a>&nbsp;<a href="/Administration/Tests/Problem/#= Id #" class="btn btn-sm btn-primary">Тестове</a>&nbsp;<button class="btn btn-sm btn-primary resource-btn" id="resource-btn-#= Id #">Ресурси</button>&nbsp;<a href="/Administration/Problems/Retest/#= Id #" class="btn btn-sm btn-primary">Ретест</a>&nbsp;<a href="/Administration/Problems/Edit/#= Id #" class="btn btn-sm btn-primary">Промяна</a>&nbsp;<a href="/Administration/Problems/Delete/#= Id #" class="btn btn-sm btn-primary">Изтриване</a></div>' }
            ],
            detailInit: detailInit,
        });

        function detailInit(e) {
            $("<div/>").appendTo(e.detailCell).kendoGrid({
                dataSource: {
                    transport: {
                        read: "/Administration/Resources/GetAll/" + e.data.Id,
                        destroy: "/Administration/Resources/Delete"
                    },
                    type: "aspnetmvc-ajax",
                    pageSize: 5,
                    schema: {
                        data: "Data",
                        total: "Total",
                        errors: "Errors",
                        model: {
                            id: "Id",
                            fields: {
                                Id: { type: "number", editable: false },
                                Name: { type: "string" },
                                Type: { type: "number" },
                                TypeName: { type: "string" },
                                OrderBy: { type: "number" },
                                Link: { type: "string" },
                            }
                        },
                    },
                    sort: { field: "OrderBy", dir: "asc" },
                    serverSorting: true,
                    serverPaging: true,
                    serverFiltering: true,
                },
                editable: "popup",
                pagable: true,
                sortable: true,
                filterable: true,
                scrollable: false,
                toolbar: [{ template: '<a href="/Administration/Resources/Create/' + e.data.Id + '" class="btn btn-sm btn-primary">Добави ресурс</a>' }],
                columns: [
                    { field: "Id", title: "Номер" },
                    { field: "Name", title: "Име" },
                    { field: "Type", title: "Тип", template: "#= TypeName #" },
                    { field: "OrderBy", title: "Подредба" },
                    { title: "Линк", template: '# if(Type == 3) { # <a href="#= Link #" class="btn btn-sm btn-primary" target="_blank">Линк</a> # } else { # <a href="/Administration/Resources/Download/#= Id #" class="btn btn-sm btn-primary" >Свали</a> # } #' },
                    { title: "Операции", template: "<a href='/Administration/Resources/Edit/#= Id #' class='btn btn-sm btn-primary'>Промяна</a> <a href='\\#' class='btn btn-sm btn-primary k-grid-delete'>Изтрий</a>" }
                ]
            });
        }

            $('.resource-btn').click(function(e) {
                var target = $(e.target);
                var tr = target.closest("tr");
                var grid = $("#problems-grid").data("kendoGrid");

                if (target.data('expanded') == false) {
                    grid.collapseRow(tr);
                    target.removeData('expanded');
                } else {
                    grid.expandRow(tr);
                    target.data('expanded', false);
                }
            });

        if ($('#problemId').val()) {
            $('#resource-btn-' + $('#problemId').val()).click();
        }
    });
}

function hideTheadFromGrid() {
    $('#future-grid thead').hide();

    $('[data-clickable="grid-click"]').click(function () {
        var id = $(this).data('id');

        populateDropDowns(id);
    });
};

$(document).ready(function () {
    $('#status').hide();

    if ($('#contestId').val()) {
        populateDropDowns($('#contestId').val());
    }
});