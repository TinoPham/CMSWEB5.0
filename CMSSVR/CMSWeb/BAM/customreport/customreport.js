(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('customreportCtrl', customreportCtrl);

		customreportCtrl.$inject = ['$rootScope', '$scope', '$stateParams', '$filter', '$timeout', '$window', 'cmsBase', 'bamhelperSvc', 'AppDefine'];

		function customreportCtrl($rootScope, $scope, $stateParams, $filter, $timeout, $window, cmsBase, bamhelperSvc, AppDefine) {

			var vm = this;
			vm.isfilter = false;
			vm.sitesKeys = [];
			vm.params = {
				reportId: $stateParams.id,
				type: "rdlc",
				options: {
					languageId: cmsBase.translateSvc.getCurrentLanguage(),
					sites: "",
					sdate: "",
					edate: "",
					typeCustom: ""
				}

			}
			console.log(vm.params);

			active();

			function active() {
				vm.isfilter = true;


				setFrameheight();

				if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.BamSelectedNode) {
					vm.sitesKeys[0] = $scope.$parent.$parent.BamSelectedNode.ID;
				} else {
					if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {

						var firstsite = bamhelperSvc.getFirstSites($scope.$parent.$parent.treeSiteFilter);
						if (firstsite) {
							vm.sitesKeys[0] = firstsite.ID;
						} else {
							return;
						}
					} else {
						return;
					}
				}
				if (CheckReport() === true) {
					if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
						vm.sitesKeys = [];
						bamhelperSvc.getSitesFromNode($scope.$parent.$parent.treeSiteFilter, vm.sitesKeys);
					}
				}
				vm.params.options.languageId = cmsBase.translateSvc.getCurrentLanguage();
				vm.params.options.sdate = $rootScope.BamFilter.dateReport.toDateParam();
				vm.params.options.edate = $rootScope.BamFilter.startdateReport.toDateParam();
				vm.params.options.typeCustom = $rootScope.CustomFilter.TypeCustom.ID;
				vm.params.options.sites = vm.sitesKeys.join();
				vm.isfilter = false;
			}

			function setFrameheight() {
				var croll = angular.element('.tree-panel');
				var oftop = findOffsetTop(croll[0]);
				var scrollelementtree = angular.element('.report-custom-frame');
				scrollelementtree.css('height', $window.innerHeight - oftop - 50 + 'px');
			}

			function findOffsetTop(elm) {
				var result = 0;
				if (elm.offsetParent) {
					do {
						result += elm.offsetTop;
					} while (elm = elm.offsetParent);
				}
				return result;
			}

			$scope.$on(AppDefine.Events.BAMSELECTNODE, function (e, node) {

				if (node.Type === AppDefine.NodeType.Site) {
					vm.sitesKeys[0] = node.ID;
				} else if (node.ParentKey == null || node.$parent == undefined) {
					$scope.sitesKey = [];
					var firstsite = bamhelperSvc.getFirstSites($scope.$parent.$parent.treeSiteFilter);
					if (firstsite) {
						vm.sitesKeys[0] = firstsite.ID;
					}
				}
				if (CheckReport() === true) {
					vm.sitesKeys = [];
					bamhelperSvc.getSitesFromNode(node, vm.sitesKeys);
				}
				vm.params.options.languageId = cmsBase.translateSvc.getCurrentLanguage();
				vm.params.options.sdate = $rootScope.BamFilter.dateReport.toDateParam();
				vm.params.options.edate = $rootScope.BamFilter.startdateReport.toDateParam();
				vm.params.options.typeCustom = $rootScope.CustomFilter.TypeCustom.ID;
				vm.params.options.sites = vm.sitesKeys.join();
				vm.isfilter = false;

			});

			$scope.$on(AppDefine.Events.GETBAMREPORTDATA, function (e) {

				if (vm.sitesKeys.length === 0) {

					if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
						vm.isfilter = true;
						var firstsite = bamhelperSvc.getFirstSites($scope.$parent.$parent.treeSiteFilter);
						if (firstsite) {
							vm.sitesKeys[0] = firstsite.ID;
						}
					}
				}
				
				if (CheckReport() === true) {
					if ($scope.$parent && $scope.$parent.$parent && $scope.$parent.$parent.treeSiteFilter) {
						vm.sitesKeys = [];
						bamhelperSvc.getSitesFromNode($scope.$parent.$parent.treeSiteFilter, vm.sitesKeys);
					}
				}

				vm.isfilter = true;
				vm.params.options.languageId = cmsBase.translateSvc.getCurrentLanguage();
				vm.params.options.sdate = $rootScope.BamFilter.dateReport.toDateParam();
				vm.params.options.edate = $rootScope.BamFilter.startdateReport.toDateParam();
				vm.params.options.typeCustom = $rootScope.CustomFilter.TypeCustom.ID;
				vm.params.options.sites = vm.sitesKeys.join();
				$timeout(function () { vm.isfilter = false; }, 100);

			});

			function CheckReport() {
				var reportId = $stateParams.id;
				if (reportId) {
					var menu = Enumerable.From($rootScope.GCustomMenus).Where(function (x) { return x.ReportId == reportId; }).FirstOrDefault();

					if (menu) {
						// currentCustomReports check show/hide Combobox TypesCustom
						vm.currentCustomReports = menu.Name;
						if (menu.Name === AppDefine.State.BAM_KEYPERFORMANCEINDICATORS) {
							return true;
						}
					}
				}
				return false;
			}

			// init

			//$('.report-custom-frame').bind('load', function (event) {
			//    Zoom();
			//});

			function Zoom() {
				$timeout(function () {
					//alert("Hello");
					var content = angular.element(".panelContent");
					var body = window.innerWidth;
					//var parent = angular.element("#ReportViewerManager_ctl09");
					//var parent = document.getElementById("ReportViewerManager_ctl09");
					//var x = angular.element("#VisibleReportContentReportViewerManager_ctl09");
					var iframe = angular.element(".report-frame");
					if (iframe === undefined || iframe.length === 0) {
						return;
					}
					var document = iframe.contents();
					var parent = document[0].getElementById("ReportViewerManager_ctl09");
					var x = document[0].getElementById("VisibleReportContentReportViewerManager_ctl09");

					var reportDiv = $("div[id*='_oReportCell']").css("overflow-x");

					var form = document[0].getElementById("form1");
					//var reportDiv = $("div[id$='P4d4efb60980a401f873815114950fa2b_1_oReportCell']").css("overflow-x");


					//var namecss = $(".report-frame").find("#form1").css("overflow-x");

					if (content === undefined || x === undefined || content === null || x === null) {
						return;
					}
					if (x.offsetWidth > 0 && x.offsetWidth !== undefined) {
						//var div = x.firstChild;
						//var table = div.firstChild;
						var table = x.firstChild;
						do {
							if (table === undefined || table === null) {
								return;
							}
							if (table.localName === "table") {
								break;
							}
							table = table.firstChild;
						}
						while (1);
						if (table !== undefined && table !== null) {
							if (table.offsetWidth > 0 && (content[0].offsetWidth - table.offsetWidth > 10)) {
								for (var i = 0; i <= 10; i++) {
									table.style.zoom = (100 + (i * 5)) / 100;
									table.style.MozTransform = 'scale(' + (100 + ((i - 1) * 5)) / 100 + ')';
									table.style.MozTransformOrigin = 'top';
									var widthparent = $(".report-frame").width() - 20;
									//var widthchild = form.scrollWidth;
									var widthchild = table.scrollWidth * ((100 + (i * 5)) / 100);
									if (table.offsetWidth > 0 && widthparent <= widthchild) {
										if (((100 + ((i - 1) * 5)) / 100) < 1) {
											table.style.zoom = 1;
											table.style.MozTransform = 'scale(1)';
											table.style.MozTransformOrigin = 'top';
										} else {
											table.style.zoom = (100 + ((i - 1) * 5)) / 100;
											table.style.MozTransform = 'scale(' + (100 + ((i - 1) * 5)) / 100 + ')';
											table.style.MozTransformOrigin = 'top';
										}
										return;
									}
								}
								//table.style.zoom = temp.style.zoom;
							} else if (table.offsetWidth > 0 && content[0].offsetWidth - table.offsetWidth <= 10) {
								table.style.zoom = 1;
								table.style.MozTransform = 'scale(1)';
								table.style.MozTransformOrigin = 'top';
							}
						}
					}
				}, 200);
			}

			// Change zoom when hide/show tree right
			$scope.$watch(
				function () {
					var iframe = angular.element(".report-frame");
					if (iframe === undefined || iframe.length == 0) {
						return;
					}
					var document = iframe.contents();
					var parent = document[0].getElementById("ReportViewerManager");
					if (parent === undefined || parent === null) {
						return;
					}
					if (parent.offsetWidth === null || parent.offsetWidth === undefined) {
						return;
					}
					return parent.offsetWidth;
				},
				function (newValue, oldValue) {
					if (newValue != oldValue) {

						Zoom();
					}
				}
			);

			// Resize
			angular.element($window).on("resize", Zoom);


		}
	});
})();