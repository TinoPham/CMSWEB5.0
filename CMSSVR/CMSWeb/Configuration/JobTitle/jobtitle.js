(function () {
	'use strict';
	define(['cms', 'DataServices/Configuration/jobtitleSvc'
		, 'Configuration/Jobtitle/edit', 'Configuration/Jobtitle/delete']
		, function (cms) {
			cms.register.controller('jobtitleCtrl', ['$scope', '$modal', '$filter', 'dialogSvc', 'JobTitleSvc', 'cmsBase', 'colorSvc', 'AppDefine', '$timeout',
			function ($scope, $modal, $filter, dialogSvc, JobTitleSvc, cmsBase, colorSvc, AppDefine, $timeout) {
				var vm = this;
				vm.selectedRows = [];
				vm.GridData = [];
				vm.query = '';
				vm.listView = false; //default view grid Mode
				vm.itemEdited = null;
				vm.allSelected = false;

				vm.disableSearch = false;
				vm.activeRows = [];

				active();

				function active() {
					setGridMode();
					getData();
				}

				function setGridMode() {
					if ($('body').hasClass("mobile")) {
						vm.listView = true;
					}
				}

				function getData() {
					JobTitleSvc.GetJobTitle().then(function (data) {
						vm.JobTitleData = data;
						vm.GridData = angular.copy(vm.JobTitleData, []);
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
						getData();
					}, 10);
				}

				//ThangPham, Handler ngGrid data change event, July  01 2015
				$scope.$on('ngGridEventData', function (event, arg) {
					if (vm.itemEdited) {
						//(vm.selectedRows) {
						//selectRowEdited(vm.selectedRows);
						//vm.itemEdited = null;
						setSelectedRow(vm.itemEdited);
						vm.itemEdited = null;
					}
				});

				$scope.$watch('vm.query', function (data) {
					if (vm.disableSearch) return;

					getCheckedRows();
					if (!data) {
						vm.GridData = angular.copy(vm.JobTitleData, []);
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
						vm.GridData = angular.copy(vm.JobTitleData, []);
						vm.GridData = vm.GridData.filter(function (item) {
							if (!$.isEmptyObject(item.PositionName) && item.PositionName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(data.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
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
										ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
										<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
					columnDefs: [
						{
							field: 'PositionName', displayName: $filter('translate')(AppDefine.Resx.JOB_NAME)
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
						, {
							field: 'Color', displayName: $filter('translate')(AppDefine.Resx.JOB_COLOR), width: '55px', sortable: false
							, cellTemplate: '<div class="ngCellText item-info"><span class="i-color" ng-style="{ \'border-color\': vm.convertColor(row.entity.Color) }"></span></div>'
						}
						, {
							field: 'Description', displayName: $filter('translate')(AppDefine.Resx.JOB_DESCRIPTION)
								, cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true"><span>{{row.getProperty(col.field)}}</span></div>'
					}
					],
					afterSelectionChange: function (data) {
						afterSelectedRow();
					}
				};

				vm.listOptions = {
					data: 'vm.GridData',
					rowHeight: 50,
					multiSelect: true,
					selectedItems: vm.selectedRows,
					//filterOptions: vm.query,
					rowTemplate: '<div ng-dblclick="vm.doubleClickOnRow(row.entity)" ng-style="{ \'cursor\': row.cursor }" \
										ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"> \
										<div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
					columnDefs: [
						{
							field: 'PositionName', displayName: $filter('translate')(AppDefine.Resx.JOB_NAME)
								, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="list-tooltip" tooltip-append-to-body="true"> \
													<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
													<span class="i-color" ng-style="{ \'border-color\': vm.convertColor(row.entity.Color) }"></span> \
													<div class="i-name">{{row.entity.PositionName}}</div> \
													<div class="i-des">{{row.entity.Description}}</div> \
												</div>'
						}
					],
					afterSelectionChange: function (data) {
						afterSelectedRow();
					}
				};
			
				vm.modalShown = false;

				vm.addJobTitle = function () {
					showDialog();
				};

				vm.editJobTitle = function (valueBTN) {
						if (valueBTN.length == 1) {
						showDialog(valueBTN[valueBTN.length - 1]); //Only edit when 1 item selected.
					}
				};

				vm.doubleClickOnRow = function (value) {
					vm.toggleSelectAll(false);
					vm.allSelected = false;
					//selectRowEdited(value);
					setSelectedRow(value);
					showDialog(value);
				}

				vm.deleteJob = function (jobs) {
						if (!$.isEmptyObject(jobs)) {
							if (!vm.modalShown) {
								vm.modalShown = true;
									var jobDeleteInstance = $modal.open({
										templateUrl: 'configuration/jobtitle/delete.html',
										controller: 'jobdeleteCtrl as vm',
										size: 'sm',
										backdrop: 'static',
										backdropClass: 'modal-backdrop',
										keyboard: false,
										resolve: {
											items: function () {
												return jobs;
											}
										}
									});

									jobDeleteInstance.result.then(function (data) {
									vm.modalShown = false;
									if (data != AppDefine.ModalConfirmResponse.CLOSE) {
										vm.toggleSelectAll(false);
										getData();
										ResetQuery();
									}
								});
							}
					}
				};

				vm.convertColor = function (data) {
					return colorSvc.numtoRGB(data);
				};

				vm.toggleSelectAll = function (isCheckAll) {
					if (typeof vm.gridOptions.selectAll == 'function')
						vm.gridOptions.selectAll(isCheckAll);
					if (typeof vm.listOptions.selectAll == 'function')
						vm.listOptions.selectAll(isCheckAll);
				}

				function showDialog(valueBTN) {
					if (!vm.modalShown) {
						vm.modalShown = true;
						var jobInstance = $modal.open({
							templateUrl: 'Configuration/JobTitle/edit.html',
							controller: 'editaddjobtitleCtrl',
								size: 'sm',
								backdrop: 'static',
								backdropClass: 'modal-backdrop',
								keyboard: false,
								resolve: {
									items: function () {
									return valueBTN;
								}
							}
						});

						jobInstance.result.then(function (data) {
							vm.modalShown = false;
							if (data != AppDefine.ModalConfirmResponse.CLOSE) {
								vm.toggleSelectAll(false);
								vm.itemEdited = data;
								vm.activeRows = [];
								vm.activeRows.push(data);
								getData();
								ResetQuery();
							}
						});
					}
				}

				function selectRowEdited(selectedItems) {
					if (!selectedItems) { return; }
					for (var i = 0; i < selectedItems.length; i++) {
						angular.forEach(vm.GridData, function (job, index) {
									if (job.PositionID == selectedItems[i].PositionID) {
								if (typeof vm.gridOptions.selectRow == "function") {
									vm.gridOptions.selectRow(index, true);
								}
								else if (typeof vm.listOptions.selectRow == "function") {
									vm.listOptions.selectRow(index, true);
								}
							}
						});
					}
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
						.Where(function (i) { return ((i.PositionID == selData.PositionID) || (selData.PositionID == 0 && i.PositionName == selData.PositionName)) })
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
		}]);
	});
})();