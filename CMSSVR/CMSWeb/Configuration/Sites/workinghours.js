(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('workinghoursCtrl', workinghoursCtrl);

		workinghoursCtrl.$inject = ['$scope', '$modalInstance', 'cmsBase', 'dataContext', 'items', 'siteId', 'AppDefine'];
		function workinghoursCtrl($scope, $modalInstance, cmsBase, dataContext, items, siteId, AppDefine) {
			var vm = this;
			var defaultDate = function () {
				var d = new Date();
				d.setHours(0, 0, 0);
				return d;
			}
			var workHours = function () {
				return {
					ScheduleId: 0,
					OpenTime: defaultDate(),
					CloseTime: defaultDate(),
					SiteId: 0
				}
			};
			vm.workingHours = [
                { Id: 0, Name: 'SUN', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 1, Name: 'MON', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 2, Name: 'TUE', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 3, Name: 'WED', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 4, Name: 'THU', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 5, Name: 'FRI', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 6, Name: 'SAT', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false },
                { Id: 7, Name: 'HOL', isEdit: false, OpenTime: defaultDate(), CloseTime: defaultDate(), Checked: false }
			];
			function resetDate(curDate, wkTime) {
				wkTime.setDate(curDate.getDate());
				wkTime.setMonth(curDate.getMonth());
				wkTime.setFullYear(curDate.getFullYear());
			}
			function setTimes(curDate, wkTime) {
				wkTime.setHours(curDate.getHours());
				wkTime.setMinutes(curDate.getMinutes());
				wkTime.setSeconds(curDate.getSeconds());
			}

			active();

			function active() {
				var wksize = vm.workingHours.length;
				for (var i = 0; i < wksize; i++) {
					setWkHours(vm.workingHours[i]);
				}
			}

			function setWkHours(wkh) {
				if (!items) { return; }
				var wksize = items.length;
				for (var i = 0; i < wksize; i++) {
					if (items[i].ScheduleId === wkh.Id) {
						wkh.Checked = true;
						//wkh.OpenTime = new Date(items[i].OpenTime);
						//wkh.CloseTime = new Date(items[i].CloseTime);
						setTimes(new Date(items[i].OpenTime), wkh.OpenTime);
						setTimes(new Date(items[i].CloseTime), wkh.CloseTime);
					}
				}
			}

			vm.SaveWkHours = function () {
				var curDay = new Date();
				var result = [];
				var wksize = vm.workingHours.length;
				for (var i = 0; i < wksize; i++) {
					if (vm.workingHours[i].Checked === true) {
						var wkh = new workHours();
						wkh.SiteId = siteId;
						wkh.ScheduleId = vm.workingHours[i].Id;
						resetDate(curDay, vm.workingHours[i].OpenTime);
						resetDate(curDay, vm.workingHours[i].CloseTime);
						if (vm.workingHours[i].OpenTime.getTime() > vm.workingHours[i].CloseTime.getTime()) {
							var msg = vm.workingHours[i].Name + ": " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.WORKING_HOUR_VALIDATE);
							cmsBase.cmsLog.warning(msg);
							return;
						}
						else {
							wkh.OpenTime = vm.workingHours[i].OpenTime;
							wkh.CloseTime = vm.workingHours[i].CloseTime;
							result.push(wkh);
						}
					}
				}
				$modalInstance.close(result);
			}

			$scope.CloseWk = function () {
				$modalInstance.close();
			}
		}

	});
})();