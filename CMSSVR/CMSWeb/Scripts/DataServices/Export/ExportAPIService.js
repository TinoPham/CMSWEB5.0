(function () {
	define(['cms'], function (cms) {
		cms.register.service('ExportAPISvc', ExportAPISvc);
		ExportAPISvc.$inject = ['$resource', '$http', '$q', 'AppDefine'];

		function ExportAPISvc($resource, $http, $q, AppDefine) {

			var apibase = AppDefine.Api.Export + '/:dest';
			var ExportSrc = $resource(apibase, { dest: "@dest" }, {
				'BAMDashboardToExcelAPI': { method: 'POST', headers: cms.EncryptHeader(), isArray: false, params: { dest: "BAMDashboardToExcel" }, interceptor: { response: function (response) { return response; } } }
                , 'ExportExcel': { method: 'POST', headers: cms.EncryptHeader(), isArray: false, params: { dest: "ExportExcel" }, interceptor: { response: function (response) { return response; } } }
				, 'BAMDashboardToPDFAPI': { method: 'POST', headers: cms.EncryptHeader(), isArray: false, params: { dest: "BAMDashboardToPDF" }, interceptor: { response: function (response) { return response; } } }
			});

			var ReportInfoModel = function () {
				return {
					ReportName: '',
					CustomerName: '',
					RegionName: '',
					Location: '',
					WeekIndex: null,
					Footer: '',
					CreatedBy: '',
					CreateDate: null
				};
			};

			var ChartDataItem = function () {
				return {
					Name: '',
					Value: '',
					Color: 0,
				};
			};

			var ChartData = function () {
				return {
					ChartDataItems: [],
					Title: '',
					ChartType: -1
				};
			};

			var ColData = function () {
				return {
					Value: '',
					Color: 0
				};
			};

			var RowData = function () {
				return {
					Type: -1,
					ColDatas: []
				};
			};

			var GridData = function () {
				return {
					Name: '',
					RowDatas: [],
					OptionDatas: {},
					Format: {}
				};
			};

			//Make sure ExportModel same as model on Server API.
			var BAMExportModel = BAMExportModelFunc();

			function BAMExportModelFunc() {
				return {
					ReportInfo: ReportInfoModel,
					GridModels: [], // Array GridData
					ChartModels: [] // Array ChartData
				};
			}

			var ExportExcel = function (param) {
				var def = $q.defer();
				ExportSrc.ExportExcel(param).$promise.then(
					function (response) {
						var data = cms.GetResponseData(response);
						def.resolve(data);
					},
					function (error) {
						console.log(error);
						def.reject(error);
					}
				);
				return def.promise;
			};

			var BAMDashboardToExcel = function (param) {
				var def = $q.defer();
				ExportSrc.BAMDashboardToExcelAPI(param).$promise.then(
					function (response) {
						var data = cms.GetResponseData(response);
						def.resolve(data);
					},
					function (error) {
						console.log(error);
						def.reject(error);
					}
				);
				return def.promise;
			};

			var BAMDashboardToPDF = function (param) {
				var def = $q.defer();
				ExportSrc.BAMDashboardToPDFAPI(param).$promise.then(
					function (response) {
						var data = cms.GetResponseData(response);
						def.resolve(data);
					},
					function (error) {
						console.log(error);
						def.reject(error);
					}
				);
				return def.promise;
			};

			var DownloadExport = function (param) {
				return AppDefine.Api.Export + "/DownloadExport?filename=" + param;
			}

			return {
				BAMDashboardToExcel: BAMDashboardToExcel,
				BAMDashboardToPDF: BAMDashboardToPDF,
				DownloadExport: DownloadExport,
				BAMExportModel: function () { return BAMExportModel; },
				ReportInfoModel: function () { return ReportInfoModel; },
				ChartDataItem: function () { return ChartDataItem; },
				ChartData: function () { return ChartData; },
				ColData: function () { return ColData; },
				RowData: function () { return RowData; },
				GridData: function () { return GridData; },
				exportExcel: ExportExcel
			};
		}
	});
})();
