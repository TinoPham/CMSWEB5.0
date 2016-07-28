(function () {
	'use strict';

	define(function () {
		var events = {
			APPDATALOADED: 'appDataLoaded',
			PAGENOTFOUND: 'pagenotfound',
			LOGINSUCCESS: 'loginsuccess',
			LOGOUTSUCCESS: 'logoutsuccess',
			DASHBOARDSETTING: 'dashboardsetting',
			ROWSELECTEDCHANGE: 'rowselectedchange',
			TREENODESELECTEDCHANGE: 'TreeNodeSelectedChange',
			BAMSELECTNODE: 'BAMSELECTNODE',
			CHECKNODE: 'CHECKNODE',
	        FILESELECTEDCHANGE: 'fileSelected',
			USERPHOTOCHANGED:'UserPhotoChanged',
			SAVESITE: 'saveSite',
			RESIZE: 'resize',
			GENERATESERIALNUMBER: 'GenerateSerialNumber',
			REMOVESERIALNUMBER: 'RemoveSerialNumber',
			ACTIVEDASBOARDREPORT: 'ACTIVEDASBOARDREPORT',
			ACTIVESALESREPORT: 'ACTIVESALESREPORT',
			ACTIVENORMALIZEREPORT: 'ACTIVENORMALIZEREPORT',
			CHARTDATALOADED: 'CHARTDATALOADED',
			GETBAMREPORTDATA: 'GetBamReportData',
			SUBMITGETBAMREPORT: 'SubmitGetBamReport',
			STATECHANGESUCCESSHANDLER: 'stateChangeSuccessHandler',
			REBARSEARCH: 'REBARSEARCH',
			PAGEREADY: 'pageReady',
			PAGEEDITED: 'pageEdited',
			CANNEDGROUPBYCHANGED: 'CannedGroupByChanged',
			IDLETIMEOUT: 'IDLETIMEOUT',
			EXPORTEVENT: 'EXPORTEVENT',

	        SITE_TAPS: {
	            GET_ALERTS: "GET_ALERTS",
	            SELECT_MAPS: "SELECT_MAPS",
	            DES_SELECT_MAPS: "DES_SELECT_MAPS",
	            SELECT_REC: "SELECT_REC",
	            DES_SELECT_REC: "DES_SELECT_REC",
	            DES_ALERTS: "DES_ALERTS",
                CHANNEL_LOAD: 'CHANNEL_LOAD',
	            SHOW_HIDE_TREE: "SHOW_HIDE_TREE",
	            CHANGE_NODE_TREE: "CHANGE_NODE_TREE",
	            BACK_ACTION: "BACK_ACTION",
	            CANCEL_ACTION: "CANCEL_ACTION",
	            UPLOAD_COMPLETE: "UPLOAD_COMPLETE",
	            UPLOADFROMDIALOG_COMPLETE: "UPLOADFROMDIALOG_COMPLETE",
	            DELETEFROMBUTONX_COMPLETE: "DELETEFROMBUTONX_COMPLETE",
	            UPDATE_PROGRESS: "UPDATE_PROGRESS",
	            UPDATE_MODEL: "UPDATE_MODEL",
	            UPDATEFROMDIALOG_MODEL: "UPDATEFROMDIALOG_MODEL",
	            FILESELECTEDCHANGE: "filesSelected",
	            SELECT_SITES: "SELECT_SITES",
	            DES_SELECT_SITES: "DES_SELECT_SITES",
	            CHANGE_DATA :"CHANGE_DATA"
	        },

	        HEAT_MAP: {
	            UPLOAD_COMPLETE: "UPLOAD_COMPLETE"
            }
		};
		var state = {
		    PAGENOTFOUND: 'pagenotfound'
            , LOGIN: 'login'
            , LOSTPASSWORD: 'forgotpassword'
            , HOME: 'home'
            , DASHBOARD: 'dashboard'
            , SITES: 'sites'
            , BAM: 'bam'
			, BAM_DASHBOARD: 'bam.dashboard'
			, WEEKATAGLANCE: 'bam.weekataglance'
            , CUSTOMREPORT: 'bam.customreport'
			, SALEREPORTS: 'bam.sales'
			, DRIVETHROUGH: 'bam.drivethrough'
			, DISTRIBUTION: 'bam.distribution'
            , HEATMAP: 'bam.heatmap'
            , BAM_NORMALIZE: 'bam.normalize'
            , BAM_KEYPERFORMANCEINDICATORS: 'KeyPerformanceIndicators'
            , BAM_PERFORMANCECOMPARISION: 'PerformanceComparision'
            , BAM_YTDConv: 'YTDConv'
            , BAM_ConversionComparision: 'ConversionComparision'
            , ROOT: 'root'
            , REBAR: 'rebar'
			, REBAR_ADHOC: 'rebar.adhoc'
            , REBAR_ADHOCDETAILS: 'rebar.adhocdetails'
            , REBAR_DASHBOARD: 'rebar.dashboard'
            , REBAR_WEEKATGLANCE: 'rebar.dashboard'
			, REBAR_QUICKSEARCH: 'rebar.quicksearch'
			, REBAR_REFUNDS: 'rebar.refunds'
			, REBAR_VOIDS: 'rebar.voids'
			, REBAR_CANCELS: 'rebar.cancels'
			, REBAR_NOSALES: 'rebar.nosales'
			, REBAR_DISCOUNTS: 'rebar.discounts'
		};
		return { Events : events, State : state};
	});
})();