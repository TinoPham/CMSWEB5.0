(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.filter('workday', ['cmsBase', function (cmsBase) {
			var wDate = [
                { Id: 0, Name: 'SUN' },
                { Id: 1, Name: 'MON' },
                { Id: 2, Name: 'TUE' },
                { Id: 3, Name: 'WED' },
                { Id: 4, Name: 'THU' },
                { Id: 5, Name: 'FRI' },
                { Id: 6, Name: 'SAT' },
                { Id: 7, Name: 'HOL' }
			];

			return function (day) {
				var result = '';

				wDate.forEach(function (wdate) {
					if (wdate.Id === day) {
						result = cmsBase.translateSvc.getTranslate(wdate.Name);
					}
				});

				return result;
			}
		}]);

		cms.register.filter('cmshighlight', ['$sce', function ($sce) {
			return function (text, phrase) {
				if (phrase)
					text = text.replace(new RegExp('(' + phrase + ')', 'gi'),
                        '<span class="highlighted">$1</span>');

				return $sce.trustAsHtml(text);
			}
		}]);

		//ThangPham, Filter Number support round number, Jan 08 2016
		cms.register.filter('salenumber', function () {
			return function (value, format) {
				return numeral(value).format(format);
			}
		});

		cms.register.service('siteadminService', ['cmsBase', 'AppDefine', function (cmsBase, AppDefine) {

			var svc = {
				ShowError: showError,
				filterSites: filterSites,
				findNode: findNode,
				siteCountFn: siteCountFn
			}

			return svc;

			function findNode(site, type, id) {
				return finditem(site, type, id);
			}

			function finditem(site, type, id) {

				if (site.Type === type && site.ID === id) {
					return site;
				}

				var len = site.Sites.length;
				var result;
				for (var i = 0; i < len; i++) {
					var s = site.Sites[i];
					if (s.Sites.length > 0) {
						var rt = finditem(s, type, id);
						if (rt) {
							result = rt;
							break;
						}
					} else {
						if (s.Type === type && s.ID === id) {
							result = s;
							break;
						}
					}
				}
				return result;
			}

			function showError(error) {
				if (error.data && error.data.ReturnMessage) {
					var msg = "Unknow_Error";
					if (error.data.ReturnMessage.length > 0) {
						msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					} else {
						if (error.data.Data) {
							if (error.data.Data.ReturnMessage.length > 0) {
								msg = cmsBase.translateSvc.getTranslate(error.data.Data.ReturnMessage[0]);
								angular.forEach(error.data.Data.ValidationErrors, function (fieldError, index) {
									msg += fieldError;
								});
							}
						}
					}
					cmsBase.cmsLog.error(msg);
				} else {
					cmsBase.cmsLog.error(error.data.Message);
				}
			}

			function filterSites(site, fText) {
				var len = site.Sites.length;
				var temp = false, isChildshow = false;

				if (fText == "" || site.Name && site.Name.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(fText.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
					isChildshow = true;
				}

				for (var i = 0; i < len; i++) {
					var s = site.Sites[i];
					if (s.Sites.length > 0) {
						var rt = filterSites(s, fText);
						if (rt === true) {
							//s.Sites = rt;
							//temp.push(s);
							s.isShow = true;
							temp = true;
							continue;
						}
						if (isChildshow === true || s.Name && s.Name.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(fText.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
							//s.Sites = [];
							s.isShow = true;
							temp = true;
						} else {
							s.isShow = false;
						}
					} else {
						if (isChildshow === true || s.Name && s.Name.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(fText.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
							s.isShow = true;
							temp = true;
						} else {
							s.isShow = false;
						}
					}
				}
				return temp;
			}

			function siteCountFn(node) {
				var sum = 0;
				if (!node) return 0;

				if (node.Type === AppDefine.NodeType.Site && (angular.isUndefined(node.isShow) || node.isShow === true)) {
					sum = sum + 1;
				} else {
					if (node.Sites.length > 0 && node.Type === AppDefine.NodeType.Region && (angular.isUndefined(node.isShow) || node.isShow === true)) {
						node.Sites.forEach(function (n) {
							sum = sum + siteCountFn(n);
						});
					} else {
						return 0;
					}
				}
				return sum;
			}
		}]);

	});
})();