$(function () {
    var myCommands = elFinder.prototype._options.commands;
    var disabled = ['extract', 'archive', 'help', 'select'];
    $.each(disabled, function (i, cmd) {
        var idx;
        (idx = $.inArray(cmd, myCommands)) !== -1 && myCommands.splice(idx, 1);
    });
    var selectedFile = null;
    var options = {
        url: '/Administration/Files/connector',
        commands: myCommands,
        lang: 'en',
        uiOptions: {
            toolbar: [
                ['back', 'forward'],
                ['reload'],
                ['home', 'up'],
                ['mkdir', 'mkfile', 'upload'],
                ['open', 'download'],
                ['info'],
                ['quicklook'],
                ['copy', 'cut', 'paste'],
                ['rm'],
                ['duplicate', 'rename', 'edit', 'resize'],
                ['view', 'sort']
            ]
        },
        handlers: {
            select: function (event, elfinderInstance) {

                if (event.data.selected.length == 1) {
                    var item = $('#' + event.data.selected[0]);
                    if (!item.hasClass('directory')) {
                        selectedFile = event.data.selected[0];
                        $('#elfinder-selectFile').show();
                        return;
                    }
                }
                $('#elfinder-selectFile').hide();
                selectedFile = null;
            }
        }
    };
    $('#elfinder').elfinder(options).elfinder('instance');

    $('.elfinder-toolbar:first').append('<div class="ui-widget-content ui-corner-all elfinder-buttonset" id="elfinder-selectFile" style="display:none; float:right;">' +
    '<div class="ui-state-default elfinder-button" title="Select" style="width: 100px;"></div>');
    $('#elfinder-selectFile').click(function () {
        if (selectedFile != null)
            $.post('files/selectFile', { target: selectedFile }, function (response) {
                alert(response);
            });

    });
});