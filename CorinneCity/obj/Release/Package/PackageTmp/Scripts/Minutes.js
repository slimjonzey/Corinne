$(function () {
    $("#item").hide();

    $("#treeview > li a").click(function () {
        $(this).next().slideToggle("slow");
    });
});

function appendMinutes(content) {
    document.getElementById('showMinutes').innerHTML = content;
}