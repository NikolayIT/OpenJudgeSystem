/* exported additionalFilesValidation */
function additionalFilesValidation(e) {
    'use strict';

    var files = e.files;
    var validationBox = $('span[data-valmsg-for="AdditionalFiles"]');
    validateZipFile(files, validationBox, $('#edit-form form'));
}

$(document).ready(function () {
    'use strict';

    $.validator.addMethod(
        'date',
        function (value, element) {
            if (this.optional(element)) {
                return true;
            }

            var result = false;
            try {
                var date = kendo.parseDate(value, 'dd/MM/yyyy HH:mm:ss');
                result = true;
                if (!date) {
                    result = false;
                }
            } catch (err) {
                result = false;
            }

            return result;
        },
        ''
    );

    $.validator.setDefaults({ ignore: '' });

    var form = $('#edit-form form');
    var input = $('#SourceCodeSizeLimit');
    var numericTextBox = input.data('kendoNumericTextBox');
    var checkbox = $('#enable-sclimit');

    if (numericTextBox.value() != null && parseInt(numericTextBox.value()) !== 0) {
        checkbox.attr('checked', true);
        numericTextBox.enable(true);
        input.attr('data-val-required', 'Лимита е задължителен!');
        reparseForm(form);
    }

    checkbox.change(function () {
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

    addSubmitValidation($('#edit-form form'),
        $("input[name^='SubmissionTypes'][name$='.IsChecked']"),
        $("span[data-valmsg-for='SelectedSubmissionTypes']"),
        isChecked,
        'Choose at least one submission Type!');

    $('#checkers-tooltip').kendoTooltip({
        content: kendo.template($('#checkers-template').html()),
        width: 580,
        position: 'bottom'
    });
});
