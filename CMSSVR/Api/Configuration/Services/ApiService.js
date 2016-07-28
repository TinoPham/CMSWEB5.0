//var ConfigApp = angular.module('ConfigApp');
define(function (app) {

    var injectParams = ['$http', '$rootScope', '$q'];
    var factsvr = function ($http, $rootScope, $q) {

        var urlBase = '/api/converter/ConvertConfig/';
        var UserInfo = {
            ID: 0,
            Name: null,
            UserName: null,
            Password: null,
            isAuthenticated: false
        };

        function postData(url, data) {
            var deferred = $q.defer();

            $http.post(url, data)
            .success(function (response, status) {
                deferred.resolve(response);
            })
            .error(function (response, status) {
                deferred.reject(response);
            });
            return deferred.promise;
        }

        function getData(url) {
            var deferred = $q.defer();

            $http.get(url)
            .success(function (data, status) {
                deferred.resolve(data);
            })
            .error(function (data, status) {
                deferred.reject(data);
            });
            return deferred.promise;

        }

        function getData(url, pram) {
            var deferred = $q.defer();

            $http({
                url: url,
                method: "GET",
                params: pram
            })
            .success(function (result, status) {
                deferred.resolve(result);
            })
            .error(function (result, status) {
                deferred.reject(result);
            });
            return deferred.promise;

        }
        //###########################################
        //Account
        //###########################################

        this.checkaccount = function () {
            var url = urlBase + 'GetAccount';
            return getData(url);
            //var deferred = $q.defer();

            //$http.get(url)
            //.success(function (data, status) {
            //    deferred.resolve(data);
            //})
            //.error(function (data, status) {
            //    deferred.reject(data);
            //});
            //return deferred.promise;

        };

        this.LogIn = function (user) {
            var url = urlBase + 'LogIn';
            return postData(url, user);

            //var deferred = $q.defer();

            //$http.post(url, user)
            //.success(function(data, status) {
            //    deferred.resolve(data);
            //})
            //.error(function(data, status) {
            //    deferred.reject(data);
            //});
            //return deferred.promise;
        };

        this.addAccount = function (new_account) {
            var url = urlBase + 'AddAccount';
            return postData(url, new_account);

            //$http.post(url, new_account)
            //.success(function (data, status) {
            //    deferred.resolve(data);
            //})
            //.error(function (data, status) {
            //    deferred.reject(data);
            //});
            //return deferred.promise;

        };

        this.editAccount = function (edit_account) {
            var url = urlBase + 'EditAccount';
            return postData(url, edit_account);

            //$http.post(url, edit_account)
            //.success(function (data, status) {
            //    deferred.resolve(data);
            //})
            //.error(function (data, status) {
            //    deferred.reject(data);
            //});
            //return deferred.promise;
        }

        //###########################################
        //DB Config
        //###########################################
        this.GetDBConfig = function () {
            var url = urlBase + 'GetDBConnection';
            return getData(url);
        }
        this.SetDBConnection = function (model) {
            var url = urlBase + 'SetDBConnection';
            return postData(url, model);
        }
        this.TestDBConnection = function (model) {
            var url = urlBase + 'TestDBConnection';
            return postData(url, model);
        }
        //###########################################
        //DVR Auth Config
        //###########################################
        this.GetDVRAuthConfig = function () {
            var url = urlBase + 'GetDVRAuthConfig';
            return getData(url);
        }
        this.SetDVRAuthConfig = function (model) {
            var url = urlBase + 'SetDVRAuthConfig';
            return postData(url, model);

        }
        //###########################################
        //Web APi Logs
        //###########################################
        this.Logs = function (data) {
            var url = urlBase + 'Logs';
            return getData(url, data);
        }
        this.DeleteLog = function (log) {
            var url = urlBase + "DeleteLog";
            return postData(url, log);

        }
        this.DeleteLogs = function (option) {
            var url = urlBase + "DeleteLogs";
            return postData(url, option);

        }
        //###########################################
        //Web APi DVR info
        //###########################################
        this.DVRs = function () {
            var url = urlBase + 'GetDVRs';
            return getData(url);
        }


    };
    factsvr.$inject = injectParams;

    //app.register.service('ApiService', factsvr);
    return factsvr;
     
    }
    );
