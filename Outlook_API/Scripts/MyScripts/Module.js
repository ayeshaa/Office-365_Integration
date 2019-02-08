var app = angular.module("ApplicationModule", ["ngRoute", "ngSanitize"]);

app.factory("ShareData", function () {
    return { value: 0 }
});

//Showing Routing  
app.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
    debugger;
    $routeProvider.when('/Inbox',
        {
            templateUrl: 'Pages/Inbox.html',
            controller: 'InboxController',
            title: 'Inbox'
        });
    $routeProvider.when('/Mail',
        {
            templateUrl: 'Pages/Inbox.html',
            controller: 'InboxController',
            title: 'Inbox'
        });
    $routeProvider.when('/Sent',
        {
            templateUrl: 'Pages/Sent.html',
            controller: 'InboxController',
            title: 'Sent Items'
        });
    $routeProvider.when("/Draft",
        {
            templateUrl: 'Pages/Draft.html',
            controller: 'InboxController',
            title:'Draft'
        });
    $routeProvider.when('/Trash',
        {
            templateUrl: 'Pages/Trash.html',
            controller: 'InboxController',
            title: 'Trash'
        });
    $routeProvider.when('/Detail',
        {
            templateUrl: 'Pages/Detail.html',
            controller: 'InboxController',
            title: 'Detail'
        });
    $routeProvider.when('/Compose',
        {
            templateUrl: 'Pages/Compose.html',
            controller: 'InboxController',
            title: 'New Message'
        });
    $routeProvider.when('/Reply',
        {
            templateUrl: 'Pages/Reply.html',
            controller: 'InboxController',
            title: 'Reply'
        });
    $routeProvider.when('/Forward',
        {
            templateUrl: 'Pages/Forward.html',
            controller: 'InboxController',
            title: 'Forward'
        });
    $routeProvider.otherwise(
        {
            templateUrl: 'Pages/Inbox.html',
            controller: 'InboxController',
            title: 'Inbox'
        });

    //$locationProvider.html5Mode(true).hashPrefix('')
    $locationProvider.hashPrefix('#');
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: true
    });
}]);