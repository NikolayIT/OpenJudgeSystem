/* exported validateModelStateErrors */
function validateModelStateErrors(args) {
    'use strict';

    if (args.errors) {
        var grid = $('#DataGrid');
        var kendoGrid = grid.data('kendoGrid');
        var validationTemplate = kendo.template($('#model-state-errors-template').html());
        var errorsContainer = $('<div id="errors" class="alert alert-danger"></div>');

        $('.k-edit-form-container').prepend(errorsContainer);
        kendoGrid.one('dataBinding', function (ev) {
            ev.preventDefault();

            $.each(args.errors, function (propertyName) {
                var renderedTemplate = validationTemplate({ field: propertyName, errors: this.errors });

                if (kendoGrid.editable) {
                    kendoGrid.editable.element.find('#errors').append(renderedTemplate);
                } else {
                    errorsContainer.append(renderedTemplate);
                    grid.before(errorsContainer);
                    kendoGrid.cancelChanges();

                    grid.find('*').click(function () {
                        removeErrorsContainer();
                    });
                }
            });

            $('.k-grid-update').click(function (e) {
                e.preventDefault();
                removeErrorsContainer();
            });

            $('#errors').click(function () {
                removeErrorsContainer();
            });

            $('ul').css('list-style', 'none');
        });
    }

    function removeErrorsContainer() {
        $('#errors').remove();
    }
}

function prepareAlertMessage(message, isSuccess) {
    var messageContainer = $('<div class="alert"></div>');

    if (isSuccess) {
        messageContainer.addClass('alert-success');
    } else {
        messageContainer.addClass('alert-danger');
    }

    if (message.constructor === Array) {
        messageContainer.text(message.join('\r\n'));
    } else {
        messageContainer.text(message);
    }
    
    messageContainer.click(function () {
        messageContainer.hide();
    });

    return messageContainer;
}