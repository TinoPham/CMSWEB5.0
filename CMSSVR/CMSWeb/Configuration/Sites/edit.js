(function () {
	'use strict';

	define(['cms',
        'configuration/sites/add_site'
	], function (cms) {
		cms.register.controller('editSiteDiaCtrl', editSiteDiaCtrl);

		editSiteDiaCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'items', 'addNew', 'dataContext', '$timeout', 'dialogSvc','AppDefine'];

		function editSiteDiaCtrl($scope, $modalInstance, cmsBase, items, addNew, dataContext, $timeout, dialogSvc, AppDefine) {
			var CofirmClose = 'CLOSE_FORM_HEADER';
			var CofirmCloseMSG = 'CLOSE_FORM_CONFIRM_MSG';
			$scope.isProcess = false;
			$scope.data = {
				RegionKey: 0,
				UserKey: 0,
				RegionName: "",
				RegionParentId: null,
				Description: ""
			};
			$scope.site = {};
			$scope.isSave = false;
			$scope.data = items;
			$scope.addNew = addNew;
			$scope.btn_Type = addNew ? AppDefine.Resx.BTN_NEW : AppDefine.Resx.BTN_EDIT;

			$scope.CloseRegion = function () {
				var callFn = {
					close: closeForm
				}
				$scope.$broadcast(AppDefine.Events.SAVESITE, callFn);
			}
			
			function showDialogConfirm() {
				var modalOptions = {
					headerText: CofirmClose,
					bodyText: CofirmCloseMSG
				};

				var modalDefaults = {
					backdrop: true,
					keyboard: true,
					modalFade: true,
					templateUrl: 'Widgets/DeleteDialog.html',
					size: 'sm'
				}

				dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
					if (result === AppDefine.ModalConfirmResponse.OK) {
						$modalInstance.close();
					}
				});
			}

			function closeForm(isCofirm) {
				$scope.isProcess = false;
				if (isCofirm === true) {
					showDialogConfirm();
				} else {
					$modalInstance.close();
				}
			}

			function success(data) {
				$modalInstance.close(data);
			}

			$scope.SaveRegion = function () {
				if ($scope.isProcess === true) { return; }
				$scope.isProcess = true;
				var callFn = {
					callback: success
				}
				$scope.$broadcast(AppDefine.Events.SAVESITE, callFn);
				$scope.isProcess = false;
			}
		}
	});

})();