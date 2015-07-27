
var authenticationApp = angular.module("authenticationApp",
    ["ngRoute", "LocalStorageModule", "angular-loading-bar"]);

authenticationApp.config(
    ["$routeProvider",
        function ($routeProvider) {

            $routeProvider
                .when("/home",
                {
                    templateUrl: "/app/views/home.html",
                    controller: "homeController"
                })
                .when("/login",
                {
                    templateUrl: "/app/views/login.html",
                    controller: "loginController"
                })
                .when("/signup",
                {
                    templateUrl: "/app/views/signup.html",
                    controller: "signupController"
                })
                .when("/refresh",
                {
                    templateUrl: "/app/views/refresh.html",
                    controller: "refreshController"
                })
                .when("/tokens",
                {
                    templateUrl: "/app/views/tokens.html",
                    controller: "tokensManagerController"
                })
                .when("/associate",
                {
                    templateUrl: "/app/views/associate.html",
                    controller: "associateController"
                })
                .when("/authenticatedUser",
                {
                    templateUrl: "/app/views/authenticatedUser.html",
                    controller: "authenticatedUserController"
                })            
                .otherwise({
                    redirectTo: "/home"
                });
        }]);

//var serviceBase = 'http://localhost:3157/';

var serviceBase = 'http://logicmonk-Authentication.azurewebsites.net/';


authenticationApp.constant("ngAuthSettings",
    {
        apiServiceBaseUri: serviceBase,
        clientId: "ngAuthApp"
    });


//By doing this there is no need to setup extra code for 
//setting up tokens or checking the status code, any 
//AngularJS service executes XHR requests will use 
//this interceptor. Note: this will work if you 
//are using AngularJS service $http or $resource.
authenticationApp.config(
    ["$httpProvider",
        function ($httpProvider) {
            $httpProvider.interceptors.push("authInterceptorService")
        }]);


authenticationApp.run(
    ["authService",
    function (authService) {
        authService.fillAuthData();
    }]);
