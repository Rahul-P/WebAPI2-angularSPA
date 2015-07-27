
"use strict";

// Important Notes about "Interceptor Factory Service"
//Interceptor is regular service (factory) which allow us to capture 
//every XHR request and manipulate it before sending it to the 
//back-end API or after receiving the response from the API, 
//in our case we are interested to capture each request before 
//sending it so we can set the bearer token, as well we are 
//interested in checking if the response from back-end API 
//contains errors which means we need to check the error code 
//returned so if its 401 then we redirect the user 
//to the log-in page.

authenticationApp.factory("authInterceptorService",
    ["$q", "$injector", "$location", "localStorageService",
        function ($q, $injector, $location, localStorageService) {

            var authInterceptorServiceFactory = {};

            //Request 
            var _request = function (config) {

                config.headers = config.headers || {};

                var authData = localStorageService.get('authorizationData');

                if (authData) {
                    config.headers.Authorization = 'Bearer ' + authData.token;
                }

                return config;
            }

            //Response 
            var _responseError = function (rejection) {

                if (rejection.status === 401) {
                    //var authService = $injector.get('authService');
                    //var authData = localStorageService.get('authorizationData');

                    //if (authData) {
                    //    if (authData.useRefreshTokens) {
                    //        $location.path('/refresh');
                    //        return $q.reject(rejection);
                    //    }
                    //}
                    //authService.logOut();
                    $location.path('/login');
                }
                return $q.reject(rejection);
            }

            authInterceptorServiceFactory.request = _request;
            authInterceptorServiceFactory.responseError = _responseError;

            return authInterceptorServiceFactory;
        }
    ]);

//By looking at the code above, the method “_request” will be fired 
//before $http sends the request to the back-end API, so this is 
//the right place to read the token from local storage and set it 
//into “Authorization” header with each request. Note that I’m 
//checking if the local storage object is nothing so in this 
//case this means the user is anonymous and there is no need 
//to set the token with each XHR request.

//Now the method “_responseError” will be hit after the we receive a 
//response from the Back-end API and only if there is failure status 
//returned. So we need to check the status code, in case it was 401 
//we’ll redirect the user to the log-in page where he’ll be able 
//to authenticate again.