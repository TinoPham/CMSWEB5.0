(function () {
	define(['cms',
		'DataServices/Rebar/rebar.service',
		'DataServices/Rebar/canned.service',
		'Scripts/Services/bamhelperSvc',
		'Scripts/Services/chartSvc',
		'Widgets/Rebar/transactiondetail',
		'widgets/rebar/rebar-transact-viewer',
		'widgets/rebar/transaction-list',
	], function (cms) {
		cms.register.controller('cannedCtrl', cannedCtrl);
		cannedCtrl.$inject = ['$scope', '$rootScope', '$timeout', '$filter', '$modal', 'cmsBase', 'AppDefine', 'rebarDataSvc', 'cannedSvc', 'bamhelperSvc', 'chartSvc', 'Utils', 'filterSvc', 'POSCommonSvc', 'exportSvc', 'AccountSvc'];
		function cannedCtrl($scope, $rootScope, $timeout, $filter, $modal, cmsBase, AppDefine, rebarDataSvc, cannedSvc, bamhelperSvc, chartSvc, Utils, filterSvc, POSCommonSvc, exportSvc, AccountSvc) {
			var vm = this;
			$scope.GroupByFieldConst = { Site: 1, Employee: 2 };
			$scope.cannedData = Enumerable.Empty();;
			$scope.cannedDataOriginal = Enumerable.Empty();;
			$scope.filterTypes = {
				Payment: 1,
				Register: 2,
				Employee: 3,
				Description: 4
			};
			$scope.dataTypes = {
				Canned: 1,
				CannedChild: 2,
				CannedDetail: 3
			};
			var dateFrom, dateTo, searchDate;
			$scope.totalTransCount = 0;
			vm.rebartransactviewerProperty = {
				Max: false,
				Collapse: false
			};
			$rootScope.fieldColumns = [];

			$scope.$on(AppDefine.Events.REBARSEARCH, function (e, arg) {
				var retCompDate = Utils.compareDate($rootScope.rebarSearch.DateFrom, $rootScope.rebarSearch.DateTo);
				if (retCompDate === 1) { //if DateFrom > DateTo
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					return;
				}
				$scope.$parent.$parent.$parent.reloadTreeSiteData();

				$scope.cannedDataOriginal = [];
				$scope.$parent.$parent.$parent.progressing = true;
				$scope.$parent.$parent.$parent.isStopped = false;
				$scope.$parent.$parent.$parent.dataProgressValue = 0;
				$scope.$parent.$parent.$parent.showProgressBar = true;
				$scope.totalTransCount = 0;

				$timeout(function () {
					$scope.getCannedData();
				}, 100);

			});

			$scope.$on(AppDefine.Events.PAGEREADY, function () {
				active();
			});

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
				$rootScope.fieldColumns = Enumerable.From($rootScope.fieldColumns).Where(function (w) { return w.checked === true && w.isShow === true; }).ToArray();
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
				var tableQuickSearch = buildTableCanned($scope.cannedData, options);
				tables.push(tableQuickSearch);

				var charts = [];
				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});

			vm.EmpName = function ( item ) {
				var opList = POSCommonSvc.GetCache( AppDefine.POSItemKeys.Operators, true );
				var found = $scope.$parent.LookupItem( opList, 'ID', item.T_OperatorID, 'Name' );
				if ( found.Any() )
					return found.First();
				return '';
			}
			vm.SiteName = function ( item ) {
				var sitepacInfo = $scope.$parent.$parent.$parent.$parent.sitepacInfo;
				var found = $scope.$parent.$parent.LookupItem( sitepacInfo, 'PacId', item.T_PACID, 'SiteName' );
				if ( found.Any() )
					return found.First();
				return '';
			}

			$scope.$on(AppDefine.Events.CANNEDGROUPBYCHANGED, function (e, arg) {
				var data = Enumerable.From($scope.cannedDataOriginal);
				var dataDistinct;
				var groupFieldName;
				if ( arg.groupByField === false )
					groupFieldName = "T_OperatorID";// "$.EmpID";
				else
					groupFieldName = "SiteKey";

				AppendCannedData( Enumerable.Empty(), groupFieldName );

				//if (arg.groupByField === false) {
				//	groupFieldName = "$.T_OperatorID";// "$.EmpID";

				//	dataDistinct = data.Select(function (d) {
				//		return {
				//			T_PACID: d.T_PACID,
				//			TransDate: d.TransDate,
				//			RegisterID: d.T_RegisterID,
				//			TotalAmount: d.T_6TotalAmount,
				//			Tracking: d.Tracking,
				//			TransID: d.TransID,
				//			TransNB: d.T_0TransNB,
				//			EmpID: d.T_OperatorID,
				//			EmpName: d.T_OperatorName,
				//			PaymentAmount: d.PaymentAmount,
				//			T_StoreID : d.T_StoreID,
				//			GroupByField: arg.groupByField
				//		}
				//	}).Distinct("$.TransID");

				//	$scope.cannedData = Enumerable.From(dataDistinct).GroupBy(groupFieldName, null, function (key, g) {
				//		return {
				//			Childs: [],
				//			Details: [],
				//			GroupByField: g.FirstOrDefault().GroupByField,
				//			EmpID: g.FirstOrDefault().EmpID,
				//			EmpName: g.FirstOrDefault().EmpName,
				//			TotalTrans: g.Count('$.TransID'),
				//			TransID: g.FirstOrDefault().TransID,
				//			TransNB: g.FirstOrDefault().TransNB,
				//			PACID: g.FirstOrDefault().PACID,
				//			TotalAmount: g.Sum("$.TotalAmount")
				//		};
				//	}).ToArray();
				//}
				//else {
				//	groupFieldName = "$.T_PACID";
				//	var sitepacInfo = Enumerable.From($scope.$parent.$parent.$parent.$parent.sitepacInfo);

				//	dataDistinct = data.Select(function (d) {
				//		return {
				//			T_PACID: d.T_PACID,
				//			TransDate: d.TransDate,
				//			RegisterID: d.T_RegisterID,
				//			TotalAmount: d.T_6TotalAmount,
				//			PaymentAmount: d.PaymentAmount,
				//			Tracking: d.Tracking,
				//			TransID: d.TransID,
				//			TransNB: d.T_0TransNB,
				//			GroupByField: arg.groupByField
				//		}
				//	}).Distinct("$.TransID");

				//	var dataTemp = dataDistinct.Join(sitepacInfo, "$.PACID", "$.PacId", function (d, sp) {
				//		return {
				//			T_PACID: sp.PacId,
				//			TransDate: d.TransDate,
				//			RegisterID: d.RegisterID,
				//			TotalAmount: d.TotalAmount,
				//			Tracking: d.Tracking,
				//			TransID: d.TransID,
				//			TransNB: d.TransNB,
				//			TypeID: d.TypeID,
				//			GroupByField: arg.groupByField,
				//			SiteKey: sp.SiteKey,
				//			SiteName: sp.SiteName
				//		};
				//	}).ToArray();

				//	$scope.cannedData = Enumerable.From(dataTemp).GroupBy(groupFieldName, null, function (key, g) {
				//		return {
				//			Childs: [],
				//			Details: [],
				//			GroupByField: g.FirstOrDefault().GroupByField,
				//			SiteKey: g.FirstOrDefault().SiteKey,
				//			SiteName: g.FirstOrDefault().SiteName,
				//			TotalTrans: g.Count('$.TransID'),
				//			TransID: g.FirstOrDefault().TransID,
				//			TransNB: g.FirstOrDefault().TransNB,
				//			T_PACID: g.FirstOrDefault().T_PACID,
				//			TotalAmount: g.Sum("$.TotalAmount")
				//		};
				//	}).ToArray();
				//}

			});

			$scope.$on("$destroy", function () {
				$scope.$parent.$parent.$parent.isStopped = true;
			});

			active();

			function active() {
				//getAllRegionSites();
				if ($scope.$parent.$parent.$parent.pageReady) {
					getCannedData();
				}
			}

			function getCannedData() {
				var params = angular.copy($rootScope.rebarSearch);
				if (!params) { return; }

				params.ReportID = $scope.$parent.$parent.$parent.ReportID;
				params.SiteKeys = $scope.$parent.$parent.$parent.vm.selectedSites;
				params.GroupByField = $scope.$parent.$parent.$parent.groupByField ? $scope.GroupByFieldConst.Site : $scope.GroupByFieldConst.Employee;

				$scope.cannedData = [];
				var retCompDate = compareDate(params.DateFrom, params.DateTo);
				if (retCompDate === 1) { //if DateFrom > DateTo
					cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					return;
				}

				var groupFieldName;
				if (params.GroupByField === $scope.GroupByFieldConst.Employee) {
					groupFieldName = "T_OperatorID";//"$.EmpID";
				}
				else {
					groupFieldName = "SiteKey";
				}

				var paramOptional = {
					groupFieldName: groupFieldName
				};
				loopGetData(params, $scope.dataTypes.Canned, paramOptional);
			}

			$scope.getCannedData = function () {
				getCannedData();
			}

			vm.LookupCacheItem = function ( name, fieldname, value, select_field ) {

				var _items = POSCommonSvc.GetCache( name, true );
				if ( !_items )
					return null;

				var match = $scope.LookupItem( _items, fieldname, value, select_field );
				if ( match.Any() )
					return match.First();
				return null;
			}

			vm.LookupItem = function ( src_array, fieldname, value, select_field ) {

				var ie_enum = Enumerable.From( src_array );
				var match = ie_enum.Where( function ( x ) { return x[fieldname] === value; } );
				if ( select_field == null || select_field == undefined )
					return match;
				return match.Select( function ( x ) { return x[select_field]; } );
			};
			
			$scope.getCannedChildData = function (event, cannedItem, index) {
				//if ($scope.cannedData[index].Childs.length > 0 || $scope.$parent.$parent.$parent.progressing) { return; }
				if ( cannedItem.Childs.length > 0 || $scope.$parent.$parent.$parent.progressing )
					return;
				var parent_field, child_field;
				if ( $scope.groupByField === true ) {
					child_field = "T_OperatorID";
					parent_field = "SiteKey";
				}
				else {
					parent_field = "T_OperatorID";
					child_field = "SiteKey";

				}
				CannedGroupChild( cannedItem, parent_field, child_field);
				//var params = angular.copy($rootScope.rebarSearch);
				//if (!params || !cannedItem || index === undefined) { return; }

				//params.ReportID = $scope.$parent.$parent.$parent.ReportID;
				//params.SiteKeys = $scope.$parent.$parent.$parent.vm.selectedSites;
				//var groupFieldName;
				//if ($scope.groupByField === true) {
				//	params.GroupByField = $scope.GroupByFieldConst.Employee;
				//	params.SiteKeys = [];
				//	params.SiteKeys.push(cannedItem.SiteKey);
				//	groupFieldName = "T_OperatorID";
				//}
				//else {
				//	params.GroupByField = $scope.GroupByFieldConst.Site;
				//	if (cannedItem.EmpID) {
				//		params.EmpIDs.push(cannedItem.EmpID);
				//		params.EmpIDs_AND = 1;
				//	};
				//	groupFieldName = "PACID";
				//}

				//$scope.cannedData[index].Childs = [];
				//var paramOptional = {
				//	groupFieldName: groupFieldName,
				//	index: index
				//};
				//loopGetData(params, $scope.dataTypes.CannedChild, paramOptional);
			}

			$scope.getCannedDetail = function (event, cannedItem, parentIndex, index) {
				vm.Showdetail = index;
				vm.parentshowdetail = parentIndex;
				if ( cannedItem.TransIDs == null || cannedItem.TransIDs.length == 0 )
					return;
				vm.param = {
					Trans: cannedItem.TransIDs,
					ShowBackButton: false
				};
				vm.definefield = angular.copy( filterSvc.getTransQSColumn() );
				var descol = Enumerable.From( vm.definefield ).FirstOrDefault( null, function ( x ) { return x.fieldName === 'Description'; } );
				if( descol)
					descol.isShow = false;

				$rootScope.fieldColumns = Enumerable.From(vm.definefield).Where(function (w) { return w.checked === true && w.isShow === true; }).ToArray();
				
			}

			function loadTrandata(data) {
				vm.param = {
					SiteKeys: data.SiteKeys,
					ShowBackButton: false
				};
				vm.definefield = angular.copy(filterSvc.getTransQSColumn());

				var trandatefilter = {
					SelectAnd: true,
					firstvalue: $rootScope.rebarSearch.DateFrom,
					endvalue: $rootScope.rebarSearch.DateTo,
					selectfirst: { id: 4, key: 'ge', name: "Greater than or equal" },
					selectend: { id: 6, key: 'le', name: "Less than or equal" }
				}
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
			}

			function loopGetData(params, dataType, paramOptional) {
				dateFrom = angular.copy(params.DateFrom);
				dateTo = angular.copy(params.DateTo);
				searchDate = angular.copy(params.DateFrom);

				var filterData = angular.copy(params);
				if (compareDateWithoutTime(dateFrom, dateTo) === -1) {
					filterData.DateFrom = new Date(searchDate);
					filterData.DateTo = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate(), 23, 59, 59);
				}

				$scope.$parent.$parent.$parent.showProgressBar = true;
				$scope.$parent.$parent.$parent.max = (dateTo - dateFrom) / 86400000;

				//console.log(filterData);
				switch (dataType) {
					case $scope.dataTypes.Canned:
						getCanned(filterData, paramOptional);
						break;
					case $scope.dataTypes.CannedChild:
						getCannedChild(filterData, paramOptional);
						break;
					case $scope.dataTypes.CannedDetail:
						getCannedDetail(filterData, paramOptional);
						break;
				}
			}

			function CannedGroupChild( quicksearchItem, parentfield, childfield)
			{
				var val = quicksearchItem[parentfield];
				var filter_parent = Enumerable.From( $scope.cannedDataOriginal ).Where( function ( x ) { return x[parentfield] == val; } );
				var group_child = filter_parent.GroupBy( function ( x ) { return x[childfield]; }, null, function ( ket, g ) {
					return {

						T_OperatorID: g.FirstOrDefault().T_OperatorID,
						TotalTrans: g.Distinct( '$.TransID' ).Count( '$.TransID' ),
						T_PACID: g.First().T_PACID,
						SiteKey: g.First().SiteKey,
						SiteName: g.First().SiteName,
						TotalAmount: g.Distinct( '$.TransID' ).Sum( "$.T_6TotalAmount" ),
						TransIDs: g.Select( function ( it ) { return it; } ).ToArray(),
					}
				} );

				quicksearchItem.Childs = group_child.ToArray();
			}

			function AppendCannedData( response, group_field) {
				if ( response == null )
					return;
				
				$scope.cannedDataOriginal = Enumerable.From( $scope.cannedDataOriginal ).Concat( response );
				var group;
				group = Enumerable.From( $scope.cannedDataOriginal ).GroupBy( function ( it ) { return it[group_field]; }, null, function ( key, g ) {
						return {
							T_OperatorID: g.FirstOrDefault().T_OperatorID,
							TotalTrans: g.Distinct( '$.TransID' ).Count( '$.TransID' ),
							TransIDs: g.Select( function ( it ) { return it; } ).ToArray(),
							T_PACID: g.First().T_PACID,
							SiteKey: g.First().SiteKey,
							SiteName: g.First().SiteName,
							TotalAmount: g.Distinct( '$.TransID' ).Sum( "$.T_6TotalAmount" ),
							Childs: []
						}
				} );

				$scope.cannedData = group.ToArray();//Enumerable.From( data ).GroupBy( function ( x ) { return x[paramOptional.groupFieldName]; } ).Distinct( function ( x ) { return x.TransID } );
				
			}

			function getCanned(filterData, paramOptional) {
				if ($scope.$parent.$parent.$parent.isStopped) { return; }
				
				filterData = formatDateTimeParam(filterData);
				cannedSvc.GetCannedReport(filterData,
					function (response) {
						//$scope.cannedDataOriginal = Enumerable.From($scope.cannedDataOriginal).Concat(response).ToArray();
						//var data = Enumerable.From($scope.cannedDataOriginal);
						var sitepacInfo = [];
						var dataDistinct, dataTemp;

						var value = (searchDate - dateFrom) / 86400000;
						var percent = Math.floor((value / $scope.$parent.$parent.$parent.max) * 100);
						$scope.$parent.$parent.$parent.dataProgressValue = percent;

						//if ($scope.$parent.$parent.$parent.groupByField === true) {
						//	//Group by site
						//	sitepacInfo = Enumerable.From($scope.$parent.$parent.$parent.$parent.sitepacInfo);

						//	dataDistinct = data.Select(function (d) {
						//		return {
						//			PACID: d.T_PACID,
						//			TransDate: d.TransDate,
						//			RegisterID: d.T_RegisterID,
						//			TotalAmount: d.T_6TotalAmount,
						//			Tracking: d.Tracking,
						//			TransID: d.TransID,
						//			TransNB: d.T_0TransNB,
						//			GroupByField: filterData.GroupByField
						//		}
						//	}).Distinct("$.TransID");

						//	dataTemp = dataDistinct.Join(sitepacInfo, "$.PACID", "$.PacId", function (d, sp) {
						//		return {
						//			PACID: sp.PacId,
						//			TransDate: d.TransDate,
						//			RegisterID: d.RegisterID,
						//			TotalAmount: d.TotalAmount,
						//			Tracking: d.Tracking,
						//			TransID: d.TransID,
						//			TransNB: d.TransNB,
						//			TypeID: d.TypeID,
						//			GroupByField: filterData.GroupByField,
						//			SiteKey: sp.SiteKey,
						//			SiteName: sp.SiteName
						//		};
						//	}).ToArray();

						//	$scope.cannedData = Enumerable.From(dataTemp).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						//		return {
						//			Childs: [],
						//			Details: [],
						//			SiteKey: g.FirstOrDefault().SiteKey,
						//			SiteName: g.FirstOrDefault().SiteName,
						//			TotalTrans: g.Count('$.TransID'),
						//			TransID: g.FirstOrDefault().TransID,
						//			TransNB: g.FirstOrDefault().TransNB,
						//			PACID: g.FirstOrDefault().PACID,
						//			TotalAmount: g.Sum("$.TotalAmount"),
						//			GroupByField: $scope.GroupByFieldConst.Site,
						//		};
						//	}).ToArray();
						//}
						//else {
						//	//Group by emplyee
						//	dataDistinct = data.Select(function (d) {
						//		return {
						//			PACID: d.PACID,
						//			TransDate: d.TransDate,
						//			RegisterID: d.RegisterID,
						//			TotalAmount: d.TotalAmount,
						//			Tracking: d.Tracking,
						//			TransID: d.TransID,
						//			TransNB: d.TransNB,
						//			T_OperatorID: d.T_OperatorID,
						//			T_OperatorName: d.T_OperatorName,
						//			GroupByField: filterData.GroupByField
						//		}
						//	}).Distinct("$.TransID");

						//	$scope.cannedData = Enumerable.From(dataDistinct).GroupBy(paramOptional.groupFieldName, null, function (key, g) {
						//		return {
						//			Childs: [],
						//			Details: [],
						//			T_OperatorID: g.FirstOrDefault().T_OperatorID,
						//			T_OperatorName: g.FirstOrDefault().T_OperatorName,
						//			TotalTrans: g.Count('$.TransID'),
						//			TransID: g.FirstOrDefault().TransID,
						//			TransNB: g.FirstOrDefault().TransNB,
						//			PACID: g.FirstOrDefault().PACID,
						//			TotalAmount: g.Sum("$.TotalAmount"),
						//			GroupByField: $scope.GroupByFieldConst.Employee,
						//		};
						//	}).ToArray();
						//}
						//$scope.cannedData = Enumerable.From( data ).GroupBy( function ( x ) { return x[paramOptional.groupFieldName]; } ).Distinct( function ( x ) { return x.TransID });


						var sitepacInfo = Enumerable.From( $scope.$parent.$parent.$parent.$parent.sitepacInfo );
						var siteresponse = Enumerable.From( response ).Join( sitepacInfo, "$.T_PACID", "$.PacId", function ( d, s ) {
							return {
								T_PACID: d.T_PACID,
								TransID: d.TransID,
								TypeID: d.TypeID,
								T_OperatorID: d.T_OperatorID,
								T_OperatorName: d.T_OperatorName,
								T_RegisterID: d.T_RegisterID,
								T_RegisterName: d.T_RegisterName,
								PaymentID: d.PaymentID,
								PaymentName: d.PaymentName,
								T_0TransNB: d.T_0TransNB,
								T_6TotalAmount: d.T_6TotalAmount,
								DVRDate: d.DVRDate,
								T_CameraNB: d.T_CameraNB,
								TransDate: d.TransDate,
								Taxes: d.Taxes,
								T_8ChangeAmount: d.T_8ChangeAmount,
								T_TerminalID: d.T_TerminalID,
								T_StoreID: d.T_StoreID,
								T_CheckID: d.T_CheckID,
								Tracking: d.Tracking,
								PaymentAmount: d.PaymentAmount,
								TaxID: d.TaxID,
								SiteKey: s.SiteKey,
								SiteName: s.SiteName
							}
						} );

						AppendCannedData( siteresponse, paramOptional.groupFieldName );
						//loop get data
						searchDate.setDate(searchDate.getDate() + 1);
						var cmpdate = compareDateWithoutTime(searchDate, dateTo);
						if (cmpdate === -1) {
							var iparams = angular.copy(filterData);
							iparams.DateFrom = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate());
							iparams.DateTo = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate(), 23, 59, 59);
							getCanned(iparams, paramOptional);
						}
						else if (cmpdate === 0) {
							searchDate = new Date(dateTo);
							iparams = angular.copy(filterData);
							iparams.DateFrom = new Date(searchDate.getFullYear(), searchDate.getMonth(), searchDate.getDate());
							iparams.DateTo = new Date(searchDate);
							getCanned(iparams, paramOptional);
						}
						else {
							$scope.$parent.$parent.$parent.progressing = false;
							$scope.$parent.$parent.$parent.showProgressBar = false;
							$scope.totalTransCount = Enumerable.From( $scope.cannedData ).Sum( function ( x ) { return x.TotalTrans; } );
							//$scope.totalTransCount = Enumerable.From($scope.cannedDataOriginal).Select(function (d) {
							//	return {
							//		PACID: d.T_PACID,
							//		TransDate: d.TransDate,
							//		RegisterID: d.T_RegisterID,
							//		TotalAmount: d.T_6TotalAmount,
							//		Tracking: d.Tracking,
							//		TransID: d.TransID,
							//		TransNB: d.T_0TransNB,
							//		GroupByField: filterData.GroupByField
							//	}
							//}).Distinct("$.TransID").ToArray();
						}
					},
					function (error) {
						$scope.$parent.$parent.$parent.progressing = false;
						$scope.$parent.$parent.$parent.showProgressBar = false;
					});
			}

			function getCannedChild(filterData, paramOptional) {
				$scope.$parent.$parent.$parent.showProgressBar = false;
				var data, dataDistinct, dataGroup;
				var sitepacInfo = Enumerable.From($scope.$parent.$parent.$parent.$parent.sitepacInfo);

				if ($scope.groupByField === true) {
					// Parent group by Site, Child group by Employee
					var pacids = sitepacInfo.Where(function (w) { return filterData.SiteKeys.indexOf(w.SiteKey) !== -1 })
							.Select(function (s) { return s.PacId }).Distinct("$.PacId").ToArray();
					data = Enumerable.From($scope.cannedDataOriginal)
						.Where(function (w) { return pacids.indexOf(w.T_PACID) !== -1 });
					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.T_PACID === null ? 0 : d.T_PACID,
							TransDate: d.TransDate,
							EmpID: d.T_OperatorID === null ? 0 : d.T_OperatorID,
							EmpName: d.T_OperatorName,
							RegisterID: d.T_RegisterID === null ? 0 : d.T_RegisterID,
							TotalAmount: d.T_6TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID === null ? 0 : d.TransID,
							TransNB: d.T_0TransNB,
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
					$scope.cannedData[paramOptional.index].Childs = dataGroup;
				}
				else {
					// Parent group by Employee, Child group by site
					data = Enumerable.From($scope.cannedDataOriginal).Where(function (w) { return filterData.EmpIDs.indexOf(w.T_OperatorID) !== -1 });

					dataDistinct = data.Select(function (d) {
						return {
							PACID: d.T_PACID,
							TransDate: d.TransDate,
							RegisterID: d.T_RegisterID,
							TotalAmount: d.T_6TotalAmount,
							Tracking: d.Tracking,
							TransID: d.TransID,
							TransNB: d.T_0TransNB
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
					$scope.cannedData[paramOptional.index].Childs = dataGroup;
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

			/** Export functions, begin **/
			function buildTableCanned(dtSource, options) {
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
										Value: getFieldData(field.fieldName, tran), // tran[field.fieldName], //getFieldData(field.fieldName, tran),
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
				switch (fieldName) {
					case "TaxID":
						ret = "";
						ret = $scope.$parent.LookupCacheItem(AppDefine.POSItemKeys.Taxes, 'ID', tran.TaxID, 'Name') + ",";
						ret = ret.substr(0, ret.length - 1);
						break;
					case "TaxAmount":
						ret = "$" + tran.Taxes;
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