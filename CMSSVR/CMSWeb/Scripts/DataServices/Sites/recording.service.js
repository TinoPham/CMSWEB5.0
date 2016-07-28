(function () {
	'use strict';
	define(['cms'], recordingSrv);
	function recordingSrv(cms) {
		cms.register.service('recordingservice', recordingservice);
		recordingservice.$inject = ['$resource', 'cmsBase', 'AppDefine','$log'];
		function recordingservice($resource, cmsBase, AppDefine, $log) {

		}
	}
})();