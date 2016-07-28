(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService', 'Scripts/Services/chartSvc', 'DataServices/Configuration/fiscalyearservice', 'widgets/charts/TrafficDetail'],
		function (cms) {
			cms.register.controller('trafficStatisticCtrl', trafficStatisticCtrl);
			trafficStatisticCtrl.$inject = ['$scope', 'cmsBase', 'AppDefine', 'ReportService', 'Utils', 'chartSvc', 'fiscalyearservice', '$filter'];
			function trafficStatisticCtrl($scope, cmsBase, AppDefine, rptService, utils, chartSvc, fiscalyearservice, $filter) {
				var vm = this;
				$scope.loading = false;
				var Options = JSON.parse( "{" + $scope.widgetModel.TemplateParams + "}" );
				$scope.Gui = ( Options == null || !Options.hasOwnProperty( "Gui" ) ) ? null : Options.Gui;
				$scope.Pram = ( Options == null || !Options.hasOwnProperty( "Pram" ) ) ? null : Options.Pram;
				$scope.Opt = DefaultOption();
				$scope.showToolTip = "1";
				if(jQuery.browser.mobile === true){
					$scope.showToolTip = "0";
				}

				//$scope.SelDate = {};
				//vm.Period = 0;
				$scope.ChartHeight = AppDefine.ChartHeight;
				$scope.ExportOptions = AppDefine.ChartExportOptions;
				vm.FromSelDate = new Date();
				vm.ToSelDate = new Date();

				function DefaultOption() {
					if ( !$scope.Pram || !$scope.Gui.Opts || $scope.Gui.length == 0 || !$scope.Pram.hasOwnProperty( "act" ) || $scope.Pram.act === null )
						return null;

					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}
				var exFileName = AppDefine.ExFName.Traffic + $filter('date')(vm.FromSelDate, AppDefine.DFFileName);
				$scope.chartTrafficEvents = angular.copy(chartSvc.createExportEvent('_traff', exFileName));//'TrafficStatistic'

				$scope.GetrptData = function ( opt ) {
					$scope.Opt = opt;
					if ($scope.Opt.label === 'CUSTOM') {
						$scope.SearchMode = true;
						var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
						if (delta > AppDefine.ChartTraffMaxDays) {
							var fromDate = chartSvc.dateAdds(vm.ToSelDate, 0 - AppDefine.ChartTraffMaxDays);
							vm.FromSelDate = new Date(fromDate);
						}
					}
					else {
						$scope.SearchMode = false;
						UpdateStartEnd($scope.Opt);
						GetReport($scope.Opt);
					}
				}

				$scope.isActiveOption = function ( opt ) {
					if ( $scope.Opt == null )
						return false;
					var ret = utils.Equal( opt, $scope.Opt );
					return ret;
				}

				/****************************** CALENDAR DATA - Begin ********************************/
				var FiscalYearData = null;
				//var fyStartDate = {};
				//var fyEndDate = {};
				var dateFormat = AppDefine.ParamDateFormat; // "MM/dd/yyyy";

				vm.minDate = new Date(1970, 0, 1);
				vm.maxDate = new Date();
				vm.dateFormat = dateFormat;
				vm.dateOptions = {
					formatYear: 'yy',
					startingDay: 1
				};
				vm.open = function ($event, idx) {
					$event.preventDefault();
					$event.stopPropagation();
					if (idx == 1)
						vm.opened1 = true;
					else
						vm.opened2 = true;
				};
				vm.Search = function () {
					var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
					if (delta >= 0) {
						var opt = {};
						angular.copy($scope.Opt, opt);
						utils.RemoveProperty(opt, "month");
						if (delta == 0) {
							utils.RemoveProperty(opt, "day");
							opt.hour = 23;
						}
						else {
							utils.RemoveProperty(opt, "hour");
							opt.day = delta;
						}
						GetReport(opt);
					}
					else {
						alert(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
					}
				}

				function GetCustFiscalYear(opt) {
					var def = cmsBase.$q.defer();
					var curDay = chartSvc.FYFormatDate(new Date());
					fiscalyearservice.GetCustomFiscalYear(curDay).then(function (data) {
						if (data) {
							FiscalYearData = data;
							//fyStartDate = new Date(FiscalYearData.FYDateStart);
							//fyEndDate = new Date(FiscalYearData.FYDateEnd);

							if (opt)
								UpdateStartEnd(opt);
							def.resolve();
						}
						else {
							def.reject('No data!');
						}
					},
					function (error) {
						def.reject(error);
					});
					return def.promise;
				}

				$scope.DateTimeDesc = '';

				function UpdateStartEnd(opt) {
					if (opt.hasOwnProperty('hour')) {
						vm.FromSelDate = new Date();
						//vm.ToSelDate = new Date();
						$scope.DateTimeDesc = $filter('date')(vm.FromSelDate, AppDefine.CalDateFormat);// + ' 00:00:00 - 23:59:59';
					}
					else if (opt.hasOwnProperty('day')) {
						if (FiscalYearData) {
							/*if (FiscalYearData.FYTypesID == AppDefine.FiscalTypes.NORMAL) {
								var fm = chartSvc.GetCalendarPeriod(new Date(), false);
								vm.FromSelDate = new Date(fm.StartDate);
							}
							else {
								var fw = chartSvc.GetFiscalWeek(FiscalYearData, new Date());

								var fm = chartSvc.GetFiscalPeriod(fw.WeekNo, FiscalYearData.CalendarStyle, fyStartDate, fyEndDate, false, FiscalYearData.FYNoOfWeeks);
								vm.FromSelDate = new Date(fm.StartDate);
							}*/
							var fm = chartSvc.GetFiscalPeriodInfo(FiscalYearData, -1, new Date(), false);
							var fySDateUtc = new Date(fm.StartDate);
							vm.FromSelDate = fySDateUtc;//new Date(fySDateUtc.getUTCFullYear(), fySDateUtc.getUTCMonth(), fySDateUtc.getUTCDate());//
							vm.ToSelDate = new Date();

							$scope.DateTimeDesc = $filter('date')(vm.FromSelDate, AppDefine.CalDateFormat) + ' - ' + $filter('date')(vm.ToSelDate, AppDefine.CalDateFormat);
						}
					}
					else if (opt.hasOwnProperty('month')) {
						if (FiscalYearData) {
							var fySDate = chartSvc.GetUTCDate(FiscalYearData.FYDateStart);//new Date(FiscalYearData.FYDateStart);
							vm.FromSelDate = fySDate;//new Date(fySDate.getUTCFullYear(), fySDate.getUTCMonth(), fySDate.getUTCDate());//(fyStartDate);getUTCFullYear(), getUTCMonth(), getUTCDate()
							vm.ToSelDate = new Date();

							$scope.DateTimeDesc = $filter('date')(vm.FromSelDate, AppDefine.CalDateFormat) + ' - ' + $filter('date')(vm.ToSelDate, AppDefine.CalDateFormat);
						}
					}
				}
				//
				$scope.$watch('vm.FromSelDate', function (newday, oldday) {
					if ($scope.SearchMode == true) {
						var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
						if (delta > AppDefine.ChartTraffMaxDays) {
							var toDate = chartSvc.dateAdds(vm.FromSelDate, AppDefine.ChartTraffMaxDays);
							vm.ToSelDate = new Date(toDate);
						}
					}
				});
				$scope.$watch('vm.ToSelDate', function (newday, oldday) {
					if ($scope.SearchMode == true) {
						var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
						if (delta > AppDefine.ChartTraffMaxDays) {
							var fromDate = chartSvc.dateAdds(vm.ToSelDate, 0 - AppDefine.ChartTraffMaxDays);
							vm.FromSelDate = new Date(fromDate);
						}
					}
				});
				/****************************** CALENDAR DATA - End **********************************/

				$scope.Init = function () {
					$scope.SearchMode = false;
					var opt = DefaultOption();
					if ($scope.Gui && $scope.Gui.Opts) {
						var searchOpt = { label: 'CUSTOM' };
						$scope.Gui.Opts.push(searchOpt);
					}
					GetCustFiscalYear(opt).then(function () {
						//UpdateStartEnd($scope.Opt);
						$scope.DateTimeDesc = $filter('date')(vm.FromSelDate, AppDefine.CalDateFormat);
						GetReport( $scope.Opt )
					});
				}

				function GetReport(optVal) {
					var opt = {};
					angular.copy(optVal, opt);
					//UpdateStartEnd(opt);
					opt.date = $filter('date')(vm.ToSelDate, AppDefine.DateFormatCParamED);
					if (!opt.hasOwnProperty('month')) {
						if (opt.hasOwnProperty('hour')) {
							opt.hour = 23;
						}
						else {
							var delta = chartSvc.dateDiff(vm.ToSelDate, vm.FromSelDate);
							utils.RemoveProperty(opt, "week");
							utils.RemoveProperty(opt, "month");
							utils.RemoveProperty(opt, "hour");
							opt.day = delta;
						}
					}

					var pram = angular.copy( $scope.Pram );
					utils.RemoveProperty( pram, "act" );
					if ( opt ) {
						var Props = Object.getOwnPropertyNames( opt );
						for ( var i = 0; i < Props.length; i++ ) {
							var propName = Props[i];
							if ( !propName.startsWith( "$$" ) && !propName.startsWith( "label" ) )
								utils.AddProperty( pram, propName, opt[propName] );
						}
					}

					rptService.DshTraffic( pram, RptSuccess, RptError );
				};

				function RptSuccess( response ) {

					$scope.$parent.isLoading = false;
					if ( response === null || response === undefined )
						return;
					CreateChartData( response );
					$scope.loading = true;
				}
				function RptError( response ) {

				}
				$scope.chartDSTraffic = {
					chart: {
						//caption: "Traffic Statistic",
						linethickness: "2",
						showvalues: "0",
						numvdivlines: "22",
						formatnumberscale: "1",
						labeldisplay: "ROTATE",
						slantlabels: "1",
						yAxisValuesPadding: "15",
						anchorradius: "4",
						anchorbgalpha: "50",
						anchoralpha: "100",
						showalternatevgridcolor: "0",
						showalternatehgridcolor: "0",
						animation: "1",
						legendshadow: "1",
						legendborderalpha: "30",
						bgcolor: "ffffff",
						divlineisdashed: "1",
						divlinedashlen: "2",
						divlinedashgap: "4",
						canvasborderthickness: "1",
						canvasborderalpha: "30",
						legendPosition: "right",
						legendIconScale: "1",
						showborder: "0",
						showToolTip: $scope.showToolTip,
						toolTipBgColor: "#FFFFFF",
						toolTipBgAlpha: '100',
						toolTipBorderColor: '#545454',
						//plottooltext: "<div class='chart-tooltip-nb'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div></div>",
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0',
						theme: "fint"
					},
					categories: [],
					dataset: []
				};

				function CreateChartData( resdata ) {
					if ( resdata === null || resdata === undefined )
						return;
					var datLen = resdata.length;
					var catArray = [];
					var dsArray = [];
					var toolTipArray = [];

					var catSub = [];

					var dsItemIn = {};
					dsItemIn.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_IN);//"Count In";
					dsItemIn.color = AppDefine.ChartColor.Blue;
					dsItemIn.anchorbordercolor = AppDefine.ChartColor.Blue;
					//dsItemIn.anchorBgColor = AppDefine.ChartColor.Blue;
					dsItemIn.data = [];

					var dsItemOut = {};
					dsItemOut.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COUNT_OUT);//"Count Out";
					dsItemOut.color = AppDefine.ChartColor.Yellow;//ColorRed;
					dsItemOut.anchorbordercolor = AppDefine.ChartColor.Yellow;//ColorRed;
					//dsItemOut.anchorBgColor = AppDefine.ChartColor.Yellow;
					dsItemOut.data = [];

					var dsItemForcast = {};
					dsItemForcast.seriesname = cmsBase.translateSvc.getTranslate(AppDefine.Resx.FORECAST);//"Forecast";
					dsItemForcast.color = AppDefine.ChartColor.Green;//ColorRed;
					dsItemForcast.anchorbordercolor = AppDefine.ChartColor.Green;//ColorRed;
					//dsItemForcast.anchorBgColor = AppDefine.ChartColor.Green;
					dsItemForcast.data = [];

					//data temp, hard code to test, begin
					//var channelList = [];
					//for (var num = 1; num <= 6; num++) {
					//	var name = 'Channel ' + num;
					//	channelList.push({
					//		KDVR: 40,
					//		KChannel: null,
					//		DVR_ChannelNo: num,
					//		Name: name,
					//		PAC_ChannelID: null,
					//		PAC_ChannelName: null,
					//		Dim_PACID: null,
					//		EnableTrafficCount: true
					//	});
					//}
					//data temp, hard code to test, end

					for ( var i = 0; i < datLen; i++ ) {
						var catIt = {};
						catIt.label = resdata[i].Label;
						catSub.push( catIt );

						var dsIt1 = {};
						dsIt1.value = resdata[i].countIn;
						dsItemIn.data.push(dsIt1);

						var dsIt2 = {};
						dsIt2.value = resdata[i].countOut;
						dsItemOut.data.push(dsIt2);

						var dsIt3 = {};
						dsIt3.value = resdata[i].forecast;
						dsItemForcast.data.push(dsIt3);

						//data temp, hard code to test, begin
						//if ([5,9,15,20,8].indexOf(i) > 0) {
						//	resdata[i].Channels = channelList;
						//}
						
						var toolTipData = {
							time: catIt.label,
							countInLabel: dsItemIn.seriesname,
							countInValue: dsIt1.value,
							countOutLabel: dsItemOut.seriesname,
							countOutValue: dsIt2.value,
							forcastLabel: dsItemForcast.seriesname,
							forcastValue: dsIt3.value,
							channels: resdata[i].Channels
						};

						toolTipArray.push(toolTipData);

						//data temp, hard code to test, end

						//var tooltip = "<div class='chart-tooltip-nb'><div class='tooltipHeader'><strong>" + catIt.label + "</strong></div><div class='tooltipContent'>" + dsItemIn.seriesname + ": " + dsIt1.value + "</div><div class='tooltipContent'>" + dsItemOut.seriesname + ": " + dsIt2.value + "</div><div class='tooltipContent'>" + dsItemForcast.seriesname + ": " + dsIt3.value + "</div></div>";
						
						var tooltip = BuildToolTip(toolTipData);
						dsItemIn.data[i].tooltext = tooltip;
						dsItemOut.data[i].tooltext = tooltip;
						dsItemForcast.data[i].tooltext = tooltip;

						dsItemIn.data[i].anchorbgalpha = "100";
						dsItemOut.data[i].anchorbgalpha = "100";
						dsItemForcast.data[i].anchorbgalpha = "100";

						dsItemIn.data[i].anchorBorderThickness = "2";
						dsItemOut.data[i].anchorBorderThickness = "2";
						dsItemForcast.data[i].anchorBorderThickness = "2";
					}
					var catCol = {};
					catCol.category = catSub;
					catArray.push( catCol );

					dsArray.push(dsItemIn);
					dsArray.push(dsItemOut);
					dsArray.push(dsItemForcast);

					$scope.chartDSTraffic.chartName = "TRAFFICSTATISTIC";
					$scope.chartDSTraffic.categories = catArray;
					$scope.chartDSTraffic.dataset = dsArray;
					$scope.chartDSTraffic.toolTipData = toolTipArray;
					
				}

				function BuildToolTip(data) {
					var htmlSlider = "<div class=\"chart-tooltip-nb\">" +
							"<div class=\"tooltipHeader\"><strong>" + data.time + "</strong></div>" +
							"<div class=\"tooltipContent\">" + data.countOutLabel + ": " + data.countOutValue + "</div>" +
							"<div class=\"tooltipContent\">" + data.countInLabel + ": " + data.countInValue + "</div>" +
							"<div class=\"tooltipContent\">" + data.forcastLabel + ": " + data.forcastValue + "</div>" +
						"</div>";
					//if (data.channels) {
					//	$scope.data = data;
					//	var apiURL = "/api/cmsweb/site/GetImageChannel?";
					//	var imageString = "";
					//	imageString += "<div class=\"image-slider\">";
					//	for(var c=0; c < data.channels.length; c++){
					//		var channel = data.channels[c];
					//		var url = apiURL + "name=C_" + pad(channel.DVR_ChannelNo) + ".jpg&kdvr=" + channel.KDVR;
					//		imageString += "<img src=\"" + url + "\" width=\"100\" height=\"70\" />";
					//	}
					//	imageString += "</div>";
					//	htmlSlider = "<div class=\"chart-tooltip-nb\">" + imageString +
					//			"<div class=\"tooltipHeader\"><strong>" + data.time + "</strong></div>" +
					//			"<div class=\"tooltipContent\">" + data.countOutLabel + ": " + data.countOutValue + "</div>" +
					//			"<div class=\"tooltipContent\">" + data.countInLabel + ": " + data.countInValue + "</div>" +
					//			"<div class=\"tooltipContent\">" + data.forcastLabel + ": " + data.forcastValue + "</div>" +
					//		"</div>";
					//}
					return htmlSlider;
				}

				function pad(number) {
					return (number < 10 ? '0' : '') + number.toString();
				}

				//function ReLoadData() {
				//	var param = chartSvc.createParam('1', $scope.SelDate, 0, vm.Period);
				//	ChartService.GetTrafficData(param, Success, Error);
				//	//if (!$scope.$$phase) {
				//	//	$scope.$apply();
				//	//}
				//};
				//$scope.Update = function (per) {
				//	vm.Period = per;
				//	ReLoadData();
				//}

				//$scope.$on('datechanged', function (event, data) {
				//	$scope.SelDate = data;
				//	ReLoadData();
				//});

				//$scope.GetData = function () {
				//	//var param = {};
				//	//param.sitekey = 1;
				//	//var curDate = new Date();
				//	//param.date = '2015/4/10';//DateToString(curDate);
				//	//ChartService.GetConvChartLines(param, Success, Error);

				//	var curDate = new Date();
				//	$scope.SelDate = chartSvc.dateToString(curDate);//'2014/4/15';//
				//	//vm.Period = 0;
				//	ReLoadData();
				//};
				//function Success(response) {
				//	$scope.$parent.isLoading = false;
				//	if (response === null || response === undefined)
				//		return;
				//	CreateChartData(response);
				//	$scope.loading = true;
				//}
				//function Error(response) {
				//}

				//function CreateChartData(resdata) {
				//	if (resdata === null || resdata === undefined)
				//		return;
				//	var datLen = resdata.length;
				//	var catArray = [];
				//	var dsArray = [];

				//	var catSub = [];

				//	var dsItem1 = {};
				//	dsItem1.seriesname = "Count In";
				//	dsItem1.color = AppDefine.ChartColor.Green;
				//	dsItem1.anchorbordercolor = AppDefine.ChartColor.Green;
				//	dsItem1.data = [];

				//	var dsItem2 = {};
				//	dsItem2.seriesname = "Count Out";
				//	dsItem2.color = AppDefine.ChartColor.Red;//ColorRed;
				//	dsItem2.anchorbordercolor = AppDefine.ChartColor.Red;//ColorRed;
				//	dsItem2.data = [];

				//	for (var i = 0; i < datLen; i++) {
				//		var catIt = {};
				//		catIt.label = resdata[i].label;
				//		catSub.push(catIt);

				//		var dsIt1 = {};
				//		dsIt1.value = resdata[i].countIn;
				//		dsItem1.data.push(dsIt1);

				//		var dsIt2 = {};
				//		dsIt2.value = resdata[i].countOut;
				//		dsItem2.data.push(dsIt2);
				//	}
				//	var catCol = {};
				//	catCol.category = catSub;
				//	catArray.push(catCol);

				//	dsArray.push(dsItem1);
				//	dsArray.push(dsItem2);

				//	$scope.chartDSTraffic = {
				//		chart: {
				//			//caption: "Traffic Statistic",
				//			linethickness: "2",
				//			showvalues: "0",
				//			numvdivlines: "22",
				//			formatnumberscale: "1",
				//			labeldisplay: "ROTATE",
				//			slantlabels: "1",
				//			anchorradius: "2",
				//			anchorbgalpha: "50",
				//			showalternatevgridcolor: "0",
				//			showalternatehgridcolor: "0",
				//			anchoralpha: "100",
				//			animation: "1",
				//			legendshadow: "1",
				//			legendborderalpha: "30",
				//			bgcolor: "ffffff",
				//			divlineisdashed: "1",
				//			divlinedashlen: "2",
				//			divlinedashgap: "4",
				//			canvasborderthickness: "1",
				//			canvasborderalpha: "30",
				//			legendPosition: "right",
				//			legendIconScale:"1",
				//			showborder: "0",
				//			toolTipBgColor: "#FFFFFF",
				//			toolTipBgAlpha: '100',
				//			plottooltext: "<div class='chart-tooltip-nb'><div class='tooltipHeader'>$label</div><div class='tooltipContent'>$seriesname: $value</div></div>"
				//		},
				//		categories: catArray,
				//		dataset: dsArray
				//	};
				//}

				$scope.isFull = false;
			    // Tri add feature scroll fullsize
				$scope.$parent.$parent.$watch('isMax', function (newValue, oldValue) {
				    if (newValue !== oldValue) {
				        $scope.isFull = newValue;
				    }
				});
			}
		});
})();
