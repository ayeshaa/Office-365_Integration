app.controller('InboxController', function ($templateCache, $scope, $rootScope, $route, $sce, $filter, $location, InboxService, ShareData) {
    $rootScope.$on('$routeChangeSuccess', function () {
        $rootScope.pageTitle = $route.current.title
    });

    $rootScope.selectedEmails = [];
    $scope.showDelete = true;
    if ($scope.pageTitle == 'Detail' || $scope.pageTitle == 'Reply') {
        $scope.showDelete = false;
    }
    // and fire it after definition
    //init();
    $scope.init = function loadAllInboxRecords() {
        $scope.CurrentFolder = "Inbox";
        var promiseGetInbox = InboxService.getInbox();

        promiseGetInbox.then(function (pl) {
            
            $scope.EMails = pl.data },
            function (errorPl) {
                alert(errorPl);
                $scope.error = errorPl;
            });
    }

    $scope.sentit = function loadAllSentRecords() {
        $scope.CurrentFolder = "Sent Items";
        var promiseGetInbox = InboxService.getSent();

        promiseGetInbox.then(function (pl) {
            $scope.EMails = pl.data
            console.log(pl.data);
        },
            function (errorPl) {
                alert(errorPl);
                $scope.error = errorPl;
            });
    }
    $scope.draftit = function loadAllDraftRecords() {
        $scope.CurrentFolder = "Drafts";
        var promiseGetInbox = InboxService.getDraft();

        promiseGetInbox.then(function (pl) {
            $scope.EMails = pl.data
        },
            function (errorPl) {
                alert(errorPl);
                $scope.error = errorPl;
            });
    }
    $scope.trashit = function loadAllTrashRecords() {
        $scope.CurrentFolder = "Trash";
        var promiseGetInbox = InboxService.getTrash();

        promiseGetInbox.then(function (pl) {
            $scope.EMails = pl.data
        },
            function (errorPl) {
                alert(errorPl);
                $scope.error = errorPl;
            });
    }
    $scope.detailit = function () {
        var promiseGetInbox = InboxService.getDetail(ShareData.value);

        promiseGetInbox.then(function (pl) {
            $scope.EMailDetail = pl.data
            $scope.EmailBody = $sce.trustAsHtml(pl.data.body.content);
            console.log(pl.data);
        },
            function (errorPl) {
                alert(errorPl);
                $scope.error = errorPl;
            });
    }

    //To Edit Student Information  
    $scope.getdetail = function (EmailID) {
        ShareData.value = EmailID;
        $location.path("/Detail");  
    }
    $scope.SendEmail = function () {
        
        //var body = $('.note-editable panel-body').html();
        var markupStr = $('#emailbody').summernote('code');
        
        var Member = {
            EmailTo: $scope.recipient,
            EmailSubject: $scope.subject,
            EmailBody: markupStr, 
        };

        var promisePost = InboxService.postMail(Member);

        promisePost.then(function (pl) {
            if (pl.data == "Success") {
                alert("Email Send Successfully.");
                $location.path("/Inbox");
            }
            else {
                alert(pl.data);
            }
            
        },
            function (errorPl) {
                $scope.error = 'failure sending email', errorPl;
            });  
        
           
    }

    $scope.CheckEmail = function (id) {
        if ($rootScope.selectedEmails.indexOf(id) !== -1) {
            var index = $rootScope.selectedEmails.indexOf(id);
            $rootScope.selectedEmails.splice(index, 1);   
        } else {
            $rootScope.selectedEmails.push(id);
        }
        
    }
    $scope.CheckEmailAndDelete = function (id) {
        $rootScope.selectedEmails.push(id);
        $scope.DeleteEmail();
    }
    $scope.DeleteEmail = function () {
        var emailArray = $rootScope.selectedEmails;
        if (emailArray.length > 0) {

            var promisePost = InboxService.deleteMail(emailArray);

            promisePost.then(function (pl) {
                if (pl.data == "Success") {
                    alert("Delete Successfully.");
                    var currentPageTemplate = $route.current.templateUrl;
                    $templateCache.remove(currentPageTemplate);
                    $route.reload();
                }
                else {
                    alert(pl.data);
                }
            },
                function (errorPl) {
                    $scope.error = 'failure deleting email', errorPl;
                });
        }
        else {
            alert("Select email to delete.");
        }
    }

    $scope.replyit = function () {
        console.log(ShareData.value);
        $scope.recipient = ShareData.value.from.emailAddress.address;
        $scope.subject = 'RE:' + ShareData.value.subject;
        $scope.replytoid = ShareData.value.id;
        var html = '<br/><br/><br/><hr />' + ShareData.value.body.content;
        $("#emailbody").summernote('code', html );
    }
    $scope.goReply = function (emailObj) {
        ShareData.value = emailObj;
        $location.path("/Reply");
    }
    $scope.ReplyEmail = function () {
        var markupStr = $('#emailbody').summernote('code');

        var Member = {
            EmailToId: $scope.replytoid,
            EmailTo: $scope.recipient,
            EmailSubject: $scope.subject,
            EmailBody: markupStr,
        };

        var promisePost = InboxService.replyMail(Member);

        promisePost.then(function (pl) {
            if (pl.data == "Success") {
                alert("Email Send Successfully.");
                $location.path("/Inbox");
            }
            else {
                alert(pl.data);
            }

        },
            function (errorPl) {
                $scope.error = 'failure sending email', errorPl;
            });  
    }

    $scope.forwardit = function () {
        $scope.subject = 'FW:' + ShareData.value.subject;
        $scope.replytoid = ShareData.value.id;
        var html = '<br/><br/><br/><hr />' + ShareData.value.body.content;
        $("#emailbody").summernote('code', html);
    }
    $scope.goForward = function (emailObj) {
        ShareData.value = emailObj;
        $location.path("/Forward");
    }
    $scope.ForwardEmail = function () {
        var markupStr = $('#emailbody').summernote('code');
        var parts = markupStr.split('<br/><br/><br/><hr />');
        var res = parts[0];
        var Member = {
            EmailToId: $scope.replytoid,
            EmailTo: $scope.recipient,
            EmailSubject: $scope.subject,
            EmailBody: res,
        };

        var promisePost = InboxService.forwardMail(Member);

        promisePost.then(function (pl) {
            if (pl.data == "Success") {
                alert("Email Send Successfully.");
                $location.path("/Inbox");
            }
            else {
                alert(pl.data);
            }

        },
            function (errorPl) {
                $scope.error = 'failure sending email', errorPl;
            });
    }
    ////To Delete a Student  
    //$scope.deleteStudent = function (StudentID) {
    //    ShareData.value = StudentID;
    //    $location.path("/deleteStudent");
    //}
});
