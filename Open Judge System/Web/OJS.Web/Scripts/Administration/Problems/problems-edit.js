$(document).ready(function () {
    var numericTextBox = $("#SourceCodeSizeLimit").data("kendoNumericTextBox");
    var checkbox = $('#enable-sclimit');

    if (numericTextBox.value() != null && numericTextBox.value() != 0)
    {
        checkbox.attr('checked', true);
        numericTextBox.enable(true);
    }

    checkbox.change(function () {

        if ($(this).is(':checked')) {
            numericTextBox.enable(true);
        }
        else {
            numericTextBox.enable(false);
        }
    });

    $('#checkers-tooltip').kendoTooltip({
        content: kendo.template($("#checkers-template").html()),
        width: 580,
        position: "bottom"
    });
});