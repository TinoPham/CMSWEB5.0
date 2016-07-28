(function() {
	'use strict';

	define(['cms'], function(cms) {
		cms.controller('aboutWidgetCtrl', aboutWidgetCtrl);

		aboutWidgetCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'AppDefine', '$timeout'];

		function aboutWidgetCtrl($scope, $modalInstance, cmsBase, AppDefine, $timeout) {

		    $scope.about = {
		        title: 'About',
		        name: 'CMS WEB',
		        version: AppDefine.CMSVERSION,
		        fullName: 'CENTER_MANAGEMENT_SOFTWARE',
		        company: 'i3 International Inc.',
		        companyUrl: 'http://i3international.com/',
		        buildDate: AppDefine.BUILDDATE,
		        buildNo: AppDefine.BUILDNO,
                copyYear: 2015,
                url: 'licenses/',
                language: 'en'
		    }

		    active();

		    function active() {
		        var currentLang = cmsBase.translateSvc.getCurrentLanguage();
		        $scope.about.url = $scope.about.url + currentLang + '.html';
		    }

		    $timeout(function() {
		        $('.about-content').on('scroll', function() {
		            if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {
		                $('#about-accepted').prop("disabled", false);
		            }
		        });
		    }, 0);

		    $scope.Close = function () {
				$modalInstance.close();
		    }
		}
	});
})();