(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('editaddregionCtrl', editaddregionCtrl);

		editaddregionCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'siteadminService', 'items', 'addNew', 'dataContext', 'AppDefine'];

		function editaddregionCtrl($scope, $modalInstance, cmsBase, siteadminService, items, addNew, dataContext, AppDefine) {
			$scope.data = {
				RegionKey: 0,
				UserKey: 0,
				RegionName: "",
				RegionParentId: null,
				Description: ""
			};
			$scope.btn_Type = addNew ? AppDefine.Resx.BTN_NEW : AppDefine.Resx.BTN_EDIT;

			active();

			function active() {
				cmsBase.translateSvc.partLoad('Sites'); //Load resource references
				dataContext.injectRepos(['configuration.siteadmin']).then(getActiveData);
			}

			function getActiveData() {
				if (addNew) {
					return;
				}

				dataContext.siteadmin.getRegion({ regionKey: items.ID }, function (data) {
					$scope.data = data;
				}, function (error) {
					var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					cmsBase.cmsLog.error(msg);
				});
			}
			
			$scope.CloseRegion = function () {
				$modalInstance.close();
			}

			$scope.SaveRegion = function () {
				if (addNew) {
					if (items.Type === AppDefine.NodeType.Site) {
						$scope.data.RegionParentId = items.ParentKey;
					}
					else {
						$scope.data.RegionParentId = items.ID;
					}

					dataContext.siteadmin.addRegion($scope.data).then(
						function (response) {
							var msg = "";
							if (response.ReturnStatus) {
								msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
								cmsBase.cmsLog.success(msg);
								$modalInstance.close(response.Data);
							}
							else {
								angular.forEach(response.ReturnMessage, function (msgKey, index) {
									if (msgKey === AppDefine.Resx.REGION_NAME_EXIST) {
										cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(msgKey), $scope.data.RegionName));
									}
									else{
										msg = cmsBase.translateSvc.getTranslate(msgKey);
										cmsBase.cmsLog.warning(msg);
									}
								});
							}
						},
						function (error) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						});
				}
				else {
					dataContext.siteadmin.editRegion($scope.data).then(
						function (response) {
							var msg = "";
							if (response.ReturnStatus) {
								msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
								cmsBase.cmsLog.success(msg);
								$modalInstance.close(response.Data);
							}
							else {
								angular.forEach(response.ReturnMessage, function (msgKey, index) {
									if (msgKey === AppDefine.Resx.REGION_NAME_EXIST) {
										cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(msgKey), $scope.data.RegionName));
									}
									else {
										msg = cmsBase.translateSvc.getTranslate(msgKey);
										cmsBase.cmsLog.warning(msg);
									}
								});
							}
						},
						function (error) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						});
				}
			}

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			};
		}
	});

})();