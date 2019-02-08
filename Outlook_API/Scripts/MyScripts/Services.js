app.service("InboxService", function ($http) {

    ////Read inbox 
    this.getInbox = function () {

        return $http.get("/Mail/Inbox");
    };
    this.getSent= function () {

        return $http.get("/Mail/Sent");
    };
    this.getDraft= function () {

        return $http.get("/Mail/Draft");
    };
    this.getTrash = function () {

        return $http.get("/Mail/Trash");
    };
    this.getDetail = function (EmailId) {
        return $http.get("/Mail/Detail/" + EmailId);
    };

    this.postMail = function (Member) {
        var request = $http({
            method: "post",
            url: "/Mail/SendEmail",
            data: Member
        });
        return request;
    };  

    this.deleteMail = function (Member) {
        var request = $http({
            method: "post",
            url: "/Mail/Delete",
            data: Member
        });
        return request;
    };

    this.replyMail = function (Member) {
        var request = $http({
            method: "post",
            url: "/Mail/ReplyEmail",
            data: Member
        });
        return request;
    }; 
    this.forwardMail = function (Member) {
        var request = $http({
            method: "post",
            url: "/Mail/ForwardEmail",
            data: Member
        });
        return request;
    }; 

    ////Fundction to Read Student by Student ID  
    //this.getStudent = function (id) {
    //    return $http.get("/api/ManageStudentInfoAPI/" + id);
    //};

    ////Function to create new Student  
    //this.post = function (Student) {
    //    var request = $http({
    //        method: "post",
    //        url: "/api/ManageStudentInfoAPI",
    //        data: Student
    //    });
    //    return request;
    //};

    ////Edit Student By ID   
    //this.put = function (id, Student) {
    //    var request = $http({
    //        method: "put",
    //        url: "/api/ManageStudentInfoAPI/" + id,
    //        data: Student
    //    });
    //    return request;
    //};

    ////Delete Student By Student ID  
    //this.delete = function (id) {
    //    var request = $http({
    //        method: "delete",
    //        url: "/api/ManageStudentInfoAPI/" + id
    //    });
    //    return request;
    //};
}); 
