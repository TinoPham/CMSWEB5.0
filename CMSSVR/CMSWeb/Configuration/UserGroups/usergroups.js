(function () {
	'use strict';
	define(['cms', 'Configuration/UserGroups/edit', 'Configuration/UserGroups/delete'],
        function (cms) {
        	cms.register.controller('usergroupsCtrl', usergroupsCtrl);
        	usergroupsCtrl.$inject = ['$scope', '$modal', '$filter', 'cmsBase', 'dataContext', 'AppDefine', 'dialogSvc', '$timeout'];
        	function usergroupsCtrl($scope, $modal, $filter, cmsBase, dataContext, AppDefine, dialogSvc, $timeout) {
        		var vm = this;
        		vm.usersGroup = [];
        		vm.query = '';
        		vm.listView = false; //set default: Grid view mode
        		vm.selectedRows = [];
        		vm.GridData = [];
        		vm.itemEdited = null;
        		vm.allSelected = false;

        		vm.disableSearch = false;
				vm.activeRows = [];

        		active();

        		function active() {
        			setGridMode();
        			dataContext.injectRepos(['configuration.usergroups']).then(getUserGroups);
        		}

        		function setGridMode() {
        			if ($('body').hasClass("mobile")) {
        				vm.listView = true;
        			}
        		}

        		function getUserGroups() {
        			dataContext.usergroups.getUserGroup().then(
						function (data) {
        					vm.usersGroup = data;
        					vm.GridData = angular.copy(vm.usersGroup, []);
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
        			getUserGroups();
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
        				vm.GridData = angular.copy(vm.usersGroup, []);
						//if (vm.selectedRows) {
						//	var rowSelected = angular.copy(vm.selectedRows);
						//	$timeout(function () {
						//		vm.toggleSelectAll(false);
						//		angular.forEach(rowSelected, function (value) {
						//			selectRowEdited(value);
						//		});
						//	}, 100);
						//}
        			}
        			else {
        				vm.GridData = angular.copy(vm.usersGroup, []);
        				vm.GridData = vm.GridData.filter(function (item) {
        					if (!$.isEmptyObject(item.GroupName) && item.GroupName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        						return true;
        					}
        					if (!$.isEmptyObject(item.NumberUser.toString()) && item.NumberUser.toString().indexOf(data) > -1) {
        						return true;
        					}
        					if (!$.isEmptyObject(item.Description)) {
        						if (item.Description.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
        							return true;
        						}
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
                                    ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}">\
                                    <div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
        			columnDefs: [
						{
							field: 'GroupName', displayName: $filter('translate')(AppDefine.Resx.USERGROUP_NAME)
							, headerCellTemplate: '<div class="checkbox checkbox-default" ng-show="multiSelect"><input type="checkbox" ng-model="vm.allSelected" ng-change="vm.toggleSelectAll(vm.allSelected)"><label></label></div> \
												<div class="ngHeaderSortColumn {{col.headerClass}}" ng-style="{\'cursor\': col.cursor}" ng-class="{ \'ngSorted\': !noSortVisible }"> \
													<div ng-click="col.sort($event)" ng-class="\'colt\' + col.index" class="ngHeaderText">{{col.displayName}}</div> \
													<div class="ngSortButtonDown" ng-show="col.showSortButtonDown()"></div> \
													<div class="ngSortButtonUp" ng-show="col.showSortButtonUp()"></div> \
													<div class="ngSortPriority">{{col.sortPriority}}</div> \
													<div ng-class="{ ngPinnedIcon: col.pinned, ngUnPinnedIcon: !col.pinned }" ng-click="togglePin(col)" ng-show="col.pinnable"></div> \
												</div> \
												<div ng-show="col.resizable" class="ngHeaderGrip" ng-click="col.gripClick($event)" ng-mousedown="col.gripOnMouseDown($event)"></div>'
							, cellTemplate: '<div class="ngCellText item-info" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div>\
												<div class="i-name">{{row.getProperty(col.field)}}</div> \
											</div>'
						}
						, { field: 'NumberUser', displayName: $filter('translate')('USERGROUP_NO_OF_USERS') }
						, {
							field: 'Description', displayName: $filter('translate')('USERGROUP_DESCRIPTION')
                            , cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true">{{row.getProperty(col.field)}}</div>'
						}
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
                                    <div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
        			columnDefs: [
						{
							field: 'GroupName', displayName: $filter('translate')(AppDefine.Resx.USERGROUP_NAME)
							, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="list-tooltip" tooltip-append-to-body="true"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<div class="i-name">{{row.entity.GroupName}}</div> \
												<div class="i-number">{{vm.GenerateField(row.entity.NumberUser)}}</div> \
												<div class="i-des">{{row.entity.Description}}</div> \
											</div>'
						}
        			],
        			afterSelectionChange: function (data) {
        				afterSelectedRow();
        			}
        		};

        		vm.addGroup = function () {
        			showDialog();
        		}

        		vm.editGroup = function (usergroup) {
        			if (usergroup.length == 1) {
        				showDialog(usergroup[usergroup.length - 1]); //Only edit when 1 item selected.
        			}
        		}

        		vm.doubleClickOnRow = function (value) {
        			vm.toggleSelectAll(false);
        			vm.allSelected = false;
					//selectRowEdited(value);
					setSelectedRow(value);
        			showDialog(value);
        		}

        		vm.deleteGroup = function (groups) {
        			if (!$.isEmptyObject(groups)) {
        				//check any usergroup that assigned to user.
        				var userGroupAssigned = $.grep(groups, function (ugroup) { return ugroup.Users.length > 0; });
        				if (groups.length > 1 && userGroupAssigned.length > 0) {
        					var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.UNABLE_TO_DELETE_MULTIPLE_USERGROUP);
        					cmsBase.cmsLog.warning(msg);
        					return;
        				}

        				if (!vm.modalShown) {
        					vm.modalShown = true;
        					var userDeleteInstance = $modal.open({
        						templateUrl: 'configuration/usergroups/delete.html',
        						controller: 'usergroupdelCtrl as vm',
        						size: 'sm',
        						backdrop: 'static',
        						backdropClass: 'modal-backdrop',
        						keyboard: false,
        						resolve: {
        							items: function () { return groups; }
        							, userGroupData: function () { return Enumerable.From(vm.usersGroup).Except(groups, "[$.GroupId, $.GroupName].join(':')").ToArray(); }
        						}
        					});

        					userDeleteInstance.result.then(function (data) {
        						vm.modalShown = false;
        						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
        							vm.toggleSelectAll(false);
        							getUserGroups();
        							//vm.query = '';
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

        		vm.GenerateField = function (value) {
        			var ret = value.toString() + " " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NUMBER);
        			if (value > 1)
        				ret = value.toString() + " " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NUMBERS);
        			return ret;
        		}

        		function showDialog(usergroup) {
        			if (!vm.modalShown) {
        				vm.modalShown = true;
        				var userInstance = $modal.open({
        					templateUrl: 'configuration/usergroups/edit.html',
        					controller: 'editaddusergroupCtrl as vm',
        					size: 'md',
        					backdrop: 'static',
        					backdropClass: 'modal-backdrop',
        					keyboard: false,
        					resolve: {
        						items: function () {
        							return usergroup;
        						}
        					}
        				});

        				userInstance.result.then(function (data) {
        					vm.modalShown = false;
        					if (data != AppDefine.ModalConfirmResponse.CLOSE) {
        						vm.toggleSelectAll(false);
        						vm.itemEdited = data;
								//vm.selectedData = data;
								vm.activeRows = [];
								vm.activeRows.push(data);

        						getUserGroups(); //refresh gridview
        						ResetQuery(); //vm.query = '';
        					}
        				});
        			}
        		}

        		function selectRowEdited(ugroupEdited) {
        			angular.forEach(vm.GridData, function (ugroup, index) {
        				if (ugroup.GroupId == ugroupEdited.GroupId) {
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
						.Where(function (i) { return ((i.GroupId == selData.GroupId) || (selData.GroupId == 0 && i.GroupName == selData.GroupName)) })
						.Select(function (x) { return x })
						.FirstOrDefault();
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
        });
})();