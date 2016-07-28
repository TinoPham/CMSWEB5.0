/***********************************************
// INTERGER INPUT DIRECTIVE
// [cms-interger] attribute: allow input number only.
// [options]= {
		maxlength: number
	}
************************************************/

(function () {
	'use strict';
	define(['cms'], function (cms) {
		var module = angular.module('cms.cmsInterger', []);
		module.directive('cmsInterger', ['AppDefine', function (AppDefine) {
			return {
				restrict: 'A',
				require: '?ngModel',
				link: function (scope, elem, attrs, ngModel) {
					var options = {};

					//Retrict input
					$(elem).on('keydown', function (event) {
						var key = event.which || event.keyCode;
						//console.log(key);
						return key == 32 || key == 107 || key == 109 
								|| key == 187 || key == 189 || key == 69 || key == 110 || key == 190 ? false : true; //32: Spacebar, 107: -, 109: +, 187: Shift + , 189: - , 69: e , 110 & 190: .
					});

					scope.$watch(attrs.ngModel, function (newValue, oldValue) {
						if (newValue == null || newValue == "") { return; }

						options = scope.$eval(attrs.cmsInterger);
						if (!options) { return; }

						if (String(newValue).length > options.maxlength) {
							ngModel.$setViewValue(oldValue);
							ngModel.$render();
						}
						else {
							var reg = AppDefine.RegExp.NumberRestriction;
							if (!reg.test(newValue)) {
								ngModel.$setViewValue(oldValue);
								ngModel.$render();
							}
						}
					});
				}
			};
		}]);
		return module;
	});
})();