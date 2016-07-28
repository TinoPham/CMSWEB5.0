(function () {
	define(['cms',
		'DataServices/Rebar/qsearch.service',
		'Scripts/Services/bamhelperSvc',
		'Scripts/Services/chartSvc',
		'Widgets/Rebar/transactiondetail',
		'widgets/rebar/transaction-list',
		'DataServices/Rebar/rebar.service',
	], function (cms) {
		cms.register.controller('quicksearchCtrl', quicksearchCtrl);
		quicksearchCtrl.$inject = ['$controller', '$scope', '$rootScope', '$timeout', '$filter', '$modal', 'cmsBase', 'AppDefine', 'qSearchDataSvc', 'bamhelperSvc', 'chartSvc', 'Utils', 'filterSvc', 'rebarDataSvc', 'POSCommonSvc', 'AccountSvc', 'exportSvc'];

		function quicksearchCtrl($controller, $scope, $rootScope, $timeout, $filter, $modal, cmsBase, AppDefine, qSearchDataSvc, bamhelperSvc, chartSvc, Utils, filterSvc, rebarDataSvc, POSCommonSvc, AccountSvc, exportSvc) {
			var vm = this;

			$scope.GroupByFieldConst = { Site: 1, Employee: 2 };
			$scope.groupByField = true; //true is group by site name, else value is employee
			$scope.quicksearchData = Enumerable.Empty(); //[];
			$scope.quicksearchDataOriginal = Enumerable.Empty();//[];
			$scope.quicksearchparam;
			$scope.filterTypes = {
				Payment: 1,
				Register: 2,
				Employee: 3,
				Description: 4
			};
			$scope.dataTypes = {
				QuickSearch: 1,
				QuickSearchChild: 2,
				QuickSearchDetail: 3
			};
			$scope.sitenodeSelected = [];
			var dateFrom, dateTo, searchDate;

			vm.rebartransactviewerProperty = {
				Max: false,
				Collapse: false
			}

			/*Progress bar, begin*/
			$scope.max = 100;
			$scope.dataProgressValue = 0;
			$scope.isStopped = false;
			$scope.progressing = false;
			$scope.showProgressBar = false;
			/*Progress bar, end*/

			/*Tree site Properties, begin*/
			vm.treeDef = {
				Id: 'ID',
				Name: 'Name',
				Type: 'Type',
				Checked: 'Checked',
				Childs: 'Sites',
				Count: 'SiteCount',
				Model: {}
			};
			vm.treeOptions = {
				Node: {
					IsShowIcon: true,
					IsShowCheckBox: true,
					IsShowNodeMenu: false,
					IsShowAddNodeButton: false,
					IsShowAddItemButton: false,
					IsShowEditButton: false,
					IsShowDelButton: false,
					IsDraggable: false
				},
				Item: {
					IsShowItemMenu: false
				}, Type: {
					Folder: 0,
					Group: 2,
					File: 1
				},
				CallBack: {
					SelectedFn: selectedFn
				}
			};
			vm.querySite = '';
			vm.treeSiteFilterS = null;
			vm.siteloaded = false;
			vm.selectedSites = [];
			/*Tree site Properties, end*/
			$rootScope.fieldColumns = [];
			$scope.qsChildIndexOpened = [];

			$scope.$on(AppDefine.Events.REBARSEARCH, function (e, arg) {
				var retCompDate = Utils.compareDate($rootScope.rebarSearch.DateFrom, $rootScope.rebarSearch.DateTo);
				if (retCompDate === 1) { //if DateFrom > DateTo
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					return;
				}

				vm.Showdetail = null;
				vm.parentshowdetail = null;

				$scope.quicksearchDataOriginal = [];
				$scope.progressing = true;
				$scope.isStopped = false;
				$scope.dataProgressValue = 0;
				$scope.showProgressBar = true;
				$scope.totalTransCount = 0;

				$timeout(function () {
					$scope.getQuickSearchData();
				}, 100);
			});

			$scope.$on(AppDefine.Events.PAGEREADY, function () {
				active();
			});

			$scope.$on("$destroy", function () {
				$scope.isStopped = true;
			});

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
				var userLogin = AccountSvc.UserModel();

				var reportInfo = {
					TemplateName: cmsBase.translateSvc.getTranslate(AppDefine.Resx.MODULE_REBAR) + "_" + cmsBase.translateSvc.getTranslate($rootScope.title).replace(/[\s]/g, ''),
					ReportName: cmsBase.translateSvc.getTranslate($rootScope.title),
					CompanyID: userLogin.CompanyID,
					RegionName: '',
					Location: '',
					WeekIndex: $rootScope.GWeek,
					Footer: '',
					CreatedBy: userLogin.FName + ' ' + userLogin.LName,
					CreateDate: $filter('date')(new Date(), AppDefine.ParamDateFormat) // MM/dd/yyyy
				};

				var tables = [];
				var options = options = { ColIndex: 1, RowIndex: 1 };
				var tableQuickSearch = buildTableQuickSearch($scope.quicksearchData, options);
				tables.push(tableQuickSearch);

				var charts = [];
				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			$scope.$watch("groupByField", function (newVal, oldVal) {
				if (newVal !== oldVal) {
					var filterData = angular.copy($rootScope.rebarSearch);
					var item_level = undefined;
					if (filterData.DescIDs.length > 0 && filterData.DescIDs_AND == 1)
						item_level = filterData.DescIDs;

					var dataDistinct;
					var groupFieldName;
					if (newVal === false) {
						groupFieldName = "T_OperatorID";
						AppendQuickSearchData(Enumerable.Empty(), groupFieldName, item_level);
					}
					else {
						groupFieldName = "SiteKey";
						AppendQuickSearchData(Enumerable.Empty(), groupFieldName, item_level);
					}
				}
			});

			//active();
			vm.Init = function () {
				//var arr_args = [AppDefine.POSItemKeys.Cameras, AppDefine.POSItemKeys.Descriptions];
				//$scope.$parent.InitCache( false, [AppDefine.POSItemKeys.Payments, AppDefine.POSItemKeys.Registers, AppDefine.POSItemKeys.Operators] ).then( function () {

				//} );
				active();
			}

			function active() {
				getAllRegionSites();
				//if ($scope.$parent.$parent.pageReady) {
				//	getQuickSearchData();
				//}
			}

			function getQuickSearchData() {
				var params = angular.copy($rootScope.rebarSearch);
				if (!params || vm.selectedSites.length === 0) { return; }

				$scope.quicksearchparam = params;
				params.SiteKeys = vm.selectedSites;
				params.GroupByField = $scope.groupByField ? $scope.GroupByFieldConst.Site : $scope.GroupByFieldConst.Employee;

				$scope.quicksearchData = [];
				var retCompDate = compareDate(params.DateFrom, params.DateTo);
				if (retCompDate === 1) { //if DateFrom > DateTo
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					return;
				}

				var groupFieldName;
				if (params.GroupByField === $scope.GroupByFieldConst.Employee) {
					groupFieldName = "$.EmpID";
				}
				else {
					groupFieldName = "$.SiteKey";
				}

				var paramOptional = {
					groupFieldName: groupFieldName
				};

				loopGetData(params, $scope.dataTypes.QuickSearch, paramOptional);
			}

			function DescriptionIncluded() {
				var filterData = $scope.quicksearchparam;

				var item_level = undefined;
				if (filterData == null || filterData == undefined)
					return item_level;

				if (filterData.DescIDs.length > 0 && filterData.DescIDs_AND == 1)
					item_level = filterData.DescIDs;
				return item_level;
			}

			$scope.getQuickSearchData = function () {
				getQuickSearchData();
			};

			$scope.getQuickSearchChildData = function (event, quicksearchItem, index) {
				$scope.qsChildIndexOpened.push(index);
				if ($scope.quicksearchData[index].Childs.length > 0 || $scope.progressing) { return; }

				var item_level = DescriptionIncluded();//undefined;
				//if ( filterData.DescIDs.length > 0 && filterData.DescIDs_AND == 1 )
				//	item_level = filterData.DescIDs;

				if ($scope.groupByField === true)
					QuickSearchGroupChild(quicksearchItem, 'T_OperatorID', item_level);
				else
					QuickSearchGroupChild(quicksearchItem, 'SiteKey', item_level);
			};

			$scope.getQuickSearchDetail = function (event, quicksearchItem, parentIndex, index) {
				vm.Showdetail = index;
				vm.parentshowdetail = parentIndex;
				var params = quicksearchItem.TransIDs;

				loadTrandata(params);
			};

			$scope.stopGetData = function () {
				$scope.dataProgressValue = 0;
				$scope.isStopped = true;
				$scope.progressing = false;
				$scope.showProgressBar = false;
			};

			function loadTrandata(data) {
				vm.param = {
					isPOSTransac: true,
					//SiteKeys: data.SiteKeys
					Trans: data
				};

				vm.definefield = angular.copy(filterSvc.getTransQSColumn());
				var item_level = DescriptionIncluded();
				if (item_level)//add item description column
				{
					var description = Enumerable.From(vm.definefield).FirstOrDefault(null, function (x) { return x.fieldName === 'Description'; })
					description.checked = true;
				}
				$rootScope.fieldColumns = Enumerable.From(vm.definefield).Where(function (w) { return w.checked === true && w.isShow === true; }).ToArray();
			}

			$scope.showTransacDetail = function (tran) {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					angular.element(document.body).addClass("rebar-content-body");

					var showAboutdModal = $modal.open({
						templateUrl: 'Widgets/Rebar/transactiondetail.html',
						controller: 'transactiondetailCtrl',
						resolve: {
							items: function () {
								return {
									TranId: tran.TransID
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

			function loopGetData(params, dataType, paramOptional) {
				dateFrom = angular.copy(params.DateFrom);
				dateTo = angular.copy(params.DateTo);
				searchDate = angular.copy(params.DateFrom);

				var filterData = angular.copy(params);
				if (compareDateWithoutTime(dateFrom, dateTo) === -1) {
					filterData.DateFrom = new Date(searchDate);
					filterData.DateTo = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate(), 23, 59, 59);
				}

				$scope.showProgressBar = true;
				$scope.max = (dateTo - dateFrom) / 86400000;

				//console.log(filterData);
				switch (dataType) {
					case $scope.dataTypes.QuickSearch:
						getQuickSearch(filterData, paramOptional);
						break;
					case $scope.dataTypes.QuickSearchChild:
						getQuickSearchChild(filterData, paramOptional);
						break;
						//case $scope.dataTypes.QuickSearchDetail:
						//	getQuickSearchDetail(filterData, paramOptional);
						//	break;
					default:
						console.log('default');
						break;
				}
			}

			vm.EmpName = function (item) {
				var opList = POSCommonSvc.GetCache(AppDefine.POSItemKeys.Operators, true);
				var found = $scope.$parent.LookupItem(opList, 'ID', item.T_OperatorID, 'Name');
				if (found.Any())
					return found.First();
				return '';
			}

			vm.SiteName = function (item) {
				var sitepacInfo = $scope.$parent.$parent.sitepacInfo;
				var found = $scope.$parent.LookupItem(sitepacInfo, 'PacId', item.PACID, 'SiteName');
				if (found.Any())
					return found.First();
				return '';
			}

			vm.SortCallback = function (trans, column, Asc, call_back) {
				if (trans == null || trans.Any() == false || column == null)
					return;
				if (column.key == "") {
					if (Asc)
						return trans.OrderBy(function (x) { return x[column.fieldName] }).Select(function (x) { return x; });
					else
						return trans.OrderByDescending(function (x) { return x[column.fieldName] }).Select(function (x) { return x; });
				}
				else {
					var caches = POSCommonSvc.GetCache(column.CacheName, true);
					var LookupItem = $scope.$parent.LookupItem;
					if (column.Property == undefined || column.Property == '') {
						if (Asc)
							return trans.OrderBy(function (x) { return LookupItem(caches, 'ID', x[column.Key], 'Name'); }).Select(function (x) { return x; });
						else
							return trans.OrderByDescending(function (x) { return LookupItem(caches, 'ID', x[column.Key], 'Name'); }).Select(function (x) { return x; });
					}
					else {

						return trans;

						//var flatten = trans.SelectMany( function ( x ) { return x[column.Property]; } );
						//flatten.Do( function ( x ) { } );
					}
				}
			}

			vm.Transactiondetail = function (trans, item_detail, success_callback, error_callback) {
				var data = _Transactiondetail(trans, item_detail, success_callback, error_callback);
				success_callback(data);
			}

			function _Transactiondetail(trans, detail_level) {

				var IEtrans = Enumerable.From(trans);
				//get transaction is missing tax collection
				var missing_tax = IEtrans.Where(function (it) { return it.Taxes == null || it.Taxes.length == 0; }).Select(function (it) {
					return {
						Tran: it,
						Tax: null
					};
				});

				var _flattentax = IEtrans.SelectMany(function (tr) { return tr.Taxes; }, function (tran, tax) {
					return {
						Tran: tran,
						Tax: tax
					};
				});

				_flattentax = _flattentax.Any() == false ? missing_tax : _flattentax.Concat(missing_tax);
				//get transaction is missing payment collection

				var _flattenpays = _flattentax.SelectMany(function (tt) { return tt.Tran.Payments; }, function (tran, pay) {
					return {
						Tran: tran.Tran,
						Tax: tran.Tax,
						Pay: pay
					};
				});

				var missing_pays = _flattentax.Where(function (tt) { return tt.Tran.Payments == null || tt.Tran.Payments.length == 0; }).Select(function (it) {
					return {
						Tran: it.Tran,
						Tax: it.Tax,
						Pay: null
					};
				});

				_flattenpays = _flattenpays.Any() == false ? missing_pays : _flattenpays.Concat(missing_pays);
				var tran_flatten = _flattenpays.Select(function (x) {
					return {
						Tran: x.Tran,
						Tax: x.Tax,
						Pay: x.Pay
					}
				});

				var retsult = [];
				var tran_flatten_result;
				if (!detail_level) {
					tran_flatten_result = tran_flatten;
				}
				else {
					var missing_retail = tran_flatten.Where(function (x) { return x.Tran.Description == null || x.Tran.Description.length == 0 }).Select(function (it) { return { Tran: it.Tran, Tax: it.Tax, Pay: it.Pay, Description: null } });
					var itemFlatten = tran_flatten.SelectMany(function (x) { return x.Tran.Description }, function (tran, item) { return { Tran: tran.Tran, Tax: tran.Tax, Pay: tran.Pay, Description: item }; });
					tran_flatten_result = itemFlatten.Any() == false ? missing_retail : itemFlatten.Concat(missing_retail);
				}

				tran_flatten_result.ForEach(function (x) {
					var item = {
						TransID: x.Tran.TransID,
						PACID: x.Tran.PACID,
						T_OperatorID: x.Tran.T_OperatorID,

						T_6TotalAmount: x.Tran.T_6TotalAmount,
						DVRDate: x.Tran.DVRDate,

						PaymentAmount: x.Pay == null ? 0 : x.Pay.PaymentAmount,
						T_00TransNBText: x.Tran.T_00TransNBText,
						T_0TransNB: x.Tran.T_0TransNB,
						T_1SubTotal: x.Tran.T_1SubTotal,
						T_8ChangeAmount: x.Tran.T_8ChangeAmount,
						T_9RecItemCount: x.Tran.T_9RecItemCount,
						T_CameraNB: x.Tran.T_CameraNB,

						TaxAmount: x.Tax == null ? 0 : x.Tax.TaxAmount,
						TransDate: x.Tran.TransDate
						//Description: x.Tran.Description
					};

					//item.T_OperatorID = $scope.$parent.LookupCacheItem( AppDefine.POSItemKeys.Operators, 'ID', x.Tran.T_OperatorID, 'Name' ); //x.Tran.T_OperatorID,
					item.T_OperatorName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Operators, 'ID', x.Tran.T_OperatorID, 'Name'); //x.Tran.T_OperatorID,

					item.PaymentID = x.Pay == null ? null : x.Pay.PaymentID; //x.Tran.Pay.PaymentID,
					item.PaymentName = x.Pay == null ? null : $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Payments, 'ID', x.Pay.PaymentID, 'Name'); //x.Tran.Pay.PaymentID,
					item.T_CardID = x.Tran.T_CardID; //x.Tran.T_CardID,
					item.T_CardName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.CardIDs, 'ID', x.Tran.T_CardID, 'Name'); //x.Tran.T_CardID,

					item.T_CheckID = x.Tran.T_CheckID; //x.Tran.T_CheckID,
					item.T_CheckName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.CheckIDs, 'ID', x.Tran.T_CheckID, 'Name'); //x.Tran.T_CheckID,

					item.T_RegisterID = x.Tran.T_RegisterID; //x.Tran.T_RegisterID,
					item.T_RegisterName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Registers, 'ID', x.Tran.T_RegisterID, 'Name'); //x.Tran.T_RegisterID,

					item.T_ShiftID = x.Tran.T_ShiftID//x.Tran.T_ShiftID,
					item.T_ShiftName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Shifts, 'ID', x.Tran.T_ShiftID, 'Name');//x.Tran.T_ShiftID,

					item.T_StoreID = x.Tran.T_StoreID;//x.Tran.T_StoreID,
					item.StoreName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Stores, 'ID', x.Tran.T_StoreID, 'Name');//x.Tran.T_StoreID,

					item.T_TerminalID = x.Tran.T_TerminalID;//x.Tran.T_TerminalID,
					item.T_TerminalName = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Terminals, 'ID', x.Tran.T_TerminalID, 'Name');//x.Tran.T_TerminalID,

					item.TaxID = x.Tax == null ? null : $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Taxes, 'ID', x.Tax.TaxID, 'Name');//x.Tran.Tax.TaxID,
					item.TaxName = x.Tax == null ? null : $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Taxes, 'ID', x.Tax.TaxID, 'Name');//x.Tran.Tax.TaxID,

					if (x.Description) {

						item.DescriptionID = x.Description.Description;//x.Tran.Tax.TaxID,
						item.Description = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Descriptions, 'ID', x.Description.Description, 'Name');//x.Tran.Tax.TaxID,
					}
					retsult.push(item);
				});
				return retsult;
				//qSearchDataSvc.quicksearchDetail( trans.toString(), success_callback, error_callback );
			}

			function QuickSearchGroupChild(quicksearchItem, group_field, ItemRetails) {
				var trans = Enumerable.From(quicksearchItem.TransIDs);
				var raw = Enumerable.From($scope.quicksearchDataOriginal);

				var match = raw.Join(trans, "$.TransID", "$.TransID", function (_raw, _trans) { return _raw; });
				var group;
				if (ItemRetails != undefined) {
					var items = Enumerable.From(ItemRetails)
					group = Enumerable.From($scope.quicksearchDataOriginal).GroupBy(function (it) { return it[group_field]; }, null, function (key, g) {
						return {
							T_OperatorID: g.FirstOrDefault().T_OperatorID,
							TotalTrans: g.Sum(function (x) {
								var counttmp = items.Join(x.Description, '$', '$.Description', function (l, r) { return l; }).Count();
								return counttmp;
							}),
							TransIDs: g.Select(function (it) { return it; }).ToArray(),
							PACID: g.First().PACID,
							SiteKey: g.First().SiteKey,
							SiteName: g.First().SiteName,
							TotalAmount: g.Sum(function (x) {
								if (x.Description == null || x.Description.length == 0)
									return 0;

								return Enumerable.From(x.Description).Sum(function (r) { return r.R_0Amount; });
							})
						}
					});

				}
				else {
					group = match.GroupBy(function (x) { return x[group_field]; }, null, function (key, g) {
						return {
							T_OperatorID: g.FirstOrDefault().T_OperatorID,
							TotalTrans: g.Count('$.TransID'),
							TransIDs: g.Select(function (it) { return it; }).ToArray(),
							PACID: g.First().PACID,
							SiteKey: g.First().SiteKey,
							SiteName: g.First().SiteName,
							TotalAmount: g.Sum("$.T_6TotalAmount"),
						}
					});
				}
				quicksearchItem.Childs = group.ToArray();
			}

			function AppendQuickSearchData(response, group_field, ItemRetails) {
				if (response == null)
					return;
				$scope.quicksearchDataOriginal = Enumerable.From($scope.quicksearchDataOriginal).Concat(response);
				var group;
				if (ItemRetails != undefined) {
					var items = Enumerable.From(ItemRetails)
					group = Enumerable.From($scope.quicksearchDataOriginal).GroupBy(function (it) { return it[group_field]; }, null, function (key, g) {
						return {
							T_OperatorID: g.FirstOrDefault().T_OperatorID,
							TotalTrans: g.Sum(function (x) {
								return items.Join(x.Description, '$', '$.Description', function (l, r) { return l; }).Count();
							}),
							TransIDs: g.Select(function (it) { return it; }).ToArray(),
							PACID: g.First().PACID,
							SiteKey: g.First().SiteKey,
							SiteName: g.First().SiteName,
							TotalAmount: g.Sum(
								function (x) {
									if (x.Description == null || x.Description.length == 0)
										return 0;

									return Enumerable.From(x.Description).Sum(function (r) { return r.R_0Amount; });
								}),
							Childs: []
						}
					});

				}
				else {
					group = Enumerable.From($scope.quicksearchDataOriginal).GroupBy(function (it) { return it[group_field]; }, null, function (key, g) {
						return {
							T_OperatorID: g.FirstOrDefault().T_OperatorID,
							TotalTrans: g.Count('$.TransID'),
							TransIDs: g.Select(function (it) { return it; }).ToArray(),
							PACID: g.First().PACID,
							SiteKey: g.First().SiteKey,
							SiteName: g.First().SiteName,
							TotalAmount: g.Sum("$.T_6TotalAmount"),
							Childs: []
						}
					});
				}

				$scope.quicksearchData = group.ToArray();

				//console.log($scope.quicksearchData);
			}

			function getQuickSearch(filterData, paramOptional) {
				if ($scope.isStopped) { return; }

				filterData = formatDateTimeParam(filterData);
				//console.log(filterData);

				var item_level = undefined;
				if (filterData.DescIDs.length > 0 && filterData.DescIDs_AND == 1)
					item_level = filterData.DescIDs;

				qSearchDataSvc.QuickSearchReport(filterData,
					function (response) {
						var value = (searchDate - dateFrom) / 86400000;
						var percent = Math.floor((value / $scope.max) * 100);
						$scope.dataProgressValue = percent;
						var sitepacInfo = Enumerable.From($scope.$parent.$parent.sitepacInfo);
						var siteresponse = Enumerable.From(response).Join(sitepacInfo, "$.PACID", "$.PacId", function (d, s) {
							return {
								PACID: d.PACID,
								TransID: d.TransID,
								T_0TransNB: d.T_0TransNB,
								T_6TotalAmount: d.T_6TotalAmount,
								T_1SubTotal: d.T_1SubTotal,
								T_8ChangeAmount: d.T_8ChangeAmount,
								TransDate: d.TransDate,
								DVRDate: d.DVRDate,
								T_9RecItemCount: d.T_9RecItemCount,
								T_CameraNB: d.T_CameraNB,
								T_OperatorID: d.T_OperatorID,
								T_StoreID: d.T_StoreID,
								T_TerminalID: d.T_TerminalID,
								T_RegisterID: d.T_RegisterID,
								T_ShiftID: d.T_ShiftID,
								T_CheckID: d.T_CheckID,
								T_CardID: d.T_CardID,
								T_00TransNBText: d.T_00TransNBText,
								Description: d.Description,
								Taxes: d.Taxes,
								Payments: d.Payments,
								SiteKey: s.SiteKey,
								SiteName: s.SiteName
							}
						});
						if ($scope.groupByField === true) {
							AppendQuickSearchData(siteresponse, 'SiteKey', item_level);
						}
						else {
							AppendQuickSearchData(siteresponse, 'T_OperatorID', item_level);
						}

						//loop get data
						searchDate.setDate(searchDate.getDate() + 1);
						var cmpdate = compareDateWithoutTime(searchDate, dateTo);
						if (cmpdate === -1) {
							var iparams = angular.copy(filterData);
							iparams.DateFrom = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate());
							iparams.DateTo = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate(), 23, 59, 59);
							getQuickSearch(iparams, paramOptional);
						}
						else if (cmpdate === 0) {
							searchDate = new Date(dateTo);
							iparams = angular.copy(filterData);
							iparams.DateFrom = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate());
							iparams.DateTo = new Date(searchDate);
							getQuickSearch(iparams, paramOptional);
						}
						else {
							$scope.progressing = false;
							$scope.showProgressBar = false;
							$scope.totalTransCount = Enumerable.From($scope.quicksearchData).Sum(function (x) { return x.TotalTrans; });
						}
					},
					function (error) {
						console.log(error);
					});
			}

			function getQuickSearchChild(filterData, paramOptional) {
				$scope.showProgressBar = false;
				var data, dataDistinct, dataGroup;
				var sitepacInfo = Enumerable.From($scope.$parent.$parent.sitepacInfo);

				if ($scope.groupByField === true) {
					// Parent group by Site, Child group by Employee
					var pacids = sitepacInfo.Where(function (w) { return filterData.SiteKeys.indexOf(w.SiteKey) !== -1 })
							.Select(function (s) { return s.PacId }).Distinct("$.PacId").ToArray();
					data = Enumerable.From($scope.quicksearchDataOriginal)
						.Where(function (w) { return pacids.indexOf(w.PACID) !== -1 });
					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.PACID === null ? 0 : d.PACID,
							TransDate: d.TransDate,
							EmpID: d.EmpID === null ? 0 : d.EmpID,
							EmpName: d.EmpName,
							RegisterID: d.RegisterID === null ? 0 : d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID === null ? 0 : d.TransID,
							TransNB: d.TransNB,
							TypeID: d.TypeID === null ? 0 : d.TypeID,
							GroupByField: filterData.GroupByField
						}
					}).Distinct("$.TransID");

					dataGroup = Enumerable.From(dataDistinct).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						return {
							Childs: [],
							Details: [],
							EmpID: g.FirstOrDefault().EmpID,
							EmpName: g.FirstOrDefault().EmpName,
							GroupByField: g.FirstOrDefault().GroupByField,
							TotalTrans: g.Count('$.TransID'),
							TransID: g.FirstOrDefault().TransID,
							TransNB: g.FirstOrDefault().TransNB,
							TypeID: g.FirstOrDefault().TypeID,
							PACID: g.FirstOrDefault().PACID,
							TotalAmount: g.Sum("$.TotalAmount")
						};
					}).ToArray();
					$scope.quicksearchData[paramOptional.index].Childs = dataGroup;
				}
				else {
					// Parent group by Employee, Child group by site
					data = Enumerable.From($scope.quicksearchDataOriginal).Where(function (w) { return filterData.EmpIDs.indexOf(w.EmpID) !== -1 });

					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.PACID,
							TransDate: d.TransDate,
							RegisterID: d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID,
							TransNB: d.TransNB
						}
					}).Distinct("$.TransID");

					var dataTemp = dataDistinct.Join(sitepacInfo, "$.PACID", "$.PacId", function (d, sp) {
						return {
							PACID: sp.PacId,
							TransDate: d.TransDate,
							RegisterID: d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID,
							TransNB: d.TransNB,
							TypeID: d.TypeID,
							SiteKey: sp.SiteKey,
							SiteName: sp.SiteName
						};
					}).ToArray();

					dataGroup = Enumerable.From(dataTemp).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						return {
							Childs: [],
							Details: [],
							SiteKey: g.FirstOrDefault().SiteKey,
							SiteName: g.FirstOrDefault().SiteName,
							TotalTrans: g.Count('$.TransID'),
							TransID: g.FirstOrDefault().TransID,
							TransNB: g.FirstOrDefault().TransNB,
							TypeID: g.FirstOrDefault().TypeID,
							PACID: g.FirstOrDefault().PACID,
							TotalAmount: g.Sum("$.TotalAmount")
						};
					}).ToArray();
					$scope.quicksearchData[paramOptional.index].Childs = dataGroup;
				}
			}

			function getQuickSearchChildDetail(filterData, paramOptional) {
				$scope.showProgressBar = false;
				var data, dataDistinct, dataGroup;
				var sitepacInfo = Enumerable.From($scope.$parent.$parent.sitepacInfo);

				if ($scope.groupByField === true) {
					// Parent group by Site, Child group by Employee
					var pacids = sitepacInfo.Where(function (w) { return filterData.SiteKeys.indexOf(w.SiteKey) !== -1 })
							.Select(function (s) { return s.PacId }).Distinct("$.PacId").ToArray();
					data = Enumerable.From($scope.quicksearchDataOriginal)
						.Where(function (w) { return pacids.indexOf(w.PACID) !== -1 });
					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.PACID === null ? 0 : d.PACID,
							TransDate: d.TransDate,
							EmpID: d.EmpID === null ? 0 : d.EmpID,
							EmpName: d.EmpName,
							RegisterID: d.RegisterID === null ? 0 : d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID === null ? 0 : d.TransID,
							TransNB: d.TransNB,
							TypeID: d.TypeID === null ? 0 : d.TypeID,
							GroupByField: filterData.GroupByField
						}
					}).Distinct("$.TransID");

					dataGroup = Enumerable.From(dataDistinct).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						return {
							Childs: [],
							Details: [],
							EmpID: g.FirstOrDefault().EmpID,
							EmpName: g.FirstOrDefault().EmpName,
							GroupByField: g.FirstOrDefault().GroupByField,
							TotalTrans: g.Count('$.TransID'),
							TransID: g.FirstOrDefault().TransID,
							TransNB: g.FirstOrDefault().TransNB,
							TypeID: g.FirstOrDefault().TypeID,
							PACID: g.FirstOrDefault().PACID,
							TotalAmount: g.Sum("$.TotalAmount")
						};
					}).ToArray();
					$scope.quicksearchData[paramOptional.index].Childs = dataGroup;
				}
				else {
					// Parent group by Employee, Child group by site
					data = Enumerable.From($scope.quicksearchDataOriginal).Where(function (w) { return filterData.EmpIDs.indexOf(w.EmpID) !== -1 });

					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.PACID,
							TransDate: d.TransDate,
							RegisterID: d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID,
							TransNB: d.TransNB
						}
					}).Distinct("$.TransID");

					var dataTemp = dataDistinct.Join(sitepacInfo, "$.PACID", "$.PacId", function (d, sp) {
						return {
							PACID: sp.PacId,
							TransDate: d.TransDate,
							RegisterID: d.RegisterID,
							TotalAmount: d.TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID,
							TransNB: d.TransNB,
							TypeID: d.TypeID,
							SiteKey: sp.SiteKey,
							SiteName: sp.SiteName
						};
					}).ToArray();

					dataGroup = Enumerable.From(dataTemp).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						return {
							Childs: [],
							Details: [],
							SiteKey: g.FirstOrDefault().SiteKey,
							SiteName: g.FirstOrDefault().SiteName,
							TotalTrans: g.Count('$.TransID'),
							TransID: g.FirstOrDefault().TransID,
							TransNB: g.FirstOrDefault().TransNB,
							TypeID: g.FirstOrDefault().TypeID,
							PACID: g.FirstOrDefault().PACID,
							TotalAmount: g.Sum("$.TotalAmount")
						};
					}).ToArray();
					$scope.quicksearchData[paramOptional.index].Childs = dataGroup;
				}
			}

			function compareDateWithoutTime(date1, date2) {
				//We don't need compare time. Because when user chosen date, maybe 2 date different about hour, minute, second.
				/*
				 * Return 0, if date1 === date
				 * Return 1, if date1 > date2
				 * Return -1, if date1 < date2
				 */
				var d1 = new Date(date1).setHours(0, 0, 0, 0);
				var d2 = new Date(date2).setHours(0, 0, 0, 0);
				if (d1 === d2) {
					return 0;
				}
				else if (d1 > d2) {
					return 1;
				}
				else {
					return -1;
				}
			}

			function compareDate(date1, date2) {
				var d1 = date1.getTime();
				var d2 = date2.getTime();
				if (d1 === d2) {
					return 0;
				}
				else if (d1 > d2) {
					return 1;
				}
				else {
					return -1;
				}
			}

			function formatDateTimeParam(params) {
				params.DateFrom = Utils.toUTCDate(params.DateFrom);
				params.DateTo = Utils.toUTCDate(params.DateTo);
				return params;
			}

			/*Tree site methods, begin*/
			function getAllRegionSites(siteSelected) {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					var data = $scope.$parent.$parent.treeSiteFilter;
					vm.treeSiteFilterS = data;
					if (siteSelected && siteSelected.length > 0) {
						bamhelperSvc.setNodeSelected($scope.treeSiteFilter, siteSelected);
						$rootScope.$broadcast('cmsTreeRefresh');
						vm.selectedSites = siteSelected;
						vm.siteloaded = true;
					}
					else {
						bamhelperSvc.checkallNode($scope.treeSiteFilter);
						var checkedIDs = [];
						chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
						vm.selectedSites = checkedIDs;
						vm.siteloaded = true;
					}
					$rootScope.siteIDs = vm.selectedSites;
				}

				//$timeout(function () {
				//	getQuickSearchData();
				//}, 100);
			}

			vm.TreeSiteClose = function () {
				if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					$("#btn-popMenuConvSites").parent().removeClass("open");
					$("#btn-popMenuConvSites").prop("aria-expanded", false);
				}
				var checkedIDs = [];
				var checkedNames = [];
				var sitenodeSelected = [];
				var checkedIDs = [];
				chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				vm.selectedSites = checkedIDs;
				$rootScope.siteIDs = vm.selectedSites;
				//treeSiteSvc.getSiteSelectedNames(checkedIDs, checkedNames, vm.treeSiteFilterS.Sites);
				//vm.selectedSites = checkedIDs;
				//chartSvc.GetSiteSelectedNames(vm.treeSiteFilterS.Sites, sitenodeSelected);
				//$scope.sitenodeSelected = sitenodeSelected;
				//vm.selectedSites = Enumerable.From(sitenodeSelected).Select(function (x) { return x.ID; }).ToArray();

				//$scope.$broadcast(AppDefine.Events.REBARSEARCH, $rootScope.rebarSearch);
				//console.log(checkedIDs);
			};

			function selectedFn(node, scope) {

			}

			$scope.clickOutside = function (event, element) {

				//if (angular.element(element).hasClass('rebar-btn-tree')) {
				//	var checkedIDs = [];
				//	chartSvc.GetSiteSelectedIDs(checkedIDs, vm.treeSiteFilterS.Sites);
				//	vm.selectedSites = checkedIDs;
				//}

				// 2015-05-25 Tri fix bug 3289
				// update state tree when without click Done.
				bamhelperSvc.setNodeSelected(vm.treeSiteFilterS, vm.selectedSites);
				$scope.$broadcast('cmsTreeRefresh', vm.treeSiteFilterS);

				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
				}
			};
			/*Tree site methods, end*/

			/** Export functions, begin **/
			function buildTableQuickSearch(dtSource, options) {
				var startCol = options.ColIndex;
				var startRow = options.RowIndex;
				var colIndex = startCol;
				var table = {
					Name: options.tableName,
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};

				//Header row, begin

				var RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				var ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NAME_STRING),
					Color: AppDefine.ExportColors.GridHeaderListGroup,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 6, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL_AMOUNT),
					Color: AppDefine.ExportColors.GridHeaderListGroup,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_STRING),
					Color: AppDefine.ExportColors.GridHeaderListGroup,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 2, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				table.RowDatas.push(RowData);
				//Header row, end

				//Body content, begin
				startRow++;
				angular.forEach(dtSource, function (item) {
					colIndex = 1;
					RowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};

					ColData = {
						Value: item.SiteName,
						Color: AppDefine.ExportColors.GridGroupFirstCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 6, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};

					if (!$scope.groupByField) { //group by Employee
						ColData.Value = vm.EmpName({ T_OperatorID: item.T_OperatorID });
					}
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: "$" + $filter('salenumber')(item.TotalAmount, '0,0.00'),
						Color: AppDefine.ExportColors.GridGroupCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					ColData = {
						Value: item.TotalTrans,
						Color: AppDefine.ExportColors.GridGroupCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;

					table.RowDatas.push(RowData);

					//Add childs content, begin
					startRow++;
					angular.forEach(item.Childs, function (child, childIndex) {
						colIndex = 1;
						RowData = {
							Type: AppDefine.tableExport.Body,
							ColDatas: []
						};
						ColData = {
							Value: vm.EmpName({ T_OperatorID: child.T_OperatorID }),
							Color: AppDefine.ExportColors.GridRegionCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 6, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow
						};
						if (!$scope.groupByField) { //group by Employee
							ColData.Value = child.SiteName;
						}
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;

						ColData = {
							Value: "$" + $filter('salenumber')(child.TotalAmount, '0,0.00'),
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 3, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow
						};
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;

						ColData = {
							Value: child.TotalTrans,
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 2, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow
						};
						RowData.ColDatas.push(ColData);

						table.RowDatas.push(RowData);

						/** Add Trasaction list, begin **/
						if ($rootScope.fieldColumns.length > 0 && childIndex === vm.Showdetail) {
							//Header Trans list, begin
							startRow++;
							RowData = {
								Type: AppDefine.tableExport.Header,
								ColDatas: []
							};
							colIndex = 1;
							angular.forEach($rootScope.fieldColumns, function (field, index) {
								ColData = {
									Value: cmsBase.translateSvc.getTranslate(field.fieldName),
									Color: index === 0 ? AppDefine.ExportColors.GridHeaderFirstCell : AppDefine.ExportColors.GridHeaderCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 3, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: startRow
								};
								RowData.ColDatas.push(ColData);
								colIndex = ColData.MergeCells.Cells + colIndex;
							});
							table.RowDatas.push(RowData);
							//Header Trans list, end


							//Body Trans list, begin
							startRow++;
							angular.forEach(child.TransIDs, function (tran) {
								RowData = {
									Type: AppDefine.tableExport.Body,
									ColDatas: []
								};
								colIndex = 1;
								angular.forEach($rootScope.fieldColumns, function (field) {
									ColData = {
										Value: getFieldData(field.fieldName, tran),
										Color: AppDefine.ExportColors.GridNormalCell,
										CustomerWidth: false,
										Width: 0,
										MergeCells: { Cells: 3, Rows: 1 },
										ColIndex: colIndex,
										RowIndex: startRow
									};
									RowData.ColDatas.push(ColData);
									colIndex = ColData.MergeCells.Cells + colIndex;
								});
								table.RowDatas.push(RowData);
								startRow++;
							});
							//Body Trans list, end
						}
						else {
							startRow++;
						}

						/** Add Trasaction list, end **/
					});
					//Add childs content, end
				});
				//Body content, end

				return table;
			}

			function getFieldData(fieldName, tran) {
				var ret = tran[fieldName];
				switch(fieldName){
					case "PaymentName":
						ret = "";
						angular.forEach(tran.Payments, function (pay) {
							ret = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Payments, 'ID', pay.PaymentID, 'Name') + ",";
						});
						ret = ret.substr(0, ret.length - 1);
						break;
					case "PaymentAmount":
						ret = "";
						angular.forEach(tran.Payments, function (pay) {
							ret = !pay.PaymentAmount? "0" : pay.PaymentAmount + ",";
						});
						ret = !ret? "$0" : "$" + ret.substr(0, ret.length - 1);
						break;
					case "Description":
						ret = "";
						angular.forEach(tran.Description, function (des) {
							ret = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Descriptions, 'ID', des.Description, 'Name') + ",";
						});
						ret = ret.substr(0, ret.length - 1);
						break;
					case "TaxID":
						ret = "";
						angular.forEach(tran.Taxes, function (tax) {
							ret = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Taxes, 'ID', tax.TaxID, 'Name') + ",";
						});
						ret = ret.substr(0, ret.length - 1);
						break;
					case "TaxAmount":
						ret = "";
						angular.forEach(tran.Taxes, function (tax) {
							ret = tax.TaxAmount + ",";
						});
						ret = !ret ? "$0" : +ret.substr(0, ret.length - 1);
						break;
					case "T_OperatorName":
						ret = vm.EmpName({ T_OperatorID: tran.T_OperatorID });
						break;
					case "T_TerminalName":
						ret = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Terminals, 'ID', tran.T_TerminalID, 'Name');
						break;
						
				}
				return ret;
			}

			/** Export functions, end **/
		}
	});
})();