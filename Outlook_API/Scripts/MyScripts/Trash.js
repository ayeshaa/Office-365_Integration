setTimeout(function () {
    var data = angular.element(document.getElementById('InboxID')).scope().EMailDetail;
    alert(data);
}, 1000);

    //$(".inbox-message").after(EMailDetail.Body.Content);


function goReply() {
    debugger;
    var id = $("#emaildetail_ID").val();

    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/home/reply/?id=" + id, //you can get also action attribute from form using form.attr('action')
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

function goForward() {

    var id = $("#emaildetail_ID").val();

    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/Home/Forward/?id=" + id, //you can get also action attribute from form using form.attr('action')
        type: 'Get',
        datatype: "html",
        contentType: false, // Not to set any content header

        //data: formData,
    }).done(function (result) {

        $("#LoadingArea").html(result);
        $("#viewbagTitle").text("Forward Email");
        $('#viewbagTitle').html('Forward Email');
        $('#gif').css('visibility', 'hidden');
    });
}

function CheckEmailAndDelete() {

    var id = $("#emaildetail_ID").val();

    $('#gif').css('visibility', 'visible');
    $.ajax({
        url: "/Mail/Delete/?id=" + id, //you can get also action attribute from form using form.attr('action')
        type: 'Get',
        datatype: "html",
        contentType: false, // Not to set any content header

        //data: formData,
    }).done(function (result) {

        $("#LoadingArea").html(result);
        //$("#viewbagTitle").text("Forward Email");
        //$('#viewbagTitle').html('Forward Email');
        //$('#gif').css('visibility', 'hidden');
    });
}