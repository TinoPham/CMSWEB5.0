(function () {
	'use strict';
	define(['cms', 'DataServices/ReportService', 'Scripts/Services/chartSvc'],
		function (cms) {
			cms.register.controller('convRateSiteCtrl', convRateSiteCtrl);
			convRateSiteCtrl.$inject = ['$scope', 'AppDefine', 'ReportService', 'Utils', 'chartSvc', '$filter', 'cmsBase'];
			function convRateSiteCtrl($scope, AppDefine, rptService, utils, chartSvc, $filter, cmsBase) {
				var vm = this;

				var Options = JSON.parse( "{" + $scope.widgetModel.TemplateParams + "}" );
				$scope.Gui = ( Options == null || !Options.hasOwnProperty( "Gui" ) ) ? null : Options.Gui;
				$scope.Pram = ( Options == null || !Options.hasOwnProperty( "Pram" ) ) ? null : Options.Pram;
				$scope.Opt = DefaultOption();

				$scope.loading = true;
				$scope.ChartDatas = [];
				$scope.ChartHeight = AppDefine.ChartHeight - 50;
				$scope.ExportOptions = AppDefine.ChartExportOptions;
				vm.SelectedDate = new Date();

				
				var SMALL_WIDTH_MOBILE_PATTERN = 375;
				var SMALL_WIDTH_HORIZONTAL_MOBILE_PATTERN = 667;
				vm.WidthDevice = document.body.clientWidth;
				if (cmsBase.isMobile) { /*For mobile*/
				    if (document.body.clientWidth <= SMALL_WIDTH_MOBILE_PATTERN) {
				        $scope.ChartHeight = AppDefine.ChartHeight - 150;
				    }
				}

				$scope.$watch(function () {
				    //return angular.element('#sidebar')[0].clientWidth;
				    return document.body.clientWidth;
				}, function (newvalue, oldvalue) {
				    if (newvalue != oldvalue && newvalue <= SMALL_WIDTH_HORIZONTAL_MOBILE_PATTERN)
				    {
				        if (document.body.clientWidth <= SMALL_WIDTH_MOBILE_PATTERN) {
				            $scope.ChartHeight = AppDefine.ChartHeight - 150;
				        } else {
				            $scope.ChartHeight = AppDefine.ChartHeight - 50;
				        }
				    }
				});

				function DefaultOption() {
					if ( !$scope.Pram || !$scope.Gui.Opts || $scope.Gui.length == 0 || !$scope.Pram.hasOwnProperty( "act" ) || $scope.Pram.act === null )
						return null;

					var obj = $scope.Gui.Opts[$scope.Pram.act];
					return obj;
				}

				var exFileName = AppDefine.ExFName.Conversion + $filter('date')(vm.SelectedDate, AppDefine.DFFileName);
				$scope.chartConvEvents = angular.copy(chartSvc.createExportEvent('_cnv', exFileName));

				//*********** DATE PICKER **************//
				vm.minDate = new Date(1970, 0, 1);
				vm.maxDate = new Date();
				vm.dateFormat = AppDefine.CalDateFormat;//"MM/dd/yyyy";
				vm.dateOptions = {
					formatYear: 'yy',
					startingDay: 1
				};

				vm.open = function ($event) {
					$event.preventDefault();
					$event.stopPropagation();

					vm.opened = true;
				};

				$scope.$watch('vm.SelectedDate', function (newday, oldday) {
					if (newday.getDate() != oldday.getDate() || newday.getMonth() != oldday.getMonth() || newday.getFullYear() != oldday.getFullYear()) {
						//$scope.Opt.date = $filter('date')(newday, AppDefine.DateFormatCParamED); //'yyyyMMdd235959'
						//GetReport($scope.Opt);
						GetReportByDate(newday);
					}
				});
				//*********** END DATE PICKER **********//
				$scope.Init = function () {
					//var opt = DefaultOption();
					var curDate = new Date();
					GetReportByDate(curDate);
				}

				function GetReportByDate(rptDate) {
					$scope.Opt.date = $filter('date')(rptDate, AppDefine.DateFormatCParamED);
					GetReport($scope.Opt)
				}
				function GetReport( opt ) {
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
					pram.day = 0;
					rptService.DshConversion( pram, RptSuccess, RptError );
				};

				$scope.isActiveOption = function ( opt ) {
					if ( $scope.Opt == null )
						return false;
					var ret = utils.Equal( opt, $scope.Opt );
					return ret;
				}

				function RptSuccess( response ) {

					$scope.$parent.isLoading = false;
					if ( response === null || response === undefined ) {
						return;
					}

					//CreateChartData(response);
					$scope.chartDSConv.colorRange.color[0].maxValue = response.goalMin;
					$scope.chartDSConv.colorRange.color[1].minValue = response.goalMin;
					$scope.chartDSConv.colorRange.color[1].maxValue = response.goalMax;
					$scope.chartDSConv.colorRange.color[2].minValue = response.goalMax;
					$scope.chartDSConv.dials.dial[0].value = Math.min(response.Value, 100);

					$scope.Conversion = response.Value.toFixed(2);//'30';//
					$scope.TotalTransact = response.trans;//'1233342';//
					$scope.TotalTraffic = response.traffic;//'34234234';//
					$scope.loading = true;

				}
				function RptError( response ) {

				}

				$scope.Conversion = 0;
				$scope.TotalTransact = 0;
				$scope.TotalTraffic = 0;
				$scope.chartDSConv = {
					chart: {
						//caption: 'Conversion Rate',
						//subcaption: 'Last day',
						lowerLimit: '0',
						upperLimit: '100',
						lowerLimitDisplay: '0 %',
						upperLimitDisplay: '100 %',
						showValue: '1',
						valueBelowPivot: '0',
						theme: 'fint',
						exportEnabled: '1',
						exportAtClient: '0',
						exportHandler: AppDefine.ChartExportURL,
						exportAction: 'download',
						exportShowMenuItem: '0'
					},
					colorRange: {
						color: [
							{
								minValue: '0',
								maxValue: '35',
								code: AppDefine.ChartColor.Red
							},
							{
								minValue: '35',
								maxValue: '60',
								code: AppDefine.ChartColor.Yellow
							},
							{
								minValue: '60',
								maxValue: '100',
								code: AppDefine.ChartColor.Green
							}
						]
					},
					dials: {
						dial: [
							{
								value: '0'
							}
						]
					}
				};
			}
		});
})();