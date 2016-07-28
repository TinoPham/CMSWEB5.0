(function () {
	'use strict';

	define([
        'cms',
        'DataServices/Bam/usermetricSvc'
	], function (cms) {
		cms.register.controller('metricviewtCtrl', metricviewtCtrl);

		metricviewtCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'items', 'dataContext', 'usermetricSvc', 'AppDefine'];

		function metricviewtCtrl($scope, $modalInstance, cmsBase, items, dataContext, usermetricSvc, AppDefine) {
			var reqMsg = 'SELECT_AT_LEAST_ONE_MSG';

			active();

			function active() {
				usermetricSvc.getMetricReport({ reportId: items.reportId }, function (result) {
					$scope.metricList = result;
				}, function (error) {
					console.log(error);
					$scope.Close();
				});
			}


			$scope.Close = function () {
				$modalInstance.close();
			};

			$scope.Save = function () {
				var metricactives = Enumerable.From($scope.metricList)
					.Where(function (x) { return x.Active === true; })
					.Select(function (x) { return x; })
					.ToArray();

				if (!metricactives || metricactives.length == 0) {
					var msg = cmsBase.translateSvc.getTranslate(reqMsg);
					cmsBase.cmsLog.error(msg);
					return;
				}

				var modelsave = {
					ReportId: items.reportId,
					Metrics: metricactives
				};

				usermetricSvc.updateMetricReport(modelsave, function (result) {
					$modalInstance.close(true);
				}, function (error) {
					console.log(error);
				});

			}
		}
	});
})();