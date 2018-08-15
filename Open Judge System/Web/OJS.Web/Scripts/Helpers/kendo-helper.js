function escapeSpecialSymbols(text) {
    return text
        .replace(/#/g, '\\#')
        .replace(/'/g, '\\\'')
        .replace(/"/g, '&quot;');
}

function encodeSpecialSymbols(text) {
    return text
        .replace(/"/g, '%22')
        .replace(/#/g, '%23')
        .replace(/&/g, '%26')
        .replace(/'/g, '%27');
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
    buttons.removeAttr('disabled'); // Fixes incorrect Kendo UI enabling of disabled element
    buttons.kendoButton({
        enable: true
    });

    buttons.css({
        'pointer-events': 'auto',
        cursor: 'pointer'
    });
}