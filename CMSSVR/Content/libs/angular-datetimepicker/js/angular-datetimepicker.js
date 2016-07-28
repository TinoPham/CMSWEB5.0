//source: https://github.com/diosney/angular-bootstrap-datetimepicker-directive

'use strict';
angular.module('datetimepicker', []).provider('datetimepicker', function () {
	var default_options = {};
	this.setOptions = function (options) {
		default_options = options;
	};

	this.$get = function () {
		return {
			getOptions: function () {
				return default_options;
			}
		};
	};
})
.directive('datetimepicker', ['$timeout', 'datetimepicker',
	function ($timeout, datetimepicker) {
		var default_options = datetimepicker.getOptions();

		return {
			require: '?ngModel',
			restrict: 'AE',
			scope: {
				datetimepickerOptions: '@'
			},
			link: function ($scope, $element, $attrs, ngModelCtrl) {
				var passed_in_options = $scope.$eval($attrs.datetimepickerOptions); //$scope.$eval($scope.datetimepickerOptions);
				var options = jQuery.extend({}, default_options, passed_in_options);

				$element.on('dp.change', function (e) {
					if (ngModelCtrl) {
						$timeout(function () {
							//ngModelCtrl.$setViewValue(e.target.value);
							ngModelCtrl.$setViewValue(e.date._d);
						});
						$scope.$applyAsync();
					}
				}).datetimepicker(options);

				$scope.$watch('datetimepickerOptions', function (newVal, oldVal) {
					console.log(newVal);
					if (newVal !== oldVal) {
						passed_in_options = $scope.$eval(newVal);
						options = jQuery.extend({}, default_options, passed_in_options);
						$element.data('DateTimePicker').options(options);
						$scope.$applyAsync();
					}
				});

				function setPickerValue() {
					var date = options.defaultDate || null;

					if (ngModelCtrl && ngModelCtrl.$viewValue) {
						date = ngModelCtrl.$viewValue;
					}

					$element.data('DateTimePicker').date(moment(date));
					//ngModelCtrl.$setViewValue(date);
				}

				if (ngModelCtrl) {
					ngModelCtrl.$render = function () {
						setPickerValue();
					};
				}

				setPickerValue();
			}
		};
	}
])
.directive('cmsdatetimepicker', ['$timeout',
	function ($timeout) {
		return {
			require: '?ngModel',
			restrict: 'EA',
			scope: {
				options: '@',
				onChange: '&',
				onClick: '&'
			},
			link: function ($scope, $element, $attrs, controller) {
				$scope.$watch('options', function (newVal, oldVal) {
					if (newVal !== oldVal) {
						$element.data('DateTimePicker').options($scope.$eval(newVal));
						$scope.$applyAsync();
					}
				});

				$element.on('dp.change', function () {
					$timeout(function () {
						var dtp = $element.data('DateTimePicker');
						if (!dtp) { return; }
						controller.$setViewValue(dtp.date()._d);
						$scope.onChange();
					});
				});

				$element.on('click', function () {
					//show popup datepicker
					var options = $scope.$eval($attrs.options);
					if (options.ignoreReadonly) {
						$scope.onClick();
					}
				});

				controller.$render = function () {
					if (!!controller) {
						if (controller.$viewValue === undefined) {
							controller.$viewValue = null;
						}
						else if (!(controller.$viewValue instanceof moment)) {
							controller.$viewValue = moment(controller.$viewValue);
						}
						$element.data('DateTimePicker').date(controller.$viewValue);
					}
				};

				$element.datetimepicker($scope.$eval($attrs.options));
			}
		};
	}
]);