(function () {
	'use strict';
	define(['cms', 'DataServices/Configuration/ldapSvc', 'configuration/ldap/edit'], function (cms) {
		cms.register.controller('ldapsettingCtrl', ['$http', '$scope', '$element', '$modal', '$filter', 'dialogSvc', 'LdapSvc', 'AccountSvc', 'AppDefine', 'cmsBase',
            function ($http, $scope, $element, $modal, $filter, dialogSvc, LdapSvc, AccountSvc, AppDefine, cmsBase) {
            	$scope.selectedRows = [];
            	var rowIndex = 0;
            	$scope.query = {
            		filterText: ''
            	};
            	$scope.selected = {};

            	LdapSvc.GetAllSynUser().then(function (data) {
            		$scope.valueLdap = data;
            	});

            	$scope.gridOptions = {
            		data: 'valueLdap',
            		multiSelect: false,
            		selectedItems: $scope.selectedRows,
            		filterOptions: $scope.query,
            		rowTemplate: '<div ng-dblclick="editLDAP(selected)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
            		columnDefs: [{ field: '', displayName: 'No.', width: '5%', cellTemplate: '<div style="padding-left:5px; padding-top:5px;">{{row.rowIndex + 1}}</div>' }
								, { field: 'ServerIP', displayName: $filter('translate')(AppDefine.Resx.LDAP_SERVERIP) }
								, { field: 'UserID', displayName: $filter('translate')(AppDefine.Resx.LDAP_USER_NAME) }
								, { field: 'PassWord', displayName: $filter('translate')(AppDefine.Resx.LDAP_PASSWORD), cellTemplate: '<div>********</div>' }
								, { field: 'Interval', displayName: $filter('translate')(AppDefine.Resx.LDAP_INTERVAL) }
								, { field: 'Time', displayName: $filter('translate')(AppDefine.Resx.LDAP_TIME) }
								, { field: 'LastSyn', displayName: $filter('translate')(AppDefine.Resx.LDAP_LAST_SYN), cellFilter: "date:'MM-dd-yyyy'" }
								, { field: 'SynName', displayName: $filter('translate')(AppDefine.Resx.LDAP_USER_SYSTERM) }
								, { field: 'UUsername', displayName: $filter('translate')(AppDefine.Resx.CREATE_BY) }
								, { field: 'LastSynresult', displayName: $filter('translate')(AppDefine.Resx.LDAP_SYN_RESULT), cellFilter: "date:'MM-dd-yyyy'" }
            		],
            		afterSelectionChange: function (data) {
            			$scope.selected = $scope.selectedRows[0];
            			rowIndex = this.rowIndex;
            		}
            	};
            	$scope.$on('ngGridEventData', function () {
            		if (rowIndex != 0) {
            			$scope.gridOptions.selectRow(rowIndex, true);
            		}
            	});

            	$scope.modalShown = false;
            	$scope.addLDAP = function () {
            		showDialog();
            	}
            	$scope.editLDAP = function (valueBTN) {
            		if ($.isEmptyObject(valueBTN)) {
            			showDialogConfirm();
            		} else {
            			showDialog(valueBTN);
            		}
            	}
            	function showDialog(valueBTN) {
            		if (!$scope.modalShown) {
            			$scope.modalShown = true;
            			var ldapInstance = $modal.open({
            				templateUrl: 'configuration/ldap/edit.html',
            				controller: 'editaddldapCtrl',
            				size: 'md',
            				backdrop: 'static',
            				backdropClass: 'modal-backdrop',
            				keyboard: false,
            				resolve: {
            					items: function () {
            						return valueBTN;
            					}
            				}
            			});

            			ldapInstance.result.then(function (data) {
            				$scope.modalShown = false;
            				if (data != AppDefine.ModalConfirmResponse.CLOSE) {
            					LdapSvc.GetAllSynUser().then(function (data) {
            						$scope.valueLdap = data;
            						vm.query = '';
            					});
            				}
            			});
            		}
            	}

            	function showDialogConfirm() {
            		var modalOptions = {
            			headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
            			bodyText: AppDefine.Resx.LDAP_CONFIRM_BODY
            		};

            		var modalDefaults = {
            			backdrop: true,
            			keyboard: true,
            			modalFade: true,
            			templateUrl: 'Widgets/ConfirmDialog.html'
            		}

            		dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
            		});
            	}

            	$scope.deleteLDAP = function (value) {
            		if ($.isEmptyObject(value)) {
            			showDialogConfirm();
            		} else {
            			var modalOptions = {
            				closeButtonText: AppDefine.Resx.BTN_CANCEL,
            				actionButtonText: AppDefine.Resx.BTN_DELETE,
            				headerText: AppDefine.Resx.BTN_DELETE,
            				bodyText: AppDefine.Resx.DELETE_CONFIRM_MSG
            			};

            			dialogSvc.showModal({}, modalOptions).then(function (result) {
            				if (result === AppDefine.ModalConfirmResponse.OK) {
            					DeleteSynUser(value.SynID);
            				}
            			});
            		}
            	}

            	function DeleteSynUser(SynID) {
            		LdapSvc.DeleteSynUser(SynID)
					.then(
						function (data) {
							if (data == true) {
								$scope.valueLdap.splice(rowIndex, 1);
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_SUCCESS_MSG);
								cmsBase.cmsLog.info(msg);
							}
							else {
								console.log(data);
								var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
							}
						}
						, function (data, error) {
							console.log(data + error);
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG) + data + error;
							cmsBase.cmsLog.error(msg);
						}
					);
            	}
            }]);
	});
})();