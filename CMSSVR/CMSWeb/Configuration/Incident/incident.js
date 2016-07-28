(function () {
	'use strict';
	define(['cms', 'DataServices/Configuration/incidentSvc'], function (cms) {
		cms.register.controller('incidentCtrl', ['$http', '$scope', '$element', '$modal', '$filter', 'dialogSvc', 'IncidentSvc', 'AccountSvc', 'cmsBase', 'AppDefine',
            function ($http, $scope, $element, $modal, $filter, dialogSvc, IncidentSvc, AccountSvc, cmsBase, AppDefine) {
            	$scope.user = AccountSvc.UserModel();
            	$scope.typeSelected = [];
            	$scope.mandatorys = [];
            	$scope.fields = [];
            	//$scope.alerts = [];
            	//var alert = {};
            	$scope.Resx = AppDefine.Resx;

            	IncidentSvc.GetCaseType().then(function (data) {
            		$scope.incidentTypes = data;
            		$scope.typeSelected = $scope.incidentTypes[0];
            		DisplayValue();
            	});

            	$scope.ChangeValue = function () {
            		$scope.alerts = [];
            		DisplayValue();
            	}

            	function DisplayValue() {
            		var caseID = $scope.typeSelected.CaseTypeID;

            		IncidentSvc.GetIncidentManagent(caseID).then(function (data) {
            			$scope.mandatorys = data.MandatoryFields;
            			$scope.fields = data.FieldSelection;
            		});
            	}

            	$scope.Save = function () {
            		$scope.alerts = [];
            		var valueUpdate = [];
            		for (var i = 0; i < $scope.fields.length; i++) {
            			var model = angular.copy(IncidentSvc.IncidentFieldModel());
            			model.CaseTypeID = $scope.typeSelected.CaseTypeID;
            			model.FieldsGUIID = $scope.fields[i].FieldsGUIID;
            			model.Status = $scope.fields[i].Status;
            			valueUpdate.push(model);
            		}

            		IncidentSvc.UpdateIncidentField(valueUpdate)
						.then(
							function (data) {
								if (data.ReturnStatus == false) {
									var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
									cmsBase.cmsLog.error(msg);
									//alert = { type: 'danger', msg: data.ReturnMessage[0] };
									//$scope.alerts.push(alert);
								}
								else {
									console.log(data);
									var msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
									cmsBase.cmsLog.info(msg);
									//alert = { type: 'success', msg: 'INCIDENT_SAVE_SUCCESS' };
									//$scope.alerts.push(alert);
								}
							}
							, function (data, error) {
								console.log(data + error);
								var msg = data + error;
								cmsBase.cmsLog.error(AppDefine.Resx.EDIT_FAIL_MSG);
								//alert = { type: 'warning', msg: data + error };
								//$scope.alerts.push(alert);
							}
						);
            	}
            }]);
	});
})();