(function () {
	'use strict';

	define(['cms', 'DataServices/Configuration/sitemetricSvc'
		, 'configuration/sitemetric/edit', 'configuration/sitemetric/delete']
		, function (cms) {
			cms.register.controller('sitemetricCtrl', ['$scope', '$modal', '$filter', 'dialogSvc', 'SiteMetricSvc', 'cmsBase', 'AppDefine', '$timeout',
			function ($scope, $modal, $filter, dialogSvc, SiteMetricSvc, cmsBase, AppDefine, $timeout) {
				var vm = this;
				vm.selectedRows = [];
				vm.query = '';
				vm.GridData = [];
				vm.listView = false; //default view grid Mode
				vm.itemEdited = null;
				vm.allSelected = false;

				vm.activeRows = [];
				vm.disableSearch = false;

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
					SiteMetricSvc.GetAllMetric().then(function (data) {
						vm.metricData = data;
						vm.GridData = angular.copy(vm.metricData, []);
						//filterGrid(vm.query);
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
				$scope.$on('ngGridEventData', function () {
					//console.log("change data");
					//if (vm.itemEdited) {
					//	$timeout(function () { selectRowEdited(vm.itemEdited); }, 3000);
					//	//selectRowEdited(vm.itemEdited);
					//	//vm.itemEdited = null;
					//}
					if (vm.itemEdited) {
						//selectRowEdited(vm.itemEdited);
						setSelectedRow(vm.itemEdited);
						vm.itemEdited = null;
					}
				});

				$scope.$watch('vm.query', function (data, oldvalue) {
					if (vm.disableSearch) return;
					filterGrid(data);
				});
				vm.gridOptions = {
					data: 'vm.GridData',
					rowHeight: 40,
					minWidth: 120,
					multiSelect: true,
					selectedItems: vm.selectedRows,//[], //
					//filterOptions: vm.query,
					enableSorting: false,
					rowTemplate: '<div ng-dblclick="vm.doubleClickOnRow(row.entity)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
					columnDefs: [
						{
							field: 'MetricName', displayName: $filter('translate')(AppDefine.Resx.SITEMETRIC_NAME)
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
							field: 'MetricMeasure', displayName: $filter('translate')(AppDefine.Resx.SITEMETRIC_DESCRIPTION)
						, cellTemplate: '<div class="ngCellText" tooltip="{{row.getProperty(col.field)}}" tooltip-trigger="mouseenter" tooltip-class="grid-tooltip" tooltip-append-to-body="true">{{row.getProperty(col.field)}}</div>'
						}
						, { field: 'UUsername', displayName: $filter('translate')(AppDefine.Resx.CREATE_BY) }
						, { field: 'MListEditedDate', displayName: $filter('translate')(AppDefine.Resx.CREATE_DATE), cellFilter: "date:'MM/dd/yyyy'" }
					],
					afterSelectionChange: function (data) {
						afterSelectedRow(data);
					}
				};

				vm.listOptions = {
					data: 'vm.GridData',
					rowHeight: 70,
					multiSelect: true,
					selectedItems: vm.selectedRows,//[], //
					//filterOptions: vm.query,
					rowTemplate: '<div ng-dblclick="vm.doubleClickOnRow(row.entity)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
					columnDefs: [
						{
							field: 'MetricName', displayName: $filter('translate')(AppDefine.Resx.SITEMETRIC_NAME)
							, cellTemplate: '<div class="ngCellText item-info" tooltip-template="\'Widgets/Templates/tooltip.html\'" tooltip-trigger="mouseenter" tooltip-class="list-tooltip" tooltip-append-to-body="true"> \
												<div class="checkbox checkbox-default"><input type="checkbox" tabindex="-1" ng-checked="row.selected"><label></label></div> \
												<div class="i-name">{{row.entity.MetricName}}</div> \
												<div class="i-des">{{row.entity.MetricMeasure}}</div> \
												<div class="i-createdby">{{row.entity.UUsername}}</div> \
												<div class="i-date"> | Date: {{row.entity.MListEditedDate | date:"MM/dd/yyyy" }}</div> \
											</div>'
						}
					],
					afterSelectionChange: function (data) {
						afterSelectedRow(data);
					}
				};

				vm.modalShown = false;

				vm.addMetric = function () {
					showDialog();
				};

				vm.editMetric = function (valueBTN) {
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

				vm.deleteMetric = function (metrics) {
					if (!$.isEmptyObject(metrics)) {
						if (!vm.modalShown) {
							vm.modalShown = true;
							var metricDeleteInstance = $modal.open({
								templateUrl: 'configuration/sitemetric/delete.html',
								controller: 'metricdeleteCtrl as vm',
								size: 'sm',
								backdrop: 'static',
								backdropClass: 'modal-backdrop',
								keyboard: false,
								resolve: {
									items: function () {
										return metrics;
									}
								}
							});

							metricDeleteInstance.result.then(function (data) {
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

				vm.toggleSelectAll = function (isCheckAll) {
					if (typeof vm.gridOptions.selectAll == 'function') {
						vm.gridOptions.selectAll(isCheckAll);
						//if (!isCheckAll) {
						//	vm.selectedRows = [];
						//}
						//else {
						//	vm.selectedRows = vm.gridOptions.selectedItems;
						//}
					}
					if (typeof vm.listOptions.selectAll == 'function') {
						vm.listOptions.selectAll(isCheckAll);
						//if (!isCheckAll) {
						//	vm.selectedRows = [];
						//}
						//else {
						//	vm.selectedRows = vm.listOptions.selectedItems;
						//}
					}
					afterSelectedRow(null);
				}

				function showDialog(valueBTN) {
					if (!vm.modalShown) {
						vm.modalShown = true;
						var metricInstance = $modal.open({
							templateUrl: 'configuration/sitemetric/edit.html',
							controller: 'editaddsitemetricCtrl as vm',
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

						metricInstance.result.then(function (data) {
							vm.modalShown = false;
							if (data != AppDefine.ModalConfirmResponse.CLOSE) {
								vm.toggleSelectAll(false);
								if (data.length > 1) {
									angular.forEach(data, function (metric, index) {
										if (metric.ParentID == null) {
											vm.itemEdited = metric;
										}
									});
								}
								else {
									vm.itemEdited = data;
								}
								//Reresh Grid
								vm.activeRows = [];
								vm.activeRows.push(vm.itemEdited);
								getData();
							ResetQuery();
							}
						});
					}
				}

				function selectRowEdited(metricEdited) {
					angular.forEach(vm.GridData, function (metric, index) {
						if (metric.MListID == metricEdited.MListID) {
							if (typeof vm.gridOptions.selectRow == "function") {
								vm.gridOptions.selectRow(index, true);
							}
							else if (typeof vm.listOptions.selectRow == "function") {
								vm.listOptions.selectRow(index, true);
							}
						}
					});
				}

				function filterGrid(searchText) {
					getCheckedRows();
					if (!searchText) {
						vm.GridData = angular.copy(vm.metricData, []);
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
						//vm.GridData = angular.copy(vm.metricData, []);
						var count = 0;
						vm.GridData = vm.metricData.filter(function (item) {
							count++;
							if (!$.isEmptyObject(item.MetricName) && item.MetricName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(searchText.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
								return true;
							}
							if (!$.isEmptyObject(item.UUsername) && item.UUsername.indexOf(searchText) > -1) {
								return true;
							}
							if (!$.isEmptyObject(item.MListEditedDate)
								&& $filter('date')(item.MListEditedDate, "MM/dd/yyyy").toString().indexOf(searchText) > -1) {
								return true;
							}
							if (!$.isEmptyObject(item.MetricMeasure)) {
								if (item.MetricMeasure.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(searchText.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
									return true;
								}
							}
						});
					}
					updateChecked();
				}
				//****************************************************************************
				function afterSelectedRow(seldata) {
					//if (seldata != null) {
					//	updateSelectedRow(seldata.selected, seldata.entity);
					//}
					$scope.$broadcast(AppDefine.Events.ROWSELECTEDCHANGE, vm.selectedRows);

					if (vm.selectedRows.length == vm.GridData.length && vm.selectedRows.length > 0) {
						vm.allSelected = true;
					}
					else {//if (vm.selectedRows.length == 0) {
						vm.allSelected = false;
					}
				}

				//$scope.$watch('vm.listView', function (data, oldval) {
				//	updateChecked();
				//});
				function getCheckedRows() {
					vm.activeRows = angular.copy(vm.selectedRows);
				}
				function updateChecked() {
					if (vm.activeRows && vm.activeRows.length > 0) {
						var rowSelected = angular.copy(vm.activeRows);
						$timeout(function () {
							vm.toggleSelectAll(false);
							angular.forEach(rowSelected, function (value) {//rowSelected
								//selectRowEdited(value);
								//if (setSelectedRow(value)) {
								//	vm.activeRows.push(value);
								//}
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
						.Where(function (i) { return ((i.MListID == selData.MListID) || (selData.MListID == 0 && i.MetricName == selData.MetricName)) })
						.Select(function (x) { return x })
						.FirstOrDefault();
					if (foundItem) {
						//vm.gridOptions.selectedItems.push(foundItem);
						//vm.listOptions.selectedItems.push(foundItem);
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
				function updateSelectedRow(isSel, entity) {
					//var index = Enumerable.From(vm.selectedRows).IndexOf(entity);
					//if (isSel) {
					//	if (index < 0) {
					//		vm.selectedRows.push(entity);
					//	}
					//}
					//else {
					//	if (index >= 0) {
					//		vm.selectedRows.splice(index, 1);
					//	}
					//}
				}
				//****************************************************************************
			}]);
		});
})();