(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.service('todo.service', todoSvc);

        todoSvc.$inject = ['$resource', 'cmsBase', 'AppDefine'];

        function todoSvc($resource, cmsBase, AppDefine) {

            var urltodo = AppDefine.Api.Todo;

            var todoResource = $resource(urltodo, { UId: '@UId' }, {
                getTodo: { method: 'GET', url: urltodo + '/GetTodo', headers: cms.EncryptHeader() },
                insertTodo: { method: 'POST', url: urltodo + '/InsertTodo', headers: cms.EncryptHeader() },
                editTodo: { method: 'POST', url: urltodo + '/EditTodo', headers: cms.EncryptHeader() },
                deleteTodo: { method: 'POST', url: urltodo + '/DeleteTodo', headers: cms.EncryptHeader() }
            });

            var userService = {
                getTodo: GetTodo,
                insertTodo: InsertTodo,
                editTodo: EditTodo,
                deleteTodo: DeleteTodo,
            };

            function createRepo() {
                return userService;
            }

            return {
                create: createRepo
            };

            function GetTodo(successFn, errorFn) {
                todoResource.getTodo().$promise.then(function (result) {
                    var rlst = cms.GetResponseData(result);
                    successFn(rlst);
                }, function (error) {
                    errorFn(error);
                });
            }

            function InsertTodo(data, successFn, errorFn) {
                todoResource.insertTodo(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function EditTodo(data, successFn, errorFn) {
                todoResource.editTodo(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function DeleteTodo(data, successFn, errorFn) {
                todoResource.deleteTodo(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

        }
    });
})();