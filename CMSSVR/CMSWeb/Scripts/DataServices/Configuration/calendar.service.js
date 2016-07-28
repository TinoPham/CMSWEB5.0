(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.factory('calendar.service', calendarSvc);

		calendarSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http','$q'];

		function calendarSvc(AppDefine, Cookies, $resource, $http , $q) {
			//var serviceBase = "/";
			var apibase = AppDefine.Api.Calendar;
			var CalendarAPI = $resource(apibase, {id :'@id'}, {
			    get: { method: 'GET', url: apibase + '/CalendarEvent', isArray: false, headers: cms.EncryptHeader(),interceptor: { response: function ( response ) { return response; } } }
				, post: { method: 'POST', url: apibase + '/CalendarEvent', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } }
				, edit: { method: 'PUT', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } }
				, del: { method: 'DELETE', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } }
				, getCalendarList: { method: 'GET', url: apibase + '/GetCalendarList', isArray: false, headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } }
			});
			//, getsites: { method: 'GET', url: apibase + '/RegionSites' }
			//, getrecipients: { method: 'GET', url: apibase + '/Recipients', isArray: true }

			var getAgenda = function (model, successFn, ErrorFn) {
			    CalendarAPI.get( function ( response ) {
			        var data = cms.GetResponseData(response);
					if (successFn != null)
					    successFn( data )
				}, function(response){
					if (ErrorFn != null)
						ErrorFn(response);
				});
			}
			var getDataDay = function (model, successfunction, ErrorFunction) {
				CalendarAPI.post(model).$promise.then(
					function ( response ) {
					    var data = cms.GetResponseData( response );
						if (successfunction != null)
						    successfunction( data )
					},
					function (response) {
						if (ErrorFunction != null)
							ErrorFunction(response);
					}
				);
			}
			var getDataWeek = function (model, successfunction, ErrorFunction) {
				CalendarAPI.post(model).$promise.then(
					function ( response ) {
					    var data = cms.GetResponseData( response );
						if (successfunction != null)
							successfunction(data)
					},
					function (response) {
						if (ErrorFunction != null)
							ErrorFunction(response);
					}
				);
			}

			//var getRegionSites = function (model, successFn, ErrorFn) {
			//	CalendarAPI.getsites(function (response) {
			//		if (successFn != null)
			//			successFn(response)
			//	}, function (response) {
			//		if (ErrorFn != null)
			//			ErrorFn(response);
			//	});
			//}
			//var getRecipients = function (model, successFn, ErrorFn) {
			//	CalendarAPI.getrecipients(function (response) {
			//		if (successFn != null)
			//			successFn(response)
			//	}, function (response) {
			//		if (ErrorFn != null)
			//			ErrorFn(response);
			//	});
			//}
			var getCalendarList = function (successFn, ErrorFn) {
			    CalendarAPI.getCalendarList( function ( response ) {
			        var data = cms.GetResponseData( response );
					if (successFn != null)
						successFn(data)
				}, function (response) {
					if (ErrorFn != null)
						ErrorFn(response);
				});
			}
			var saveCalendar = function (model, successFn, ErrorFn) {
			    CalendarAPI.post( model, function ( response ) {
			        var data = cms.GetResponseData( response );
					if (successFn != null)
						successFn(data)
				}, function (response) {
					if (ErrorFn != null)
						ErrorFn(response);
				});
			}
			return {
				create: createRepo // factory function to create the repository
			};

			function createRepo() {
				var dataRet = {
					CalAgenda: getAgenda,
					getDataByDay: getDataDay,
					getDataByWeek: getDataWeek,
					//RegionSites: getRegionSites,
					//Recipients: getRecipients,
					GetCalendarList: getCalendarList,
					SaveCalendar: saveCalendar
				}
				return dataRet;
			}

			/*
			function getAll() {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/calendar/getAll').success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {
					defer.reject(err);
				});
				return defer.promise;
			}
			function getDataByDay(curDay) {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/calendar/getDay/' + curDay).success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {
					defer.reject(err);
				});
				return defer.promise;
			}
			function getDataByWeek(startDay) {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/calendar/getWeek/' + startDay).success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {
					defer.reject(err);
				});
				return defer.promise;
			}
			function getAgenda() {
				var defer = $q.defer();
				$http.get(serviceBase + 'api/calendar/getAgenda').success(function (response) {
					defer.resolve(response);
				}).error(function (err, status) {
					defer.reject(err);
				});
				return defer.promise;
			}
			*/
		}
	});
})();