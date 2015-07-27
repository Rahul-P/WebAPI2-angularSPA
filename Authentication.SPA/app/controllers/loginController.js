
"use strict";

authenticationApp.controller("loginController",
    ["$scope", "$location", "authService",
        function ($scope, $location, authService) {

            $scope.loginData = {
                userName: "",
                password: "",
                useRefreshTokens: false
            };

            $scope.message = "";

            $scope.login = function () {
                authService.login($scope.loginData)
                    .then(function (response) {
                        $location.path('/authenticatedUser');
                    },
                    function (err) {
                        $scope.message = err.error_description;
                    });
            };
}]);