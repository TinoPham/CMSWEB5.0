(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/sitemetricSvc'], function (cms) {
		cms.register.controller('editaddsitemetricCtrl', editaddsitemetricCtrl);

		editaddsitemetricCtrl.$inject = ['$scope', '$modalInstance', '$filter', 'items', 'SiteMetricSvc', 'AccountSvc', 'AppDefine', 'cmsBase'];

		function editaddsitemetricCtrl($scope, $modalInstance, $filter, items, SiteMetricSvc, AccountSvc, AppDefine, cmsBase) {
			var vm = this;
			vm.btn_Type = AppDefine.Resx.SITEMETRIC_BTN_ADD;
			vm.Resx = AppDefine.Resx;
			vm.metricSelected = {};
			vm.metricList = [];
			var MIN_CHILD_METRIC_NUMBER = 1; //SET FOR GUI

			active();

			function active() {
			    if (items != null) {
					vm.btn_Type = AppDefine.Resx.SITEMETRIC_BTN_EDIT;
			        vm.metricSelected = angular.copy(items);
			        //Get Metrics Child
			        SiteMetricSvc.GetMetricChild(vm.metricSelected.MListID)
                        .then(function (data) {
                            vm.metricList = data;
                        });
			    }
			    else {
			        vm.metricSelected = angular.copy(new SiteMetricSvc.MetricModel());			        
			    }

			    if (vm.metricList.length == 0) {
			        var childMetric = angular.copy(new SiteMetricSvc.MetricModel());
			        childMetric.MListID = vm.metricList.length;
			        vm.metricList.push(childMetric);
			    }
			}			

			vm.AddChildMetric = function () {
					var childMetric = angular.copy(new SiteMetricSvc.MetricModel());
					childMetric.MListID = vm.metricList.length;
					vm.metricList.push(childMetric);
				}

			vm.RemoveChildMetric = function (metric) {
				if (!metric){ return; }
				
				if (vm.metricList.length > MIN_CHILD_METRIC_NUMBER) {
					var index = vm.metricList.indexOf(metric);
					if (index != -1) {
						vm.metricList.splice(index, 1);
					}
				}
			}

			vm.Cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.SaveMetric = function () {
				vm.msgValidator = '';
				if (!validatorMetricChild(vm.metricList).isValid) {
					return;
				}
				var userLogin = AccountSvc.UserModel();
				var metricListDB = angular.copy(vm.metricList);
				vm.metricSelected.CreateBy = userLogin.UserID;
				//vm.metricSelected.MetricMeasure = vm.metricSelected.MetricMeasure;
				vm.metricSelected.isDefault = true;
				vm.metricSelected.MListEditedDate = new Date();
				metricListDB.push(vm.metricSelected); //push metric parent
				//update child Metric list value
				angular.forEach(metricListDB, function (metricChild, index) {
					if (metricChild.MetricName != null && metricChild.MetricName != "") {
						metricChild.CreateBy = vm.metricSelected.CreateBy;
						//metricChild.MetricMeasure = metricChild.MetricName;
						metricChild.MListEditedDate = new Date();
					}
				});

				SiteMetricSvc.AddMetricSite(metricListDB).then(
					function (response) {
						if (response.ReturnStatus) {
							var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close(response.Data);
						}
						else {
							var msg = "";
							angular.forEach(response.ReturnMessage, function (message, index) {
								if (message === AppDefine.Resx.SITEMETRIC_NAME_EXIST) {
									msg = formatString(cmsBase.translateSvc.getTranslate(message), vm.metricSelected.MetricName);
									cmsBase.cmsLog.warning(msg);
								}
								else if (message === AppDefine.Resx.SITEMETRIC_IS_USED) {
									var mtricName = [];
									$.each(response.Data, function (i, item) {
										mtricName.push(item.MetricName);
									});
									msg = formatString(cmsBase.translateSvc.getTranslate(message), mtricName.toString());
									cmsBase.cmsLog.warning(msg);
								}
								else {
									msg = cmsBase.translateSvc.getTranslate(message);
									cmsBase.cmsLog.warning(msg);
						}
							});
						}
					}
					, function (error) {
						if (vm.metricSelected.MListID != 0) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					}
						else {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						}
					}
				);
			}

			//Validator Functions
			function validatorMetricChild(metrics) {
				var ret = { msg: '', isValid: true };
				var arrName = [];

				if (!metrics) { return ret;}

				angular.forEach(metrics, function (metric, index) {
					if (!metric.MetricName) {
						vm.msgValidator = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITEMETRIC_LIST_REQUIRED);
						return ret = { msg: cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITEMETRIC_LIST_REQUIRED), isValid: false };
					}
					arrName.push(metric.MetricName);
				});

				var retDouplicateName = isExitedMetric(arrName);
				if (retDouplicateName.isExisted) {
					vm.msgValidator = retDouplicateName.msg;
					return ret = { msg: retDouplicateName.msg, isValid: false };
				}
				return ret;
			}

			function isExitedMetric(listName) {
				var result = { msg: '', isExisted: false };
				if (!listName)
					return result;
				var sorted_arr = listName.sort();
				var arrDouplicate = [];
				for (var i = 0; i < listName.length - 1; i++) {
					if (sorted_arr[i + 1] == sorted_arr[i]) {
						arrDouplicate.push(sorted_arr[i]);
					}
				}

				if (arrDouplicate.length > 0) {
					var message = formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITEMETRIC_LIST_EXIST), $.unique(arrDouplicate).sort().toString());
					return result = { msg: message, isExisted: true };
				}
					
				return result;
			}

			vm.validMetricNameFunc = function (metrics, index) {
				if (index === undefined || !metrics) { return true; }
				var metricInput = angular.element("#listname" + index)[0];
				if (!metricInput || !metricInput.value) { return true; }
				if (checkExitedMetric(metrics, index) === true) {
					return formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITEMETRIC_LIST_EXIST), metricInput.value);
				}
				else {
					return true;
				}
			}

			function checkExitedMetric(metrics, index) {
				if (!metrics || index === undefined) { return false; }
				var result = false;
				var metricCompare = metrics[index];
				for (var i = 0; i < metrics.length; i++) {
					var metricItem = metrics[i];
					if (i != index && metricCompare && isDuplicateMetric(metricCompare, metricItem)) {
						result = true;
						break;
					}
				}
				return result;
			}

			function isDuplicateMetric(metric1, metric2) {
				return (metric1.MetricName
						&& metric2
						&& metric2.MetricName
						&& metric2.MetricName.toLowerCase().replace(/\s+/g, '') == metric1.MetricName.toLowerCase().replace(/\s+/g, ''));
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

