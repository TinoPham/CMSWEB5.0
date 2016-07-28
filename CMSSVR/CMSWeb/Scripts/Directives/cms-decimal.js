/***********************************************
// DECIMAL INPUT DIRECTIVE
// [cms-decimal] attribute: allow input decimal number only.
// [options] = {
		maxlength: number
		digits: number
	}
Ex: 
1. <input tyep="text" cms-decimal={maxlength: 5, digits: 4} />
2. <input tyep="number" cms-decimal={maxlength: 5, digits: 4} />

************************************************/

(function () {
	'use strict';
	define(['cms'], function (cms) {
		var module = angular.module('cms.cmsDecimal', []);
		module.directive('cmsDecimal', ['AppDefine', function (AppDefine) {
			return {
				restrict: 'A',
				require: '?ngModel',
				link: function (scope, elem, attrs, ngModel) {
					var options = {};
					
					//Retrict input
					$(elem).on('keydown', function (event, data) {
						var key = event.which || event.keyCode;
						//console.log(key);
						//32: Spacebar, 107: -, 109: +, 187: Shift + , 189: - , 69: e
						return key == 32 || key == 107 || key == 109 || key == 187
							|| key == 189 || key == 69 ? false : true; 
					});

					scope.$watch(attrs.ngModel, function (newValue, oldValue) {
						if (newValue === undefined || newValue === null || newValue === "") {
							return;
						}

						options = scope.$eval(attrs.cmsDecimal);
						if (!options) { return; }

						if (String(newValue).length > options.maxlength) {
							ngModel.$setViewValue(oldValue);
							ngModel.$render();
						}
						else {
							var reg = AppDefine.RegExp.DecimalRestriction;
							if (!reg.test(newValue)) {
								ngModel.$setViewValue(oldValue);
								ngModel.$render();
							}
							else {
							var decimalArr = String(newValue).split('.');
							var decimal = decimalArr[0];
							var digits = decimalArr[1];

							if (digits != null && (digits.length > options.digits)) { //|| !parseInt(digits)
								ngModel.$setViewValue(oldValue);
								ngModel.$render();
							}
							}
						}
					});
				}
			};
		}]);
		return module;
	});
})();