/*
* Kendo UI v2014.3.1411 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.data", "./kendo.combobox", "./kendo.dropdownlist", "./kendo.multiselect", "./kendo.validator" ], f);
})(function(){

(function ($, undefined) {
    var kendo = window.kendo,
        escapeQuoteRegExp = /'/ig,
        extend = $.extend,
        isArray = $.isArray,
        isPlainObject = $.isPlainObject,
        POINT = ".";

    function parameterMap(options, operation, encode, stringifyDates) {
       var result = {};

       if (options.sort) {
           result[this.options.prefix + "sort"] = $.map(options.sort, function(sort) {
               return sort.field + "-" + sort.dir;
           }).join("~");

           delete options.sort;
       } else {
           result[this.options.prefix + "sort"] = "";
       }

       if (options.page) {
           result[this.options.prefix + "page"] = options.page;

           delete options.page;
       }

       if (options.pageSize) {
           result[this.options.prefix + "pageSize"] = options.pageSize;

           delete options.pageSize;
       }

       if (options.group) {
            result[this.options.prefix + "group"] = $.map(options.group, function(group) {
               return group.field + "-" + group.dir;
           }).join("~");

           delete options.group;
       } else {
            result[this.options.prefix + "group"] = "";
       }

       if (options.aggregate) {
           result[this.options.prefix + "aggregate"] =  $.map(options.aggregate, function(aggregate) {
               return aggregate.field + "-" + aggregate.aggregate;
           }).join("~");

           delete options.aggregate;
       }

       if (options.filter) {
           result[this.options.prefix + "filter"] = serializeFilter(options.filter, encode);

           delete options.filter;
       } else {
           result[this.options.prefix + "filter"] = "";
           delete options.filter;
       }

       delete options.take;
       delete options.skip;

       serializeItem(result, options, "", stringifyDates);

       return result;
    }

    function convertNumber(value){
        var separator = kendo.culture().numberFormat[POINT];
        value = value.toString().replace(POINT, separator);

        return value;
    }

    function convert(value, stringifyDates) {
        if (value instanceof Date) {
            if (stringifyDates) {
                value = kendo.stringify(value).replace(/"/g, "");
            } else {
                value = kendo.format("{0:G}", value);
            }
        } else if (typeof value === "number") {
            value = convertNumber(value);
        }

        return value;
    }

    function serialize(result, value, data, key, prefix, stringifyDates) {
        if (isArray(value)) {
            serializeArray(result, value, prefix, stringifyDates);
        } else if (isPlainObject(value)) {
            serializeItem(result, value, prefix, stringifyDates);
        } else {
            if (result[prefix] === undefined) {
                result[prefix] = data[key]  = convert(value, stringifyDates);
            }
        }
    }

    function serializeItem(result, data, prefix, stringifyDates) {
        for (var key in data) {
            var valuePrefix = prefix ? prefix + "." + key : key,
                value = data[key];
            serialize(result, value, data, key, valuePrefix, stringifyDates);
        }
    }

    function serializeArray(result, data, prefix, stringifyDates) {
        for (var sourceIndex = 0, destinationIndex = 0; sourceIndex < data.length; sourceIndex++) {
            var value = data[sourceIndex],
                key = "[" + destinationIndex + "]",
                valuePrefix = prefix + key;
            serialize(result, value, data, key, valuePrefix, stringifyDates);

            destinationIndex++;
        }
    }

    function serializeFilter(filter, encode) {
       if (filter.filters) {
           return $.map(filter.filters, function(f) {
               var hasChildren = f.filters && f.filters.length > 1,
                   result = serializeFilter(f, encode);

               if (result && hasChildren) {
                   result = "(" + result + ")";
               }

               return result;
           }).join("~" + filter.logic + "~");
       }

       if (filter.field) {
           return filter.field + "~" + filter.operator + "~" + encodeFilterValue(filter.value, encode);
       } else {
           return undefined;
       }
    }

    function encodeFilterValue(value, encode) {
       if (typeof value === "string") {
           if (value.indexOf('Date(') > -1) {
               value = new Date(parseInt(value.replace(/^\/Date\((.*?)\)\/$/, '$1'), 10));
           } else {
               value = value.replace(escapeQuoteRegExp, "''");

               if (encode) {
                   value = encodeURIComponent(value);
               }

               return "'" + value + "'";
           }
       }

       if (value && value.getTime) {
           return "datetime'" + kendo.format("{0:yyyy-MM-ddTHH-mm-ss}", value) + "'";
       }
       return value;
    }

    function translateGroup(group) {
       return {
           value: typeof group.Key !== "undefined" ? group.Key : group.value,
           field: group.Member || group.field,
           hasSubgroups: group.HasSubgroups || group.hasSubgroups || false,
           aggregates: translateAggregate(group.Aggregates || group.aggregates),
           items: group.HasSubgroups ? $.map(group.Items || group.items, translateGroup) : (group.Items || group.items)
       };
    }

    function translateAggregateResults(aggregate) {
       var obj = {};
           obj[aggregate.AggregateMethodName.toLowerCase()] = aggregate.Value;

       return obj;
    }

    function translateAggregate(aggregates) {
        var functionResult = {},
            key,
            functionName,
            aggregate;

        for (key in aggregates) {
            functionResult = {};
            aggregate = aggregates[key];

            for (functionName in aggregate) {
               functionResult[functionName.toLowerCase()] = aggregate[functionName];
            }

            aggregates[key] = functionResult;
        }

        return aggregates;
    }

    function convertAggregates(aggregates) {
        var idx, length, aggregate;
        var result = {};

        for (idx = 0, length = aggregates.length; idx < length; idx++) {
            aggregate = aggregates[idx];
            result[aggregate.Member] = extend(true, result[aggregate.Member], translateAggregateResults(aggregate));
        }

        return result;
    }

    extend(true, kendo.data, {
        schemas: {
            "aspnetmvc-ajax": {
                groups: function(data) {
                    return $.map(this._dataAccessFunction(data), translateGroup);
                },
                aggregates: function(data) {
                    data = data.d || data;
                    var aggregates = data.AggregateResults || [];

                    if (!$.isArray(aggregates)) {
                        for (var key in aggregates) {
                            aggregates[key] = convertAggregates(aggregates[key]);
                        }

                        return aggregates;
                    }

                    return convertAggregates(aggregates);
                }
            }
        }
    });

    extend(true, kendo.data, {
        transports: {
            "aspnetmvc-ajax": kendo.data.RemoteTransport.extend({
                init: function(options) {
                    var that = this,
                        stringifyDates = (options || {}).stringifyDates;

                    kendo.data.RemoteTransport.fn.init.call(this,
                        extend(true, {}, this.options, options, {
                            parameterMap: function(options, operation) {
                                return parameterMap.call(that, options, operation, false, stringifyDates);
                            }
                        })
                    );
                },
                read: function(options) {
                    var data = this.options.data,
                        url = this.options.read.url;

                    if (isPlainObject(data)) {
                        if (url) {
                            this.options.data = null;
                        }

                        if (!data.Data.length && url) {
                            kendo.data.RemoteTransport.fn.read.call(this, options);
                        } else {
                            options.success(data);
                        }
                    } else {
                        kendo.data.RemoteTransport.fn.read.call(this, options);
                    }
                },
                options: {
                    read: {
                        type: "POST"
                    },
                    update: {
                        type: "POST"
                    },
                    create: {
                        type: "POST"
                    },
                    destroy: {
                        type: "POST"
                    },
                    parameterMap: parameterMap,
                    prefix: ""
                }
            })
        }
    });

    extend(true, kendo.data, {
        schemas: {
            "webapi": kendo.data.schemas["aspnetmvc-ajax"]
        }
    });

    extend(true, kendo.data, {
        transports: {
            "webapi": kendo.data.RemoteTransport.extend({
                init: function(options) {
                    var that = this;
                    var stringifyDates = (options || {}).stringifyDates;

                    if (options.update) {
                        var updateUrl = typeof options.update === "string" ? options.update : options.update.url;

                        options.update = extend(options.update, {url: function (data) {
                            return kendo.format(updateUrl, data[options.idField]);
                        }});
                    }

                    if (options.destroy) {
                        var destroyUrl = typeof options.destroy === "string" ? options.destroy : options.destroy.url;

                        options.destroy = extend(options.destroy, {url: function (data) {
                            return kendo.format(destroyUrl, data[options.idField]);
                        }});
                    }

                    if(options.create && typeof options.create === "string") {
                        options.create = {
                            url: options.create
                        };
                    }

                    kendo.data.RemoteTransport.fn.init.call(this,
                        extend(true, {}, this.options, options, {
                            parameterMap: function(options, operation) {
                                return parameterMap.call(that, options, operation, false, stringifyDates);
                            }
                        })
                    );
                },
                read: function(options) {
                    var data = this.options.data,
                        url = this.options.read.url;

                    if (isPlainObject(data)) {
                        if (url) {
                            this.options.data = null;
                        }

                        if (!data.Data.length && url) {
                            kendo.data.RemoteTransport.fn.read.call(this, options);
                        } else {
                            options.success(data);
                        }
                    } else {
                        kendo.data.RemoteTransport.fn.read.call(this, options);
                    }
                },
                options: {
                    read: {
                        type: "GET"
                    },
                    update: {
                        type: "PUT"
                    },
                    create: {
                        type: "POST"
                    },
                    destroy: {
                        type: "DELETE"
                    },
                    parameterMap: parameterMap,
                    prefix: ""
                }
            })
        }
    });

    extend(true, kendo.data, {
        transports: {
            "aspnetmvc-server": kendo.data.RemoteTransport.extend({
                init: function(options) {
                    var that = this;

                    kendo.data.RemoteTransport.fn.init.call(this,
                        extend(options, {
                            parameterMap: function(options, operation) {
                                return parameterMap.call(that, options, operation, true);
                            }
                        }
                    ));
                },
                read: function(options) {
                    var url,
                        prefix = this.options.prefix,
                        params = [prefix + "sort",
                            prefix + "page",
                            prefix + "pageSize",
                            prefix + "group",
                            prefix + "aggregate",
                            prefix + "filter"],
                        regExp = new RegExp("(" + params.join('|') + ")=[^&]*&?", "g"),
                        query;

                    query = location.search.replace(regExp, "").replace("?", "");

                    if (query.length && !(/&$/.test(query))) {
                        query += "&";
                    }

                    options = this.setup(options, "read");

                    url = options.url;

                    if (url.indexOf("?") >= 0) {
                        query = query.replace(/(.*?=.*?)&/g, function(match){
                            if(url.indexOf(match.substr(0, match.indexOf("="))) >= 0){
                               return "";
                            }
                            return match;
                        });
                        url += "&" + query;
                    } else {
                        url += "?" + query;
                    }

                    url += $.map(options.data, function(value, key) {
                        return key + "=" + value;
                    }).join("&");

                    location.href = url;
                }
            })
        }
    });
})(window.kendo.jQuery);

(function ($, undefined) {
    var kendo = window.kendo,
        ui = kendo.ui;

    if (ui && ui.ComboBox) {
        ui.ComboBox.requestData = function (selector) {
            var combobox = $(selector).data("kendoComboBox"),
                filters = combobox.dataSource.filter(),
                value = combobox.input.val();

            if (!filters) {
                value = "";
            }

            return { text: value };
        };
    }

})(window.kendo.jQuery);

(function ($, undefined) {
    var kendo = window.kendo,
        ui = kendo.ui;

    if (ui && ui.DropDownList) {
        ui.DropDownList.requestData = function (selector) {
            var dropdownlist = $(selector).data("kendoDropDownList"),
                filters = dropdownlist.dataSource.filter(),
                filterInput = dropdownlist.filterInput,
                value = filterInput ? filterInput.val() : "";

            if (!filters) {
                value = "";
            }

            return { text: value };
        };
    }

})(window.kendo.jQuery);

(function ($, undefined) {
    var kendo = window.kendo,
        ui = kendo.ui;

    if (ui && ui.MultiSelect) {
        ui.MultiSelect.requestData = function (selector) {
            var multiselect = $(selector).data("kendoMultiSelect");
            var text = multiselect.input.val();
            
            return { text: text !== multiselect.options.placeholder ? text : "" };
        };
    }

})(window.kendo.jQuery);

(function ($, undefined) {
    var kendo = window.kendo,
        ui = kendo.ui,
        extend = $.extend,
        isFunction = $.isFunction;

    extend(true, kendo.data, {
        schemas: {
            "imagebrowser-aspnetmvc": {
                data: function(data) {
                    return data || [];
                },
                model: {
                    id: "name",
                    fields: {
                        name: { field: "Name" },
                        size: { field: "Size" },
                        type: { field: "EntryType", parse: function(value) {  return value == 0 ? "f" : "d" } }
                    }
                }
            }
        }
    });

    extend(true, kendo.data, {
        schemas: {
            "filebrowser-aspnetmvc": kendo.data.schemas["imagebrowser-aspnetmvc"]
        }
    });

    extend(true, kendo.data, {
        transports: {
            "imagebrowser-aspnetmvc": kendo.data.RemoteTransport.extend({
                init: function(options) {
                    kendo.data.RemoteTransport.fn.init.call(this, $.extend(true, {}, this.options, options));
                },
                _call: function(type, options) {
                    options.data = $.extend({}, options.data, { path: this.options.path() });

                    if (isFunction(this.options[type])) {
                        this.options[type].call(this, options);
                    } else {
                        kendo.data.RemoteTransport.fn[type].call(this, options);
                    }
                },
                read: function(options) {
                    this._call("read", options);
                },
                create: function(options) {
                    this._call("create", options);
                },
                destroy: function(options) {
                    this._call("destroy", options);
                },
                update: function() {
                    //updates are handled by the upload
                },
                options: {
                    read: {
                        type: "POST"
                    },
                    update: {
                        type: "POST"
                    },
                    create: {
                        type: "POST"
                    },
                    destroy: {
                        type: "POST"
                    },
                    parameterMap: function(options, type) {
                        if (type != "read") {
                            options.EntryType = options.EntryType === "f" ? 0 : 1;
                        }
                        return options;
                    }
                }
            })
        }
    });

    extend(true, kendo.data, {
        transports: {
            "filebrowser-aspnetmvc": kendo.data.transports["imagebrowser-aspnetmvc"]
        }
    });

})(window.kendo.jQuery);

(function ($, undefined) {
    var nameSpecialCharRegExp = /("|\%|'|\[|\]|\$|\.|\,|\:|\;|\+|\*|\&|\!|\#|\(|\)|<|>|\=|\?|\@|\^|\{|\}|\~|\/|\||`)/g;

    function generateMessages() {
        var name,
            messages = {};

        for (name in validationRules) {
            messages["mvc" + name] = createMessage(name);
        }
        return messages;
    }

    function generateRules() {
         var name,
             rules = {};

         for (name in validationRules) {
             rules["mvc" + name] = createRule(name);
        }
        return rules;
    }

    function extractParams(input, ruleName) {
        var params = {},
            index,
            data = input.data(),
            length = ruleName.length,
            rule,
            key;

        for (key in data) {
            rule = key.toLowerCase();
            index = rule.indexOf(ruleName);
            if (index > -1) {
                rule = rule.substring(index + length, key.length);
                if (rule) {
                    params[rule] = data[key];
                }
            }
        }
        return params;
    }

    function rulesFromData(metadata) {
        var idx,
            length,
            fields = metadata.Fields || [],
            rules = {};

        for (idx = 0, length = fields.length; idx < length; idx++) {
            $.extend(true, rules, rulesForField(fields[idx]));
        }
        return rules;
    }

    function rulesForField(field) {
        var rules = {},
            messages = {},
            fieldName = field.FieldName,
            fieldRules = field.ValidationRules,
            validationType,
            validationParams,
            idx,
            length;

        for (idx = 0, length = fieldRules.length; idx < length; idx++) {
            validationType = fieldRules[idx].ValidationType;
            validationParams = fieldRules[idx].ValidationParameters;

            rules[fieldName + validationType] = createMetaRule(fieldName, validationType, validationParams);

            messages[fieldName + validationType] = createMetaMessage(fieldRules[idx].ErrorMessage);
        }
        return { rules: rules, messages: messages };
    }

    function createMessage(rule) {
        return function (input) {
            return input.attr("data-val-" + rule);
        };
    }

    function createRule(ruleName) {
        return function (input) {
            if (input.filter("[data-val-" + ruleName + "]").length) {
                return validationRules[ruleName](input, extractParams(input, ruleName));
            }
            return true;
        };
    }

    function createMetaMessage(message) {
        return function() { return message; };
    }

    function createMetaRule(fieldName, type, params) {
        return function (input) {
            if (input.filter("[name=" + fieldName + "]").length) {
                return validationRules[type](input, params);
            }
            return true;
        };
    }

    function patternMatcher(value, pattern) {
        if (typeof pattern === "string") {
            pattern = new RegExp('^(?:' + pattern + ')$');
        }
        return pattern.test(value);
    }

    var validationRules = {
        required: function (input) {
            var value = input.val(),
                checkbox = input.filter("[type=checkbox]"),
                name;

            if (checkbox.length) {
                name = checkbox[0].name.replace(nameSpecialCharRegExp, "\\$1");
                var hiddenSelector = "input:hidden[name='" + name + "']";
                var hidden = checkbox.next(hiddenSelector);

                if (!hidden.length) {
                    hidden = checkbox.next("label.k-checkbox-label").next(hiddenSelector);
                }

                if (hidden.length) {
                    value = hidden.val();
                } else {
                    value = input.attr("checked") === "checked";
                }
            }

            return !(value === "" || !value);
        },
        number: function (input) {
            return input.val() === "" || input.val() == null || kendo.parseFloat(input.val()) !== null;
        },
        regex: function (input, params) {
            if (input.val() !== "") {
                return patternMatcher(input.val(), params.pattern);
            }
            return true;
        },
        range: function(input, params) {
            if (input.val() !== "") {
                return this.min(input, params) && this.max(input, params);
            }
            return true;
        },
        min: function(input, params) {
            var min = parseFloat(params.min) || 0,
                val = kendo.parseFloat(input.val());

            return min <= val;
        },
        max: function(input, params) {
            var max = parseFloat(params.max) || 0,
                val = kendo.parseFloat(input.val());

            return val <= max;
        },
        date: function(input) {
            return input.val() === "" || kendo.parseDate(input.val()) !== null;
        },
        length: function(input, params) {
            if (input.val() !== "") {
                var len = $.trim(input.val()).length;
                return (!params.min || len >= (params.min || 0)) && (!params.max || len <= (params.max || 0));
            }
            return true;
        }
    };

    $.extend(true, kendo.ui.validator, {
        rules: generateRules(),
        messages: generateMessages(),
        messageLocators: {
            mvcLocator: {
                locate: function (element, fieldName) {
                    fieldName = fieldName.replace(nameSpecialCharRegExp, "\\$1");
                    return element.find(".field-validation-valid[data-valmsg-for='" + fieldName + "'], .field-validation-error[data-valmsg-for='" + fieldName + "']");
                },
                decorate: function (message, fieldName) {
                    message.addClass("field-validation-error").attr("data-valmsg-for", fieldName || "");
                }
            },
            mvcMetadataLocator: {
                locate: function (element, fieldName) {
                    fieldName = fieldName.replace(nameSpecialCharRegExp, "\\$1");
                    return element.find("#" + fieldName + "_validationMessage.field-validation-valid");
                },
                decorate: function (message, fieldName) {
                    message.addClass("field-validation-error").attr("id", fieldName + "_validationMessage");
                }
            }
        },
        ruleResolvers: {
            mvcMetaDataResolver: {
                resolve: function (element) {
                    var metadata = window.mvcClientValidationMetadata || [];

                    if (metadata.length) {
                        element = $(element);
                        for (var idx = 0; idx < metadata.length; idx++) {
                            if (metadata[idx].FormId == element.attr("id")) {
                                return rulesFromData(metadata[idx]);
                            }
                        }
                    }
                    return {};
                }
            }
        }
    });
})(window.kendo.jQuery);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });