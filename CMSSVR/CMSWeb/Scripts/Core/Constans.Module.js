﻿( function () {
	'use strict';

	define( function () {
		var Module = {
			MODULE_SITE: 1,
			MODULE_BAM: 2,
			MODULE_REBAR: 3,
			MODULE_CMSREPORTS: 4,
			MODULE_CONFIGURATION: 5,
			MODULE_INCIDENTREPORTS: 6
		};

		var Function = {
			FUNC_SITEAUDIT: 1,
			FUNC_DVRHEALTH_CHECK: 2,
			FUNC_BAM_REPORT: 3,
			FUNC_DASHBOARD: 4,
			FUNC_EXCEPTION_REPORT: 5,
			FUNC_CASE_MANAGEMENT: 6,
			FUNC_CMS_REPORT: 7,
			FUNC_ADMINISTRATOR: 8,
			FUNC_INCIDENT_REPORT: 9,
			FUNC_CRYSTAL_REPORT: 10,
		    FUNC_HEATMAP: 11
		};
		var Levels = {
			LEVEL_NONE: 0,
			LEVEL_NATIONAL_REPORT: 1,
			LEVEL_REGIONAL_MANAGER: 2,
			LEVEL_DISTRIC_MANAGER: 3,
			LEVEL_STORE_MANAGER: 4,
			LEVEL_INCIDENT_OWNER: 5,
			LEVEL_INCIDENT_APPROVER: 6
		};
		Module.Function = Function;
		Module.Level = Levels;
		return Module;
	} );
} )();