function onEditResourceTypeSelect() {
    var val = this.value();
    hideInput(val);
}

function hideInput(val) {
    if (val == 3) {
        $('#file-select').hide();
        $('#link-input').show();
    }
    else {
        $('#link-input').hide();
        $('#file-select').show();
    }
}

$(document).ready(function () {
    var value = $('#Type').data('kendoDropDownList').value();
    hideInput(value);

    $('#file-button-resource').click(function () {
        $('#input-file-resource').click();
    })

    $('#input-file-resource').change(function () {
        var fileName = this.files[0].name;
        $('#file-button-resource').text(fileName.length > 20 ? fileName.substring(0, 20) + '...' : fileName);
    })
});