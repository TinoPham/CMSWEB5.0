(function() {
    'use strict';

    define(['cms'], function(cms) {

        cms.factory('dataContext', dataContext);

        dataContext.$inject = ['$injector', '$q'];

        function dataContext($injector,$q) {
            var repoNames = [];
            var contextList = [];
            var ops = [];

            var service = {
                injectRepos: injectRepos
            };

            return service;

            function active() {
                lazyRegistryRepos();
            }

            function injectRepos(injectdata) {
                var def = $q.defer();

                contextList = injectdata;
                loadFile()
                    .then(function() {
                        active();
                    })
                    .then(function() {
                        def.resolve();
                    });

                return def.promise;
            }

            function loadFile() {
                var def = $q.defer();
                if (contextList.length > 0) {
                    //repoNames = [];
                    angular.forEach(contextList, function (dep) {
                        var file = [], nameSvc=dep, localpath=dep;
                        var pathsplit = dep.split('.');
                        
                        if (pathsplit.length > 0) {
                            nameSvc = pathsplit[pathsplit.length - 1];
                            localpath = dep.replace(".", "/");
                        }
                        repoNames.push(nameSvc);
                        file = ['Scripts/DataServices/' + localpath + '.service.js'];
                        ops.push(requireLoad(file));
                    });

                    $q.all(ops).then(function() {
                        def.resolve();
                    });
                }
                return def.promise;
            }

            function requireLoad(file) {
                var defer = $q.defer();
                require(file, function () {
                    defer.resolve();
                });

                return defer.promise;
            }

            function getRepo(repoName) {
                var fullRepoName = repoName.toLowerCase() + '.service';
                var factory = $injector.get(fullRepoName);
                return factory.create();
            }

            function lazyRegistryRepos() {
                repoNames.forEach(function(name) {
                    if (name && !service.hasOwnProperty(name)) {
                        Object.defineProperty(service, name, {
                            configurable: true,
                            get: function() { return getObjs(service, name); }
                        });
                    }
                });
            }

            function getObjs(svc, name) {
                var repo = getRepo(name);
                Object.defineProperty(service, name, {
                    value: repo,
                    configurable: false,
                    enumerable: true
                });
                return repo;
            }
        }
    });

})();