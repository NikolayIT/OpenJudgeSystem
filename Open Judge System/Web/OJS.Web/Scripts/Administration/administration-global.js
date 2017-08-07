function validateModelStateErrors(args) {
    'use strict';

    if (args.errors) {
        var grid = $('#DataGrid').data('kendoGrid');
        var validationTemplate = kendo.template($('#model-state-errors-template').html());
        $('.k-edit-form-container').prepend('<div id="errors" class="alert alert-danger"></div>');
        grid.one('dataBinding', function (e) {
            e.preventDefault();

            $.each(args.errors, function (propertyName) {
                var renderedTemplate = validationTemplate({ field: propertyName, errors: this.errors });
                grid.editable.element.find('#errors').append(renderedTemplate);
            });

            $('.k-grid-update').click(function (ev) {
                ev.preventDefault();
                $('#errors').remove();
            });
        });
    }
}

