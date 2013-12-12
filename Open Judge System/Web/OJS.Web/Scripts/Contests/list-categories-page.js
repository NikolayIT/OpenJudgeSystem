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
    var categoryId = getCategoryIdFromHash();

    var treeview = $("#contestsCategories").data("kendoTreeView");
    var element = treeview.dataSource.get(categoryId);

    var nodeToSelect = {
        elementId: categoryId,
        elementName: element !== undefined ? element.NameUrl : null,
        uid: element !== undefined ? element.uid : null
    };

    if (categoryId) {
        treeview.trigger('select', nodeToSelect);
    }
}

// TODO: refactor and add documentation
function CategoryExpander(categoriesArray, treeview, treeviewSelector, grid) {
    var data = categoriesArray;
    var currentlySelectedId = data[data.length - 1];
    var self = this;

    var expandSubcategories = function () {
        for (var i = 0; i < data.length; i++) {
            var el = treeview.dataSource.get(data[i]);
            var element = treeviewSelector.find('[data-uid=' + el.uid + ']');
            if (i !== data.length - 1) {
                if (!treeview._expanded(element)) {
                    treeview.expand(element);
                    break;
                }
            } else if (data.length > 0) {
                treeview.expand(element);
                var id = data[i];
                data = [];
                this.select(id);
            }
        }
    }

    var select = function (id) {
        var el = treeview.dataSource.get(id);
        var element = treeviewSelector.find('[data-uid=' + el.uid + ']');

        currentId = id;
        var nodeToSelect = {
            elementId: id
        }

        treeview.select(element);
        treeview.trigger('select', nodeToSelect);
    }

    var currentId = function () {
        return currentlySelectedId;
    }

    return {
        expandSubcategories: expandSubcategories,
        select: select,
        currentId: currentId
    };
}

function getCategoryIdFromHash() {
    var hash = window.location.hash;
    var categoryId = hash.split('/')[3];
    return categoryId;
}

$(document).ready(function () {
    $(window).on("hashchange", function (ev) {
        var categoryId = getCategoryIdFromHash();
        if (expander && categoryId !== expander.currentId()) {
            expander.select(categoryId);
        }
    });
})