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