
"use strict";

authenticationApp.factory("authenticatedUserService",
    ["$http", "ngAuthSettings", function ($http, ngAuthSettings) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var authenticatedUserServiceFactory = {};

        var _getProtectedResource = function () {

            return $http.get(serviceBase + 'api/protectedResource')
                .then(function (results) {
                    return results;
                });
        };

        authenticatedUserServiceFactory.getProtectedResource = _getProtectedResource;

        return authenticatedUserServiceFactory;
}]);