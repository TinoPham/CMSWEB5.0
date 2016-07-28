(function () {
	'use strict';
	define([], function () {
		var module = angular.module('cms.log', []).factory('cmsLog', cmsLog);
		cmsLog.$inject = ['$log'];
		function cmsLog($log) {

			var service = {
				error: error,
				info: info,
				success: success,
				warning: warning,
				log: $log.log
			};

			return service;

			function error(message, data, title) {
				toastr.error(message, title);
			}

			function info(message, data, title) {
				toastr.info(message, title);
			}

			function success(message, data, title) {
				toastr.success(message, title);
			}

			function warning(message, data, title) {
				toastr.warning(message, title);
			}
		}
		return module;

	});
})();