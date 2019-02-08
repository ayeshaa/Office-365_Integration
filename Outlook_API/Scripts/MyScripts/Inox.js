function OpenDetail(id) {
    debugger;
    var id = $("#" + id).val();

    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/home/detail/?id=" + id, //you can get also action attribute from form using form.attr('action')
        type: 'Get',
        datatype: "html",
        contentType: false, // Not to set any content header

        //data: formData,
    }).done(function (result) {
        debugger;
        $("#LoadingArea").html(result);
        $("#viewbagTitle").text("Sent");
        $('#viewbagTitle').html('Sent');
        $('#gif').css('visibility', 'hidden');
    });
}


function OpenTrashDetail(id) {
    var id = $("#" + id).val();

    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/home/TrashDetail/?id=" + id, //you can get also action attribute from form using form.attr('action')
        type: 'Get',
        datatype: "html",
        contentType: false, // Not to set any content header

        //data: formData,
    }).done(function (result) {

        $("#LoadingArea").html(result);
        $("#viewbagTitle").text("Sent");
        $('#viewbagTitle').html('Sent');
        $('#gif').css('visibility', 'hidden');
    });
}

function LoadInbox() {
    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/Home/Inbox", //you can get also action attribute from form using form.attr('action')
        type: 'Get',
        datatype: "html",
        contentType: false, // Not to set any content header

        //data: formData,
    }).done(function (result) {

        $("#LoadingArea").html(result);
        $("#viewbagTitle").text("Inbox");
        $('#viewbagTitle').html('Inbox');
        $('#gif').css('visibility', 'hidden');
    });
}

setInterval(function () {
    // LoadInbox();
}, 480000);