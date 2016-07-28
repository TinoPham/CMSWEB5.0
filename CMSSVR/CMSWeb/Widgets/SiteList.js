//$(document).ready(function () {
//	/*Check all function,start*/
//	$(function () {
//		$('.checkall').on('click', function (e) {
//			$('#user-list-permission').find(':checkbox').prop('checked', this.checked);
//			$('#user-list-permission').find('.user-permiss-box').toggleClass("active",this.checked);
//		});
//	});
//	/*Check all function,end*/
//	var selector = '#user-list-permission .user-permiss-box';
//	$(selector).on('click', function (e) {
//		var cb = $(this).find(":checkbox")[0];
//		if (e.target != cb) cb.checked = !cb.checked;
//		$(this).toggleClass("selected", cb.checked);
//		$(this).toggleClass("active", cb.checked);
//		if (!cb.checked) $(this).focusout();
//		//if (cb.checked) {
//		//	$(this).addClass('active');
//		//}
//		//else {
//		//	$(this).removeClass('active');
//		//}
//	});


//	/*Multiple select Calendar event, Goal ID,start*/
//	$('#ddlCalendarEvent').SumoSelect({ placeholder: 'Select calendar event' });
//	$('#ddlGoalID').SumoSelect({ placeholder: 'Select a goal' });
//	/*Multiple select calendar event,end*/

//	$('[data-toggle=popover]').on('shown.bs.popover', function () {
//		// $('.popover').css('top',parseInt($('.popover').css('top')) + (-10) + 'px')
//	})
//	$('[data-toggle="popover"]').popover({
//		placement: function (tip, ele) {
//			var width_device = $(window).width();
//			var placement = 'right';
//			if (width_device < 768) { //phones
//				placement = 'bottom';
//			}
//			else if (width_device >= 768 && width_device < 992) { //tablets
//				placement = 'bottom';
//			}
//			else if (width_device >= 992 && width_device < 1200) {//Laptops
//				placement = 'right';
//			}
//			else if (width_device >= 1200) { //desktop
//				placement = 'right';
//			}
//			return placement;
//		},
//		template: '<div class="popover"><div class="arrow"></div><div class="popover-inner"><div class="popover-content"><p></p></div></div></div>'
//	});
//});
(function () {
	'use strict';
	define(['cms'],
		function (cms) {
			cms.register.controller('sitelistCtrl', sitelistCtrl);
			sitelistCtrl.$inject = ['$scope', '$modal', 'cmsBase', 'dataContext'];
			function sitelistCtrl($scope, $model, cmsBase, dataContext) {
				var vm = this;
				$scope.sites = [
					{ siteKey: 1, serverID: 'Site Toronto 1', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '',selected: false }
					, { siteKey: 2, serverID: 'Site Toronto 2', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 3, serverID: 'Site Toronto 3', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 4, serverID: 'Site Toronto 4', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 5, serverID: 'Site Toronto 5', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 6, serverID: 'Site Toronto 6', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 7, serverID: 'Site Toronto 7', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 8, serverID: 'Site Toronto 8', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 9, serverID: 'Site Toronto 9', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 10, serverID: 'Site Toronto 10', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 11, serverID: 'Site Toronto 11', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 12, serverID: 'Site Toronto 12', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 13, serverID: 'Site Toronto 13', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 14, serverID: 'Site Toronto 14', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 15, serverID: 'Site Toronto 15', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 16, serverID: 'Site Toronto 16', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 17, serverID: 'Site Toronto 17', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 18, serverID: 'Site Toronto 18', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 19, serverID: 'Site Toronto 19', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 20, serverID: 'Site Toronto 20', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 21, serverID: 'Site Toronto 21', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 22, serverID: 'Site Toronto 22', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 23, serverID: 'Site Toronto 23', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 24, serverID: 'Site Toronto 24', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 25, serverID: 'Site Toronto 25', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 26, serverID: 'Site Toronto 26', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 27, serverID: 'Site Toronto 27', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 28, serverID: 'Site Toronto 28', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
					, { siteKey: 29, serverID: 'Site Toronto 29', macAddress: '', userID: 1, regionKey: 1, licenseID: '', goalName: '', calendarEvent: '', selected: false }
				];
				$scope.pop = { data: $scope.sites, mytime : $scope.mytime };

				//var dataTooltips = "<div class='caption-cus'><h4 class='caption-title'>Site Toronto 2</h4><div class='img-camera-info'><div class='user-image-info-item'><label class='mac-address-key'>Mac Address: </label><label class='mac-address-value'>00-1c-c0-f0-cf-01</label></div><div class='user-image-info-item'><label class='item-key'>License ID: </label><label>89094832</label></div><div class='user-image-info-item'><label class='item-key'>Goal name: </label><label>goal1, goal2</label></div><div class='user-image-info-item'><label class='item-key'>Calendar Event: </label><label>event A, event B, event C</label></div></div></div>";
				//vm.htmlTooltip = dataTooltips;

				vm.modalShown = false;
				vm.showDialog = function (site) {
					//if (!$scope.modalShown) {
					//	$scope.modalShown = true;
					//	var userInstance = $modal.open({
					//		templateUrl: '',
					//		controller: 'editadduserCtrl as vm',
					//		size: 'md',
					//		backdrop: 'static',
					//		backdropClass: 'modal-backdrop',
					//		keyboard: false,
					//		resolve: {
					//			items: function () {
					//				return user;
					//			}
					//		}
					//	});

					//	userInstance.result.then(function (data) {
					//		user = data;
					//		$scope.modalShown = false;
					//	});
					//}
				}
				vm.query = '';
				vm.selected = {};
				vm.select = function (site) {
					if (site.selected)
						site.selected = false;
					else
						site.selected = true;
				};
			}
		}
	);
})();