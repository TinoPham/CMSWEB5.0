(function () {
	'use strict';
	define(['cms'],
		function (cms) {
			cms.register.controller('boxgadgetCtrl', boxgadgetCtrl);
			boxgadgetCtrl.$inject = ['$scope', 'ReportService', 'AppDefine', 'cmsBase', '$filter', '$timeout', '$state', '$rootScope', 'AccountSvc'];
			function boxgadgetCtrl($scope, ReportService, AppDefine, cmsBase, $filter, $timeout, $state, $rootScope, AccountSvc) {

				var scopemodel = $scope.widgetModel;
				var iconList = { 'traffic': 'icon-users', 'alert': 'icon-attention-1', 'dvrOff': 'icon-videocam', 'sale': 'icon-certificate' };
				var Resx = AppDefine.Resx;
				var p_GUI, param;

				

				
				if (scopemodel) {
					active(scopemodel);
				}

				$scope.init = function (box) {
					if (!box || !box.modelBox) return;

					$scope.configBox = box;
					active(box.modelBox);
				}

				function active(model) {
					$scope.ResxName = model.Name;
					$scope.prefixName = AppDefine.Resx.TOTAL_STRING;
					param = JSON.parse("{" + model.TemplateParams + "}");
					p_GUI = param.hasOwnProperty("Gui") ? param.Gui : null;
					$scope.percent = '';//percent sign
					$scope.TrandLabel = '';//increase or decrease
					$scope.Label = '';//data type			        
					$scope.icon = '';
					$scope.widgetClass = '';

					if (param && param.hasOwnProperty("Gui")) {
						$scope.widgetClass = param.Gui.WidgetSelectClass;
					}

					$scope.showTranding = (p_GUI == null || !p_GUI.hasOwnProperty("Tranding")) ? true : p_GUI.Tranding;
					var parRequest = param.hasOwnProperty("Pram") ? param.Pram : param;
					var curDate = new Date();
					parRequest.date = $filter('date')(curDate, AppDefine.DateFormatCParamED);
					$scope.datefiter = curDate;
					parRequest.int = 0; //From start of day
					ReportService.AlertCompare(parRequest, Success, Error);

					function Success(data) {
						$scope.model = data;
						if ($scope.model == null) {
							$scope.model = { Value: 0, Increase: null, CmpValue: 0 };
						}

						switch ($scope.ResxName) {
							case AppDefine.Resx.NUMBER_TRAFFIC:
								$scope.icon = iconList.traffic;
								break;
							case AppDefine.Resx.NUMBER_ALERTS:
								$scope.icon = iconList.alert;
								break;
							case AppDefine.Resx.NUMBER_DVROFF:
								$scope.icon = iconList.dvrOff;
								break;
							case AppDefine.Resx.NUMBER_SALE:
								$scope.icon = iconList.sale;
								break;
							default:
								$scope.icon = '';
								break;
						}

						if ($scope.ResxName == AppDefine.Resx.NUMBER_CONVERSION)
							$scope.model.Value = $scope.model.Value + '%';

						if (!$scope.model || $scope.model.Increase == null) {
							$scope.iconCmpValue = 'icon-right-3';
						}
						else if ($scope.model.Increase) {
							$scope.iconCmpValue = 'icon-up-3';
						}
						else {
							$scope.iconCmpValue = 'icon-down-3';
						}

						if ($scope.showTranding) {
							$scope.percent = '%';
							$scope.TrandLabel = (!$scope.model || !$scope.model.Increase) ? Resx.DECREASE_IN : Resx.INCREASE_IN;
							$scope.Label = (!p_GUI || !p_GUI.hasOwnProperty('label')) ? null : p_GUI.label;

							//$scope.model.CmpValue = $scope.model.CmpValue + cmsBase.translateSvc.getTranslate( trending );
						}

						if (!String.format) {
							String.format = function (format) {
								var args = Array.prototype.slice.call(arguments, 1);
								return format.replace(/{(\d+)}/g, function (match, number) {
									return typeof args[number] != 'undefined' ? args[number] : match;
								});
							};
						}

						if ($scope.ResxName == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
							$scope.Title = String.format(cmsBase.translateSvc.getTranslate($scope.ResxName), $scope.model.CmpValue);
						}
						else {
							$scope.Title = cmsBase.translateSvc.getTranslate($scope.ResxName);
						}
						$scope.$parent.isLoading = false;
					}

					function Error(data) {
						if ($scope.model == null) {
							$scope.model = { Value: 0, Increase: null, CmpValue: 0 };
						}
					}

					$scope.$on('changedLanguage_BoxGadget', function () {

						$timeout(function () {
							if ($scope.ResxName == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
								$scope.Title = String.format(cmsBase.translateSvc.getTranslate($scope.ResxName), $scope.model.CmpValue);
							}
							else {
								$scope.Title = cmsBase.translateSvc.getTranslate($scope.ResxName);
							}
						}, 500);

					});
				}
				//$scope.mouseOver = function () {
				//	var param = $scope.ResxName;
				//	if (param == "NUMBER_DVROFF" || param == "NUMBER_SENSOR" || param == "NUMBER_DVRRECORDLESS") {
				//		$scope.myStyle = "mouse-over";
				//	}
				//}
				//console.log("AccountSvc");
				$scope.User = AccountSvc.UserModel();
				var FunctionsCMS = $scope.User.Functions;
				$scope.moduleSite = false;

				FunctionsCMS.forEach(function (entry) {
					var ModuleID = entry.ModuleID;
					if (ModuleID == 1) {
						$scope.moduleSite = true;
						return false;
					}
				});
				console.log($scope.moduleSite);
				
				$scope.redirectURL = function () {

					var param = {
						name: $scope.ResxName,
						//param: JSON.parse("{" + $scope.widgetModel.TemplateParams + "}"),
						date: $scope.datefiter
					};
					if (param.name == AppDefine.Resx.NUMBER_DVROFF || param.name == AppDefine.Resx.NUMBER_SENSOR || param.name == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
						if ($state.current.name == AppDefine.State.SITES) {
							$rootScope.$emit("boxclick", param);
							$rootScope.$broadcast("boxclick", param);
						}
						else {
							if (angular.isUndefined($scope.ResxName) == false && angular.isUndefined($scope.datefiter) == false) {
								if (param.name == AppDefine.Resx.NUMBER_DVROFF || param.name == AppDefine.Resx.NUMBER_SENSOR || param.name == AppDefine.Resx.NUMBER_DVRRECORDLESS) {
									$state.go(AppDefine.State.SITES, { obj: { param: param } });
								}
							}
						}
					}

				}
			}
		});
})();