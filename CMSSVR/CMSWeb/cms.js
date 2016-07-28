(function() {

'use strict';
    define([], function () {
		//App Start
        var cms = angular.module('cms', [
            //vendor
            'ngGrid',
            'ngResource',
            //'ngAnimate',
            'ui.bootstrap',
            'cms.common',
            'cms.auth',
            'ui.tree',
            'ngplus',
            'ngAnimate',
            'ngCookies',
            'ui.checkbox',
            //'ui.select',
            'ui.utils',
            'cms.directives',
            //'ui.bootstrap.cmsdatepicker',
			'angularValidator',
            'angularFileUpload',
			'angularSpectrumColorpicker',
			'ui.calendar',
			'ng-fusioncharts',
			'cms.cmsInterger',
			'cms.cmsDecimal',
			'ui.multiselect',
            'jQueryScrollbar',
			'timepickerPop',
			'am.resetField',
			'angucomplete-alt'
			, 'ui.sortable'
			, 'ngSelectSerial'
			, 'datetimepicker'
        ]);
        
        return cms;
    });
})()