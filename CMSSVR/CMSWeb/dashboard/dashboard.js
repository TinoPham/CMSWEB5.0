(function () {
	'use strict';
	define(['cms',
		'Services/dialogService',
		'Services/localStorage',
        'Directives/widget',
        'DataServices/ReportService'
	],
        function (cms) {
            cms.register.controller('dashboardCtrl', dashboardCtrl);
            dashboardCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', '$localStorage', 'AppDefine', '$stateParams'];
            function dashboardCtrl($scope, $modal, cmsBase, dataContext, $localStorage, AppDefine, $stateParams) {
                $scope.isLoaded = false;
                $scope.editMode = false;
				$scope.levelMode = 1;
                active();

                function active() {

                    var val = $stateParams.obj;
                    if (val && val.isEdit) {
                        $scope.editMode = val.isEdit;
                    }
                    $scope.isLoaded = false;
                    dataContext.injectRepos(['dashboard']).then(getActiveData);
                }

                $scope.ready = false;

                function getActiveData() {
                    dataContext.dashboard.getDashboard(success, function(error) {
                        cmsBase.cmsLog.error(error);
                    });
                    $scope.ready = true;
                }

                $scope.saveDashboard = function (editMode) {
                    var def = cmsBase.$q.defer();
                    dataContext.dashboard.editDashboard($scope.dashboard,
                        function (result) {
                            $scope.editMode = !editMode;
                            def.resolve();
                        },
                        function(error) {
                            if (error.data && error.data.ReturnMessage) {
                                var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
                                cmsBase.cmsLog.error(msg);
                            } else {
                                cmsBase.cmsLog.error(error.data.Message);
                            }
                            def.reject();
                        });
                    return def.promise;
                }

                function success(data) {
                    $scope.dashboard = data;
                    $scope.isLoaded = true;
                }

                $scope.loadDefinedDashboard = function (lvlMode) {
                	$scope.isLoaded = false;
                	$scope.ready = false;
                	var def = cmsBase.$q.defer();
                	dataContext.dashboard.getDefinedDashboard(lvlMode,
						function (result) {
							$scope.dashboard = result;
							$scope.isLoaded = true;
							def.resolve(result);
						},
						function (error) {
							cmsBase.cmsLog.error(error);
							def.reject();
                	});
                	$scope.ready = true;
                	$scope.editMode = true;
                	return def.promise;
                }
            }
        });
})();