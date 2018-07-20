function escapeSpecialSymbols(text) {
    return text.replace('#', '\\#');
}

function encodeSpecialSymbols(text) {
    return text.replace('#', '%23');
}

function disableKendoButtons(buttons) {
    buttons.kendoButton({
        enable: false
    });

    buttons.css({
        'pointer-events': 'none',
        cursor: 'default'
    });
}

function enableKendoButtons(buttons) {
    buttons.removeAttr('disabled'); // Fixes incorrect kendo ui enabling of disabled element
    buttons.kendoButton({
        enable: true
    });

    buttons.css({
        'pointer-events': 'auto',
        cursor: 'pointer'
    });
}