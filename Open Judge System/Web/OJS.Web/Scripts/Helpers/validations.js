// TODO: Add to module

/* exported addSubmitValidation */
function addSubmitValidation(form, elements, validartionField, validationFunction, validationMessage) {
    'use strict';

    form.on('submit', function (ev) {
        if (validationFunction(elements)) {
            validartionField.attr('class', 'field-validation-valid');
            validartionField.text('');
        } else {
            validartionField.attr('class', 'field-validation-error');
            validartionField.text(validationMessage);
            ev.preventDefault();
        }
    });
}

/* exported validateZipFile */
function validateZipFile(files, validationField, form) {
    'use strict';

    if (files) {
        if (files[0].extension.toLowerCase() === '.zip') {
            validationField.attr('class', 'field-validation-valid');
            validationField.text('');
        } else {
            validationField.attr('class', 'field-validation-error');
            validationField.text('Uploaded file must be in .zip format');
            e.preventDefault();
        }
    }

    reparseForm(form);
}

var reparseForm = function (form) {
    'use strict';

    form.removeData('validator');
    form.removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(form);
};
