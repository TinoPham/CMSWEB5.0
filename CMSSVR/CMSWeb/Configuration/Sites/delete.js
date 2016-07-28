(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('delSiteNodeCtrl', delSiteNodeCtrl);

		delSiteNodeCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine'];

		function delSiteNodeCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine) {

			$scope.delMsg = '';

			if (items.Type === AppDefine.NodeType.Region) {
				$scope.delMsg = AppDefine.Resx.DELETE_REGION_MSG_CONFIRM;
				$scope.node = items;
				//Check this region contains site
				if ($scope.node.Sites.length > 0) {
					var msg = $scope.formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLED_DELETE_REGION_EXIST_SUB_REGION), $scope.node.Name, $scope.node.Sites.length);
					cmsBase.cmsLog.warning(msg);
					$scope.msgConfirm = msg;
				}
			}
			else {
				if (items.Type === AppDefine.NodeType.Site) {
					$scope.delMsg = AppDefine.Resx.DELETE_SITE_MSG_CONFIRM;
					$scope.node = items;
				}
				else {
					$scope.delMsg = AppDefine.Resx.DELETE_DVR_MSG_CONFIRM;
					$scope.node = items;
				}
			}

			$scope.save = function (node) {
				$modalInstance.close(node);
			}

			$scope.cancel = function () {
				$modalInstance.close();
			}
		}
	});
})();