(function () {
	'use strict';

	define(['cms', 'widgets/rebar/column-option',
		'widgets/rebar/dynamic-filter',
		'Scripts/Services/filterSvc',
		'DataServices/POSCommon.service',
	],
		function (cms) {
			cms.register.controller('transactionListCtrl', transactionListCtrl);

			transactionListCtrl.$inject = ['$rootScope', '$scope', '$modal', '$filter', '$state', '$timeout', 'dataContext', 'cmsBase', 'AppDefine', 'rebarDataSvc', 'filterSvc', 'POSCommonSvc'];

			function transactionListCtrl($rootScope, $scope, $modal, $filter, $state, $timeout, dataContext, cmsBase, AppDefine, rebarDataSvc, filterSvc, POSCommonSvc) {
				var rvm = this;
				$scope.query = {
					'$select': '',
					"$filter": '',
					"$top": 10,
					"$skip": 0,
					"inlinecount": "allpages"
				};

				$scope.pasteRegx = AppDefine.RegExp.PasteExp;
				$scope.inputRex = AppDefine.RegExp.NumberRestriction;
				$scope.PreventPaste = cmsBase.PreventPaste;
				$scope.PreventLetter = cmsBase.PreventInputKeyPress;

				$scope.definefield = angular.copy(filterSvc.getTransQSColumn());
				rvm.dataSource = Enumerable.Empty();
				rvm.InitParam = null;
				$scope.sortAsc = false;
				rvm.EnableFitler = false;
				$scope.pageSize = 10;
				rvm.currentPage = 1;
				rvm.ShowBackButton = true;
				var meta = new filterSvc.Metadata();
				$scope.fieldName = 'TransID';

				$scope.initCtrl = function (param, Field_Function_Callback, definedFilter, options) {
					rvm.InitParam = {};
					//rvm.InitParam.param = angular.copy(param);
					rvm.InitParam.Field_Function_Callback = Field_Function_Callback;
					//rvm.InitParam.calbackFn = calbackFn;
					$scope.definefield = definedFilter;
					rvm.InitParam.definedFilter = angular.copy(definedFilter);
					rvm.InitParam.options = angular.copy(options);
					rvm.dataSource = param.Trans;
					rvm.dataSource = $scope.sortAsc ? Enumerable.From(rvm.dataSource).OrderByDescending(function (value) { return value[$scope.fieldName] }) : Enumerable.From(rvm.dataSource).OrderBy(function (value) { return value[$scope.fieldName] });
					var totalCount = rvm.dataSource.Count();
					$scope.totalPages = parseInt(totalCount / $scope.pageSize);
					if (totalCount % $scope.pageSize > 0) {
						$scope.totalPages = $scope.totalPages + 1;
					}
					var array = rvm.dataSource.Skip($scope.pageSize * (rvm.currentPage - 1)).Take($scope.pageSize).ToArray();
					$scope.data = array; //$scope.sortAsc ? Enumerable.From( array ).OrderByDescending( function ( value ) { return value[$scope.fieldName] } ).ToArray() : Enumerable.From( array ).OrderBy( function ( value ) { return value[$scope.fieldName] } ).ToArray();
					loadpagingData(rvm.currentPage);
					//refreshAllData(param, calbackFn, definedFilter, options);
				};

				$scope.initQSearchCtrl = function (param, calbackFn, definedFilter, options, dataservie_callback, ColumnSort_callback) {
					rvm.InitParam = {};
					rvm.InitParam.param = angular.copy(param.data);
					rvm.InitParam.calbackFn = angular.copy(calbackFn);
					rvm.InitParam.definedFilter = angular.copy(definedFilter);
					$scope.definefield = angular.copy(definedFilter);
					rvm.InitParam.options = angular.copy(options);
					rvm.callback = dataservie_callback;
					rvm.ColumnSortcallback = ColumnSort_callback;
					rvm.dataSource = Enumerable.From(param.Trans);

					rvm.dataSource = $scope.sortAsc ? Enumerable.From(rvm.dataSource).OrderByDescending(function (value) { return value[$scope.fieldName] }) : Enumerable.From(rvm.dataSource).OrderBy(function (value) { return value[$scope.fieldName] });
					var totalCount = rvm.dataSource.Count();
					$scope.totalPages = parseInt(totalCount / $scope.pageSize);
					if (totalCount % $scope.pageSize > 0) {
						$scope.totalPages = $scope.totalPages + 1;
					}
					var array = rvm.dataSource.Skip($scope.pageSize * (rvm.currentPage - 1)).Take($scope.pageSize).ToArray();
					$scope.data = array; //$scope.sortAsc ? Enumerable.From( array ).OrderByDescending( function ( value ) { return value[$scope.fieldName] } ).ToArray() : Enumerable.From( array ).OrderBy( function ( value ) { return value[$scope.fieldName] } ).ToArray();
					loadpagingData(rvm.currentPage);
				};

				rvm.FieldValue = function (field, value) {

					var field_name = field.fieldName;
					var key_name = field.key;
					var cache_name = field.CacheName;
					var val = value[field_name];
					if (val && (cache_name == '' || cache_name == undefined))
						return val;

					if (rvm.InitParam.Field_Function_Callback) {
						val = key_name === '' ? value[field_name] : value[key_name];
						return rvm.InitParam.Field_Function_Callback(cache_name, 'ID', val, 'Name');
					}
					return val;
				}

				function loadpagingData(currentPage) {
					$scope.selectsDef = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true; }).ToArray();
					$scope.fieldData = Enumerable.From($scope.definefield).ToArray();
					var array = rvm.dataSource.Skip($scope.pageSize * (currentPage - 1)).Take($scope.pageSize).ToArray();
					//var data = $scope.sortAsc ? Enumerable.From( array ).OrderByDescending( function ( value ) { return value[$scope.fieldName] } ).ToArray() : Enumerable.From( array ).OrderBy( function ( value ) { return value[$scope.fieldName] } ).ToArray();
					if (rvm.callback) {
						var item_level = Enumerable.From($scope.definefield).FirstOrDefault(null, function (x) { return x.fieldName === 'Description' && x.checked === true });
						rvm.callback(array, item_level != null, function (result) {
							$scope.data = result;
							//$scope.data = Enumerable.From(result)
							//		.Where(function (w) {
							//			return (w.T_OperatorID === $scope.$parent.$parent.$parent.$parent.vm.myParam.EmpID
							//				&& w.PACID === $scope.$parent.$parent.$parent.$parent.vm.myParam.PACID)
							//		}).Distinct("$.TransID").ToArray();
							$scope.$applyAsync();
						}, function (error) { });

					}//end check call back
					else {

						$scope.data = Enumerable.From(array).Select(function (x) {
							return {
								TransID: x.TransID,
								T_0TransNB: x.T_0TransNB,
								T_PACID: x.T_PACID,
								T_StoreID: x.T_StoreID,
								T_StoreName: x.T_StoreName,
								T_CheckID: x.T_CheckID,
								T_CheckName: x.T_CheckName,
								T_OperatorID: x.T_OperatorID,
								T_OperatorName: x.T_OperatorName,
								T_RegisterID: x.T_RegisterID,
								T_RegisterName: x.T_RegisterName,
								T_TerminalID: x.T_TerminalID,
								T_TerminalName: x.T_TerminalName,
								T_6TotalAmount: x.T_6TotalAmount,
								T_8ChangeAmount: x.T_8ChangeAmount,
								DVRDate: x.DVRDate,
								TransDate: x.TransDate,
								TaxID: x.TaxID,
								TaxAmount: x.Taxes,
								PaymentName: x.PaymentName,
								PaymentID: x.PaymentID,
								PaymentAmount: x.PaymentAmount
				    		}} ).ToArray();
							}
					
				}

				rvm.RefreshList = function () {

					$scope.sortAsc = false;
					$scope.sortItem = null;
					POSCommonSvc.ClearCache();
					rvm.dataSource = $scope.sortAsc ? Enumerable.From(rvm.dataSource).OrderByDescending(function (value) { return value[$scope.fieldName] }) : Enumerable.From(rvm.dataSource).OrderBy(function (value) { return value[$scope.fieldName] });
					loadpagingData(rvm.currentPage);

					//if (rvm.InitParam) {
					//	var initPar = angular.copy(rvm.InitParam);
					//	refreshAllData(initPar.param, initPar.calbackFn, initPar.definedFilter, initPar.options);
					//}
				};

				$scope.showTransacDetail = function (tran) {
					if (!$scope.modalShown) {
						$scope.modalShown = true;
						angular.element(document.body).addClass("rebar-content-body");

						var showAboutdModal = $modal.open({
							templateUrl: 'widgets/rebar/transactiondetail.html',
							controller: 'transactiondetailCtrl',
							resolve: {
								items: function () {
									return {
										TranId: tran.TransID,
										T_PACID: tran.T_PACID,
										RegisterId: tran.RegisterID
									};
								}
							},
							size: 'lg',
							windowClass: 'transactiondetailCtrl',
							backdrop: 'static',
							keyboard: false
						});

						showAboutdModal.result.then(function (data) {
							$scope.modalShown = false;
							angular.element(document.body).removeClass("rebar-content-body");
						});
					}
				};

				$scope.sortFilter = function (item) {
					if (item.Expand) { return; }

					$scope.sortItem = item;
					if ($scope.sortItem && $scope.sortItem.id === item.id) {
						$scope.sortAsc = !$scope.sortAsc;
					}
					else {
						$scope.sortAsc = false;
					}
					$scope.fieldName = item.fieldName;
					if (rvm.currentPage > $scope.totalPages) {
						rvm.currentPage = 1;
					}
					if ( rvm.ColumnSortcallback == undefined || rvm.ColumnSortcallback == null )
					{
						getDataPaging($scope.pageSize, rvm.currentPage, null);
					}
					else {

						//rvm.ColumnSortcallback( rvm.dataSource, item, $scope.sortAsc, function ( result ) {
						//	$scope.data = result;
						//	$scope.$applyAsync();
						//} );
						var result = rvm.ColumnSortcallback(rvm.dataSource, item, $scope.sortAsc);
						rvm.dataSource = result;
						//$scope.data = result;
						loadpagingData(rvm.currentPage);
					}

				};

				function loadData(currentPage) {
					$scope.selectsDef = Enumerable.From($scope.definefield).Where(function (x) { return x.checked === true; }).ToArray();
					$scope.fieldData = Enumerable.From($scope.definefield).ToArray();
					getDataPaging($scope.pageSize, currentPage, null);
				}

				$scope.columnOption = function () {
					if (!$scope.modalShown) {
						$scope.modalShown = true;
						var showFilterModal = $modal.open({
							templateUrl: 'widgets/rebar/column-option.html',
							controller: 'columnoptionCtrl',
							resolve: {
								items: function () {
									return $scope.definefield;
								}
							},
							size: 'sm',
							backdrop: 'static',
							keyboard: false
						});

						showFilterModal.result.then(function (data) {
							if (data) {
								$scope.definefield = data;
								$rootScope.fieldColumns = Enumerable.From($scope.definefield).Where(function (w) { return w.checked === true && w.isShow === true; }).ToArray();
								if ($state.current.name !== AppDefine.State.REBAR_QUICKSEARCH) {
									//loadData(rvm.currentPage);
									loadpagingData(rvm.currentPage);
								}
								else {
									loadpagingData(rvm.currentPage);

								}
							}
							$scope.modalShown = false;
						});
					}
				};

				$scope.columnFilter = function () {
					rvm.EnableFitler = !rvm.EnableFitler;

					if (rvm.EnableFitler === false) {
						$scope.definefield = angular.copy(filterSvc.getTransQSColumn());
						loadpagingData(rvm.currentPage);
					}
				};

				$scope.selectfilters = function (item) {
					if (!$scope.modalShown) {
						$scope.modalShown = true;
						var showFilterModal = $modal.open({
							templateUrl: 'widgets/rebar/dynamic-filter.html',
							controller: 'dynamicFilterCtrl',
							resolve: {
								items: function () {
									return {
										colInfo: item,
										dataFilter: $scope.dataFilter
									};
								}
							},
							size: 'sm',
							backdrop: 'static',
							keyboard: false
						});

						showFilterModal.result.then(function (dataFilter) {
							$scope.modalShown = false;
							rvm.currentPage = 1;
							$scope.dataFilter = dataFilter;
							getDataPaging($scope.pageSize, rvm.currentPage, dataFilter);
						});
					}
				};

				$scope.nextPage = function (data) {
					//if (data) {
					//    rvm.currentPage = data;
					//}
					//else {
					//    if (!cmsBase.RebarModule.CheckCurrentPage(rvm.currentPage,$scope.totalPages)) return;
					//    rvm.currentPage = parseInt(rvm.currentPage) + 1;
					//}
					//loadData();
					rvm.currentPage = cmsBase.RebarModule.Next(data, rvm.currentPage, $scope.totalPages, loadpagingData);
				}

				$scope.gotoPage = function () {
					//if (!cmsBase.RebarModule.CheckCurrentPage(rvm.currentPage, $scope.totalPages)) return;
					//if (rvm.currentPage > 0 && rvm.currentPage <= $scope.totalPages) {
					//    loadData();
					//}
					rvm.currentPage = cmsBase.RebarModule.Goto(rvm.currentPage, $scope.totalPages, loadpagingData);
				}

				$scope.prevPage = function (data) {

					//if (rvm.currentPage <= 1) {
					//    return;
					//}
					//if (data) {
					//    rvm.currentPage = 1;
					//}
					//else {
					//    if (!cmsBase.RebarModule.CheckCurrentPage()) return;
					//    rvm.currentPage = parseInt(rvm.currentPage) - 1;
					//}
					//loadData();

					rvm.currentPage = cmsBase.RebarModule.Prev(data, rvm.currentPage, $scope.totalPages, loadpagingData)
				}

				$scope.cancel = function () {
					$modalInstance.close();
				};

				function getDataPaging(pageSize, pageNumber, dataFilter) {
					var dataSource = [];
					var data;
					switch ($state.current.name) {
						case AppDefine.State.REBAR_REFUNDS:
						case AppDefine.State.REBAR_VOIDS:
						case AppDefine.State.REBAR_CANCELS:
						case AppDefine.State.REBAR_NOSALES:
						case AppDefine.State.REBAR_DISCOUNTS:
							data = $scope.$parent.$parent.$parent.$parent.cannedDataOriginal;
							rvm.dataSource = Enumerable.From(data);
							break;
						case AppDefine.State.REBAR_QUICKSEARCH:
							data = $scope.$parent.$parent.$parent.$parent.quicksearchDataOriginal

							break;
						default:
							data = null;
							break;
					}
					//var dataSource = Enumerable.From(data)
					//			.Where(function (w) {
					//				return (w.T_OperatorID === $scope.$parent.$parent.$parent.$parent.vm.myParam.EmpID
					//					&& w.T_PACID === $scope.$parent.$parent.$parent.$parent.vm.myParam.PACID)
					//			})
					//			.Select(function (d) {
					//				return buildDataRespone(d)
					//			}).Distinct("$.TransID");
					//	if (dataFilter && dataFilter.value !== "" && dataFilter.value !== null) {
					//		dataSource = filterDataByOperator(dataSource, dataFilter);
					//	}
					//dataSource = $scope.sortAsc ? Enumerable.From(dataSource).OrderByDescending(function (value) { return value[$scope.fieldName] }) : Enumerable.From(dataSource).OrderBy(function (value) { return value[$scope.fieldName] });
					//var totalCount = dataSource.ToArray().length;
					//$scope.totalPages = parseInt(totalCount / $scope.pageSize);
					//if (totalCount % $scope.pageSize > 0) {
					//	$scope.totalPages = $scope.totalPages + 1;
					//}
					//	var array = dataSource.Skip($scope.pageSize * (pageNumber - 1)).Take($scope.pageSize).ToArray();
					//$scope.data = $scope.sortAsc ? Enumerable.From(array).OrderByDescending(function (value) { return value[$scope.fieldName] }).ToArray() : Enumerable.From(array).OrderBy(function (value) { return value[$scope.fieldName] }).ToArray();//dataSource.Skip($scope.pageSize * (rvm.currentPage - 1)).Take($scope.pageSize).ToArray();

					//var dataOriginal = [];
					//var extraParams = [];
					//$scope.selectsDef.forEach(function (x) {
					//	if (!x.checked) { return; }

					//	if ($state.current.name === AppDefine.State.REBAR_REFUNDS
					//	|| $state.current.name === AppDefine.State.REBAR_VOIDS
					//	|| $state.current.name === AppDefine.State.REBAR_CANCELS
					//	|| $state.current.name === AppDefine.State.REBAR_NOSALES
					//	|| $state.current.name === AppDefine.State.REBAR_DISCOUNTS) {
					//		dataOriginal = Enumerable.From($scope.data); //Enumerable.From($scope.$parent.$parent.$parent.$parent.cannedDataOriginal);
					//	}
					//	else if ($state.current.name === AppDefine.State.REBAR_QUICKSEARCH) {
					//		dataOriginal = Enumerable.From($scope.data); //Enumerable.From($scope.$parent.$parent.$parent.$parent.quicksearchDataOriginal);
					//	}

					//	var listIDs = [];
					//	switch (x.fieldName) {
					//		case "TypeName":
					//			listIDs = dataOriginal.Where(function (w) { return w.TypeID !== null; }).Select(function (s) { return s.TypeID; }).Distinct().ToArray();
					//			if (listIDs.length > 0 && listIDs.toString() !== "") {
					//				extraParams.push({ Name: x.fieldName, Keys: listIDs.toString(), PrimaryField: x.key });
					//			}
					//			break;
					//		case "DescriptionName":
					//			listIDs = dataOriginal.Where(function (w) { return w.DescriptionID !== null; }).Select(function (s) { return s.DescriptionID; }).Distinct().ToArray();
					//			if (listIDs.length > 0 && listIDs.toString() !== "") {
					//				extraParams.push({ Name: x.fieldName, Keys: listIDs.toString(), PrimaryField: x.key });
					//			}
					//        break;
					//		case "TerminalName":
					//			listIDs = dataOriginal.Where(function (w) { return w.TerminalID !== null; }).Select(function (s) { return s.TerminalID; }).Distinct().ToArray();
					//			if (listIDs.length > 0 && listIDs.toString() !== "") {
					//				extraParams.push({ Name: x.fieldName, Keys: listIDs.toString(), PrimaryField: x.key });
					//			}
					//        break;
					//		case "StoreName":
					//			listIDs = dataOriginal.Where(function (w) { return w.StoreID !== null; }).Select(function (s) { return s.StoreID; }).Distinct().ToArray();
					//			if (listIDs.length > 0 && listIDs.toString() !== "") {
					//				extraParams.push({ Name: x.fieldName, Keys: listIDs.toString(), PrimaryField: x.key });
					//			}
					//        break;
					//		case "CheckName":
					//			listIDs = dataOriginal.Where(function (w) { return w.CheckID !== null; }).Select(function (s) { return s.CheckID; }).Distinct().ToArray();
					//			if (listIDs.length > 0 && listIDs.toString() !== "") {
					//				extraParams.push({ Name: x.fieldName, Keys: listIDs.toString(), PrimaryField: x.key });
					//}
					//			break;
					//	}
					//});

					//rebarDataSvc.getColumnOpion(extraParams, function (response) {
					//	if (response.length > 0) {
					//	response.forEach(function (x) {
					//		if (x.Data) {
					//			var extraData = Enumerable.From(x.Data);
					//			var dataJoin = getLeftOuterJoin(dataOriginal, extraData, x.PrimaryField, "ID", x.Name).ToArray();

					//			if (($state.current.name === AppDefine.State.REBAR_REFUNDS
					//			|| $state.current.name === AppDefine.State.REBAR_VOIDS
					//			|| $state.current.name === AppDefine.State.REBAR_CANCELS
					//			|| $state.current.name === AppDefine.State.REBAR_NOSALES
					//			|| $state.current.name === AppDefine.State.REBAR_DISCOUNTS) && dataJoin.length > 0) {
					//					$scope.data = dataJoin; //$scope.$parent.$parent.$parent.$parent.cannedDataOriginal = dataJoin;
					//					dataOriginal = Enumerable.From($scope.data); //Enumerable.From($scope.$parent.$parent.$parent.$parent.cannedDataOriginal);
					//			}
					//			else if ($state.current.name === AppDefine.State.REBAR_QUICKSEARCH && dataJoin.length > 0) {
					//					$scope.data = dataJoin; //$scope.$parent.$parent.$parent.$parent.quicksearchDataOriginal = dataJoin;
					//					dataOriginal = Enumerable.From($scope.data); //dataOriginal = Enumerable.From($scope.$parent.$parent.$parent.$parent.quicksearchDataOriginal);
					//			}
					//		}
					//	});
					//	}
					//}, function (error) { });
				}

				function buildDataRespone(data) {
					var dataModel = {};
					$scope.fieldData.forEach(function (x) {
						dataModel[x.fieldName] = data[x.fieldName] === null ? 0 : data[x.fieldName];
					});
					return dataModel;
				}

				function refreshAllData(param, calbackFn, definedFilter, options) {
					if (options) {
						rvm.optionsShow = options;
					}

					if (definedFilter) {
						$scope.definefield = definedFilter;
					}
					else {
						$scope.definefield = angular.copy(filterSvc.getTransQSColumn());
					}

					if (calbackFn) {
						$scope.calbackFn = calbackFn;
					}

					if (param && param.ShowBackButton !== undefined) {
						rvm.ShowBackButton = param.ShowBackButton;
					}

					if (param) {
						rvm.param = param; //.push(sites.SiteKey);
						if (rvm.param.StartDate) {

							var filter = {
								SelectAnd: true,
								firstvalue: rvm.param.StartDate,
								endvalue: rvm.param.EndDate,
								selectfirst: { id: 4, key: 'ge', name: "Greater than or equal" },
								selectend: { id: 6, key: 'le', name: "Less than or equal" }
							}

							var trandate = Enumerable.From($scope.definefield).Where(function (x) { return x.fieldName === 'TranDate'; }).FirstOrDefault();
							trandate.filter = filter;
						}
					}

					//loadpagingData(rvm.currentPage);
					loadData(rvm.currentPage);
				}

				function filterDataByOperator(data, filter) {
					if (filter.value === null || filter.value === "") { return; }

					var tempData = [];
					if (filter.type === "string") {
						var listIds = filter.value.split(',');
						tempData = Enumerable.From(data)
								.Where(function (w) { return filter.value.indexOf(w[filter.foreignField]) !== -1; });
						return tempData;
					}


					switch (filter.operators.key) {
						case "eq":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] === filter.value });
							break;
						case "ne":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] !== filter.value });
							break;
						case "gt":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] > filter.value });
							break;
						case "ge":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] >= filter.value });
							break;
						case "lt":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] < filter.value });
							break;
						case "le":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name] <= filter.value });
							break;
						case "startswith":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name].toLowerCase().startsWith(filter.value.toLowerCase()) });
							break;
						case "endswith":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name].toLowerCase().endsWith(filter.value.toLowerCase()) });
							break;
						case "contains":
							tempData = Enumerable.From(data)
								.Where(function (w) { return w[filter.name].toLowerCase().indexOf(filter.value.toLowerCase()) !== -1 });
							break;
					}
					return tempData;
				}

				function getLeftOuterJoin(data1, data2, innerKey, outerKey, fieldName) {
					if (!data1 || !data2) { return; }

					var dataInnerJoin = data1.Join(data2, "$." + innerKey, "$." + outerKey, function (t, et) {
						if (fieldName === "TypeName") {
							return {
								T_PACID: t.T_PACID,
								TransID: t.TransID,
								TypeID: t.TypeID,
								TypeName: et.Name,
								EmpID: t.EmpID,
								EmpName: t.EmpName,
								RegisterID: t.RegisterID,
								RegisterName: t.RegisterName,
								PaymentID: t.PaymentID,
								PaymentName: t.PaymentName,
								TransNB: t.TransNB,
								TotalAmount: t.TotalAmount,
								DVRDate: t.DVRDate,
								T_CameraNB: t.T_CameraNB,
								TransDate: t.TransDate,
								TAX: t.TAX,
								ChangeAmount: t.ChangeAmount,
								TerminalID: t.TerminalID,
								TerminalName: t.TerminalName,
								StoreID: t.StoreID,
								StoreName: t.StoreName,
								CheckID: t.CheckID,
								CheckName: t.CheckName,
								ExtraString: t.ExtraString,
								ExtraNumber: t.ExtraNumber,
								Tracking: t.Tracking
								//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
								//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
							};
						}
						else if (fieldName === "TerminalName") {
							return {
								PACID: t.PACID,
								TransID: t.TransID,
								TypeID: t.TypeID,
								TypeName: t.TypeName,
								EmpID: t.EmpID,
								EmpName: t.EmpName,
								RegisterID: t.RegisterID,
								RegisterName: t.RegisterName,
								PaymentID: t.PaymentID,
								PaymentName: t.PaymentName,
								TransNB: t.TransNB,
								TotalAmount: t.TotalAmount,
								DVRDate: t.DVRDate,
								T_CameraNB: t.T_CameraNB,
								TransDate: t.TransDate,
								TAX: t.TAX,
								ChangeAmount: t.ChangeAmount,
								TerminalID: t.TerminalID,
								TerminalName: et.Name,
								StoreID: t.StoreID,
								StoreName: t.StoreName,
								CheckID: t.CheckID,
								CheckName: t.CheckName,
								ExtraString: t.ExtraString,
								ExtraNumber: t.ExtraNumber,
								Tracking: t.Tracking
								//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
								//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
							};
						}
						else if (fieldName === "StoreName") {
							return {
								PACID: t.PACID,
								TransID: t.TransID,
								TypeID: t.TypeID,
								TypeName: t.TypeName,
								EmpID: t.EmpID,
								EmpName: t.EmpName,
								RegisterID: t.RegisterID,
								RegisterName: t.RegisterName,
								PaymentID: t.PaymentID,
								PaymentName: t.PaymentName,
								TransNB: t.TransNB,
								TotalAmount: t.TotalAmount,
								DVRDate: t.DVRDate,
								T_CameraNB: t.T_CameraNB,
								TransDate: t.TransDate,
								TAX: t.TAX,
								ChangeAmount: t.ChangeAmount,
								TerminalID: t.TerminalID,
								TerminalName: t.TerminalName,
								StoreID: t.StoreID,
								StoreName: et.Name,
								CheckID: t.CheckID,
								CheckName: t.CheckName,
								ExtraString: t.ExtraString,
								ExtraNumber: t.ExtraNumber,
								Tracking: t.Tracking
								//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
								//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
							};
						}
						else if (fieldName === "CheckName") {
							return {
								PACID: t.PACID,
								TransID: t.TransID,
								TypeID: t.TypeID,
								TypeName: t.TypeName,
								EmpID: t.EmpID,
								EmpName: t.EmpName,
								RegisterID: t.RegisterID,
								RegisterName: t.RegisterName,
								PaymentID: t.PaymentID,
								PaymentName: t.PaymentName,
								TransNB: t.TransNB,
								TotalAmount: t.TotalAmount,
								DVRDate: t.DVRDate,
								T_CameraNB: t.T_CameraNB,
								TransDate: t.TransDate,
								TAX: t.TAX,
								ChangeAmount: t.ChangeAmount,
								TerminalID: t.TerminalID,
								TerminalName: t.TerminalName,
								StoreID: t.StoreID,
								StoreName: t.StoreName,
								CheckID: t.CheckID,
								CheckName: et.Name,
								ExtraString: t.ExtraString,
								ExtraNumber: t.ExtraNumber,
								Tracking: t.Tracking
								//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
								//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
							};
						}
						else {
							return {
								PACID: t.PACID,
								TransID: t.TransID,
								TypeID: t.TypeID,
								TypeName: t.TypeName,
								EmpID: t.EmpID,
								EmpName: t.EmpName,
								RegisterID: t.RegisterID,
								RegisterName: t.RegisterName,
								PaymentID: t.PaymentID,
								PaymentName: t.PaymentName,
								TransNB: t.TransNB,
								TotalAmount: t.TotalAmount,
								DVRDate: t.DVRDate,
								T_CameraNB: t.T_CameraNB,
								TransDate: t.TransDate,
								TAX: t.TAX,
								ChangeAmount: t.ChangeAmount,
								TerminalID: t.TerminalID,
								TerminalName: t.TerminalName,
								StoreID: t.StoreID,
								StoreName: t.StoreName,
								CheckID: t.CheckID,
								CheckName: t.CheckName,
								ExtraString: t.ExtraString,
								ExtraNumber: t.ExtraNumber,
								Tracking: t.Tracking
								//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
								//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
							};
						}
					});
					var transIds = dataInnerJoin.Select(function (s) { return s.TransID; }).ToArray();
					var dataExcept = Enumerable.From(data1).Where(function (w) { return transIds.indexOf(w.TransID) === -1; })
					.Select(function (t) {
						return {
							PACID: t.PACID,
							TransID: t.TransID,
							TypeID: t.TypeID,
							TypeName: !t.TypeName ? "None" : t.TypeName,
							EmpID: t.EmpID,
							EmpName: t.EmpName,
							RegisterID: t.RegisterID,
							RegisterName: t.RegisterName,
							PaymentID: t.PaymentID,
							PaymentName: t.PaymentName,
							TransNB: t.TransNB,
							TotalAmount: t.TotalAmount,
							DVRDate: t.DVRDate,
							T_CameraNB: t.T_CameraNB,
							TransDate: t.TransDate,
							TAX: t.TAX,
							ChangeAmount: t.ChangeAmount,
							TerminalID: t.TerminalID,
							TerminalName: !t.TerminalName ? "None" : t.TerminalName,
							StoreID: t.StoreID,
							StoreName: !t.StoreName ? "None" : t.StoreName,
							CheckID: t.CheckID,
							CheckName: !t.CheckName ? "None" : t.CheckName,
							ExtraString: t.ExtraString,
							ExtraNumber: t.ExtraNumber,
							Tracking: t.Tracking
							//DescriptionID: t.hasOwnProperty("DescriptionID") ? t.DescriptionID : null,
							//DescriptionName: t.hasOwnProperty("DescriptionName") ? t.DescriptionName : null,
						};
					});
					var dataFinal = Enumerable.From(dataInnerJoin).Union(dataExcept).OrderBy(function (o) { return o.TransID; });
					return dataFinal;
				}

			}
		});
})();