function escapeSpecialSymbols(text) {
    return text
        .replace(/#/g, '\\#')
        .replace(/'/g, '\\\'')
        .replace(/"/g, '&quot;');
}