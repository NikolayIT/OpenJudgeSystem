$(document).ready(function () {

    $('#enable-sclimit').change(function () {
        var numericTextBox = $("#SourceCodeSizeLimit").data("kendoNumericTextBox");

        if ($(this).is(':checked')) {
            numericTextBox.enable(true);
        }
        else {
            numericTextBox.value(null)
            numericTextBox.enable(false);
        }
    });

    $('#checkers-tooltip').kendoTooltip({
        content: kendo.template($("#checkers-template").html()),
        width: 580,
        position: "bottom"
    });
});