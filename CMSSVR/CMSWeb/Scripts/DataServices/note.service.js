(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.service('note.service', noteSvc);

        noteSvc.$inject = ['$resource', 'cmsBase', 'AppDefine'];

        function noteSvc($resource, cmsBase, AppDefine) {

            var urlnote = AppDefine.Api.Note;

            var noteResource = $resource(urlnote, { UId: '@UId' }, {
                getNote: { method: 'GET', url: urlnote + '/GetNote', headers: cms.EncryptHeader() },
                insertNote: { method: 'POST', url: urlnote + '/InsertNote', headers: cms.EncryptHeader() },
                editNote: { method: 'POST', url: urlnote + '/EditNote', headers: cms.EncryptHeader() },
                deleteNote: { method: 'POST', url: urlnote + '/DeleteNote', headers: cms.EncryptHeader() },
            });


            var userService = {
                getNote: GetNote,
                insertNote: InsertNote,
                editNote: EditNote,
                deleteNote: DeleteNote,
            };

            function createRepo() {
                return userService;
            }

            return {
                create: createRepo
            };

            function GetNote(successFn, errorFn) {
                noteResource.getNote().$promise.then(function (result) {
                    var rlst = cms.GetResponseData(result);
                    successFn(rlst);
                }, function (error) {
                    errorFn(error);
                });
            }

          function InsertNote(data, successFn, errorFn) {
              noteResource.insertNote(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function EditNote(data, successFn, errorFn) {
                noteResource.editNote(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }

            function DeleteNote(data, successFn, errorFn) {
                noteResource.deleteNote(data, function (result) {
                    var rlt = cms.GetResponseData(result);
                    successFn(rlt);
                }, function (error) {
                    errorFn(error);
                });
            }
        }
    });
})();