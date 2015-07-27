
"use strict";

authenticationApp.controller("authenticatedUserController",
    ["$scope", "authenticatedUserService",
        function ($scope, authenticatedUserService) {

            $scope.protectedResources = [];

            authenticatedUserService.getProtectedResource()
                .then(function (results) {
                    $scope.protectedResources = results.data;
                }, function (error) {
                    //alert(error.data.message);
                });
    }]);