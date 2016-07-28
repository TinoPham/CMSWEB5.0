(function () {
	'use strict';

	define(['cms', 'Services/dialogService'], function (cms) {
		cms.register.controller('noteCtrl', noteCtrl);

		noteCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'dialogSvc', 'AppDefine', '$timeout'];

		function noteCtrl($scope, dataContext, cmsBase, dialogSvc, AppDefine, $timeout) {
			var vm = this;
			vm.notes = [];
			vm.state = {
				add: 1,
				edit: 2,
				view: 3
			};
			//vm.DivHeight = AppDefine.ChartHeight + 32;
			vm.viewMode = vm.state.view;
			vm.contentChanged = false;
			vm.noteSelected = {};
			vm.initNote = {
				Id: 0,
				UserId: 0,
				Content: '',
				CreatedOn: null
			};

			active().finally(function () {
				$scope.$parent.isLoading = false;
				vm.viewMode = vm.state.view;
				vm.contentChanged = false;
			});

			function active() {
				angular.copy(vm.initNote, vm.noteSelected);
				var def = cmsBase.$q.defer();
				dataContext.injectRepos(['note']).then(function () {
					getData().then(function () {
						def.resolve();
					});
				});
				return def.promise;
			}

			function getData() {
				var def = cmsBase.$q.defer();
				dataContext.note.getNote(function (data) {
					vm.notes = data;
					def.resolve(data);
				}, function (error) {
					cmsBase.cmsLog.error(error.data.Data);
					def.reject();
				});
				return def.promise;
			}

			vm.addNote = function () {
				if (vm.noteSelected.Content !== '') {
					if (vm.viewMode === vm.state.edit && vm.contentChanged === true) {
						editNote();
					}
					else {
						if (vm.contentChanged === false) {
							setnewNote();
							return;
						}
						insertNote();
					}
				} else {
					setnewNote();
				}
			}

			vm.selectNote = function (note) {
				vm.noteSelected = note;
				vm.viewMode = vm.state.edit;
				triggerNoteFocus();
			}

			vm.deleteNote = function (note) {
				var modalData = {
					closeButtonText: AppDefine.Resx.BTN_CANCEL,
					actionButtonText: AppDefine.Resx.BTN_DELETE,
					headerText: AppDefine.Resx.NOTE_FIELD,
					bodyText: AppDefine.Resx.DELETE_NOTE_CONFIRM
				};
				var modalOptions = {
					size: 'sm'
				};

				dialogSvc.showModal(modalOptions, modalData).then(function (result) {
					if (result === AppDefine.ModalConfirmResponse.OK) {
						dataContext.note.deleteNote(note.Id, function () {
							vm.notes.splice(vm.notes.indexOf(note), 1);
							vm.viewMode = vm.state.view;
							angular.copy(vm.initNote, vm.noteSelected);
							vm.contentChanged = false;
						}, function (error) {
							cmsBase.cmsLog.error(error.data.Data);
						});
					}
				});
			}

			function setnewNote() {
				vm.noteSelected = {};
				vm.viewMode = vm.state.add;
				angular.copy(vm.initNote, vm.noteSelected);
				vm.contentChanged = false;
				triggerNoteFocus();
			}

			vm.change = function () {
				vm.contentChanged = true;
			}

			vm.viewNote = function () {
				if (!vm.noteSelected.Content && vm.noteSelected.Content === '') {
					vm.viewMode = vm.state.view;
					vm.contentChanged = false;
					return;
				}

				if (vm.viewMode === vm.state.add) {
					dataContext.note.insertNote(vm.noteSelected, function (data) {
						vm.notes.push(data);
						vm.viewMode = vm.state.view;
						angular.copy(vm.initNote, vm.noteSelected);
						vm.contentChanged = false;
						noteSavedAlert();
					}, function (error) {
						cmsBase.cmsLog.error(error.data.Data);
					});
				} else {

					if (vm.contentChanged === false) {
						vm.viewMode = vm.state.view;
						return;
					}

					dataContext.note.editNote(vm.noteSelected,
						function () {
							vm.viewMode = vm.state.view;
							//angular.copy(vm.initNote, vm.noteSelected);
							vm.contentChanged = false;
							noteSavedAlert();
						},
						function (error) {
							cmsBase.cmsLog.error(error.data.Data);
						});
				}
			}

			function noteSavedAlert() {
				vm.saved = "saved";
				$timeout(function () {
					vm.saved = "";
				}, 1500);
			}

			function insertNote() {
				dataContext.note.insertNote(vm.noteSelected,
					function (data) {
						vm.notes.push(data);
						setnewNote();
					},
					function (error) {
						cmsBase.cmsLog.error(error.data.Data);
					});
			}

			function editNote() {
				dataContext.note.editNote(vm.noteSelected,
					function (data) {
						setnewNote();
					},
					function (error) {
						cmsBase.cmsLog.error(error.data.Data);
					});
			}

			function triggerNoteFocus(){
				$timeout(function () {
					$("#noteText").focus();
				});
			}
		}
	});
})();