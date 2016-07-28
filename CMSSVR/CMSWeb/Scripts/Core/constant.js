(function() {
	'use strict';

	define(['cms', 'Core/Constans.Resource', 'Core/Constant.Events', 'Core/Constans.Module', 'Core/Constans.RegExp'], function (cms, resx, event, modules, regExp) {

        var cmsconstant = {
            CMSVERSION: 'v5.0.1.10 Beta',
            BUILDDATE: 'Jan 21, 2016',
            BUILDNO: '5.0.1 Alpha 1',
            APPDATALOADED: 'appDataLoaded',
            PAGENOTFOUND: 'pagenotfound',
            PAGE_LOG_IN: 'login',
            loginSuccess: 'auth-login-success',
            loginFailed: 'auth-login-failed',
            logoutSuccess: 'auth-logout-success',
            sessionTimeout: 'auth-session-timeout',
            notAuthenticated: 'auth-not-authenticated',
            notAuthorized: 'auth-not-authorized',
            SID: 'SID',
            Encrypt_Header: 'application/encrypt',
            XSRF_TOKEN_KEY: "XSRF-TOKEN",
            HTTP_PROTOCOL: "http:",
            HTTPS_PROTOCOL: "https:",
            HTTP_LOCALHOST: "http://127.0.0.1:2737/video?s=",
            HTTPS_LOCALHOST: "https://127.0.0.1:2727/video?s="
        };

        var keyCodes = {
            backspace: 8,
            tab: 9,
            enter: 13,
            esc: 27,
            space: 32,
            pageup: 33,
            pagedown: 34,
            end: 35,
            home: 36,
            left: 37,
            up: 38,
            right: 39,
            down: 40,
            insert: 45,
            del: 46
        };

        var treeStatus = {
            Uncheck: false,
            Checked: true,
            Indeterm: null
        };

	    var NodeType = {
	        Region: 0,
	        Site: 1,
	        DVR: 2,
	        Channel: 3
	}

	    var apibase = '../api/cmsweb/';
		var api = {
			Account: apibase + 'Account',
			Calendar: apibase + 'Calendar',
			Site: apibase + 'site',
			GoalType: apibase + 'GoalType',
			SiteMetric: apibase + 'MetricSite',
			LDAP: apibase + 'SynUser',
			fiscalyear: apibase + 'fiscalyear',
			Recipient: apibase + 'Recipient',
			Incident: apibase + 'Incident',
			JobTitle: apibase + 'JobTitle',
			UserManager: apibase + 'Users',
			Company: apibase + 'Company',
			BoxGadget: apibase + 'Dashboard',
			Sites: apibase + 'Site',
			Dashboard: apibase + 'Dashboard',
			Note: apibase + 'Note',
			Todo: apibase + 'Todo',
			Chart: apibase + 'Chart',
			Alert: apibase + 'Alerts',
			Report: apibase + 'Reports',
			UserGroup: apibase + 'UserGroup',
			SiteAlerts: apibase + 'SiteAlerts',
			Maps: apibase + 'Maps',
			Notify: apibase + 'notify',
			SaleReports: apibase + 'SaleReports',
			DashboardReports: apibase + 'Bam',
			Rebar: apibase + 'Rebar',
			Adhoc: apibase + 'Adhoc',
			QuickSearch: apibase + "QSearch",
			Canned: apibase + 'Canned',
			BamHeaderReports: apibase + 'BamHeader',
			Distribution: apibase + 'Distribution',
			DistributionImages: apibase + "Distribution/" + 'Images?img=',
			Export: apibase + "Export",
			POSItems: apibase + 'positem',
			SiteChanImage: apibase + "site/GetImageChannel?",
			SiteFirstImage: apibase + "site/GetFirstImage?"
		};

		var _POSItemKeys = {
			Cameras: "Cameras",
			CardIDs: "CardIDs",
			CheckIDs: "CheckIDs",
			Descriptions: "Descriptions",
			ItemCodes: "ItemCodes",
			ExtraNames: "ExtraNames",
			Operators: "Operators",
			Payments: "Payments",
			Registers: "Registers",
			Shifts: "Shifts",
			Stores: "Stores",
			Taxes: "Taxes",
			Terminals: "Terminals"
		}

		var SiteAPI = {
			GET_FILE: "/GetFile",
			ZIP_CODE: "/ZipCode"
		};
		var notify = {
			Alert: 'Alert'

		};
		
		cmsconstant.Api = api;
		cmsconstant.POSItemKeys = _POSItemKeys;
		cmsconstant.SiteAPI = SiteAPI;
		cmsconstant.Notify = notify;
		cmsconstant.NodeType = NodeType;
		cmsconstant.treeStatus = treeStatus;
		cmsconstant.keyCodes = keyCodes;
		cmsconstant.Resx = resx;
		cmsconstant.Events = event.Events;
		cmsconstant.State = event.State;
		cmsconstant.Modules = modules;
		cmsconstant.RegExp = regExp;

		var dayHours = ['12 AM', '1 AM', '2 AM', '3 AM', '4 AM', '5 AM', '6 AM', '7 AM', '8 AM', '9 AM', '10 AM', '11 AM', '12 PM', '1 PM', '2 PM', '3 PM', '4 PM', '5 PM', '6 PM', '7 PM', '8 PM', '9 PM', '10 AM', '11 PM'];
		var weekDays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
		var _monthNamesShort = ['JAN_S', 'FEB_S', 'MAR_S', 'APR_S', 'MAY_S', 'JUN_S', 'JUL_S', 'AUG_S', 'SEP_S', 'OCT_S', 'NOV_S', 'DEC_S'];
		cmsconstant.monthNamesShort = _monthNamesShort;
		cmsconstant.DayHours = dayHours;
		cmsconstant.WeekDays = weekDays;
		cmsconstant.CalDateFormat = 'MM/dd/yyyy';
		cmsconstant.CalDateTimeFormat = 'MM-dd-yyyy HH:mm:ss';
		cmsconstant.ParamDateFormat = 'MM/dd/yyyy';
		cmsconstant.DateFormatCParamED = 'yyyyMMdd235959';
		cmsconstant.DateFormatCParamST = 'yyyyMMdd000000';
		cmsconstant.DFFileName = 'yyyyMMdd';
		cmsconstant.DateTimeUTC = "yyyy-MM-ddTHH:mm:ss+00:00";
		cmsconstant.ShortDateMD = 'MM/dd';
		cmsconstant.BAMDateFormat = 'MM/dd/yy';

		var chartColors = {
			Red: '#e44a00',
			Green: '#6baa01',
			Yellow: '#FFD700',
			Blue: '#0000FF',
			LessMinGoal: '#eb605b',
			InGoal: '#efad4d',
			GreaterMaxGoal: '#17b374',
			White:'#ffffff'
		};

		var chartExportType = {
			LineChart: 1,
			BarChart: 2,
			ColumnChart: 3,
			PieChart: 4
		};

		var tableExport = {
			Header: 1,
			Body: 2,
			Footer: 3,
            Child:4
		};

		var exportColors = { //ThangPham, Define follow CellFontFormat API, Feb 24 2016
			Default: 0,
			ReportTitle: 1,
			ChartTitle: 2,
			GridHeaderCell: 3,
			GridHeaderFirstCell: 4,
			GridHeaderEndCell: 5,
			TextGreaterGoal: 6,
			TextInGoal: 7,
			TextLessGoal: 8,
			GreaterGoalCell: 9,
			InGoalCell: 10,
			LessGoalCell: 11,
			GridSumCell: 12,
			ForecastCell: 13,
			GridSubHeaderCell: 14,
			GridNormalCell: 15,
			GridRegionCell: 16,
			GridSiteCell: 17,
			RiskFactorNumberCell: 18,
			GridHeaderListGroup: 19,
			GridGroupFirstCell: 20,
			GridGroupCell: 21,
			GridGroupHeaderCell: 22,
		};

		var chartExportColor = { //ThangPham, Define follow ExportConst.ChartColor in API, Feb 24 2016
			Default: 0,
			Red: 1,
			Green: 2,
			Yellow: 3,
			Blue: 4,
			Orange: 5
		};

		var tableNameExport = {
			DashboardMetric: 'DashboardMetric',
			DashboardMetricDetail: 'DashboardMetricDetail',
			MetricSummary: 'MetricSummary',
			MetricDetail: 'MetricDetail'
		};

		var numberFormat = {
			number: '',
			Dollar: '$',
			Percent: '%'
		};

		var mostAlertCountRanges = [{ min: 0, max: 5, color: '#E4DB7B' }, { min: 5, max: 10, color: '#ECDE49' }, { min: 10, max: 20, color: '#ECC049' }, { min: 20, max: 30, color: '#EE9E46' }, { min: 30, max: 50, color: '#D97431' }, { min: 50, max: -1, color: '#D95531' }];
		var ConversionDPO = [{ min: 0, max: 15, color: '#eb605b' }, { min: 15, max: 30, color: '#efad4d' }, { min: 30, max: 100, color: '#17b374' }];
		var convMapRanges = [{ min: 0, max: 20, color: '#F8BD19' }, { min: 20, max: 40, color: '#D0DFA3' }, { min: 40, max: 60, color: '#B0BF92' }, { min: 60, max: 80, color: '#91AF64' }, { min: 80, max: 100, color: '#5A9502' }];
		var chartExportOptions = [{ id: 'exportpdf', label: 'EXPORT_TO_PDF' }, { id: 'exportcsv', label: 'EXPORT_TO_CSV' }, { id: 'exportpng', label: 'EXPORT_TO_PNG' }]
		var barChartName = {
			DVRCount: 'DVRCount',
			Alert: 'AlertCount',
			MostAlertDVR: 'DVRMostAlert'
		};
		var ExportFileName = {
			DVRCount: 'Overall_Statistic_',
			Alert: 'Alerts_',
			MostAlertDVR: 'DVR_with_Most_Alerts_',
			Conversion: 'Conversion_Rate_',
			Traffic: 'Traffic_Statistic_',
			ConvBySites: 'Conversion_by_Sites_',
			ConvByRegions: 'Conversion_by_Regions_'
		};

		var FileUploadTypes = {
			Images: '.jpg, .jpeg, .png, .gif, .bmp',
			Audios: '.mp3, .wav, .wma',
			Videos: '.mp4, .wmv, .avi',
			Documents: '.doc, .docx, .pdf, .xls, .xlsx, .txt'
		}

		var CHANNEL_STATUS = [
		    { value: 0, icon: 'icon-cam-block-2 chs_disable', Des: 'CHS_DISABLE' },
		    { value: 1, icon: 'icon-videocam-2 chs_notrecording', Des: 'CHS_NOTRECORDING' },
		    { value: 2, icon: 'icon-videocam-2 chs_recording', Des: 'CHS_RECORDING' },
		    { value: 3, icon: 'icon-cam-block-2 chs_videoloss', Des: 'CHS_VIDEOLOSS' },
		    { value: 4, icon: 'icon-videocam-2 chs_resdisable', Des: 'CHS_RESDISABLE' },
		    { value: 5, icon: 'icon-videocam-2 chs_null', Des: 'NO_ASSIGNED_CHANNEL' }];
		
		var SITE_TAPS = {
	           
		    filename_p: "&filename=",
	        thumbnail_p: "&thumbnail=",
	        thumbnail_true: "&thumbnail=true",
	        thumbnail_false:"&thumbnail=false",
	        filesSelected: "filesSelected",
	        timezoneFormat: 'yyyy-MM-dd HH:mm:ss.sss',
	        datedataformat: "yyyyMMddHHmmss",
	        mindate: "1970-1-1",
	        formatDateCompare: "yyyy-MM-dd",
	        endtimedateformat: "235959",
	        iconList: [{ Des: 'FOLDER', icon: "icon-folder fa-4x" },
                       { Des: 'DVR', icon: "icon-home fa-4x" },
                       { Des: 'DVR', icon: "icon-home fa-4x" },
                       { Des: 'CHANNEL', icon: "icon-videocam fa-4x" }]
		};

		var HEAT_MAP = {
		    filename_p: "&name=",
		    typestime_p: "&typestime=",
            user: "by", 
		    isManual_p: "&typeimage=Manual",
		    isSchedule_p: "&typeimage=Schedules"
		}

		var ImageOptions = {
			PrefixImageEmbedding: "data:image/jpeg;base64,",
			ImageJPEG: "image/jpeg",
			ImageFullQuality: 1.0,
			ImageMeiumQuality: 0.5,
			ImageLowQuality: 0.1,
			UserPhotoWidth: 100,
			UserPhotoHeigth: 100,
			CompanyPhotoWidth: 300,
			CompanyPhotoHeigth: 200
		}

		var ModalConfirmResponse = {
			OK: 'ok',
			CANCEL: 'cancel',
			CLOSE: 'close'
		};

		var SiteUploadField = {
		    imageSiteField: "ImageSite",
		    fixturePlanField: "FixturePlan",
		    actualBudgetField: "ActualBudget"
		};
		var AlertTypes = {
		    VIDEO_LOSS: 5,
		    DVR_is_off_line: 32,
		    DVR_VA_detection: 36,
		    DVR_Record_Less_Than: 37,
		    CMSWEB_Conversion_rate_above_100: 106,
		    CMSWEB_Door_count_0: 107,
		    CMSWEB_POS_data_missing: 108,
		    DVR_CPU_Temperature_High: 4,
		    DVR_disconnect_from_CMS_server: 17,
		    CMS_Registration_Expire_Soon: 23,
		    CMS_Registration_Expired: 24,
		    DVR_connected_CMS_Server: 33,
		    CMS_HASP_Unplugged: 101,
		    CMS_HASP_Found: 102,
		    CMS_HASP_Removed: 103,
		    CMS_HASP_Expired: 104,
		    CMS_Server_HASP_Limit_Exceeded: 105
		};
		

		var chartAlertSeverity = [{ id: '1', label: 'ALERT_SEVERITY_NORMAL', checked: false }, { id: '2', label: 'ALERT_SEVERITY_CAUTION', checked: true }, { id: '3', label: 'ALERT_SEVERITY_WARNING', checked: true }, { id: '4', label: 'ALERT_SEVERITY_URGENT', checked: true }];

		var FiscalTypes = {
			NORMAL: 1,
			FISCAL: 2,
			WEEKS_52: 3,
			WEEKS_53: 4,
			WEEKS_52_53: 5
		};

		var BamDataTypes = {
			TRAFFIC_IN: 1,
			TRAFFIC_OUT: 2,
			POS: 3,
			CONVERSION: 4,
			COUNT: 5,
			DWELL: 6,
			FORECAST: 7
		};
		var BamReportTypes = {
			DASHBOARD: 1,
			WEEKAAGLANCE: 2,
			SALE: 3,
			DRIVETHROUGH: 4,
			DISTRIBUTE: 5
		};
		var saleReportTypes = {
			Hourly: 1,
			Daily: 2,
			Weekly: 3,
			WTD: 4,
			PTD: 5,
			Monthly: 6
		};

		var heatMapTypes = {
		    Hourly: 1,
		    Daily: 2,
		    Weekly: 3,
		};

		var cannedReportType = {
			Refund: 1,
			Void: 2,
			Cancel: 3,
			NoSale: 4,
			Discount: 5,
			QuickSearch: 10
		};

		var typeTimeLines = {
		    Ten_minute: 1,
		    One_Hour: 2,
		    One_Day: 3
		};

		cmsconstant.ChartColor = chartColors;
		cmsconstant.chartExportType = chartExportType;
		cmsconstant.tableExport = tableExport;
		cmsconstant.ExportColors = exportColors;
		cmsconstant.ChartExportColor = chartExportColor;
		cmsconstant.TableNameExport = tableNameExport;
		cmsconstant.NumberFormat = numberFormat;
		cmsconstant.AlertCountRanges = mostAlertCountRanges;
		cmsconstant.ConversionDPO = ConversionDPO;
		cmsconstant.ConvMapRanges = convMapRanges;
		cmsconstant.ChartHeight = 270;
		cmsconstant.ChartTraffMaxDays = 30;
		cmsconstant.ChartExportOptions = chartExportOptions;
		cmsconstant.ChartExportURL = '../ChartExports/FCExporter.aspx';//http://localhost:5000
		cmsconstant.BarChartName = barChartName;
		cmsconstant.ExFName = ExportFileName;
		cmsconstant.FileUploadTypes = FileUploadTypes;
		cmsconstant.SITE_TAPS = SITE_TAPS;
		cmsconstant.CHANNEL_STATUS = CHANNEL_STATUS;
		cmsconstant.ImageOptions = ImageOptions;
		cmsconstant.ModalConfirmResponse = ModalConfirmResponse;
		cmsconstant.AlertSeverity = chartAlertSeverity;
		cmsconstant.SiteUploadField = SiteUploadField;
		cmsconstant.AlertTypes = AlertTypes;
		cmsconstant.FiscalTypes = FiscalTypes;
		cmsconstant.BamDataTypes = BamDataTypes;
		cmsconstant.SaleReportTypes = saleReportTypes;
		cmsconstant.HeatMapTypes = heatMapTypes;
		cmsconstant.HEAT_MAP = HEAT_MAP;
		cmsconstant.BamReportTypes = BamReportTypes;
		cmsconstant.CannedReportType = cannedReportType;
		cmsconstant.TypeTimeLines = typeTimeLines;

		cms.constant('AppDefine', cmsconstant);
	});
})();