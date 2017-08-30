// TODO: Convert these events to unobtrusive with $(parent).on('click')...

/* exported startUploadForm */
function startUploadForm(e) {
    'use strict';

    var id = $(e).data('id');
    $('#Resources_' + id + '__File').click();
}

/* exported selectedFile */
function selectedFile(e) {
    'use strict';

    var id = $(e).data('id');
    var fileName = e.files[0].name;
    $('#file-button-' + id).text(fileName.length > 20 ? fileName.substring(0, 20) + '...' : fileName);
}

/* exported additionalFilesValidation */
function additionalFilesValidation(e) {
    'use strict';

    var files = e.files;
    var validationBox = $('span[data-valmsg-for="AdditionalFiles"]');
    validateZipFile(files, validationBox, $('#create-form form'));
}

/* exported testsValidation */
function testsValidation(e) {
    'use strict';

    var files = e.files;
    var validationBox = $('span[data-valmsg-for="Tests"]');
    validateZipFile(files, validationBox, $('#create-form form'));
}

/* exported onResourceTypeSelect */
function onResourceTypeSelect() {
    'use strict';

    var val = parseInt(this.value());
    var id = this.element.attr('modelid');

    var resourceContainer = $('#resource-row-' + id + ' [data-type="resource-content"]');

    // TODO: Refactor these either with methods to generate HTML or better - use hidden div and .hide()/.show()

    if (val === 3) {
        resourceContainer.html("<div class='pull-right' data-type='resource-content'>" +
            "<label for='Resources_" + id + "__Link'>Линк</label>" +
            "<input type='text' class='form-control full-editor' name='Resources[" + id +
            "].RawLink' id='Resources_" + id + "__Link' />" +
            '</div>');
    } else {
        resourceContainer.html('<div><strong>Файл</strong></div>' +
            "<div id='file-button-" + id + "' data-file-upload-button='true' data-id='" + id +
            "' class='btn btn-sm btn-primary' onclick='startUploadForm(this)'>Избери ресурс</div>" +
            "<div class='hidden-file-upload'>" +
            "<input type='file' name='Resources[" + id +
            "].File' id='Resources_" + id + "__File' data-id='" + id +
            "' onchange='selectedFile(this)' />" +
            '</div>');
    }
}

$(document).ready(function () {
    'use strict';

    $.validator.setDefaults({ ignore: '' });

    var form = $('#create-form form');
    var input = $('#SourceCodeSizeLimit');
    var numericTextBox = input.data('kendoNumericTextBox');
    var checkbox = $('#enable-sclimit');

    if (numericTextBox.value() != null && parseInt(numericTextBox.value()) !== 0) {
        checkbox.attr('checked', true);
        numericTextBox.enable(true);
        input.attr('data-val-required', 'Лимита е задължителен!');
        reparseForm(form);
    }

    $('#enable-sclimit').change(function () {
        if ($(this).is(':checked')) {
            numericTextBox.enable(true);
            input.attr('data-val-required', 'Лимита е задължителен!');
            reparseForm(form);
        } else {
            numericTextBox.enable(false);
            input.removeAttr('data-val-required');
            reparseForm(form);
        }
    });

    function isChecked(elements) {
        return elements.is(':checked');
    }

    addSubmitValidation($('#create-form form'),
        $("input[name^='SubmissionTypes'][name$='.IsChecked']"),
        $("span[data-valmsg-for='SelectedSubmissionTypes']"),
        isChecked,
        'Choose at least one submission Type!');

    $('#checkers-tooltip').kendoTooltip({
        content: kendo.template($('#checkers-template').html()),
        width: 580,
        position: 'bottom'
    });

    /* eslint max-nested-callbacks: 0 */
    // TODO: Callback hell - refactor using promises or async/await
    $('#add-resource').click(function (e) {
        e.preventDefault();

        var itemIndex = $('#resources input.hidden-field').length;

        $.get('/Administration/Problems/AddResourceForm/' + itemIndex, function (data) {
            $('#resources').append(data);

            $('#remove-resource').removeAttr('disabled');

            $('#resources .required-resource-field').each(function () {
                $(this).rules('add', { required: true, messages: { required: 'Задължително поле' } });
            });
        });
    });

    $('#remove-resource').click(function (e) {
        e.preventDefault();

        if ($(this).is(':disabled')) {
            return false;
        }

        var itemIndex = $('#resources input.hidden-field').length - 1;

        $('#resource-row-' + itemIndex).remove();

        if (itemIndex === 0) {
            $('#remove-resource').attr('disabled', 'disabled');
        }
    });
});
