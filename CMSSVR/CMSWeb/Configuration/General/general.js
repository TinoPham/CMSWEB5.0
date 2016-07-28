(function () {
	'use strict';
	define(['cms',
        'configuration/UserGroups/userGroupsDialog.js',
		'Widgets/EventActivities.js',
		'Widgets/UserList.js',
		'Widgets/SiteList.js'
	],
        function (cms) {
            cms.register.controller('generalCtrl', generalCtrl);
            generalCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext'];
            function generalCtrl($scope, $modal, cmsBase, dataContext) {
        		
        	}
        });
})();