(function () {
	'use strict';
	define(['cms', 'DataServices/Configuration/recipientSvc'
		, 'configuration/recipient/edit', 'configuration/recipient/delete']
		, function (cms) {
		cms.register.controller('recipientCtrl', ['$scope', '$modal', '$filter', 'dialogSvc', 'RecipientSvc', 'cmsBase', 'AppDefine',
		function ($scope, $modal, $filter, dialogSvc, RecipientSvc, cmsBase, AppDefine) {
			var vm = this;
			vm.selectedRows = [];
			var rowIndex = 0;
			vm.query = {
				filterText: ''
			};
			vm.listView = false; //set default: Grid view mode
			vm.itemEdited = null;

			getRecipient();
			
			function getRecipient() {
				RecipientSvc.GetRecipient().then(function (data) {
					vm.valueRecipient = data;
				});
			}

			vm.gridOptions = {
				data: 'vm.valueRecipient',
				rowHeight: 40,
				minWidth: 120,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				filterOptions: vm.query,
				rowTemplate: '<div ng-dblclick="editRecipient(row.entity)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [
					{
						field: 'FirstName', displayName: $filter('translate')(AppDefine.Resx.RECIPIENT_NAME)
						, headerCellTemplate: '<div class="ngHeaderSortColumn {{col.headerClass}}" ng-style="{\'cursor\': col.cursor}" ng-class="{ \'ngSorted\': !noSortVisible }"> \
												<div ng-click="col.sort($event)" ng-class="\'colt\' + col.index" class="ngHeaderText"> \
													<div class="checkbox checkbox-default ngSelectionHeader" ng-show="multiSelect"> \
														<input type="checkbox" ng-model="allSelected" ng-change="toggleSelectAll(allSelected)"> \
														<label ng-click="col.sort($event)">{{col.displayName}}</label> \
													</div> \
												</div> \
												<div class="ngSortButtonDown" ng-show="col.showSortButtonDown()"></div> \
												<div class="ngSortButtonUp" ng-show="col.showSortButtonUp()"></div> \
												<div class="ngSortPriority">{{col.sortPriority}}</div> \
												<div ng-class="{ ngPinnedIcon: col.pinned, ngUnPinnedIcon: !col.pinned }" ng-click="togglePin(col)" ng-show="col.pinnable"></div> \
											</div> \
											<div ng-show="col.resizable" class="ngHeaderGrip" ng-click="col.gripClick($event)" ng-mousedown="col.gripOnMouseDown($event)"></div>'
						, cellTemplate: '<div class="ngCellText ngSelectionCell checkbox checkbox-default"> \
										<input type="checkbox" class="ngSelectionCheckbox" tabindex="-1" ng-checked="row.selected">\
                                        <label title="{{row.entity.FirstName}} {{row.entity.LastName}}">{{row.entity.FirstName}} {{row.entity.LastName}}</label> \
									</div>'
					}
					, { field: 'Email', displayName: $filter('translate')(AppDefine.Resx.EMAIL) }
					, { field: 'UUsername', displayName: $filter('translate')(AppDefine.Resx.CREATE_BY) }
				],
				afterSelectionChange: function (data) {
					rowIndex = this.rowIndex;
				}
			};

			vm.listOptions = {
				data: 'vm.valueRecipient',
				rowHeight: 70,
				multiSelect: true,
				selectedItems: vm.selectedRows,
				filterOptions: vm.query,
				rowTemplate: '<div ng-dblclick="editRecipient(row.entity)" ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>',
				columnDefs: [
					{
						field: 'FirstName', displayName: $filter('translate')(AppDefine.Resx.RECIPIENT_NAME)
						, cellTemplate: '<div class="rep-info ngCellText ngSelectionCell checkbox checkbox-default"> \
											<input type="checkbox" class="ngSelectionCheckbox" tabindex="-1" ng-checked="row.selected"><label></label> \
											<div class="rep-name">{{row.entity.FirstName}} {{row.entity.LastName}}</div> \
											<div class="rep-email">{{row.entity.Email}}</div> \
											<div class="rep-created-by">{{row.entity.UUsername}}</div> \
										</div>'
					}
				],
				afterSelectionChange: function (data) {
					rowIndex = this.rowIndex;
				}
			};
			
			vm.modalShown = false;

			vm.addRecipient = function () {
				showDialog();
			}

			//ThangPham, Handler ngGrid data change event, July  01 2015
			$scope.$on('ngGridEventData', function () {
				if (vm.itemEdited) {
					selectRowEdited(vm.itemEdited);
					vm.itemEdited = null;
				}
			});

			vm.editRecipient = function (valueBTN) {
				if ($.isEmptyObject(valueBTN)) {
					showDialogConfirm();
				} else {
					showDialog(valueBTN);
				}
			}

			vm.deleteRecipient = function (recipients) {
				if ($.isEmptyObject(recipients)) {
					showDialogConfirm();
				} else {
					if (!vm.modalShown) {
						vm.modalShown = true;
						var recipientdelInstance = $modal.open({
							templateUrl: 'configuration/Recipient/delete.html',
							controller: 'recipientdelCtrl as vm',
							size: 'sm',
							backdrop: 'static',
							backdropClass: 'modal-backdrop',
							keyboard: false,
							resolve: {
								items: function () {
									return recipients;
								}
							}
						});

						recipientdelInstance.result.then(function (data) {
							vm.modalShown = false;
							if (data != AppDefine.ModalConfirmResponse.CLOSE) {
								getRecipient();
								destroySelectItem();
							}
						});
					}
				}
			}

			function showDialog(valueBTN) {
				if (!vm.modalShown) {
					vm.modalShown = true;
					var Instance = $modal.open({
						templateUrl: 'configuration/Recipient/edit.html',
						controller: 'editaddrecipientCtrl as vm',
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

					Instance.result.then(function (data) {
						vm.modalShown = false;
						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
							destroySelectItem();
							vm.itemEdited = null;
							getRecipient();
							vm.query = '';
						}
					});
				}
			}

			function showDialogConfirm() {
				var modalOptions = {
					headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
					bodyText: AppDefine.Resx.RECIPIENT_CONFIRM_BODY
				};

				var modalDefaults = {
					backdrop: true,
					keyboard: true,
					modalFade: true,
					templateUrl: 'Widgets/ConfirmDialog.html',
					size: 'sm'
				}

				dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
				});
			}

			function destroySelectItem() {
				if (typeof vm.gridOptions.selectAll == 'function')
					vm.gridOptions.selectAll(false);
				if (typeof vm.listOptions.selectAll == 'function')
					vm.listOptions.selectAll(false);
			}

			function selectRowEdited(repEdited) {
				angular.forEach(vm.GridData, function (rep, index) {
					if (rep.PositionID == repEdited.PositionID) {
						if (typeof vm.gridOptions.selectRow == "function") {
							vm.gridOptions.selectRow(index, true);
						}
						else if (typeof vm.listOptions.selectRow == "function") {
							vm.listOptions.selectRow(index, true);
						}
					}
				});
			}
		}]);
	});
})();