
"use strict";

// Important regarding: localStorageService
//The best way to store this token is to use AngularJS module named “angular-local-storage” 
//which gives access to the browsers local storage with cookie fallback if you are using 
//old browser, so I will depend on this module to store the token and the logged in 
//username in key named “authorizationData”. We will use this key in different places 
//in our app to read the token value from it.

authenticationApp.factory("authService", ["$http", "$q", "localStorageService", "ngAuthSettings",
    function ($http, $q, localStorageService, ngAuthSettings) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var authServiceFactory = {};

        var _authentication = {
            isAuth: false,
            userName: "",
            useRefreshTokens: false
        };

        var _saveRegistration = function (registration) {
            _logOut();

            return $http.post(serviceBase + 'api/account/register', registration)
                .then(function (response)
                {
                    return response;
                });
        };

        var _login = function (loginData) {
            var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

            if (loginData.useRefreshTokens) {
                data = data + "&client_id=" + ngAuthSettings.clientId;
            }

            var deferred = $q.defer();

            $http.post(serviceBase + 'token', data,
                {
                    //we have configured the POST request for this endpoint to use 
                    //“application/x-www-form-urlencoded” as its Content-Type and sent 
                    //the data as string not JSON object.
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                })
                .success(function (response)
                {

                    if (loginData.useRefreshTokens) {
                        localStorageService.set('authorizationData',
                            {
                                token: response.access_token, userName: loginData.userName,
                                refreshToken: response.refresh_token, useRefreshTokens: true
                            });
                    }
                    else {
                        localStorageService.set('authorizationData',
                            {
                                token: response.access_token, userName: loginData.userName,
                                refreshToken: "", useRefreshTokens: false
                            });
                    }

                    // Now we are using Refresh Tokens!
                    //localStorageService.set('authorizationData',
                    //    { token: response.access_token, userName: loginData.userName });

                    _authentication.isAuth = true;
                    _authentication.userName = loginData.userName;
                    _authentication.useRefreshTokens = loginData.useRefreshTokens;

                    deferred.resolve(response);
                })
                .error(function (err, status) {
                    _logOut();
                    deferred.reject(err);
                });

            return deferred.promise;
        };


        var _logOut = function () {
            localStorageService.remove('authorizationData');

            _authentication.isAuth = false;
            _authentication.userName = "";
            _authentication.useRefreshTokens = false;
        };


        var _fillAuthData = function () {
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                _authentication.isAuth = true;
                _authentication.userName = authData.userName;
                _authentication.useRefreshTokens = authData.useRefreshTokens;
            }
        };

        var _refreshToken = function () {
            var deferred = $q.defer();

            var authData = localStorageService.get('authorizationData');

            if (authData) {

                if (authData.useRefreshTokens) {

                    var data = "grant_type=refresh_token&refresh_token=" +
                        authData.refreshToken + "&client_id=" + ngAuthSettings.clientId;

                    localStorageService.remove('authorizationData');

                    $http.post(serviceBase + 'token', data,
                        { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })
                        .success(function (response) {
                            localStorageService.set('authorizationData',
                                {
                                    token: response.access_token, userName: response.userName,
                                    refreshToken: response.refresh_token, useRefreshTokens: true
                                });

                            deferred.resolve(response);

                    }).error(function (err, status) {
                        _logOut();
                        deferred.reject(err);
                    });
                }
            }
            return deferred.promise;
        };

        var _obtainAccessToken = function (externalData) {

            var deferred = $q.defer();

            $http.get(serviceBase + 'api/account/ObtainLocalAccessToken',
                { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } })
                .success(function (response) {
                    localStorageService.set('authorizationData',
                        {
                            token: response.access_token, userName: response.userName,
                            refreshToken: "", useRefreshTokens: false
                        });

                     _authentication.isAuth = true;
                     _authentication.userName = response.userName;
                     _authentication.useRefreshTokens = false;

                     deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        authServiceFactory.saveRegistration = _saveRegistration;
        authServiceFactory.login = _login;
        authServiceFactory.logOut = _logOut;
        authServiceFactory.fillAuthData = _fillAuthData;
        authServiceFactory.authentication = _authentication;
        authServiceFactory.refreshToken = _refreshToken;

        return authServiceFactory;
}]);