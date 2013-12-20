// TODO: Fix nesting problem
function CategoryExpander() {
    var treeview, treeviewSelector, currentlySelectedId;
    var data = [];
    var firstLoad = true;

    var self;

    var init = function (treeView, treeViewSelector) {
        treeview = treeView;
        treeviewSelector = treeViewSelector;
        self = this;
    }

    function onDataBound() {
        var categoryId;
        if (firstLoad && window.location.hash) {
            categoryId = getCategoryIdFromHash();
            self.select(categoryId);
            firstLoad = false;
        } else {
            categoryId = currentlySelectedId;
        }

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

    var categorySelected = function (e) {
        $("#contestsList").html("");
        $("#contestsList").addClass("k-loading");

        var elementId;
        var elementName;
        var elementNode;

        if (e.elementId) {
            elementId = parseInt(e.elementId);
            elementName = e.elementName;
            var el = treeview.dataSource.get(elementId);
            if (el) {
                elementNode = treeviewSelector.find('[data-uid=' + el.uid + ']');
            }
        } else {
            elementNode = e.node;
            var element = treeview.dataItem(elementNode);
            elementId = element.Id;
            elementName = element.NameUrl;
        }

        if (elementNode) {
            treeview.expand(elementNode);
        }

        if (window.location.hash !== undefined && elementName) {
            window.location.hash = '!/List/ByCategory/' + elementId + '/' + elementName;
        }

        var ajaxUrl = "/Contests/List/ByCategory/" + elementId;
        $("#contestsList").load(ajaxUrl, function () {
            $("#contestsList").removeClass("k-loading");
        });
    }

    var setNestingData = function (categoriesArray) {
        if (categoriesArray) {
            data = categoriesArray;
        }
    }

    var expandSubcategories = function () {
        for (var i = 0; i < data.length; i++) {
            var id = data[i];
            self.select(id);
        }
    }

    var select = function (id) {
        currentlySelectedId = id;
        
        var el = treeview.dataSource.get(id);
        if (!el && data.indexOf(id) < 0) {
            var parentsUrl = "/Contests/List/GetParents/" + id;

            $.ajax({
                url: parentsUrl,
                success: function (data) {
                    self.setNestingData(data);
                    self.expandSubcategories();
                }
            });
        } else if (el) {
            var element = treeviewSelector.find('[data-uid=' + el.uid + ']');
            
            var elementObj = {
                elementId: id
            }

            treeview.trigger('select', elementObj);
            treeview.expand(element);
            treeview.select(element);
        }
    }

    var currentId = function () {
        return currentlySelectedId;
    }

    return {
        expandSubcategories: expandSubcategories,
        select: select,
        currentId: currentId,
        setNestingData: setNestingData,
        onDataBound: onDataBound,
        categorySelected: categorySelected,
        init: init
    };
}

function getCategoryIdFromHash() {
    var hash = window.location.hash;
    var categoryId = hash.split('/')[3];
    return categoryId;
}

var expander = new CategoryExpander();

$(document).ready(function () {
    $(window).on("hashchange", function (ev) {
        var categoryId = getCategoryIdFromHash();
        if (expander && categoryId !== expander.currentId()) {
            expander.select(categoryId);
        }
    });

    var treeviewSelector = $("#contestsCategories");
    var treeview = treeviewSelector.data("kendoTreeView");
    expander.init(treeview, treeviewSelector);
})