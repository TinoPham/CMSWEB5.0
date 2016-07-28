(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('viewreportCtrl', viewreportCtrl);

		viewreportCtrl.$inject = ['$scope', '$modalInstance', '$filter', 'items', 'AccountSvc', 'AppDefine', 'cmsBase', 'adhocDataSvc', 'filterSvc', 'rebarDataSvc', '$rootScope'];

		function viewreportCtrl($scope, $modalInstance, $filter, items, AccountSvc, AppDefine, cmsBase, adhocDataSvc, filterSvc, rebarDataSvc, $rootScope) {
			var vm = this;

			vm.pageSize = 10;
			vm.currentPage = 1;
			vm.Nodata = "";
			active();

			vm.folder = {
				FolderID: 0,
				FolderName: "",
				folderNameDes: ""
			};

			vm.rebartransactviewerProperty = {
				Max: false,
				Collapse: false
			}

			function active() {

				adhocDataSvc.getAhocReportColumn(function (data) {
					vm.columnDefs = data;
				}, function (error) { });

				if (items) {
					adhocDataSvc.getAdhocReportById({ reportId: items.reportInfo.Id }, function (result) {
						vm.reportSetting = result;
						console.log(result);
						filterSvc.reset();
						var filtergroup = buildfieldterGroup(vm.reportSetting, items);
						var metaTEst = new filterSvc.Metadata();
						metaTEst.resetFilter();
						metaTEst.resetSelect();
						metaTEst.resetApply();

						var apply = new filterSvc.Metadata.ApplyFilter();
						apply.reset();
						apply.applyFilter(filtergroup);

						var group = Enumerable.From(vm.reportSetting.ColumnSelect).Where(function (x) { return x.GroupBy === true; }).FirstOrDefault();
						var groupStr, orderStr;
						if (group) {
							vm.GroupProperty = group.DisplayName;
							groupStr = "PacId," + group.DisplayName;
							orderStr = group.DisplayName;
							//orderStr = "PacId";
						} else {
							groupStr = "PacId";
							orderStr = "PacId";
						}

						apply.applyGroup(groupStr);
						apply.TotalAggregate("Total", "TotalTran");
						apply.CountAggregate("TranId", "CountTran");

						var query = metaTEst.apply(apply).getQuery();
						//query.topgroup = vm.pageSize;
						//query.skipgroup = vm.pageSize * (vm.currentPage - 1);
						//query.ordergroup = orderStr;
						query.isPOSTransac = true;
						if (items && items.SiteKeys) {
							query.keys = items.SiteKeys.join();
						}
						vm.Nodata = "";
						rebarDataSvc.getTransactionViewer(query, function (data) {
							vm.data = data;
							metaTEst.resetFilter();
							metaTEst.resetSelect();
							metaTEst.resetApply();
							apply.reset();
							filterSvc.reset();
							query = null;

							if (vm.data == "") {
								vm.Nodata = "Report setting is wrong.";
								return;
							}

							if (vm.data && vm.data.DataResult && vm.data.DataResult.length == 0) {
								vm.Nodata = "The report does not data to show.";
								return;
							}

							if (vm.data.DataResult) {
								vm.data.DataResult.forEach(function (x) {
									var site = Enumerable.From(items.pacInfo).Where(function (p) { return p.KDVR === x.PacId; }).FirstOrDefault();
									if (site) {
										x.SiteKey = site.SiteKey;
										x.SiteName = site.SiteName;
									} else {
										x.SiteKey = null;
										x.SiteName = "";
									}
								});
							}

							//vm.totalPages = parseInt(vm.data.Countline / vm.pageSize);
							//if (vm.data.Countline % vm.pageSize > 0) {
							//    vm.totalPages = vm.totalPages + 1;
							//}
							console.log(data);

						}, function (error) {
							vm.Nodata = "Report setting is wrong.";
						});

					}, function (err) {
					});
				}

			}

			function buildfieldterGroup(report, item) {

				var startdate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')(cmsBase.DateUtils.endOfDate(item.dateInfo.EndTranDate), "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "le");
				var groupdate = new filterSvc.Metadata.PrecedenceGroup(startdate);
				var enddate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')(cmsBase.DateUtils.startOfDate(item.dateInfo.StartTranDate), "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "ge");
				groupdate = groupdate.andFilter(enddate);

				//var filter1 = new filterSvc.Metadata.FilterClause("PacId").ge(0);
				//var group = new filterSvc.Metadata.PrecedenceGroup(filter1);
				var group = groupdate;
				report.ColumnFilter.forEach(function (x) {

					var filte2;
					var lengFiler = x.ColumnValue.length, i = 0;
					x.ColumnValue.forEach(function (t) {

						if (i == 0) {
							filte2 = new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator);
						} else {
							filte2.andFilter(new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator));
						}

						i++;
					});

					var group1 = new filterSvc.Metadata.PrecedenceGroup(filte2);
					if (x.AND_OP === true) {
						group.andGroupFilter(group1);
					} else {
						group.orGroupFilter(group1);
					}

				});


				// report.ColumnFilter.forEach(function (x) {
				//var filter1 = new filterSvc.Metadata.FilterClause("EmployeeId").gt(1);
				//    var group = new filterSvc.Metadata.PrecedenceGroup(filter1);
				//    var filte2 = new filterSvc.Metadata.FilterClause("TranDate").oper("2016-04-02T00:00:00.000Z", "le");
				//    var group1 = new filterSvc.Metadata.PrecedenceGroup(filte2);
				//    var group2 = new filterSvc.Metadata.FilterClause("TranDate").oper("2015-03-27T00:00:00.000Z", "ge");
				//    group1 = group1.andFilter(group2);
				////filte2 = filte2.and(group2);
				//    group.andGroupFilter(group1);
				//    //group.andFilter(filte2);


				// return group;
				// });

				return group;
			}

			function buildparamTransactionDetail() {
				vm.paramTran = {
					isPOSTransac: true,
					SiteKeys: items.SiteKeys
				};
				vm.definefield = angular.copy(filterSvc.getTransactionColumn());

				var pac = {
					SelectAnd: true,
					firstvalue: vm.selectRow.PacId,
					endvalue: null,
					selectfirst: { id: 1, key: 'eq', name: 'Equal' },
					selectend: { id: 0, key: '', name: '-Select filter-' }
				};
				var pacitem = Enumerable.From(vm.definefield).Where(function (x) { return x.fieldName === 'PacId'; }).FirstOrDefault();
				pacitem.filter = pac;


				var group = Enumerable.From(vm.reportSetting.ColumnSelect).Where(function (x) { return x.GroupBy === true; }).FirstOrDefault();
				if (group) {
					var col = Enumerable.From(vm.definefield).Where(function (c) { return c.fieldName === group.DisplayName; }).FirstOrDefault();
					if (col) {
						var fil = {
							firstvalue: vm.selectRow[group.DisplayName],
							SelectAnd: true,
							selectfirst: { id: 1, key: 'eq', name: 'Equal' },
							endvalue: null,
							selectend: { id: 0, key: '', name: '-Select filter-' }
						}
						col.filter = fil;
					}
				}

				var trandatefilter = {
					SelectAnd: true,
					firstvalue: $rootScope.rebarSearch.DateFrom,
					endvalue: $rootScope.rebarSearch.DateTo,
					selectfirst: { id: 4, key: 'ge', name: "Greater than or equal" },
					selectend: { id: 6, key: 'le', name: "Less than or equal" }
				};
				var trandate = Enumerable.From(vm.definefield).Where(function (x) { return x.fieldName === 'TranDate'; }).FirstOrDefault();
				trandate.filter = trandatefilter;

				vm.reportSetting.ColumnFilter.forEach(function (x) {
					var i = 0;
					x.ColumnValue.forEach(function (t) {
						if (i == 0) {
							var col = Enumerable.From(vm.definefield).Where(function (c) { return c.fieldName === t.Column; }).FirstOrDefault();
							if (col) {
								var cusfilter = filterSvc.getfilterDef(col.fieldType);
								var fil = {
									firstvalue: x.ColumnValue[0].CriteriaValue_1,
									SelectAnd: x.AND_OP,
									selectfirst: Enumerable.From(cusfilter).Where(function (u) { return u.key === x.ColumnValue[0].Operator; }).FirstOrDefault(),
									endvalue: null,
									selectend: { id: 0, key: '', name: '-Select filter-' }
								}
								col.filter = fil;
							}
						} else {
							// filte2.andFilter(new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator));
						}

						i++;
					});
				});

				vm.definefield.forEach(function (x) {
					x.checked = false;
				});

				vm.reportSetting.ColumnSelect.forEach(function (x) {
					var col = Enumerable.From(vm.definefield).Where(function (c) { return c.fieldName === x.DisplayName; }).FirstOrDefault();
					if (col) {
						col.checked = true;
						col.isShow = true;
					}
				});
			}

			vm.showContent = true;
			vm.SelectRow = function (index, row) {
				if (vm.selectIndex === index && vm.selectRow && vm.selectRow.Id === row.Id) {
					vm.showContent = !vm.showContent;
				}

				if (vm.selectIndex !== index) {
					vm.showContent = true;
				}

				vm.selectIndex = index;
				vm.selectRow = row;

				buildparamTransactionDetail();
			}

			vm.Cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.Save = function () {
				adhocDataSvc.addAdhocReportFolder(vm.folder, function (result) {
					vm.folder = result;
					$modalInstance.close(vm.folder);
				}, function (err) {
					var msg = cmsBase.translateSvc.getTranslate(err.data.Data.ReturnMessage[0]);
					cmsBase.cmsLog.error(msg);
				});

			}

			vm.nextPage = function (data) {

				if (vm.currentPage >= $scope.totalPages) {
					return;
				}

				if (data) {
					vm.currentPage = data;
				} else {
					vm.currentPage = vm.currentPage + 1;
				}
				active();
			}

			vm.gotoPage = function () {

				if (vm.currentPage > 0 && vm.currentPage <= $scope.totalPages) {
					active();
				}
			}

			vm.prevPage = function (data) {

				if (vm.currentPage <= 1) {
					return;
				}

				if (data) {
					vm.currentPage = 1;
				} else {
					vm.currentPage = vm.currentPage - 1;
				}
				active();
			}
		}
	});
})();

