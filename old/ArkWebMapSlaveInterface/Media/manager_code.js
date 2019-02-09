function ActivateButton(text) {
    var b = document.getElementById('main_btn');
    b.innerText = text;
    b.className = "nb_button_blue nb_big_padding_bottom";
};

function DeactivateButton(text) {
    var b = document.getElementById('main_btn');
    b.innerText = text;
    b.className = "nb_button_blue nb_big_padding_bottom nb_btn_disable";
};

function SetValue(id, value) {
    document.getElementById(id).value = value;
}