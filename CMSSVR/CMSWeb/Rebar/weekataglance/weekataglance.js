(function () {
	'use strict';

	define([
        'cms',
        'widgets/rebar/rebar-transact-viewer',
        'widgets/rebar/transactiondetail'
	], function (cms) {
		cms.register.controller('rebarweekataglanceCtrl', rebarweekataglanceCtrl);
		rebarweekataglanceCtrl.$inject = ['$timeout', '$rootScope', '$scope', 'cmsBase', 'AppDefine', '$filter', 'chartSvc', 'bamhelperSvc', 'dataContext', 'rebarDataSvc', '$modal', 'Utils', 'filterSvc'];

		function rebarweekataglanceCtrl($timeout, $rootScope, $scope, cmsBase, AppDefine, $filter, chartSvc, bamhelperSvc, dataContext, rebarDataSvc, $modal, Utils, filterSvc) {
			var vm = this;
			vm.siteloaded = false;
			$scope.selectedSites = [];

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
			$scope.definedFilter = filterSvc.getTransactionColumn();
			$scope.definedFilter[12].checked = true; //default show Employee column

			$scope.$watch('vm.viewByEmployee', function (oldval, newval) {
				if (oldval !== newval) {
					vm.currPage = 1;
					if (!vm.rebarTree) return;
					vm.siteloaded = true;
					$scope.states = vm.viewByEmployee ? { Employee: 0, Date: 1, Transact: 2 } : { Site: 0, Employee: 1, Date: 2, Transact: 3 };
					$scope.state = 0;
					$scope.previousData = [];
					vm.showDetail = false;

					loadData(vm.viewByEmployee, vm.currPage, $scope.selectedSites);
				}
			});

			$scope.$watch('vm.optionview', function (oldval, newval) {
				if (oldval !== newval) {
					renderChart(vm.data);
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
				loadData(vm.viewByEmployee, vm.currPage, $scope.selectedSites);
			});

			$scope.$on(AppDefine.Events.PAGEREADY, function () {
				active();
			});

			$scope.optionsShow = {
				isBack: true,
				Max: false,
				Collapse: false
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
				var startDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 0, 0, 0, 0);
				var endDate = new Date(detail.Year, parseInt(detail.Month) - 1, detail.Day, 23, 59, 59, 99);
				if (detail.EmployerId) {
					var filter = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause('EmployeeId').eq(detail.EmployerId));
					// console.log(filter); return;
					vm.paramShowTran =
                    {
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

				$scope.definedFilter = filterSvc.getTransactionColumn();
				$scope.definedFilter[13].checked = true;

				vm.showDetail = detail;
				$rootScope.$broadcast('reloadTransactionViewer', { param: vm.paramShowTran, calbackFn: vm.closeDetail, definedFilter: $scope.definedFilter, options: $scope.optionsShow });
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
				vm.employeedatatableProperty.Collapse = false;
				vm.showDetail = null;
			};

			vm.nextPage = function (data) {
				vm.currPage = cmsBase.RebarModule.Next(data, vm.currPage, vm.data.TotalPages, function (currentPage) { loadData(vm.viewByEmployee, currentPage, $scope.selectedSites); });
			}
			vm.gotoPage = function () {
				vm.currPage = cmsBase.RebarModule.Goto(vm.currPage, vm.data.TotalPages, function (currentPage) { loadData(vm.viewByEmployee, currentPage, $scope.selectedSites); });
			}
			vm.prevPage = function (data) {
				vm.currPage = cmsBase.RebarModule.Prev(data, vm.currPage, vm.data.TotalPages, function (currentPage) { loadData(vm.viewByEmployee, currentPage, $scope.selectedSites); });
			}

			active();

			function selectedFn() { }

			function active() {
				getAllRegionSites();
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
			}

			$scope.states = vm.viewByEmployee ? { Employee: 0, Date: 1, Transact: 2 } : { Site: 0, Employee: 1, Date: 2, Transact: 3 };
			//$scope.states = 
			$scope.state = 0;
			$scope.previousData = [];
			$scope.convertDate = function (sum) {

				return $filter('date')(new Date(sum.Year, parseInt(sum.Month) - 1, sum.Day), "MM/dd/yyyy");
			}
			$scope.GetBackData = function () {
				vm.data = $scope.previousData[$scope.state - 1].data;
				vm.currPage = $scope.previousData[$scope.state - 1].page;
				$scope.selectedSites = $scope.previousData[$scope.state - 1].selectedSites;
				$scope.currEmployee = $scope.previousData[$scope.state - 1].Employee;
				$scope.state = $scope.state - 1;
			}

			$scope.currEmployee = null;
			$scope.ChangeState = function (_data) {
				$scope.currEmployee = null;
				switch ($scope.state) {

					case $scope.states.Employee:
					case $scope.states.Site:
						vm.startFilter = new Date($rootScope.rebarSearch.DateFrom);
						vm.endFilter = new Date($rootScope.rebarSearch.DateTo);
						vm.endFilter.setHours(23);
						vm.endFilter.setMinutes(59);
						vm.endFilter.setSeconds(59);
						var arr = [];
						arr.push(_data.SiteKey);

						var param =
                            {

                            	Type: 0,
                            	SiteKeys: arr,
                            	StartTranDate: Utils.toUTCDate(vm.startFilter),// $filter('date')(vm.startFilter, AppDefine.CalDateTimeFormat),
                            	EndTranDate: Utils.toUTCDate(vm.endFilter),//$filter('date')(vm.endFilter, AppDefine.CalDateTimeFormat),
                            	PageNo: 1,
                            	PageSize: 10,
                            	Employees: _data.EmployerId

                            }
						param.Type = 0;
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						$scope.selectedSites = arr;
						vm.currPage = 1;
						$scope.currEmployee = _data.EmployerId;
						rebarDataSvc.getEmployeeRisks(param, function (data) {
							vm.data = data;
							vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
							//renderChart(vm.data);
							// vm.viewByEmployee = true;
							$scope.state = $scope.state + 1;


						}, function (error) {

						});
						break;

					case $scope.states.Date:
						$scope.previousData[$scope.state] = { data: vm.data, page: vm.currPage, selectedSites: $scope.selectedSites, Employee: _data.EmployerId };
						vm.showTransactionInfo(_data)
						$scope.state = $scope.state + 1;
						break;

				}

			}

			function loadSumaryData() {

				vm.startFilter = new Date($rootScope.rebarSearch.DateFrom);
				vm.endFilter = new Date($rootScope.rebarSearch.DateTo);
				vm.endFilter.setHours(23);
				vm.endFilter.setMinutes(59);
				vm.endFilter.setSeconds(59);

				var param = {
					Type: 0,
					SiteKeys: $scope.selectedSites,
					StartTranDate: Utils.toUTCDate(vm.startFilter),// $filter('date')(vm.startFilter, AppDefine.CalDateTimeFormat),
					EndTranDate: Utils.toUTCDate(vm.endFilter),//$filter('date')(vm.endFilter, AppDefine.CalDateTimeFormat),
					PageNo: 0,
					PageSize: 0
				}
				rebarDataSvc.getWeekAtGlanceSummary(param, function (data) {
					vm.dataStatis = data;
					vm.dataStatis = Enumerable.From(data).Where(function (x) { return x.Name === 'REFUND' || x.Name === 'CUSTOMER_WOT' || x.Name === 'CAR_WOT'; }).ToArray();
					renderSummaryChart(vm.dataStatis);
				}, function (error) {

				});
			}

			function loadData(viewByEmployee, page, selectedSites) {
				vm.startFilter = new Date($rootScope.rebarSearch.DateFrom);
				vm.endFilter = new Date($rootScope.rebarSearch.DateTo);
				vm.endFilter.setHours(23);
				vm.endFilter.setMinutes(59);
				vm.endFilter.setSeconds(59);
				var param = {
					Type: 0,
					SiteKeys: selectedSites,
					StartTranDate: Utils.toUTCDate(vm.startFilter),//vm.startFilter,
					EndTranDate: Utils.toUTCDate(vm.endFilter),//vm.endFilter,
					PageNo: page,
					PageSize: pagesize
				}

				switch ($scope.state) {
					case $scope.states.Date:
						param.Type = 0;
						param.Employees = $scope.currEmployee;
						rebarDataSvc.getEmployeeRisks(param, function (data) {
							vm.data = data;
							vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
							//renderChart(vm.data);

						}, function (error) {

						});
						break;
					case $scope.states.Employee:
						param.Type = 0;
						rebarDataSvc.getEmployeeRisks(param, function (data) {
							vm.data = data;
							vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
							renderChart(vm.data);

						}, function (error) {

						});
						break;
					case $scope.states.Site:
						param.Type = 1;
						rebarDataSvc.getSitesRisks(param, function (data) {
							vm.data = data;
							vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
							renderChart(vm.data);
						},
                        function (error) {
                        	console.log(error);
                        });
						break;


				}
				//if (vm.viewByEmployee === true)
				//{
				//	param.Type = 0;
				//	rebarDataSvc.getEmployeeRisks(param, function (data) {
				//		vm.data = data;
				//		vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
				//		//renderChart(vm.data);

				//	}, function (error) {

				//	});
				//}
				//else
				//{
				//	param.Type = 1;
				//	rebarDataSvc.getSitesRisks(param,function (data) {
				//		vm.data = data;
				//		vm.totalRisk = Enumerable.From(data.Data).Sum("$.RiskFactor");
				//		//renderChart(vm.data);
				//	},
				//    function (error) {
				//         console.log(error);
				//	});
				//}
			}

			function renderSummaryChart(data) {
				var captionName = cmsBase.translateSvc.getTranslate("SMART_EXCEPTION");
				var No = cmsBase.translateSvc.getTranslate("NO");
				var Total = cmsBase.translateSvc.getTranslate("TOTAL");

				var datatotal = Enumerable.From(data).OrderBy(function (x) { return x.Id; }).Select("i=>{Value: i.TotalTrans}").ToArray();
				var datatotalAmmunt = Enumerable.From(data).OrderBy(function (x) { return x.Id; }).Select("i=>{Value: i.TotalAmmount}").ToArray();

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
							"legendItemFontColor": "#666666"
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
							},
							{
								"seriesname": Total,

								"data": datatotalAmmunt
							}
						]
					}
				});

				chart.render();
			}

			function renderChart(data) {
				var collectName = "$.RiskFactor";
				var subfix = "";
				var prefix = "";
				switch (vm.optionview) {
					case "1":
						{
							collectName = "$.RiskFactor";
							break;
						}
					case "2":
						{
							collectName = "$.TotalAmmount";
							prefix = "$";
							break;
						}
					case "3":
						{
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
							"legendItemFontColor": "#666666"
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
					vm.showDetail = false;
					$scope.ChangeState(dataItem.data);
					if (temp != null && temp != undefined)
						$scope.previousData[0].data = temp;
					//vm.showTransactionInfo(dataItem.data); 
				}

			}

		}
	});
})();