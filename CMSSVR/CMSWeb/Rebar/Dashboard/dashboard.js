(function () {
	'use strict';

	define([
        'cms',
        'widgets/rebar/transactiondetail',
        'widgets/rebar/select-tran-flag',
        'widgets/rebar/rebar-transact-viewer'
	], function (cms) {
		cms.register.controller('rebardashboardCtrl', rebardashboardCtrl);
		rebardashboardCtrl.$inject = ['$timeout', '$rootScope', '$scope', 'cmsBase', 'AppDefine', '$filter', 'chartSvc', 'bamhelperSvc', 'dataContext', 'rebarDataSvc', '$modal', 'Utils', 'filterSvc', 'AccountSvc', 'exportSvc', 'SiteSvc'];

		function rebardashboardCtrl($timeout, $rootScope, $scope, cmsBase, AppDefine, $filter, chartSvc, bamhelperSvc, dataContext, rebarDataSvc, $modal, Utils, filterSvc, AccountSvc, exportSvc, SiteSvc) {
			var vm = this;
			vm.siteloaded = false;
			$scope.selectedSites = [];
		    $scope.GroupBy = {
		        SITE: 0,
		        EMPL: 1,
		        DATE: 2

		    };
		    $scope.currentSelect = {
		        TypeReport: undefined,
		        Site: undefined,
		        DateTime: undefined,
		        EmployerName: undefined
		    }
		    var DETAIL_STATE = 3;
		    $scope.GridInfoType = {
		    	Transac: 0,
		    	Customer: 1,
		    	Car: 2

		    };
			vm.treeDef = {
				Id: 'ID',
				Name: 'Name',
				Type: 'Type',
				Checked: 'Checked',
				Childs: 'Sites',
				Count: 'SiteCount',
				Model: {}
			};

			$scope.pasteRegx = AppDefine.RegExp.PasteExp;
			$scope.inputRex = AppDefine.RegExp.NumberRestriction;
			$scope.PreventPaste = cmsBase.PreventPaste;
			$scope.PreventLetter = cmsBase.PreventInputKeyPress;
			vm.treeOptions = {
				Node: {
					IsShowIcon: true,
					IsShowCheckBox: true,
					IsShowNodeMenu: false,
					IsShowAddNodeButton: false,
					IsShowAddItemButton: false,
					IsShowEditButton: true,
					IsShowDelButton: true,
					IsDraggable: false
				},
				Item: {
					IsShowItemMenu: false
				},
				Type: {
					Folder: 0,
					Group: 2,
					File: 1
				},
				CallBack: {
					SelectedFn: selectedFn
				}
			};
			vm.smartexceptionProperty = {
				Max: false,
				Collapse: false
			};
			vm.employeedataProperty = {
				Max: false,
				Collapse: false
			};
			vm.employeedatatableProperty = {
				Max: false,
				Collapse: true //default collapse employee data table
			};
			vm.viewByEmployee = false;
			var pagesize = 10;
			vm.currPage = 1;
			vm.optionview = 1;
			vm.totalRisk = 0;
			vm.GridDataType = 0; //0: Risk, 1: Smart Exception
			vm.GridDataInfoType = 0; //0: transaction, 1: customer, 2: car

			var definedFilter = filterSvc.getTransactionColumn();
		    $scope.definedFilter = angular.copy(definedFilter);
			angular.forEach($scope.definedFilter, function (value) { if (value.fieldName === 'EmployeeName') { value.checked = true; } }); //ThangPham, show Employee Name column for Transaction list on Dashboard.
			//$scope.definedFilter[12].checked = true; //default show Employee column
			vm.arrSmartExNames = ['REFUND', 'CUSTOMER_WOT', 'CAR_WOT'];

			$scope.$watch('vm.viewByEmployee', function (oldval, newval) {
			    if (oldval !== newval) {
			        setCurrentSelect(AppDefine.Resx.RISK_FACTOR);
					vm.currPage = 1;
					if (!vm.rebarTree) return;
					vm.siteloaded = true;
					$scope.states = vm.viewByEmployee ? { Employee: 0, Date: 1, Transact: 2 } : { Site: 0, Employee: 1, Date: 2, Transact: 3 };
					$scope.state = 0;
					$scope.previousData = [];
					vm.showDetail = false;

					loadData(vm.viewByEmployee, vm.currPage,vm.selectedSites);
				}
			});

			$scope.$watch('vm.optionview', function (oldval, newval) {
			    if (oldval !== newval) {
			        setCurrentSelect(AppDefine.Resx.RISK_FACTOR);
					vm.currPage = 1;
					if (!vm.rebarTree) return;
					vm.siteloaded = true;
					$scope.states = vm.viewByEmployee ? { Employee: 0, Date: 1, Transact: 2 } : { Site: 0, Employee: 1, Date: 2, Transact: 3 };
					$scope.state = 0;
					$scope.previousData = [];
					vm.showDetail = false;

		            loadData(vm.viewByEmployee, vm.currPage, vm.selectedSites);
					//renderChart(vm.data);
				}
			});

			$scope.$on(AppDefine.Events.REBARSEARCH, function () {
				vm.showDetail = null;
				vm.employeedatatableProperty.Collapse = true;

				vm.currPage = 1;
				if (!vm.rebarTree) return;
				vm.siteloaded = true;
				$scope.previousData = [];
				$scope.state = 0;
				loadSumaryData();
				loadData(vm.viewByEmployee, vm.currPage, vm.selectedSites);
				resetCurrentSelect();
			});

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
			    //console.log($scope.state);
				loadSumaryDataToExport().then(function () {
				var userLogin = AccountSvc.UserModel();

				var reportInfo = {
						TemplateName: cmsBase.translateSvc.getTranslate(AppDefine.Resx.MODULE_REBAR) + "_" + cmsBase.translateSvc.getTranslate($rootScope.title).replace(/[\s]/g, ''),
					ReportType: 2,
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
				var options = { ColIndex: 1, RowIndex: 1 };
					var tbMarkException = buildTableMarkException(vm.dataStatisExport, options);
				tables.push(tbMarkException);
				options = { ColIndex: 14, RowIndex: 1 };
					var tableRiskFactor = buildTableRiskFactor(vm.totalRiskExport, options);
				tables.push(tableRiskFactor);
				var tableRiskFactorDetail;
					if (vm.totalRiskExport > 0) {
					options = { ColIndex: 1, RowIndex: 6, DataType: $scope.state };
						tableRiskFactorDetail = buildTableRiskFactorDetail(vm.dataExport.Data, options);
					tables.push(tableRiskFactorDetail);
				}

				var charts = [];
				options = {
					Width: 10,
					Height: 18,
					ColIndex: 1,
					RowIndex: !tableRiskFactorDetail ? 5 : tableRiskFactorDetail.RowDatas.length + tbMarkException.RowDatas.length + 3
				};
					var chartMarkException = buildChartMarkException(vm.dataStatisExport, options);
				charts.push(chartMarkException);

				options = {
					Width: 10,
					Height: 18,
					ColIndex: options.Width + 3,
					RowIndex: options.RowIndex
				};
					chartMarkException = buildChartRiskFactor(vm.dataExport.Data, options);
				charts.push(chartMarkException);

				exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			});
			});

			//$scope.$on(AppDefine.Events.PAGEREADY, function () {
			//	active();
			//});

			$scope.optionsShow = {
				isBack: true,
				Max: false,
				Collapse: false,
				DateFormatFilter: 'HH:mm:ss'
			};

			$scope.clickOutside = function ($event, element) {
				//if (angular.element(element).hasClass('tree-dropdown-button')) {
				//	var checkedIDs = [];
				//	chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
				//	$scope.selectedSites = checkedIDs;
				//}
				// 2015-05-25 Tri fix bug 3289
				// update state tree when without click Done.
				bamhelperSvc.setNodeSelected(vm.rebarTree, vm.selectedSites);
				$scope.$broadcast('cmsTreeRefresh', vm.rebarTree);

				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
					// console.log('have open');
				} else {
					// console.log('dont have open');
				}
			};

			vm.closeDetail = function () {
				vm.showDetail = null;
				//$scope.state = $scope.state - 1;
				$scope.GetBackData();
			}

			vm.showTransactionInfo = function (detail) {
				//var startDate = new Date(Date.UTC(detail.Year, parseInt(detail.Month)-1, detail.Day, 0, 0, 0, 0));
				//var endDate = new Date(Date.UTC(detail.Year, parseInt(detail.Month)- 1, detail.Day, 23, 59, 59, 99));
				var startDate = null;//new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 0, 0, 0, 0);
				var endDate = null;//new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 23, 59, 59, 99);
				if (detail.hasOwnProperty("Year") && detail.hasOwnProperty("Month") && detail.hasOwnProperty("Day")) {
					startDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 0, 0, 0, 0);
					endDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 23, 59, 59, 99);
				}
				else {//if (detail.hasOwnProperty("Date")) {
					var date = chartSvc.GetUTCDate(detail.Date);
					startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0, 0);
					endDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 59, 99);
				}

				if (detail.EmployerId) {
					var filter = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause('EmployeeId').eq(detail.EmployerId));
					// console.log(filter); return;
					vm.paramShowTran = {
						EmployerId: detail.EmployerId ? detail.EmployerId : null,
						SiteKey: detail.SiteKey ? detail.SiteKey : null,
						//StartDate: vm.startFilter ? vm.startFilter : null,
						StartDate: startDate,
						EndDate: endDate,
						filter: filter
					};
				}
				else {
					vm.paramShowTran = {
						SiteKey: detail.SiteKey ? detail.SiteKey : null,
						StartDate: startDate,
						EndDate: endDate
					};
				}

				$scope.definedFilter = angular.copy(definedFilter);

				if (vm.GridDataType === 1) {
					var exception = Enumerable.From($scope.definedFilter).Where(function (x) { return x.fieldName === "ExceptionTypes"; }).FirstOrDefault();
					exception.filter = [AppDefine.CannedReportType.Refund];
				}

				angular.forEach($scope.definedFilter, function (value) { if (value.fieldName === 'EmployeeName') { value.checked = true; } }); //ThangPham, show Employee Name column for Transaction list on Dashboard.

				vm.showDetail = detail;
				$rootScope.$broadcast('reloadTransactionViewer', { param: vm.paramShowTran, calbackFn: vm.closeDetail, definedFilter: $scope.definedFilter, options: $scope.optionsShow });
			}

			vm.showIOPCInfo = function (detail, page) {
				vm.showDetail = detail;
				var startDate = null;
				var endDate = null;
				if (detail.hasOwnProperty("Year") && detail.hasOwnProperty("Month") && detail.hasOwnProperty("Day")) {
					startDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 0, 0, 0, 0);
					endDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 23, 59, 59, 99);
				}
				else {//if (detail.hasOwnProperty("Date")) {
					$scope.curDate = detail.Date;
					var date = chartSvc.GetUTCDate(detail.Date);
					startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0, 0);
					endDate = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 23, 59, 59, 99);
				}
				var arrSiteKey = [];
				if (detail.SiteKey)
					arrSiteKey.push();
				else
					arrSiteKey = $scope.selectedSites;
				var pageNo = page;
				if (!pageNo || pageNo <= 0) pageNo = 1;
				var param = {
					SiteKeys: arrSiteKey,
					StartTranDate: Utils.toUTCDate(startDate),
					EndTranDate: Utils.toUTCDate(endDate),
					PageNo: pageNo,
					PageSize: pagesize,
				};
				vm.iopcData = null;
				if (vm.GridDataInfoType === 1) {
					rebarDataSvc.getIOPCCustomer(param, function (data) {
						updateServerDate(data);
						vm.iopcData = data;
					}, function (error) {
					});
				} else {
					rebarDataSvc.getIOPCCar(param, function (data) {
						updateServerDate(data);
						vm.iopcData = data;
					}, function (error) {
					});
				}
			}

			vm.TreeSiteClose = function () {
				var checkedIDs = [];
				chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
				vm.selectedSites = checkedIDs;
				$scope.selectedSites = vm.selectedSites;
				if ($("#btn-popMenuConvSites").parent().hasClass("open")) {
					$("#btn-popMenuConvSites").parent().removeClass("open");
					$("#btn-popMenuConvSites").prop("aria-expanded", false);
				}
			}

			vm.reloadMaxException = function () {
				loadSumaryData();
			};

			vm.reloadEmployerTran = function () {
				vm.currPage = 1;
				loadData(vm.viewByEmployee, vm.currPage, $scope.selectedSites);
			};

			vm.reloadEmployerSumTable = function () {
				vm.currPage = 1;
				loadData(vm.viewByEmployee, vm.currPage, $scope.selectedSites);
			};

			vm.showEmployeeTable = function () {
			    setCurrentSelect(AppDefine.Resx.RISK_FACTOR);

				vm.GridDataType = 0;
				vm.GridDataInfoType = 0;

				vm.employeedatatableProperty.Collapse = false;
				vm.showDetail = null;
				$scope.state = 0;
				vm.currPage = 1;
				$scope.selectedSites = vm.selectedSites;
				loadData(vm.viewByEmployee, 1, $scope.selectedSites);
			};

			vm.nextPage = function (data) {
				var totalPage = vm.data.TotalPages;
				if ($scope.state === DETAIL_STATE) totalPage = vm.iopcData.TotalPages;

				vm.currPage = cmsBase.RebarModule.Next(data, vm.currPage, totalPage, function (currentPage) {
					loadData(vm.viewByEmployee, currentPage, $scope.selectedSites);
				});
			}

			vm.gotoPage = function () {
				var totalPage = vm.data.TotalPages;
				if ($scope.state === DETAIL_STATE) totalPage = vm.iopcData.TotalPages;
				vm.currPage = cmsBase.RebarModule.Goto(vm.currPage, totalPage, function (currentPage) {
					loadData(vm.viewByEmployee, currentPage, $scope.selectedSites);
				});
			}

			vm.prevPage = function (data) {
				var totalPage = vm.data.TotalPages;
				if ($scope.state === DETAIL_STATE) totalPage = vm.iopcData.TotalPages;
				vm.currPage = cmsBase.RebarModule.Prev(data, vm.currPage, totalPage, function (currentPage) {
					loadData(vm.viewByEmployee, currentPage, $scope.selectedSites);
				});
			}

			active();

			function updateServerDate(data) {
				if (data && data.Data) {
					data.Data.forEach(function (item) {
						if (item) {
							item.StartDate = chartSvc.GetUTCDate(item.StartDate);
							item.EndDate = chartSvc.GetUTCDate(item.EndDate);
							item.DVRDate = chartSvc.GetUTCDate(item.DVRDate);
						}
					});
				}
			}

			function selectedFn() { }

			function active() {
				$scope.states = vm.viewByEmployee ? { Employee: 0, Date: 1, Transact: 2 } : { Site: 0, Employee: 1, Date: 2, Transact: 3 };
				$timeout(function () { getAllRegionSites(); }, 300);
			}

			function getAllRegionSites() {
				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
					$scope.data = $scope.$parent.$parent.treeSiteFilter;
					vm.rebarTree = $scope.data;
					bamhelperSvc.checkallNode(vm.rebarTree);
					var checkedIDs = [];
					chartSvc.GetSiteSelectedIDs(checkedIDs, vm.rebarTree.Sites);
					$scope.selectedSites = checkedIDs;
					vm.selectedSites = checkedIDs;
					vm.siteloaded = true;
					loadSumaryData();
					loadData(vm.viewByEmployee, vm.currPage, $scope.selectedSites);

				}
				else
				    $timeout(function () { getAllRegionSites(); }, 300);
			}


			//$scope.states = 
			$scope.state = 0;
			$scope.previousData = [];

			$scope.convertDate = function (sum) {
				if (sum.hasOwnProperty("Year") && sum.hasOwnProperty("Month") && sum.hasOwnProperty("Day")) {
					return $filter('date')(new Date(sum.Year, parseInt(sum.Month) - 1, sum.Day), "MM/dd/yyyy");
				}
				else if (sum.hasOwnProperty("Date")) {
					var date = chartSvc.GetUTCDate(sum.Date);
					return $filter('date')(date, "MM/dd/yyyy");
				}
				//else {
				//	return $filter('date')(new Date(sum.Year, parseInt(sum.Month) - 1, sum.Day), "MM/dd/yyyy");
				//}
			}

			$scope.GetBackData = function () {
			    if ($scope.state == $scope.states.Site) { return; }
			    var backState = $scope.state - 1;
				if (vm.GridDataInfoType === 1 || vm.GridDataInfoType === 2) {
					if (backState === $scope.states.Employee) {
						backState = $scope.states.Site;
					}
					if (vm.showDetail) vm.showDetail = null;
				}
				vm.data = $scope.previousData[backState].data;
				vm.currPage = $scope.previousData[backState].page;
				$scope.selectedSites = $scope.previousData[backState].selectedSites;
				$scope.currEmployee = $scope.previousData[backState].Employee;
				$scope.state = backState;//$scope.state - 1;

				switch ($scope.state) {
				    case $scope.states.Site:
				        setCurrentSelect($scope.currentSelect.TypeReport, undefined, undefined, undefined);
				        break;
				    case $scope.states.Employee:
				        setCurrentSelect($scope.currentSelect.TypeReport, $scope.currentSelect.Site, undefined);
				        break;
				    case $scope.states.Date:
				        setCurrentSelect($scope.currentSelect.TypeReport, $scope.currentSelect.Site, undefined, $scope.currentSelect.EmployerName);
				        break;
				}
			}

			$scope.currEmployee = null;
			$scope.ChangeState = function (_data) {
				vm.GridDataInfoType = 0;
				$scope.currEmployee = null;
				if ($scope.state == $scope.states.Site) {
				    setCurrentSelect(undefined, _data.SiteName);
				}
				switch ($scope.state) {
				    case $scope.states.Site:
				    case $scope.states.Employee:
				        setCurrentSelect(undefined, $scope.currentSelect.Site, undefined, _data.EmployerName ? _data.EmployerName : undefined);
						vm.startFilter = new Date($rootScope.rebarSearch.DateFrom);
						vm.startFilter = cmsBase.DateUtils.startOfDate(vm.startFilter);
						vm.endFilter = new Date($rootScope.rebarSearch.DateTo);
						vm.endFilter = cmsBase.DateUtils.endOfDate(vm.endFilter)
						var arr = [];
						arr.push(_data.SiteKey);

						var param = {
								Type: 0,
								SiteKeys: arr,
								StartTranDate: Utils.toUTCDate(vm.startFilter),// $filter('date')(vm.startFilter, AppDefine.CalDateTimeFormat),
								EndTranDate: Utils.toUTCDate(vm.endFilter),//$filter('date')(vm.endFilter, AppDefine.CalDateTimeFormat),
								PageNo: 1,
								PageSize: 10,
								Employees: _data.EmployerId,
								Sort: parseInt(vm.optionview),
								GroupBy: $scope.states.Employee == $scope.state ? $scope.GroupBy.DATE : $scope.GroupBy.EMPL
							};
						param.Type = 0;
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						$scope.selectedSites = arr;
						vm.currPage = 1;
						$scope.currEmployee = _data.EmployerId;

						if (vm.GridDataType === 0) {
							rebarDataSvc.getEmployeeRisks(param, function (data) {
								vm.data = data;
								// vm.totalRisk = data.SumRiskFactors;// Enumerable.From(data.Data).Sum("$.RiskFactor");
								//renderChart(vm.data);
								// vm.viewByEmployee = true;
								$scope.state = $scope.state + 1;
							}, function (error) {});
						}
						else {
							$scope.state = $scope.state + 1;
							loadTransWOCustData(vm.currPage, $scope.selectedSites);
						}
						break;

				    case $scope.states.Date:
				        var dateTitle = _data.Date;
				        if (_data.hasOwnProperty("Year") && _data.hasOwnProperty("Month") && _data.hasOwnProperty("Day")) {
				            var dateTitle = new Date(_data.Year, parseInt(_data.Month) - 1, _data.Day, 0, 0, 0, 0)
				        }
				        
				        setCurrentSelect(undefined, $scope.currentSelect.Site, dateTitle, $scope.currentSelect.EmployerName);
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						vm.showTransactionInfo(_data);
						$scope.state = $scope.state + 1;
						break;
				}
			}

			$scope.ChangeStateIOPC = function (_data) {
				$scope.currEmployee = null;
				switch ($scope.state) {
				    case $scope.states.Site:
				        setCurrentSelect(undefined, _data.SiteName);

						vm.startFilter = new Date($rootScope.rebarSearch.DateFrom);
						vm.endFilter = new Date($rootScope.rebarSearch.DateTo);
						vm.endFilter.setHours(23);
						vm.endFilter.setMinutes(59);
						vm.endFilter.setSeconds(59);
						var arr = [];
						arr.push(_data.SiteKey);

						var param = {
								Type: 0,
								SiteKeys: arr,
								StartTranDate: Utils.toUTCDate(vm.startFilter),// $filter('date')(vm.startFilter, AppDefine.CalDateTimeFormat),
								EndTranDate: Utils.toUTCDate(vm.endFilter),//$filter('date')(vm.endFilter, AppDefine.CalDateTimeFormat),
								PageNo: 1,
								PageSize: 10,
								Employees: _data.EmployerId,
								Sort: parseInt(vm.optionview),
								GroupBy: $scope.states.Employee == $scope.state ? $scope.GroupBy.DATE : $scope.GroupBy.EMPL
							};
						param.Type = 0;
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						$scope.selectedSites = arr;
						vm.currPage = 1;
						$scope.currEmployee = _data.EmployerId;

						$scope.state = $scope.states.Date;
						if (vm.GridDataInfoType === 1) {
							loadCustsWOTranData(vm.currPage, $scope.selectedSites);
						}
						else {
							loadCarsWOTranData(vm.currPage, $scope.selectedSites);
						}
						break;

				    case $scope.states.Date:
				        setCurrentSelect(undefined, $scope.currentSelect.Site, _data.Date);
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						vm.showIOPCInfo(_data, 1);
						$scope.state = $scope.state + 1;
						break;
				} //switch
			}

			function GroupDataBySiteKey(data, GroupBy) {
				switch (GroupBy) {
					case $scope.GroupBy.EMPL:
						var result = Enumerable.From(data).GroupBy(function (x) { return x.EmployerId; }).Select(function (g) {
							var rs = {
								PercentToSale: g.Average("$.PercentToSale"),
								RiskFactor: g.Sum("$.RiskFactor"),
								SiteKey: g.First().SiteKey,
								SiteName: g.First().SiteName,
								TotalAmmount: g.Sum("$.TotalAmmount"),
								TotalTran: g.Sum("$.TotalTran"),
								EmployerId: g.Key(),
								EmployerName: g.First().EmployerName,
								StoreName: g.First().StoreName,
								StoreId: g.First().StoreId
							};
							return rs;
						}).ToArray();
						return result;
						break;
					case $scope.GroupBy.SITE:
						var result = Enumerable.From(data).GroupBy(function (x) { return x.SiteKey; }).Select(function (g) {
							var rs = {
								PercentToSale: g.Average("$.PercentToSale"),
								RiskFactor: g.Sum("$.RiskFactor"),
								SiteKey: g.Key(),
								SiteName: g.First().SiteName,
								TotalAmmount: g.Sum("$.TotalAmmount"),
								TotalTran: g.Sum("$.TotalTran")
							};
							return rs;
						}).ToArray();
						return result;
					default:
						return null;
				}
			}

			function loadSumaryData() {
				vm.startFilter = cmsBase.DateUtils.startOfDate(new Date($rootScope.rebarSearch.DateFrom));
				vm.endFilter = cmsBase.DateUtils.endOfDate(new Date($rootScope.rebarSearch.DateTo));

				var param = {
					Type: 0,
					SiteKeys:vm.selectedSites,
					StartTranDate: Utils.toUTCDate(vm.startFilter),
					EndTranDate: Utils.toUTCDate(vm.endFilter),
					PageNo: 0,
					PageSize: 0
				}
				rebarDataSvc.getWeekAtGlanceSummary(param, function (data) {
					vm.dataStatis = data;
					vm.dataStatis = Enumerable.From(data).Where(function (x) { return vm.arrSmartExNames.indexOf(x.Name) >= 0;/*x.Name === 'REFUND' || x.Name === 'CUSTOMER_WOT' || x.Name === 'CAR_WOT';*/ }).ToArray();
					renderSummaryChart(vm.dataStatis);
				}, function (error) {

				});
			}

			function buildParam(page, selectedSites) {
				vm.startFilter = cmsBase.DateUtils.startOfDate(new Date($rootScope.rebarSearch.DateFrom));
				vm.endFilter = cmsBase.DateUtils.endOfDate(new Date($rootScope.rebarSearch.DateTo));
				var param = {
					Type: 0,
					SiteKeys: selectedSites,
					StartTranDate: Utils.toUTCDate(vm.startFilter),//vm.startFilter,
					EndTranDate: Utils.toUTCDate(vm.endFilter),//vm.endFilter,
					PageNo: page,
					PageSize: pagesize,
					Sort: parseInt(vm.optionview)

				}
				return param;
			}

			function loadData(viewByEmployee, page, selectedSites) {
				//Anh, Add for Smart Exception part
				if (vm.GridDataType === 1) {
					loadSmartExData(page, selectedSites);
					return;
				}

			    // 2016-06-23 Tri Add: When choice one Site set group by Employee.
			    if (selectedSites.length === 1 && viewByEmployee === false) {
			        vm.viewByEmployee = true;
			        return;
			    }
				var param = buildParam(page, selectedSites);
				switch ($scope.state) {
					case $scope.states.Date:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.DATE;
						param.Employees = $scope.currEmployee;
						rebarDataSvc.getEmployeeRisks(param, function (data) {
							vm.data = data;
							vm.totalRisk = data.SumRiskFactors;
						}, function (error) {});
						break;
					case $scope.states.Employee:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.EMPL;
						rebarDataSvc.getEmployeeRisks(param, function (data) {
							vm.data = data;
						if(data.Data!=null && data.Data!=undefined)
							vm.data.Data = GroupDataBySiteKey(data.Data, $scope.GroupBy.EMPL);
							vm.totalRisk = data.SumRiskFactors;// Enumerable.From(data.Data).Sum("$.RiskFactor");
							if (vm.viewByEmployee)
								renderChart(vm.data);
						}, function (error) {
						});
						break;
					case $scope.states.Site:
					default:
						param.Type = 1;
						param.GroupBy = $scope.GroupBy.SITE;
						rebarDataSvc.getSitesRisks(param, function (data) {
							vm.data = data;
							if (data.Data != null && data.Data != undefined)
								vm.data.Data = GroupDataBySiteKey(data.Data, $scope.GroupBy.SITE);
							vm.totalRisk = data.SumRiskFactors;//Enumerable.From(data.Data).Sum("$.RiskFactor");
							renderChart(vm.data);
						}, function (error) {
							//console.log(error);
						});
						break;
				}

			}

			function buildTooltip(data) {
				var str = "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div></div>";
				if (data.hasOwnProperty('KDVRs') && data.Value > 0) { //&& data.hasOwnProperty('Channels')
					if (data.KDVRs && data.KDVRs.length > 0) {
						//var chan = data.ChannelNo + 1;
						//var strchan = chan.toString();
						//if (chan < 10) strchan = '0' + strchan;
						var strchan = '';
						var imgURL = AppDefine.Api.SiteFirstImage + "chs=" + strchan + "&kdvrs=" + data.KDVRs.toString();
						//strTooltip += '<li><img src="' + imgURL + '" alt="Image" style="width:120px;" /></li>';
						str = "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div><div class='tooltipImg'><img src='" + imgURL + "' /></div></div>";
					}
				}
				return str;
			}

			function renderSummaryChart(data) {
				var captionName = cmsBase.translateSvc.getTranslate("SMART_EXCEPTION");
				var No = cmsBase.translateSvc.getTranslate("NO");
				var Total = cmsBase.translateSvc.getTranslate("TOTAL");//, tooltext: buildTooltip()

				var datatotal = Enumerable.From(data).OrderBy(function (x) { return x.Id; }).Select("i=>{ Value: i.TotalTrans, KDVRs: i.KDVRs, Channels: i.Channels }").ToArray();
				datatotal.forEach(function (item) {
					if (item) {
						item.tooltext = buildTooltip(item);
					}
				});

				//Anh Huynh, Remove Amount data
				//var datatotalAmmunt = Enumerable.From(data).OrderBy(function (x) { return x.Id; }).Select("i=>{ Value: i.TotalAmmount, KDVRs: i.KDVRs, Channels: i.Channels }").ToArray();
				//datatotalAmmunt.forEach(function (item) {
				//	if (item) {
				//		item.tooltext = buildTooltip(item);
				//	}
				//});

				var category = Enumerable.From(data).OrderBy(function (x) { return x.Id; }).Select(function (x) {
					return { label: cmsBase.translateSvc.getTranslate(x.Name + x.Id) }
				}).ToArray();

				var chart = new FusionCharts({
					type: 'mscombidy2d',
					renderAt: 'chart-weekatglance-summary',
					width: '100%',
					height: '230',
					dataFormat: 'json',
					dataSource: {
						"chart": {
							// "caption": captionName,

							"alignCaptionWithCanvas": "0",
							"captionHorizontalPadding": "0",
							"captionOnTop": "1",
							"captionAlignment": "left",
							"plotFillAlpha": "80",

							//Cosmetics
							"paletteColors": "#0075c2,#666666",
							"baseFontColor": "#333333",
							"baseFont": "Helvetica Neue,Arial",
							"captionFontSize": "14",
							"subcaptionFontSize": "14",
							"subcaptionFontBold": "0",
							"showBorder": "0",
							"bgColor": "#ffffff",
							"showShadow": "0",
							"canvasBgColor": "#ffffff",
							"canvasBorderAlpha": "0",
							"divlineAlpha": "100",
							"divlineColor": "#999999",
							"divlineThickness": "1",
							"divLineIsDashed": "1",
							"divLineDashLen": "1",
							"divLineGapLen": "1",
							"usePlotGradientColor": "0",
							"showplotborder": "0",
							"valueFontColor": "#ffffff",
							"placeValuesInside": "1",
							"showHoverEffect": "1",
							"rotateValues": "1",
							"showXAxisLine": "1",
							"xAxisLineThickness": "1",
							"xAxisLineColor": "#999999",
							"showAlternateHGridColor": "0",
							"legendBgAlpha": "0",
							"legendBorderAlpha": "0",
							"legendShadow": "0",
							"legendItemFontSize": "10",
							"legendItemFontColor": "#666666",
							"theme": "fint",
							"toolTipBgColor": "#FFFFFF",
							"toolTipBgAlpha": '0'//,
							//"plottooltext": "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div></div>"
						},
						"categories": [
							{
								"category": category
							}
						],
						"dataset": [
							{
								"seriesname": No,
								"parentYAxis": "S",
								"renderAs": "column",
								"data": datatotal
							}//, //Anh Huynh, Remove Amount data
							//{
							//	"seriesname": Total,
							//	"data": datatotalAmmunt
							//}
						]
					},
					events: { "dataLabelClick": dataClickSmartExChart, "dataPlotClick": dataClickSmartExChart }
				});

				chart.render();
			}

			function dataClickSmartExChart(eventObj, dataObj) {
				//var dataItem = eventObj.sender.args.dataSource.categories[0].category[dataObj.dataIndex];
				//var dataNo = eventObj.sender.args.dataSource.dataset[0].data[dataObj.dataIndex];
				//if (dataNo) {
				//	var dataTotal = eventObj.sender.args.dataSource.dataset[1].data[dataObj.dataIndex];
				vm.viewByEmployee = false;
				var datIdx = dataObj.dataIndex;
				switch (datIdx) {
					case 0:
						setCurrentSelect(vm.arrSmartExNames[0]);
						showTransWOCustInfo();
						break;
					case 1:
						setCurrentSelect(vm.arrSmartExNames[2]);
						showCarsWOTranInfo();
						break;
					case 2:
						setCurrentSelect(vm.arrSmartExNames[1]);
						showCustsWOTranInfo();
						break;
				} //switch
				//} //if
			}

			vm.dataClickSmartExTable = function (exname) {
				vm.viewByEmployee = false;
				switch (exname) {
					case vm.arrSmartExNames[0]:
						setCurrentSelect(vm.arrSmartExNames[0]);
						showTransWOCustInfo();
						break;
					case vm.arrSmartExNames[1]:
						setCurrentSelect(vm.arrSmartExNames[1]);
						showCustsWOTranInfo();
						break;
					case vm.arrSmartExNames[2]:
						setCurrentSelect(vm.arrSmartExNames[2]);
						showCarsWOTranInfo();
						break;
				} //switch
			}
			//****************************  ****************************//
			function showTransWOCustInfo() {
				//vm.showDetail = false;
				//vm.currPage = 1;
				vm.GridDataType = 1; //
				vm.GridDataInfoType = $scope.GridInfoType.Transac;

				vm.employeedatatableProperty.Collapse = false;
				vm.showDetail = null;
				$scope.state = 0;
				vm.currPage = 1;
				$scope.selectedSites = vm.selectedSites;

				loadTransWOCustData(vm.currPage, $scope.selectedSites);
			}

			function loadTransWOCustData(page, selectedSites) {
				vm.data = null;
				var param = buildParam(page, selectedSites);
				switch ($scope.state) {
					case $scope.states.Date:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.DATE;
						param.Employees = $scope.currEmployee;
						rebarDataSvc.getTransWOCust(param, function (data) {
							vm.data = data;
							//renderChart(vm.data);
						}, function (error) {
						});
						break;
					case $scope.states.Employee:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.EMPL;
						rebarDataSvc.getTransWOCust(param, function (data) {
							vm.data = data;
						}, function (error) {
						});
						break;
					case $scope.states.Site:
					default:
						param.Type = 1;
						param.GroupBy = $scope.GroupBy.SITE;
						rebarDataSvc.getTransWOCust(param, function (data) {
								vm.data = data;
							}, function (error) {
								console.log(error);
							});
						break;
				}
			}

			function showCarsWOTranInfo() {
				vm.GridDataType = 1; //
				vm.GridDataInfoType = $scope.GridInfoType.Car;

				vm.employeedatatableProperty.Collapse = false;
				vm.showDetail = null;
				$scope.state = 0;
				vm.currPage = 1;
				$scope.selectedSites = vm.selectedSites;

				loadCarsWOTranData(vm.currPage, $scope.selectedSites);
			}

			function loadCarsWOTranData(page, selectedSites) {
				vm.data = null;
				var param = buildParam(page, selectedSites);
				switch ($scope.state) {
					case $scope.states.Date:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.DATE;
						param.Employees = $scope.currEmployee;
						rebarDataSvc.getCarsWOTran(param, function (data) {
							vm.data = data;
						}, function (error) {
						});
						break;
					case $scope.states.Site:
					default:
						param.Type = 1;
						param.GroupBy = $scope.GroupBy.SITE;
						rebarDataSvc.getCarsWOTran(param, function (data) {
							vm.data = data;
						}, function (error) {
							console.log(error);
						});
						break;
				}
			}

			function showCustsWOTranInfo() {
				vm.GridDataType = 1; //
				vm.GridDataInfoType = $scope.GridInfoType.Customer;

				vm.employeedatatableProperty.Collapse = false;
				vm.showDetail = null;
				$scope.state = 0;
				vm.currPage = 1;
				$scope.selectedSites = vm.selectedSites;

				loadCustsWOTranData(vm.currPage, $scope.selectedSites);
			}

			function loadCustsWOTranData(page, selectedSites) {
				vm.data = null;
				var param = buildParam(page, selectedSites);
				switch ($scope.state) {
					case $scope.states.Date:
						param.Type = 0;
						param.GroupBy = $scope.GroupBy.DATE;
						param.Employees = $scope.currEmployee;
						rebarDataSvc.getCustsWOTran(param, function (data) {
							vm.data = data;
							//renderChart(vm.data);
						}, function (error) {
						});
						break;
					case $scope.states.Site:
					default:
						param.Type = 1;
						param.GroupBy = $scope.GroupBy.SITE;
						rebarDataSvc.getCustsWOTran(param, function (data) {
							vm.data = data;
						}, function (error) {
							console.log(error);
						});
						break;
				}
			}

			function loadSmartExData(page, selectedSites) {
				if ($scope.state === DETAIL_STATE) {
					if ($scope.curDate) {
						var _data = {};
						_data.Date = $scope.curDate;
						vm.showIOPCInfo(_data, page);
					}
					return;
				}
				switch (vm.GridDataInfoType) {
					case $scope.GridInfoType.Customer:
						loadCustsWOTranData(page, selectedSites);
						break;
					case $scope.GridInfoType.Car:
						loadCarsWOTranData(page, selectedSites);
						break;
					case $scope.GridInfoType.Transac:
					default:
						loadTransWOCustData(page, selectedSites);
						break;
				}
			}
			//**************************************************************************//

			function renderChart(data) {
				var tooltipDataName = cmsBase.translateSvc.getTranslate("RISK_FACTOR");
				var collectName = "$.RiskFactor";
				var subfix = "";
				var prefix = "";
				switch (vm.optionview) {
					case "1":
						{
							tooltipDataName = cmsBase.translateSvc.getTranslate("RISK_FACTOR");
							collectName = "$.RiskFactor";
							break;
						}
					case "2":
						{
							tooltipDataName = cmsBase.translateSvc.getTranslate("TOTAL_AMOUNT");
							collectName = "$.TotalAmmount";
							prefix = "$";
							break;
						}
					case "3":
						{
							tooltipDataName = cmsBase.translateSvc.getTranslate("RATIO_TO_SALE");
							collectName = "$.PercentToSale";
							subfix = "%";
							break;
						}
				}
				var type = 0;
				if (vm.viewByEmployee === true) {
					type = 0;
				} else {
					type = 1;
				}

				var average = Enumerable.From(data.Data).Average(collectName);
				var categoryemploy = Enumerable.From(data.Data).Select(function (x) {
					return {
						label: type === 0 ? x.EmployerName + "(" + x.SiteName + ")" : x.SiteName,
						data: x
					}
				}).ToArray();
				var dataseries = Enumerable.From(data.Data).Select("$=>{Value:" + collectName + "}").ToArray();
				var averageTooltip = average;

				var chartSum = new FusionCharts({
					type: 'mscolumn2d',
					renderAt: 'chart-container',
					width: '100%',
					height: '260',
					dataFormat: 'json',
					dataSource: {
						"chart": {
							//"caption": "Comparison of Quarterly Revenue",
							//"xAxisname": "Quarter",
							//"yAxisName": "Revenues (In USD)",
							"numberSuffix": subfix,
							"numberPrefix": prefix,
							"plotFillAlpha": "80",
							"decimals": "2",
							//Cosmetics
							"paletteColors": "#FECB32",
							"baseFontColor": "#333333",
							"baseFont": "Helvetica Neue,Arial",
							"captionFontSize": "14",
							"subcaptionFontSize": "14",
							"subcaptionFontBold": "0",
							"showBorder": "0",
							"bgColor": "#ffffff",
							"showShadow": "0",
							"canvasBgColor": "#ffffff",
							"canvasBorderAlpha": "0",
							"divlineAlpha": "100",
							"divlineColor": "#999999",
							"divlineThickness": "1",
							"divLineIsDashed": "1",
							"divLineDashLen": "1",
							"divLineGapLen": "1",
							"usePlotGradientColor": "0",
							"showplotborder": "0",
							"valueFontColor": "#000",
							"placeValuesInside": "1",
							"showHoverEffect": "1",
							"rotateValues": "0",
							"showXAxisLine": "1",
							"xAxisLineThickness": "1",
							"xAxisLineColor": "#999999",
							"showAlternateHGridColor": "0",
							"legendBgAlpha": "0",
							"legendBorderAlpha": "0",
							"legendShadow": "0",
							"legendItemFontSize": "10",
							"legendItemFontColor": "#666666",
							"theme": "fint",
							"toolTipBgColor": "#FFFFFF",
							"toolTipBgAlpha": '0',
							"plottooltext": "<div class='chart-tooltip'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>" + tooltipDataName + ": $value</div></div>"
						},
						"categories": [
                            {
                            	"category": categoryemploy
                            }
						],
						"dataset": [
                            {
                            	"data": dataseries
                            }
						],
						"trendlines": [
                            {
                            	"line": [
                                    {
                                    	"startvalue": average,
                                    	"color": "#426d8f",//"#87A9C2",
                                    	//"displayvalue": "Previous{br}Average",
                                    	"valueOnRight": "1",
                                    	"thickness": "2",
                                    	"showBelow": "0",
                                    	"tooltext": averageTooltip
                                    }
                            	]
                            }
						]
					},
					events: { "dataLabelClick": dataClickPaymentChart, "dataPlotClick": dataClickPaymentChart }
				});

				chartSum.render();
			}

			function dataClickPaymentChart(eventObj, dataObj) {
			    setCurrentSelect(AppDefine.Resx.RISK_FACTOR);
				vm.GridDataType = 0;
				vm.GridDataInfoType = 0;
				var dataItem = eventObj.sender.args.dataSource.categories[0].category[dataObj.dataIndex];
				if (dataItem) {
					//switch ($scope.state)
					//{
					//    case $scope.states.Employee:
					//    case $scope.states.Site:
					//    case $scope.states.Transact:
					//}
					$scope.state = 0;
					var temp = null;
					if ($scope.previousData[0] != null && $scope.previousData[0] != undefined) {
						temp = $scope.previousData[0].data
					}
					vm.employeedatatableProperty.Collapse = false;
					vm.showDetail = false;
					$scope.ChangeState(dataItem.data);
					if (temp != null && temp != undefined)
						$scope.previousData[0].data = temp;
					//vm.showTransactionInfo(dataItem.data); 
				}

			}

			function setCurrentSelect(TypeReport, siteName, date, employerName) {
				if (TypeReport) {
					$scope.currentSelect.TypeReport = TypeReport;
				}

				if (siteName) {
					$scope.currentSelect.Site = siteName;
				} else {
					$scope.currentSelect.Site = undefined;
				}

				if (employerName) {
					$scope.currentSelect.EmployerName = employerName;
				} else {
					$scope.currentSelect.EmployerName = undefined;
				}

				if (date) {
					$scope.currentSelect.DateTime = chartSvc.GetUTCDate(date);
				} else {
					$scope.currentSelect.DateTime = undefined;
				}
			}

			function resetCurrentSelect() {
				$scope.currentSelect = {
					TypeReport: undefined,
					Site: undefined,
					DateTime: undefined,
					EmployerName: undefined
				}
			}

			$scope.showVPCFn = function (param) {
				var data = {
					PacId: param.PACID,
					CamName: param.ExternalCamera,
					DvrDate: Utils.toUTCDate(param.StartDate)
				}
				SiteSvc.GetDVRInfoRebarTransact(AppDefine.TypeTimeLines.Ten_minute, data, cmsBase.GetDVRInfoSuccess, function (errr) {
				});
			}

			/** EXPORT DATA - FUNCTIONS, BEGIN **/

			function buildTableRiskFactor(dtSource, options) {
				var startCol = options.ColIndex;
				var startRow = options.RowIndex;
				var colIndex = startCol;

				var table = {
					Name: 'RiskFactorTable',
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};
				var rowData = {};
				var colData = {};
				rowData = {
					Type: AppDefine.tableExport.Body,
					ColDatas: []
				};
				colData = {
					Value: dtSource + "\n " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL_RISKFACTOR),
					Color: AppDefine.ExportColors.RiskFactorNumberCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 11, Rows: 2 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				startRow = startRow + colData.MergeCells.Rows;
				table.RowDatas.push(rowData);

				rowData = {
					Type: AppDefine.tableExport.Body,
					ColDatas: []
				};
				var value = cmsBase.translateSvc.getTranslate(AppDefine.Resx.GROUP_BY_STRING);
				if (vm.viewByEmployee) {
					value += cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMPLOYEE_STRING);
				}
				else {
					value += cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITE_NAME_STRING);
				}
				colData = {
					Value: value,
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 11, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				startRow = startRow + colData.MergeCells.Rows;
				table.RowDatas.push(rowData);

				rowData = {
					Type: AppDefine.tableExport.Body,
					ColDatas: []
				};
				value = "Data view: ";
				switch (vm.optionview) {
					case 1:
						value = value + cmsBase.translateSvc.getTranslate(AppDefine.Resx.RISK_FACTOR);
						break;
					case 2:
						value = value + cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL_AMOUNT);
						break;
					case 3:
						value = value + cmsBase.translateSvc.getTranslate(AppDefine.Resx.RATIO_TO_SALE);
						break;
				}
				colData = {
					Value: value,
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 11, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				table.RowDatas.push(rowData);

				return table;
			}

			function buildTableRiskFactorDetail(dtSource, options) {
				var startCol = options.ColIndex;
				var startRow = options.RowIndex;
				var colIndex = startCol;

				var table = {
					Name: 'RiskFactorTableDetail',
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};
				var rowData = {};
				var colData = {};


			    if ($scope.state === $scope.states.Transact) {
			        var headers = [], colIndex = startCol;
			        var rowIndex = startRow;

			        for (var name in vm.TransactionReport.DataResult[0]) {
			            
			            headers.push({
			                Value: cmsBase.translateSvc.getTranslate(name), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: false,
			                Width: 0,
			                MergeCells: { Cells: 3, Rows: 1 }
			            });
			            colIndex += 3;
			        }

			        table.RowDatas.push({
			            Type: AppDefine.tableExport.Header,
			            ColDatas: headers,
			            GridModels: null
			        });

			        vm.TransactionReport.DataResult.forEach(function (r) {
			            var row = {
			                Type: AppDefine.tableExport.Body,
			                ColDatas: [],
			                GridModels: null
			            }

			            colIndex = startCol;
			            rowIndex += 1;
			            var cols = [];
			            for (var name in vm.TransactionReport.DataResult[0]) {
			                
			                if (name === 'Payments' || name === 'Taxs' || name === 'ExceptionTypes' || name === 'Notes') {
			                    var pay = Enumerable.From(r[name]).Select(function(f) { return f.Name; }).ToArray().join();
			                    cols.push({ Value: pay ? pay : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: false, Width: 0,MergeCells: { Cells: 3, Rows: 1 }});
			                } else {
			                    if (name === 'TranDate' || name === 'DvrDate') {
			                        cols.push({ Value: r[name] ? $filter('date')(r[name], "MM/dd/yyyy HH:mm:ss") : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: false, Width: 0, MergeCells: { Cells: 3, Rows: 1 } });
			                    } else {
			                        cols.push({ Value: r[name] ? r[name] : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: false, Width: 0, MergeCells: { Cells: 3, Rows: 1 } });
			                    }
			                }
			                colIndex += 3;
			            }
			            row.ColDatas = cols;
			            table.RowDatas.push(row);
			        });
			        return table;
			    }

			    //Header, begin
				var value = "";
				switch (options.DataType) {
					case $scope.states.Site:
						value = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SITE_NAME_STRING);
						break;
					case $scope.states.Employee:
						value = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMPLOYEE_NAME);
						break;
					case $scope.states.Date:
						value = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DATE);
						break;
				}
				rowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				colData = {
					Value: value,
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 4, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colIndex + colData.MergeCells.Cells;

				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.RISK_FACTOR),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colIndex + colData.MergeCells.Cells;
				
				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL_AMOUNT),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colIndex + colData.MergeCells.Cells;

				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL_TRANSACTION),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colIndex + colData.MergeCells.Cells;

				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.RATIO_TO_SALE),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 3, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colIndex + colData.MergeCells.Cells;

				table.RowDatas.push(rowData);
				//Header, end

				//Body, start
				startRow++;
				angular.forEach(dtSource, function (item, index) {
					colIndex = 1;
					rowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};
					var value = "";
					switch (options.DataType) {
						case $scope.states.Site:
							value = item.SiteName;
							break;
						case $scope.states.Employee:
							value = item.EmployerName + " " + item.SiteName;
							break;
						case $scope.states.Date:
							var date = new Date(item.Year, item.Month -1, item.Day);
							value = $filter('date')(date, AppDefine.ParamDateFormat);
							break;
					}
					colData = {
						Value: value,
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 4, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colIndex + colData.MergeCells.Cells;

					colData = {
						Value: item.RiskFactor,
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colIndex + colData.MergeCells.Cells;

					colData = {
						Value: "$" + item.TotalAmmount,
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colIndex + colData.MergeCells.Cells;

					colData = {
						Value: item.TotalTran,
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colIndex + colData.MergeCells.Cells;

					colData = {
						Value: $filter('salenumber')(item.PercentToSale, '0,0.00') + "%",
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 3, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colIndex + colData.MergeCells.Cells;

					table.RowDatas.push(rowData);
					startRow++;
				});
				//Body, end

				return table;
			}

			function buildTableMarkException(dtSource, options) {
				//console.log(dtSource);
				var startCol = 1;
				var startRow = options.RowIndex;
				var rows = [];
				var colIndex = 1;

				var table = {
					Name: 'MarkExceptionTable',
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};

				var rowData = {};
				var colData = {};

				//Row Header
				rowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				colData = {
					Value: "",
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 4, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colData.MergeCells.Cells + colIndex;

				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.NO),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 2, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);
				colIndex = colData.MergeCells.Cells + colIndex;

				colData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.TOTAL),
					Color: AppDefine.ExportColors.GridHeaderCell,
					CustomerWidth: false,
					Width: 0,
					MergeCells: { Cells: 2, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				rowData.ColDatas.push(colData);

				table.RowDatas.push(rowData);

				//Rows Body
				startRow++;
				angular.forEach(dtSource, function (item, index) {
					colIndex = 1;
					rowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};

					colData = {
						Value: cmsBase.translateSvc.getTranslate(item.Name+item.Id),
						Color: AppDefine.ExportColors.GridSumCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 4, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colData.MergeCells.Cells + colIndex;

					colData = {
						Value: item.TotalTrans,
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colData.MergeCells.Cells + colIndex;

					colData = {
						Value: (item.Name !== 'REFUND' && item.TotalAmmount === 0) ? 'N/A' : $filter('salenumber')(item.TotalAmmount, '0,0.00'),
						Color: AppDefine.ExportColors.GridNormalCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 2, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					rowData.ColDatas.push(colData);
					colIndex = colData.MergeCells.Cells + colIndex;

					table.RowDatas.push(rowData);
					startRow++;
				});

				return table;
			}

			function buildChartMarkException(dtSource, options) {
				var chartData = {
					Name: "ChartMartException",
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.SMART_EXCEPTION),
					ChartType: AppDefine.chartExportType.ColumnChart,
					Format: {
						Width: options.Width,
						Height: options.Height,
						ColIndex: options.ColIndex,
						RowIndex: options.RowIndex
					}
				};
				var chartDataItem = {};
				angular.forEach(dtSource, function (item) {
					chartDataItem = {
						Name: cmsBase.translateSvc.getTranslate(item.Name + item.Id),
						Value: item.TotalTrans,
						Color: AppDefine.ChartExportColor.Blue,
					};
					chartData.ChartDataItems.push(chartDataItem);
				});
				return chartData;
			}

			function buildChartRiskFactor(dtSource, options) {
				//console.log(dtSource);
				var chartData = {
					Name: "ChartRiskFactor",
					ChartDataItems: [],
					Title: cmsBase.translateSvc.getTranslate(AppDefine.Resx.RISK_FACTOR),
					ChartType: AppDefine.chartExportType.ColumnChart,
					Format: {
						Width: options.Width,
						Height: options.Height,
						ColIndex: options.ColIndex,
						RowIndex: options.RowIndex
					}
				};
				var chartDataItem = {};

				angular.forEach(dtSource, function (item) {
					var value = "";
					switch (vm.optionview) {
						case "1":
							value = item.RiskFactor;
							break;
						case "2":
							value = item.TotalAmmount;
							break;
						case "3":
							value = item.PercentToSale;
							break;
					}

					chartDataItem = {
						Name: item.SiteName,
						Value: value,
						Color: AppDefine.ChartExportColor.Orange,
					};
					chartData.ChartDataItems.push(chartDataItem);
				});
				return chartData;
			}

			function loadSumaryDataToExport() {
				var def = cmsBase.$q.defer();

				vm.startFilter = cmsBase.DateUtils.startOfDate(new Date($rootScope.rebarSearch.DateFrom));
				vm.endFilter = cmsBase.DateUtils.endOfDate(new Date($rootScope.rebarSearch.DateTo));

				var param = {
					Type: 0,
					SiteKeys: vm.selectedSites,
					StartTranDate: Utils.toUTCDate(vm.startFilter),
					EndTranDate: Utils.toUTCDate(vm.endFilter),
					PageNo: 0,
					PageSize: 0
			    }
				rebarDataSvc.getWeekAtGlanceSummary(param, function (data) {
					vm.dataStatisExport = Enumerable.From(data).Where(function (x) { return vm.arrSmartExNames.indexOf(x.Name) >= 0; }).ToArray();
					//renderSummaryChart(vm.dataStatis);
					var currentPage = 1;
					loadDataToExport(vm.viewByEmployee, currentPage, vm.selectedSites).then(function() {
					    def.resolve();
					}, function() {
					    def.reject();
					});
					
				}, function (error) { });


				return def.promise;
			    }

		    function loadDataToExport(viewByEmployee, page, selectedSites) {
		        var def = cmsBase.$q.defer();
		        if (selectedSites.length === 1 && viewByEmployee === false) {
		            vm.viewByEmployee = true;
		            return;
		        }

		        vm.dataExport = {};
		        var param = buildParam(page, selectedSites);
		        param.PageSize = 100000; //get all data to export
		        switch ($scope.state) {
		        case $scope.states.Transact:
		            if (vm.paramShowTran.QueryResult) {
		                delete vm.paramShowTran.QueryResult['$skip'];
		                delete vm.paramShowTran.QueryResult['$top'];
		            }
		            rebarDataSvc.getTransactionViewer(vm.paramShowTran.QueryResult, function (data) {
		                vm.TransactionReport = data;
		                vm.totalRiskExport = vm.totalRisk;
		                def.resolve();
		            }, function (error) {

		            });
		            break;
		        case $scope.states.Date:
		            param.Type = 0;
		            param.GroupBy = $scope.GroupBy.DATE;
		            param.Employees = $scope.currEmployee;
		            rebarDataSvc.getEmployeeRisks(param, function(data) {
		                vm.dataExport = data;
		                vm.dataExport.Data = data.Data;
		                vm.totalRiskExport = data.SumRiskFactors;
		                def.resolve();
		            }, function(error) {});
		            break;
		        case $scope.states.Employee:
		            param.Type = 0;
		            param.GroupBy = $scope.GroupBy.EMPL;
		            rebarDataSvc.getEmployeeRisks(param, function(data) {
		                vm.dataExport = data;
		                if (data.Data != null && data.Data != undefined) {
		                    vm.dataExport.Data = GroupDataBySiteKey(data.Data, $scope.GroupBy.EMPL);
		                }
		                vm.totalRiskExport = data.SumRiskFactors;
		                //if (vm.viewByEmployee) {
		                //	renderChart(vm.dataExport);
		                //}
		                def.resolve();
		            }, function(error) {});
		            break;
		        case $scope.states.Site:
		        default:
		            param.Type = 1;
		            param.GroupBy = $scope.GroupBy.SITE;
		            rebarDataSvc.getSitesRisks(param, function(data) {
		                vm.dataExport = data;
		                if (data.Data != null && data.Data != undefined) {
		                    vm.dataExport.Data = GroupDataBySiteKey(data.Data, $scope.GroupBy.SITE);
		                }
		                vm.totalRiskExport = data.SumRiskFactors;
		                //renderChart(vm.dataExport);
		                def.resolve();
		            }, function(error) {});
		            break;
		        }

		        return def.promise;
		    }

		    /** EXPORT DATA - FUNCTIONS, END **/
		}
	});
})();