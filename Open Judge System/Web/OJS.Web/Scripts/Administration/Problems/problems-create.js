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

function validateFile(files, validationBox) {
    if (files) {
        if (files[0].extension.toLowerCase() !== ".zip") {
            validationBox.attr("class", "field-validation-error");
            validationBox.text("Uploaded file must be in .zip format");
            e.preventDefault();
        } else {
            validationBox.attr("class", "field-validation-valid");
            validationBox.text("");
        }
    }

    $("#create-form form").removeData("validator");
    $("#create-form  form").removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse($('#create-form form'));
}

function additionalFilesValidation(e) {
    var files = e.files;
    var validationBox = $("span[data-valmsg-for='AdditionalFiles']");
    validateFile(files, validationBox);
}

function testsValidation(e) {
    var files = e.files;
    var validationBox = $("span[data-valmsg-for='Tests']");
    validateFile(files, validationBox);
}

function onResourceTypeSelect() {
    var val = this.value();
    var id = this.element.attr('modelid');

    var resourceContainer = $('#resource-row-' + id + ' [data-type="resource-content"]');

    // TODO: Refactor these either with methods to generate HTML or better - use hidden div and .hide()/.show()

    if (val == 3) {
        resourceContainer.html('<div class="pull-right" data-type="resource-content">' +
                                    '<label for="Resources_' + id + '__Link">Линк</label>' +
                                    '<input type="text" class="form-control full-editor" name="Resources[' + id + '].RawLink" id="Resources_' + id + '__Link" />' +
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

    $.validator.setDefaults({ ignore: '' });

    var input = $("#SourceCodeSizeLimit");
    var numericTextBox = input.data("kendoNumericTextBox");
    var checkbox = $('#enable-sclimit');

    if (numericTextBox.value() != null && numericTextBox.value() != 0) {
        checkbox.attr('checked', true);
        numericTextBox.enable(true);
        input.attr("data-val-required", "Лимита е задължителен!");

        $("#create-form form").removeData("validator");
        $("#create-form form").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse($('#create-form form'));
    }

    $('#enable-sclimit').change(function () {
        if ($(this).is(':checked')) {
            numericTextBox.enable(true);
            input.attr("data-val-required", "Лимита е задължителен!");

            $("#create-form form").removeData("validator");
            $("#create-form form").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse($('#create-form form'));
        }
        else {
            numericTextBox.enable(false);
            input.removeAttr("data-val-required");

            $("#create-form form").removeData("validator");
            $("#create-form form").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse($('#create-form form'));
        }
    });

    $("#create-form form").on("submit", function(ev) {
        var submissionTypes = $('input[name^="SubmissionTypes"][name$=".IsChecked"]');
        var submissionTypesValidation = $("span[data-valmsg-for='SelectedSubmissionTypes']");
        if (submissionTypes.is(":checked")) {
            submissionTypesValidation.attr('class', 'field-validation-valid');
            submissionTypesValidation.text("");
        } else {
            submissionTypesValidation.attr('class', 'field-validation-error');
            submissionTypesValidation.text("Choose at least one submission Type!");
            ev.preventDefault();
        }
    });

    //$(".main-container form").validate({
    //    submitHandler: function(form) {
    //        var submissionTypes = $('input[name^="SubmissionTypes"][name$=".IsChecked"]');
    //        var submissionTypesValidation = $("span[data-valmsg-for='SelectedSubmissionTypes']");
    //        if (submissionTypes.is(":checked")) {
    //            submissionTypesValidation.attr('class', 'field-validation-valid');
    //            submissionTypesValidation.text("");
    //            form.submit();
    //        }

    //        submissionTypesValidation.attr('class', 'field-validation-error');
    //        submissionTypesValidation.text("Choose at least one submission Type!");
    //        $(this).cancelSubmit = true;
    //    }
    //});

    $('#checkers-tooltip').kendoTooltip({
        content: kendo.template($("#checkers-template").html()),
        width: 580,
        position: "bottom"
    });

    $('#add-resource').click(function (e) {
        e.preventDefault();

        var itemIndex = $("#resources input.hidden-field").length;

        $.get("/Administration/Problems/AddResourceForm/" + itemIndex, function (data) {
            $("#resources").append(data);

            $('#remove-resource').removeAttr('disabled');

            $('#resources .required-resource-field').each(function () {
                $(this).rules("add", { required: true, messages: { required: "Задължително поле" } });
            });
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
});