/*
* Kendo UI v2014.3.1411 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([], f);
})(function(){

(function( window, undefined ) {
    var kendo = window.kendo || (window.kendo = { cultures: {} });
    kendo.cultures["bg"] = {
        name: "bg",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": " ",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-n $","n $"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "лв."
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["неделя","понеделник","вторник","сряда","четвъртък","петък","събота"],
                    namesAbbr: ["нед","пон","вт","ср","четв","пет","съб"],
                    namesShort: ["н","п","в","с","ч","п","с"]
                },
                months: {
                    names: ["януари","февруари","март","април","май","юни","юли","август","септември","октомври","ноември","декември",""],
                    namesAbbr: ["ян","февр","март","апр","май","юни","юли","авг","септ","окт","ноември","дек",""]
                },
                AM: [""],
                PM: [""],
                patterns: {
                    d: "d.M.yyyy 'г.'",
                    D: "dd MMMM yyyy 'г.'",
                    F: "dd MMMM yyyy 'г.' HH:mm:ss 'ч.'",
                    g: "d.M.yyyy 'г.' HH:mm 'ч.'",
                    G: "d.M.yyyy 'г.' HH:mm:ss 'ч.'",
                    m: "dd MMMM",
                    M: "dd MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "HH:mm 'ч.'",
                    T: "HH:mm:ss 'ч.'",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM yyyy 'г.'",
                    Y: "MMMM yyyy 'г.'"
                },
                "/": ".",
                ":": ":",
                firstDay: 1
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });