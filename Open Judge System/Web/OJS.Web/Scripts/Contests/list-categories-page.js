var categoryHierarchy;

var expander;

function categorySelected(e) {
    $("#contestsList").html("");
    $("#contestsList").addClass("k-loading");

    var elementId;
    var elementName;

    var treeviewSelector = $("#contestsCategories");
    var treeview = treeviewSelector.data("kendoTreeView");

    if (e.elementId) {
        elementId = parseInt(e.elementId);
        elementName = e.elementName;
        var parentsUrl = "/Contests/List/GetParents/" + elementId;

        if (!expander) {
            $.ajax({
                url: parentsUrl,
                success: function (data) {
                    if (!expander) {
                        expander = new CategoryExpander(data, treeview, treeviewSelector);
                    }

                    expander.expandSubcategories();
                }
            });
        } else {
            expander.expandSubcategories();
        }

    } else {
        treeview.expand(e.node);
        var element = treeview.dataItem(e.node);
        elementId = element.Id;
        elementName = element.NameUrl;
    }

    if (window.location.hash !== undefined && elementName) {
        window.location.hash = '!/List/ByCategory/' + elementId + '/' + elementName;
    }

    var ajaxUrl = "/Contests/List/ByCategory/" + elementId;
    $("#contestsList").load(ajaxUrl, function () {
        $("#contestsList").removeClass("k-loading");
    });
}

function onDataBound() {
    var hash = window.location.hash;
    var contestId = hash.split('/')[3];

    var treeview = $("#contestsCategories").data("kendoTreeView");
    var element = treeview.dataSource.get(contestId);

    var nodeToSelect = {
        elementId: contestId,
        elementName: element !== undefined ? element.NameUrl : null,
        uid: element !== undefined ? element.uid : null
    };

    if (contestId) {
        treeview.trigger('select', nodeToSelect);
    }
}

function CategoryExpander(categoriesArray, treeview, treeviewSelector, grid) {
    var data = categoriesArray;

    var expandSubcategories = function () {
        var i;
        for (i = 0; i < data.length; i++) {
            var el = treeview.dataSource.get(data[i]);
            var element = treeviewSelector.find('[data-uid=' + el.uid + ']');
            if (!treeview._expanded(element)) {
                if (i == data.length - 1) {
                    element.find('.k-in:first').addClass('k-state-selected');
                }

                treeview.expand(element);
                break;
            }
        }
    }

    var select = function (id) {
        var el = treeview.dataSource.get(id);
        var element = treeviewSelector.find('[data-uid=' + el.uid + ']');
        $('.k-state-selected').removeClass('k-state-selected');
        element.find('.k-in:first').addClass('k-state-selected');

        categorySelected({ elementId: id });
    }

    return {
        expandSubcategories: expandSubcategories,
        select: select
    };
}

$(document).ready(function () {
    $('#contestsList').on('click', '.subcategory', function (ev) {
        ev.preventDefault();
        ev.stopPropagation();
        var id = $(this).data('id');
        if (expander) {
            console.log(id);
            expander.select(id);
        }
    })
})