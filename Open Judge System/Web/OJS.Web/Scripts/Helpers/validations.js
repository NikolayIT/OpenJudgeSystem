function addSubmitValidation(form, elements, validartionField, validationFunction, validationMessage) {
    form.on("submit", function (ev) {
        if (validationFunction(elements)) {
            validartionField.attr("class", "field-validation-valid");
            validartionField.text("");
        } else {
            validartionField.attr("class", "field-validation-error");
            validartionField.text(validationMessage);
            ev.preventDefault();
        }
    });
}

function reparseForm(form) {
    form.removeData("validator");
    form.removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
}

function validateZipFile(files, validationField, form) {
    if (files) {
        if (files[0].extension.toLowerCase() !== ".zip") {
            validationField.attr("class", "field-validation-error");
            validationField.text("Uploaded file must be in .zip format");
            e.preventDefault();
        } else {
            validationField.attr("class", "field-validation-valid");
            validationField.text("");
        }
    }

    reparseForm(form);
}