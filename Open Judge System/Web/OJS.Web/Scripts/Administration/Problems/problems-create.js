// TODO: Add custom tooltip for .zip tests file - give link to sample file with tests

// TODO: Convert these events to unobtrusive with $(parent).on('click')...

function startUploadForm(e) {
    var id = $(e).data('id');
    $("#Resources_" + id + "__File").click();
}

function selectedFile(e) {
    var id = $(e).data('id');
    var fileName = e.files[0].name;

    $('#file-button-' + id).text(fileName.length > 20 ? fileName.substring(0, 20) + '...' : fileName);
}

function onResourceTypeSelect() {
    var val = this.value();
    var id = this.element.attr('modelid');

    var resourceContainer = $('#resource-row-' + id + ' [data-type="resource-content"]');

    // TODO: Refactor these either with methods to generate HTML or better - use hidden div and .hide()/.show()

    if (val == 3) {
        resourceContainer.html('<div class="pull-right" data-type="resource-content">' +
                                    '<label for="Resources_' + id + '__Link">Линк</label>' +
                                    '<input type="text" class = "form-control full-editor" name="Resources[' + id + '].Link" id="Resources_' + id + '__Link" />' +
                                '</div>');
    }
    else {
        resourceContainer.html('<div><strong>Файл</strong></div>' +
                                '<div id="file-button-' + id + '" data-file-upload-button="true" data-id="' + id + '" class="btn btn-sm btn-primary" onclick="startUploadForm(this)">Избери ресурс</div>' +
                                '<div class="hidden-file-upload">' +
                                    '<input type="file" name="Resources[' + id + '].File" id="Resources_' + id + '__File" data-id="' + id + '" onchange="selectedFile(this)" />' +
                                '</div>');
    }
}

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

    $('#tests-tooltip').kendoTooltip({
        content: kendo.template($("#tests-template").html()),
        width: 440,
        position: "bottom"
    });

    $('#add-resource').click(function (e) {
        e.preventDefault();

        var itemIndex = $("#resources input.hidden-field").length;

        $.get("/Administration/Problems/AddResourceForm/" + itemIndex, function (data) {
            $("#resources").append(data);

            $('#remove-resource').removeAttr('disabled');
        });
    });

    $('#remove-resource').click(function (e) {
        e.preventDefault();

        if ($(this).is(':disabled')) {
            return false;
        }

        var itemIndex = $("#resources input.hidden-field").length - 1;

        $('#resource-row-' + itemIndex).remove();

        if (itemIndex == 0) {
            $('#remove-resource').attr('disabled', 'disabled');
        }
    });

    $('#tests-file-button').click(function (e) {
        $('#tests-upload-input').click();
    });

    $('#tests-upload-input').change(function (e) {
        var fileName = this.files[0].name;
        $('#tests-file-button').text(fileName.length > 30 ? fileName.substring(0, 30) + '...' : fileName);
    });
});