function CategoryExpander() {
    'use strict';

    var treeview;
    var treeviewSelector;
    var containerToFill;
    var currentlySelectedId;
    var firstLoad = true;

    /* eslint consistent-this: 0 */
    var self;

    var init = function(treeView, treeViewSelector, containerToFillSelector) {
        treeview = treeView;
        treeviewSelector = treeViewSelector;
        containerToFill = containerToFillSelector;
        self = this;
    };

    function onDataBound() {
        if (!firstLoad) {
            return;
        }

        firstLoad = false;

        if (window.location.hash) {
            var categoryId = getCategoryIdFromHash();

            var elementObj = {
                elementId: categoryId
            };

            treeview.trigger('select', elementObj);
            self.select(categoryId);
        } else {
            $.get('/Contests/List/ByCategory/', null, function (data) {
                containerToFill.append(data);
            });
        }
    }

    var categorySelected = function(e) {
        containerToFill.html('');
        $('#contest-categories-loading-mask').show();

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

        var ajaxUrl = '/Contests/List/ByCategory/' + elementId;
        containerToFill.load(ajaxUrl, function () {
            $('#contest-categories-loading-mask').hide();
        });
    };

    var expandSubcategories = function (data) {
        var selectedCategoryId = data.pop();
        treeview.expandPath(data, function () {
            self.select(selectedCategoryId);
        });
    };

    var select = function(id) {
        currentlySelectedId = id;

        var el = treeview.dataSource.get(id);
        if (!el) {
            var parentsUrl = '/Contests/List/GetParents/' + id;

            $.ajax({
                url: parentsUrl,
                success: function(result) {
                    self.expandSubcategories(result);
                }
            });
        } else {
            var element = treeviewSelector.find('[data-uid=' + el.uid + ']');

            var elementObj = {
                elementId: id
            };

            var isNonTreeCall = self.getQueryParameterFromUrl('nonTreeCall');

            if (isNonTreeCall) {
                window.location.href = window.location.href.split('?')[0];
                treeview.trigger('select', elementObj);
                treeview.expand(element);
            }

            treeview.select(element);
        }
    };

    var currentId = function() {
        return currentlySelectedId;
    };

    var getQueryParameterFromUrl = function (queryParameterName) {
        var query = window.location.href.split('?').pop();
        var queryParameters = query.split('&');

        for (var i = 0; i < queryParameters.length; i++) {
            var parameterName = queryParameters[i].split('=');

            if (parameterName[0] === queryParameterName) {
                return parameterName[1] === undefined ? true : parameterName[1];
            }
        }

        return undefined;
    };

    return {
        expandSubcategories: expandSubcategories,
        getQueryParameterFromUrl: getQueryParameterFromUrl,
        select: select,
        currentId: currentId,
        onDataBound: onDataBound,
        categorySelected: categorySelected,
        init: init
    };
}

function getCategoryIdFromHash() {
    'use strict';

    var hash = window.location.hash;
    var categoryId = hash.split('/')[3];
    return categoryId;
}

var expander = new CategoryExpander();

$(document).ready(function () {
    'use strict';

    $(window).on('hashchange', function() {
        var categoryId = getCategoryIdFromHash();
        if (expander && categoryId !== expander.currentId()) {
            expander.select(categoryId);
        }
    });

    var treeviewSelector = $('#contestsCategories');
    var containerToFillSelector = $('#contestsList');
    var treeview = treeviewSelector.data('kendoTreeView');
    
    expander.init(treeview, treeviewSelector, containerToFillSelector);
});