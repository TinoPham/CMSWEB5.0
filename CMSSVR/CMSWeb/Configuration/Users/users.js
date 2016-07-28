(function () {
	'use strict';
	define(['cms'
		, 'configuration/users/edit.js'
		, 'configuration/users/delete.js'],
        function (cms) {
        	cms.register.controller('usersCtrl', usersCtrl);

        	usersCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext', '$filter', 'colorSvc', 'dialogSvc', 'AppDefine', 'AccountSvc', '$timeout'];

        	function usersCtrl($scope, $modal, cmsBase, dataContext, $filter, colorSvc, dialogSvc, AppDefine, AccountSvc, $timeout) {
        		var vm = this;
        		vm.image_url = AccountSvc.UserModel().Settings.ImageUrl;
        		vm.listView = false; //Set default: Gridview mode
        		vm.selectedRows = [];
        		var rowIndex = 0;
        		vm.usersData = [];
        		$scope.modalShown = false;
        		vm.query = '';
        		vm.GridData = [];
        		vm.itemEdited = null;
        		vm.allSelected = false;

        		vm.disableSearch = false;
        		vm.activeRows = [];

        		active();

        		function active() {
        			setGridMode();
        			dataContext.injectRepos(['configuration.user']).then(getAllUsers);
        		}

        		function setGridMode() {
        			if ($('body').hasClass("mobile")) {
        				vm.listView = true;
        			}
        		}

        		function getAllUsers() {
        			//Load resource references
        			cmsBase.translateSvc.partLoad('Configuration/JobTitle');

        			dataContext.user.getUsers(function (data) {
        				vm.usersData = data;
        				vm.GridData = angular.copy(vm.usersData, []);
        			},
					function (error) {
						cmsBase.cmsLog.error('error');
					});
        		}

        		vm.ReloadData = function () {
        			vm.disableSearch = true;
        			vm.query = '';
        			$timeout(function () {
        				vm.disableSearch = false;
        				if (vm.selectedRows.length > 0) {
        					vm.selectedRows.splice(0, vm.selectedRows.length);
        					$scope.$broadcast(AppDefine.Events.ROWSELECTEDCHANGE, vm.selectedRows);
        				}
        				getAllUsers();
        			}, 10);
        		}

        		//ThangPham, Handler ngGrid data change event, July  01 2015
        		$scope.$on('ngGridEventData', function () {
        			if (vm.itemEdited) {
        				//selectRowEdited(vm.itemEdited);
        				setSelectedRow(vm.itemEdited);
        				vm.itemEdited = null;
        			}
        		});

        		$scope.$watch('vm.query', function (data) {
        			if (vm.disableSearch) return;

        			getCheckedRows();
        			if (!data) {
        				vm.GridData = angular.copy(vm.usersData, []);
        				/*if (vm.selectedRows) {
        					var rowSelected = angular.copy(vm.selectedRows);
        					$timeout(function () {
        						vm.toggleSelectAll(false);
        						angular.forEach(rowSelected, function (value) {
        							selectRowEdited(value);
        						});
        					}, 100);
						}*/
        			}
        			else {
        				vm.GridData = angular.copy(vm.usersData, []);
        				vm.GridData = vm.GridData.filter(function (item) {
        					var fullName = item.FName + item.LName;
        					if (!$.isEmptyObject(fullName) && fullName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        						return true;
        					}
        					//if (!$.isEmptyObject(item.FName) && item.FName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        					//	return true;
        					//}
        					//if (!$.isEmptyObject(item.LName) && item.LName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        					//	return true;
        					//}
        					if (!$.isEmptyObject(item.EmpID.toString()) && item.EmpID.toString().indexOf(data) > -1) {
        						return true;
        					}
        					if (!$.isEmptyObject(item.PosName) && item.PosName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        						return true;
        					}
        					if (!$.isEmptyObject(item.Email) && item.Email.indexOf(data) > -1) {
        						return true;
        					}
        					if (!$.isEmptyObject(item.ExDate)
								&& $filter('date')(item.ExDate, "MM-dd-yyyy").toString().indexOf(data) > -1) {
        						return true;
        					}
        				});
        			}
        			updateChecked();
        		});

        		vm.gridOptions = {
        			data: 'vm.GridData',
        			rowHeight: 40,
        			minWidth: 120,
        			multiSelect: true,
        			selectedItems: vm.selectedRows,
        			//filterOptions: vm.query,
        			enableSorting: false,
        			rowTemplate: '<div ng-dblclick="vm.doubleClickOnRow(row.entity)" ng-style="{ \'cursor\': row.cursor }" \
									ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
										<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell ng-class="{\'i-expired\': row.entity.Expired == true }"></div> \
									</div>',
        			columnDefs: [
						{
							field: '', displayName: 'check', width: '120px'
							, headerCellTemplate: '<div class="checkbox checkbox-default" ng-show="multiSelect"><input type="checkbox" ng-model="vm.allSelected" ng-change="vm.toggleSelectAll(vm.allSelected)"><label></label></div>'
							, cellTemplate: '<div class="ngCellText item-info"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<div class="i-image"> \
														<img ng-if="row.entity.UPhoto!=null" ng-src="{{vm.ApiImageUrl(row.entity.UID, row.entity.UPhoto)}}" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}" /> \
														<img ng-if="row.entity.UPhoto==null" src="../Content/Images/img_user_blank.png" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}"/>\
												</div> \
											</div>'
						}
						, {
							field: 'FName', displayName: $filter('translate')('USER_NAME')
                             , cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}} {{row.entity.LName}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true">{{row.getProperty(col.field)}} {{row.entity.LName}}</div>'
						}
						, { field: 'EmpID', displayName: $filter('translate')('EMPLOYEE_ID') }
						, {
							field: 'PosName', displayName: $filter('translate')('POSITION_NAME')
                            , cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true">{{row.getProperty(col.field)}}</div>'
						}
						, {
							field: 'Email', displayName: $filter('translate')('EMAIL')
                            , cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true">{{row.getProperty(col.field)}}</div>'
						}
						, { field: 'ExDate', displayName: $filter('translate')('EXPIRED_DATE'), cellTemplate: '<div class="ngCellText i-date">{{row.getProperty(col.field) | date:"MM-dd-yyyy"}}</div>' }
						, { field: 'UID', visible: false }
        			],
        			afterSelectionChange: function (data) {
        				afterSelectedRow();
        			}
        		};

        		vm.listOptions = {
        			data: 'vm.GridData',
        			rowHeight: 70,
        			multiSelect: true,
        			selectedItems: vm.selectedRows,
        			//filterOptions: vm.query,
        			rowTemplate: '<div ng-dblclick="vm.doubleClickOnRow(row.entity)" ng-style="{ \'cursor\': row.cursor }" \
                                    ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}">\
                                    <div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell ng-class="{\'i-expired\': row.entity.Expired == true }"></div></div>',
        			columnDefs: [
						{
							field: '', displayName: ''
							, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="list-tooltip" tooltip-append-to-body="true"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<span class="i-image"> \
													<img ng-if="row.entity.UPhoto!= null" ng-src="{{vm.ApiImageUrl(row.entity.UID, row.entity.UPhoto)}}" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}"/> \
													<img ng-if="row.entity.UPhoto== null" src="../Content/Images/img_user_blank.png" alt="" ng-style="{\'border\': \'2px solid \', \'border-color\': vm.convertColor(row.entity.PosColor)}"/>\
												</span> \
												<div class="i-name"> \
														{{row.entity.FName}} {{row.entity.LName}} \
												</div> \
												<div class="i-pos"> \
													{{row.entity.PosName}} \
												</div> \
												<div class="i-mail"> \
													{{row.entity.Email}} \
													<span ng-show="{{row.entity.Expired}}"> \
														| <span class="i-date"><i class="icon-clock"></i> Expired {{row.entity.ExDate | date:"MM/dd/yyyy"}} </span> \
													</span> \
												</div> \
											</div>'
						}
        			],
        			afterSelectionChange: function (data) {
        				afterSelectedRow();
        			}
        		};

        		vm.addUser = function () {
        			showDialog();
        		}

        		vm.editUser = function (user) {
        			if (user.length == 1) {
        				showDialog(user[user.length - 1]); //Only edit when 1 item selected.
        			}
        		}

        		vm.doubleClickOnRow = function (value) {
        			vm.toggleSelectAll(false);
        			vm.allSelected = false;
        			//selectRowEdited(value);
        			setSelectedRow(value);
        			showDialog(value);
        		}

        		vm.deleteUser = function (users) {
        			if (!$.isEmptyObject(users)) {
        				if (!$scope.modalShown) {
        					$scope.modalShown = true;
        					var userDeleteInstance = $modal.open({
        						templateUrl: 'configuration/users/delete.html',
        						controller: 'userdeleteCtrl as vm',
        						size: 'sm',
        						backdrop: 'static',
        						backdropClass: 'modal-backdrop',
        						keyboard: false,
        						resolve: {
        							items: function () {
        								return users;
        							}
        						}
        					});

        					userDeleteInstance.result.then(function (data) {
        						$scope.modalShown = false;
        						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
        							vm.toggleSelectAll(false);
        							getAllUsers(); //refresh gridview
        							ResetQuery();

        						}
        					});
        				}
        			}
        		}

        		vm.toggleSelectAll = function (isCheckAll) {
        			if (typeof vm.gridOptions.selectAll == 'function')
        				vm.gridOptions.selectAll(isCheckAll);
        			if (typeof vm.listOptions.selectAll == 'function')
        				vm.listOptions.selectAll(isCheckAll);
        		}

        		function showDialog(user) {
        			if (!$scope.modalShown) {
        				$scope.modalShown = true;
        				var userInstance = $modal.open({
        					templateUrl: 'configuration/users/edit.html',
        					controller: 'editadduserCtrl as vm',
        					size: 'md',
        					backdrop: 'static',
        					backdropClass: 'modal-backdrop',
        					keyboard: false,
        					resolve: {
        						items: function () {
        							return user;
        						}
        					}
        				});

        				userInstance.result.then(function (data) {
        					$scope.modalShown = false;
        					if (data != AppDefine.ModalConfirmResponse.CLOSE) {
        						vm.toggleSelectAll(false);
        						vm.itemEdited = data;
        						//vm.query = '';
        						vm.activeRows = [];
        						vm.activeRows.push(data);
        						getAllUsers(); //refresh gridview
        						ResetQuery();
        					}
        				});
        			}
        		}

        		vm.convertColor = function (data) {
        			return colorSvc.numtoRGB(data);
        		};

        		vm.ApiImageUrl = function (userId, fileName) {
        			return dataContext.user.GetUserImage(userId, fileName);
        		};

        		function selectRowEdited(userEdited) {
        			angular.forEach(vm.GridData, function (user, index) {
        				if (user.UID == userEdited.UserID || user.UID == userEdited.UID) {
        					if (typeof vm.gridOptions.selectRow == "function") {
        						vm.gridOptions.selectRow(index, true);
        					}
        					else if (typeof vm.listOptions.selectRow == "function") {
        						vm.listOptions.selectRow(index, true);
        					}
        				}
        			});
        		}

        		//function afterSelectedRow() {
        		//	$scope.$broadcast(AppDefine.Events.ROWSELECTEDCHANGE, vm.selectedRows);
        		//	if (vm.selectedRows.length == 0) {
        		//		vm.allSelected = false;
        		//	}
        		//	else if (vm.selectedRows.length == vm.GridData.length) {
        		//		vm.allSelected = true;
        		//	}
        		//}
        		//****************************************************************************
        		function afterSelectedRow(seldata) {
        			$scope.$broadcast(AppDefine.Events.ROWSELECTEDCHANGE, vm.selectedRows);
        			if (vm.selectedRows.length == vm.GridData.length && vm.selectedRows.length > 0) {
        				vm.allSelected = true;
        			}
        			else {//if (vm.selectedRows.length == 0) {
        				vm.allSelected = false;
        			}
        		}

        		function getCheckedRows() {
        			vm.activeRows = angular.copy(vm.selectedRows);
        		}
        		function updateChecked() {
        			if (vm.activeRows && vm.activeRows.length > 0) {
        				var rowSelected = angular.copy(vm.activeRows);
        				$timeout(function () {
        					vm.toggleSelectAll(false);
        					angular.forEach(rowSelected, function (value) {//rowSelected
        						setSelectedRow(value);
        					});
        					vm.activeRows = [];
        				}, 100);
        			}
        			afterSelectedRow(null);
        		}
        		function ResetQuery() {
        			vm.disableSearch = true;
        			var curquery = vm.query;
        			vm.query = '';
        			$timeout(function () {
        				vm.disableSearch = false;
        				if (curquery == '') {
        					updateChecked();
        				}
        				else {
        					vm.query = curquery;
        				}
        			}, 200);
        		}
        		function setSelectedRow(selData) {
        			var foundItem = Enumerable.From(vm.GridData)
						.Where(function (i) { return (i.UserName == selData.UserName) })
						.Select(function (x) { return x })
						.FirstOrDefault(); //(i.UID == selData.UserID) || (selData.UserID == 0 && 
        			if (foundItem) {
        				var index = Enumerable.From(vm.GridData).IndexOf(foundItem);
        				if (index >= 0) {
        					//foundItem.selected = true;
        					if (typeof vm.gridOptions.selectRow == "function") {
        						vm.gridOptions.selectRow(index, true);
        					}
        					else if (typeof vm.listOptions.selectRow == "function") {
        						vm.listOptions.selectRow(index, true);
        					}
        					return true;
        				}
        			}
        			return false;
        		}
        		//****************************************************************************
        	}
        }
    );
})();