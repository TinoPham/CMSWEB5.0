(function () {
	'use strict';
	define(['cms']
		, function (cms) {
			cms.register.controller('TrafficDetailCtrl', TrafficDetailCtrl);
			TrafficDetailCtrl.$inject = ['$scope', '$modalInstance', 'items', 'AppDefine'];

			function TrafficDetailCtrl($scope, $modalInstance, items, AppDefine) {
				var apiURL = AppDefine.Api.SiteChanImage;//"../api/cmsweb/site/GetImageChannel?";
				$scope.modalData = {
					channels: [],
					time: '',
					countInValue: null,
					countOutValue: null,
					forcastValue: null
				};

				active();

				function active() {
					if (!items || !items.data) {
						return;
					}

					$scope.headerString = items.titleString;
					$scope.modalData = {
						channels: getChannels(),
						time: items.data.time,
						countInValue: items.data.countInValue,
						countOutValue: items.data.countOutValue,
						forcastValue: items.data.forcastValue
					};
					$scope.$applyAsync();
				}

				$scope.getImages = function (channel) {
					if (channel) {
						return apiURL + "name=C_" + $scope.pad(channel.ChannelNo + 1) + ".jpg&kdvr=" + channel.KDVR.toString();
					}
				}

				$scope.pad = function (number) {
					return (number < 10 ? '0' : '') + number.toString();
				}

				$scope.cancel = function () {
					$modalInstance.close();
				};

				function getChannels() {
					var ret = [];
					angular.forEach(items.data.channels, function (x, key) {
						var file = {
							//active: false,
							ChannelNo: x.DVR_ChannelNo,
							Name: x.Name,
							KDVR: x.KDVR
						}
						ret.push(file);
					});
					return ret;
				}
			}
		});
})();