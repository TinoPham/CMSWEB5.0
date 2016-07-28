(function () {
	'use strict';

	define([
		'cms',
		'Scripts/Directives/treeComponent',
		'Scripts/Services/chartSvc',
		'DataServices/POSCommon.service',
		'DataServices/Configuration/siteadmin.service',
		'Scripts/Services/bamhelperSvc',
		'DataServices/Rebar/rebar.service',
		'Widgets/Rebar/CannedRptViewer',
        'rebar/adhocdetails/ahocreportview',
        'Widgets/Rebar/config-tran-flag',
        'DataServices/Rebar/adhoc.service',
        'rebar/adhoc/edit',
        'rebar/adhoc/editfolder',
        'rebar/adhoc/delete',
        'rebar/adhoc/filtersetting',
		'Scripts/Services/exportSvc'
	], function (cms) {
		cms.register.controller('rebarCtrl', rebarCtrl);
		rebarCtrl.$inject = ['$timeout', '$rootScope', '$scope', '$state', '$stateParams', '$filter', '$modal', 'cmsBase', 'AppDefine', 'chartSvc', 'rebarDataSvc', 'router', 'siteadmin.service', 'bamhelperSvc', 'adhocDataSvc', 'POSCommonSvc', '$q', 'exportSvc'];

		function rebarCtrl($timeout, $rootScope, $scope, $state, $stateParams, $filter, $modal, cmsBase, AppDefine, chartSvc, rebarDataSvc, router, siteDataSvc, bamhelperSvc, adhocDataSvc, POSCommonSvc, $q, exportSvc) {
			var vm = this;
			var AND_VALUE = 1;
			var OR_VALUE = 0;
			var conditionTypeConst = { Any: "Any", Lesster: "<", Equal: "=", Greater: ">" };

			var dtNow = new Date();
			$rootScope.ListReportAdhocHistory = [];
			$rootScope.rebarSearch = {
				ReportID: 1,
				DateFrom: chartSvc.beginOfDate(dtNow),
				DateTo: dtNow,
				SiteKeys: [],
				PaymentIDs: [],
				PaymentIDs_AND: AND_VALUE,
				RegIDs: [],
				RegIDs_AND: AND_VALUE,
				EmpIDs: [],
				EmpIDs_AND: AND_VALUE,
				DescIDs: [],
				DescIDs_AND: AND_VALUE,
				TransNB: 1,
				TransNB_OP: conditionTypeConst.Any,
				TransNB_AND: AND_VALUE,
				TransAmount: 1,
				TransAmount_OP: conditionTypeConst.Any,
				TransAmount_AND: AND_VALUE,
				GroupByField: 1, //default group by Site name
				PACIDs: [],
				MaxRows: 100000
			};
			$scope.maskRebarMenu = false;
			$scope.isMobile = cmsBase.isMobile;
			$scope.isiPad = navigator.userAgent.match(/iPad/i) !== null;
			var MEDIUM_PATTERN = 992;
			$scope.SearchData = {
				TenderTypeList: [],
				RegisterList: [],
				EmployeeList: [],
				DescriptionList: []
			};
			$scope.pageReady = false;
			$scope.searchBreadcrumb = {};
			$scope.conditionPrefixConst = { And: 'AND_STRING', Or: 'OR_STRING' };
			$scope.conditionTypeConst = { Any: 'Any', Lesster: "<", Equal: "=", Greater: ">" };
			$scope.searchConfig = {
				filterTypeItem: { name: 'filterTypeItem', visable: true, isReadOnly: false },
				dateFrom: { name: 'dateFrom', visable: true, isReadOnly: false, options: { format: 'L', maxDate: getMaxDate(), ignoreReadonly: false, showTodayButton: true } },
				dateTo: { name: 'dateTo', visable: true, isReadOnly: false, options: { format: 'L', maxDate: getMaxDate(), ignoreReadonly: false, showTodayButton: true } },
				tenderTypeItem: { name: 'tenderTypeItem', visable: true, conditionPrefix: $scope.conditionPrefixConst.And },
				registerItem: { name: 'registerItem', visable: true, conditionPrefix: $scope.conditionPrefixConst.And },
				transactionItem: { name: 'transactionItem', visable: true, conditionPrefix: $scope.conditionPrefixConst.And, conditionType: $scope.conditionTypeConst.Any },
				employeeItem: { name: 'employeeItem', visable: true, conditionPrefix: $scope.conditionPrefixConst.And },
				totalItem: { name: 'totalItem', visable: true, conditionPrefix: $scope.conditionPrefixConst.And, conditionType: $scope.conditionTypeConst.Any },
				descriptionItem: { isUsed: false, name: 'descriptionItem', visable: false, conditionPrefix: $scope.conditionPrefixConst.And }
			};

			$scope.isCannedRpt = false;

			$scope.$on(AppDefine.Events.STATECHANGESUCCESSHANDLER, function (event, arg) {
				$scope.isSingleDate = $state.current.name === AppDefine.State.REBAR_WEEKATGLANCE;

				checkState();
				if ($scope.isCannedRpt) {
					RefreshSiteData(AppDefine.Events.PAGEREADY, null);
				}
				else {
				    if ($state.current.name !== AppDefine.State.REBAR) {
					RefreshSiteData(null, null); //Anh, Refresh site tree when reload data, #3591, 2016-05-24
				}
				}
			});

			$scope.$on("INVALID_DATE", function (event, agr) {
				cmsBase.cmsLog.warning(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FROM_TO_DATE_INVALID_MSG));
			});

			$scope.$on("RebarDynamicFilterRender", function (event, arg) {
				$scope.searchBreadcrumb = arg;
			});

			$scope.ReLoadMenu = function (menuStatus) {
				var ID = null;
				if (vm.__thisReport !== undefined) {
					if (vm.__thisReport.IsFolder) {
						ID = vm.__thisReport.Id;
					}
				}

				adhocDataSvc.getAdhocs({ id: ID }, function (data) {
					vm.reports = data;
					console.log(vm.reports);
					if (menuStatus) {
						openPopup(".report_names");
					}
				}, function (error) {
				});
			}

			vm.params = {
				reportId: null
			};

			//active();
			vm.Init = function () {
				//if ($state.current.name === AppDefine.State.REBAR_QUICKSEARCH) {
				//    var arr_args = [AppDefine.POSItemKeys.Cameras, AppDefine.POSItemKeys.Descriptions, AppDefine.POSItemKeys.CardIDs, AppDefine.POSItemKeys.Payments];
				//$scope.InitCache(false, arr_args ).then( function () {
				//    active();
				//} );
				//} else {
				//    active();
				//}
				active();
			}

			function active() {
				menu();
				//get tree site data
				RefreshSiteData(AppDefine.Events.PAGEREADY, null);
				$scope.ReLoadMenu(false);
				checkState();
			}

			$scope.$on('$destroy', function () {
				// Make sure that the interval is destroyed too
				POSCommonSvc.ClearCache();
				console.log("rebar destroy");
			});

			$scope.InitCache = function (reload, args) {
				var deferred = $q.defer();
				var requestPromise = [];
				var time_out_load = 0;
				var valid_item;
				var cacheitem;
				for (var i = 0; i < args.length; i++) {
					valid_item = false;
					cacheitem = POSCommonSvc.GetCache(args[i]);
					valid_item = cacheitem == null || reload;
					if (valid_item == true) {
						var httpPromise = POSCommonSvc.LoadDataCache(args[i]);
						requestPromise.push(httpPromise);
						time_out_load += 1000;
					}

				}
				if (time_out_load > 0) {
					setTimeout(function () {
						deferred.resolve();
					}, time_out_load);
				}
				else
					deferred.resolve();


				return deferred.promise;
			}

			$scope.LookupCacheItem = function (name, fieldname, value, select_field) {

				var _items = POSCommonSvc.GetCache(name, true);
				if (!_items)
					return null;

				var match = $scope.LookupItem(_items, fieldname, value, select_field);
				if (match.Any())
					return match.First();
				return null;
			}

			$scope.LookupItem = function (src_array, fieldname, value, select_field) {

				var ie_enum = Enumerable.From(src_array);
				var match = ie_enum.Where(function (x) { return x[fieldname] === value; });
				if (select_field == null || select_field == undefined)
					return match;
				return match.Select(function (x) { return x[select_field]; });
			};

			function menu() {
				vm.Menus = [];
				var listMenus = router.getListCollection();
				var rebar = Enumerable.From(listMenus).Where(function (x) { return x.Name === 'rebar'; }).FirstOrDefault();

				vm.Menus = Enumerable.From(rebar.childs).GroupBy("$.Groupkey", null, function (key, g) {
					return { GroupName: key, Childs: g.ToArray() }
				}).Where(function (x) { return x.GroupName !== "hide"; }).ToArray();
			}

			function checkState() {
				$scope.advanceSearch = { isShow: true, isOpen: false };
				$scope.isSingleDate = false;
				$scope.modeSearchFor = $state.current.name;
				$scope.isCannedRpt = false;
				switch ($state.current.name) {
					case AppDefine.State.REBAR:
						//Load default page
						$scope.isSingleDate = true;
						$state.go(AppDefine.State.REBAR_WEEKATGLANCE);
						break;

					case AppDefine.State.REBAR_ADHOC:
					case AppDefine.State.REBAR_ADHOCDETAILS:
						$scope.advanceSearch = { isShow: false, isOpen: false };;
						$scope.searchConfig.dateFrom.options.format = 'L';
						$scope.searchConfig.dateFrom.options.maxDate = moment().format('L');
						$scope.searchConfig.dateFrom.isReadOnly = false;
						$scope.searchConfig.dateFrom.visable = true;
						$scope.searchConfig.dateTo.options.format = 'L';
						$scope.searchConfig.dateTo.options.maxDate = moment().format('L');
						$scope.searchConfig.dateTo.isReadOnly = false;
						$scope.searchConfig.filterTypeItem.visable = false;
						$scope.searchConfig.descriptionItem.isUsed = false;
						$scope.searchConfig.descriptionItem.visable = false;
						break;
					case AppDefine.State.REBAR_DASHBOARD:
					case AppDefine.State.REBAR_WEEKATGLANCE:
						$scope.searchConfig.dateFrom.options.format = 'L';
						$scope.searchConfig.dateFrom.isReadOnly = false;
						$scope.searchConfig.dateFrom.visable = true;
						$scope.searchConfig.dateTo.options.format = 'L';
						$scope.searchConfig.dateTo.isReadOnly = false;
						$scope.searchConfig.filterTypeItem.visable = true;
						$scope.searchConfig.descriptionItem.isUsed = false;
						$scope.searchConfig.descriptionItem.visable = false;
						break;
					case AppDefine.State.REBAR_REFUNDS:
					case AppDefine.State.REBAR_VOIDS:
					case AppDefine.State.REBAR_CANCELS:
					case AppDefine.State.REBAR_NOSALES:
					case AppDefine.State.REBAR_DISCOUNTS:
						$scope.isCannedRpt = true;
						$scope.advanceSearch = { isShow: true, isOpen: false };;
						$scope.searchConfig.dateFrom.options.format = 'MM/DD/YYYY HH:mm';
						$scope.searchConfig.dateFrom.options.maxDate = moment().format('MM/DD/YYYY HH:mm');
						$scope.searchConfig.dateFrom.isReadOnly = false;
						$scope.searchConfig.dateFrom.visable = true;
						$scope.searchConfig.dateTo.options.format = 'MM/DD/YYYY HH:mm';
						$scope.searchConfig.dateTo.options.maxDate = moment().format('MM/DD/YYYY HH:mm');
						$scope.searchConfig.dateTo.isReadOnly = false;
						$scope.searchConfig.filterTypeItem.visable = false;
						$scope.searchConfig.descriptionItem.isUsed = false;
						$scope.searchConfig.descriptionItem.visable = false;
						break;
					case AppDefine.State.REBAR_QUICKSEARCH:
						$scope.advanceSearch = { isShow: true, isOpen: true };;
						$scope.searchConfig.dateFrom.options.format = 'MM/DD/YYYY HH:mm';
						$scope.searchConfig.dateFrom.isReadOnly = false;
						$scope.searchConfig.dateFrom.visable = true;
						$scope.searchConfig.dateFrom.options.maxDate = moment().format('MM/DD/YYYY HH:mm');
						$scope.searchConfig.dateTo.options.format = 'MM/DD/YYYY HH:mm';
						$scope.searchConfig.dateTo.isReadOnly = false;
						$scope.searchConfig.dateTo.options.maxDate = moment().format('MM/DD/YYYY HH:mm');
						$scope.searchConfig.filterTypeItem.visable = false;
						$scope.searchConfig.descriptionItem.isUsed = true;
						$scope.searchConfig.descriptionItem.visable = true;
						break;
				}
			}

			$rootScope.allsites = [];
			function getallsites(sites) {
				var _temp = Enumerable.From(sites).Where(function (s) { return s.Type == 1 }).Select(function (x) { return x.ID }).ToArray();
				if (_temp.length > 0) {
					$rootScope.allsites = $rootScope.allsites.concat(_temp);
				}
				angular.forEach(sites, function (value) {
					if (value.Sites.length > 0) {
						getallsites(value.Sites);
					}
				});

			}

			function getAllRegionSites(sIDs) {
				var def = cmsBase.$q.defer();
				siteDataSvc.create().sitesByPACIDHO(function (response) {
					$scope.treeSiteFilter = response;
					$rootScope.allsites = [];
					getallsites($scope.treeSiteFilter.Sites);
					if (sIDs) {
						bamhelperSvc.setNodeSelected($scope.treeSiteFilter, sIDs);
					}
					else {
						bamhelperSvc.checkallNode($scope.treeSiteFilter);
					}

					var checkedIDs = [];
					chartSvc.GetSiteSelectedIDs(checkedIDs, $scope.treeSiteFilter.Sites);
					if (checkedIDs && checkedIDs.length > 0) {
						siteDataSvc.create().GetPacInfoBySitesHO({ siteKeys: checkedIDs.toString() }).then(function (resp) {
							console.log(resp);
							$scope.sitepacInfo = resp;
							//sessionStorage.setItem('sitepacinfo', JSON.stringify(resp));
							def.resolve(response);
						},
						function (error) {
							console.log(error);
						});
					} //if checkedIDs
				},
				function (error) {
					def.reject(error);
				});
				return def.promise;
			}

			function RefreshSiteData(evt, sIDs) {
				getAllRegionSites(sIDs).then(function () {
					if (evt == AppDefine.Events.PAGEREADY) {
						$scope.pageReady = true;
						$scope.$broadcast(AppDefine.Events.PAGEREADY, $scope.pageReady);
					}
					else {
						if (evt) {
							$scope.$broadcast(evt);
						}
					}
				});
			}

			$scope.EventRefresh = function () {
				var selectedSite = angular.copy($rootScope.siteIDs);
				RefreshSiteData(AppDefine.Events.REBARSEARCH, selectedSite); //Anh, Refresh site tree when reload data, #3591, 2016-05-24
				//$scope.$broadcast(AppDefine.Events.REBARSEARCH);
			}

			$scope.customMenuRebar = function () {
				if (document.body.clientWidth < 1201) {
					return true;
				}
				return false;
			}

			$scope.offmaskRebarMenu = function (e, item) {
				$scope.selectMenuItem = item;
				$scope.maskRebarMenu = false;
			}

			$scope.clickOutside = function (event, element) {
				if (!$scope.modalShown)
					closePopup(element);
			};

			$scope.closePopMenu = function (element) {
				closePopup(element);
			};

			function closePopup(element) {
				if (angular.element(element).hasClass('open')) {
					angular.element(element).removeClass('open');
				}
			}

			function openPopup(element) {
				if (angular.element(element).hasClass('open') == false)
					angular.element(element).addClass('open');
			}

			vm.configFlag = function () {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var showFilterModal = $modal.open({
						templateUrl: 'widgets/rebar/config-tran-flag.html',
						controller: 'configtranflagCtrl',
						resolve: {
							items: function () {
								return vm.FilterPayment;
							}
						},
						size: 'md',
						backdrop: 'static',
						keyboard: false
					});

					showFilterModal.result.then(function (data) {
						if (data) {
							vm.FilterPayment = data;
							vm.currPageEmploy = 1;
							//loadTransEmploys(vm.selectemploy, vm.currPageEmploy);
						}
						$scope.modalShown = false;
					});
				}
			}

			$scope.getItemData = function (itemName, itemData, comparison) {
				var ret;
				$scope.tooltipPayment;
				$scope.tooltipRegister;
				$scope.tooltipEmployee;

				switch (itemName) {
					case 'tenderTypeItem':
						if (!itemData || itemData.length === 0) { return 'Any'; }
						$scope.tooltipPayment = Enumerable.From($scope.SearchData.TenderTypeList).Where(function (x) { return itemData.indexOf(x.ID) !== -1; })
							.Select(function (x) { return x.Name; }).ToArray().toString();
						ret = $scope.tooltipPayment;
						break;
					case 'registerItem':
						if (!itemData || itemData.length === 0) { return 'Any'; }
						$scope.tooltipRegister = Enumerable.From($scope.SearchData.RegisterList).Where(function (x) { return itemData.indexOf(x.ID) !== -1; })
							.Select(function (x) { return x.Name; }).ToArray().toString();
						ret = $scope.tooltipRegister;
						break;
					case 'employeeItem':
						if (!itemData || itemData.length === 0) { return 'Any'; }
						$scope.tooltipEmployee = Enumerable.From($scope.SearchData.EmployeeList).Where(function (x) { return itemData.indexOf(x.ID) !== -1; })
							.Select(function (x) { return x.Name; }).ToArray().toString();
						ret = $scope.tooltipEmployee;
						break;
					case 'transactionItem':
					case 'totalItem':
						ret = comparison + " " + itemData;
						break;
				}
				return ret;
			}

			function getMaxDate() {
				return moment().format('L');
			}

			vm.showEditAhocDialog = showEditAhocDialog;
			function showEditAhocDialog(report, isNew) {

				if (report && report.IsFolder === true) {
					vm.showEditFolderAhocDialog(report, isNew);
					return;
				}

				var id = null;
				if (report) {
					id = report.Id;
				}

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var userInstance = $modal.open({
						templateUrl: 'rebar/adhoc/edit.html',
						controller: 'editadhocCtrl as vm',
						size: 'md',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return { report: report, id: id, isNew: isNew };
							}
						}
					});

					userInstance.result.then(function (data) {
						$scope.modalShown = false;
						if (data === "close") {
							$timeout(function () {
								openPopup(".report_names");
							}, 100);
							return;
						}
						else {
							if (data != null && data != undefined) {
								$scope.ReLoadMenu(true);
								if ($state && isNew == false) {
									if ($state.current.name == AppDefine.State.REBAR_ADHOCDETAILS
                                    && parseInt(data.ReportID) === parseInt($stateParams.id)) {
										//$state.go(AppDefine.State.REBAR_ADHOCDETAILS, { id: report.Id });
										$rootScope.title = data.ReportName;
										//RefreshSiteData(AppDefine.Events.PAGEEDITED, null);
										$scope.$broadcast(AppDefine.Events.REBARSEARCH);
									}
								}
							}
							return;
						}
					});
				}
			}/* END :: function showEditAhocDialog	 */

			vm.addReportInFolder = addReportInFolder;
			function addReportInFolder(report) {

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var userInstance = $modal.open({
						templateUrl: 'rebar/adhoc/edit.html',
						controller: 'editadhocCtrl as vm',
						size: 'md',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return { report: report, id: report.Id, isNew: true };
							}
						}
					});

					userInstance.result.then(function (data) {
						$scope.modalShown = false;
						if (data === "close") {
							$timeout(function () {
								openPopup(".report_names");
							}, 100);
							return;
						}
						else {
							if (data != null && data != undefined) {
								$scope.ReLoadMenu(true);
								if (data.ReportID === $stateParams.id) {
									$rootScope.title = data.ReportName;
								}
							} //if data
							return;
						}
					});
				}

			}

			vm.showEditFolderAhocDialog = showEditFolderAhocDialog;
			function showEditFolderAhocDialog(report, isNew) {

				if (vm.params.reportId && vm.params.reportId > 0) {
					//$state.go('rebar.adhoc', { id: null });
					return;
				}

				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var userInstance = $modal.open({
						templateUrl: 'rebar/adhoc/editfolder.html',
						controller: 'editfolderadhocCtrl as vm',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return { report: report, id: vm.params.reportId, isNew: isNew };
							}
						}
					});

					userInstance.result.then(function (data) {
						$scope.modalShown = false;
						if (data === "close") {
							$timeout(function () {
								openPopup(".report_names");
							}, 100);
							return;
						}
						else {
							if (data != null && data != undefined) {
								if (vm.__thisReport !== undefined) {
									vm.Foldername = data.FolderName;
                                    //vm.__thisReport = undefined;
								}
								$scope.ReLoadMenu(true);
							}
							return;
						}
					});
				}
			} /* END :: function showEditFolderAhocDialog	 */

			vm.gotoAdhocManager = function () {
				$state.go('rebar.adhoc', { id: null });
				$scope.closePopMenu('.report_names');
			}

			vm.deleteItem = function (report) {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var reportDiag = $modal.open({
						templateUrl: 'rebar/adhoc/delete.html',
						controller: 'deletereportCtrl as vm',
						size: 'sm',
						backdrop: 'static',
						backdropClass: 'modal-backdrop',
						keyboard: false,
						resolve: {
							items: function () {
								return report;
							}
						}
					});

					reportDiag.result.then(function (data) {
						$scope.modalShown = false;
						if (data === "close") {
							$timeout(function () {
								openPopup(".report_names");
							}, 100);
							return;
						}
						if (data) {
							if (report.IsFolder === true) {
								adhocDataSvc.deleteAdhocReportFolder({ folderId: report.Id }, function () {
									if (vm.__thisReport !== undefined) {
										vm.__thisReport = undefined;
										vm.SubFolder = false;
									}
									active();
									vm.BackFolderAdhoc();
								}, function (error) {
									var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
									openPopup(".report_names");
									cmsBase.cmsLog.error(msg);
								});
							} else {
								adhocDataSvc.deleteAdhocReport({ reportId: report.Id }, function () {
									$rootScope.ListReportAdhocHistory = Enumerable.From($rootScope.ListReportAdhocHistory).Where(function (x) { return x !== parseInt(report.Id) }).ToArray();
									if ($state) {
										if ($state.current.name == AppDefine.State.REBAR_ADHOCDETAILS
                                        && $stateParams.id == report.Id) {
											if ($rootScope.ListReportAdhocHistory.length > 0) {
												var currentID = parseInt($rootScope.ListReportAdhocHistory[$rootScope.ListReportAdhocHistory.length - 1]);
												$rootScope.ListReportAdhocHistory.length = $rootScope.ListReportAdhocHistory.length - 1;
												$state.go(AppDefine.State.REBAR_ADHOCDETAILS, { id: currentID });
											} else {
												$state.go('rebar');
											}
										}
									}
									//$state.go('rebar.adhoc', { id: null });
									//vm.BackFolderAdhoc();
									$scope.ReLoadMenu(true);
								}, function (error) {
									var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
									openPopup(".report_names");
									cmsBase.cmsLog.error(msg);
								});
							}
						}
					});
				}
			} /* END :: function deleteItem	 */

			vm.SubFolder = false;
			vm.Foldername = "";
			vm.AdhocReportDetail = function (report) {
				if (report.IsFolder) {
					/*Folder true => show detail*/
					adhocDataSvc.getAdhocs({ id: report.Id }, function (data) {
						vm.Foldername = report.Name;
						vm.__thisReport = report;
						vm.reports = data;
						vm.SubFolder = !vm.SubFolder;
					}, function (error) {
					});
				} else {
					/*Folder fasle => code show report detail will be here*/
					$state.go('rebar.adhocdetails', { id: report.Id });
					$scope.closePopMenu('.report_names');
				}

			}/* END :: function AdhocReportDetail */

			vm.BackFolderAdhoc = function () {
				adhocDataSvc.getAdhocs({ id: null }, function (data) {
					vm.__thisReport = undefined;
					vm.reports = data;
					openPopup(".report_names");
				}, function (error) {
					var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					cmsBase.cmsLog.error(msg);
				});
			}/* END :: function BackFolderAdhoc */

			vm.triggerChildmenu = function (e) {

				if (document.body.clientWidth < MEDIUM_PATTERN && navigator.userAgent.search("Safari") >= 0 && navigator.userAgent.search("Chrome") < 0) {
					$(e.target).trigger('click');
				}
			}

			$scope.ExportTo = function (format) {
				$scope.$broadcast(AppDefine.Events.EXPORTEVENT, { param: format });
			};
		}
	});
})();