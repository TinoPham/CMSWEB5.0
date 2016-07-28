(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('ahocreportviewCtrl', ahocreportviewCtrl);

	    ahocreportviewCtrl.$inject = ['$scope', '$filter', 'AccountSvc', 'AppDefine', 'cmsBase', 'adhocDataSvc', 'filterSvc', 'rebarDataSvc', '$rootScope', 'exportSvc'];

	    function ahocreportviewCtrl($scope, $filter, AccountSvc, AppDefine, cmsBase, adhocDataSvc, filterSvc, rebarDataSvc, $rootScope, exportSvc) {
	        var vmd = this;

	        var items = null;
	        vmd.AppDefine = AppDefine;
	        vmd.paramTran = {};
	        vmd.pageSize = 10;
	        vmd.currentPage = 1;
	        vmd.Nodata = "";
			var callbackFn = null;

			vmd.folder = {
			    FolderID:0,
	            FolderName: "",
	            folderNameDes:""
			};

			vmd.rebartransactviewerProperty = {
			    Max: false,
			    Collapse: false
			}

			vmd.initCtrl = function (item, callback) {
			    items = item;
			    if (items.groupSite) {
			        vmd.groupSite = items.groupSite;
			    }

			    if(callback){
			    	callbackFn = callback;
			    }

			    if (items.reportConfig) {
			        vmd.reportSetting = items.reportConfig;
			        active();
			    } else {
			        adhocDataSvc.getAdhocReportById({ reportId: items.reportInfo.Id }, function(result) {
			            vmd.reportSetting = result;
			            callbackFn(vmd.reportSetting);
			            active();
			        }, function(error) {});
			    }

			
			}
			function FormatHourly( number ) {
				var bgin = PaddingNumber( number ) + ':00';
				var end = PaddingNumber( number >= 23 ? 0 : number + 1 ) + ':00';
				return bgin + '-' + end;
				
			}
			function FormatDaily( year, month, day ) {
				return PaddingNumber( month ) + '/' + PaddingNumber(day) + '/' + PaddingNumber(year);
			}
			function PaddingNumber( number ) {
				if ( number < 10 ) {
					return '0' + number;
				}
				return number;
			}
			vmd.RowHeaderGroup = function ( item ) {
				var sinename = ' (' + item.SiteName + ')';
				var header = '';
				if ( vmd.GroupProperty !== 'TranDate' ) {
					header = ( item.Data[vmd.GroupProperty] ? item.Data[vmd.GroupProperty] : 'Undefined' );
				}else
				switch ( vmd.GroupFieldSelected.ColWidth ) {
					case 1://hourly
					    header = FormatDaily(item.Data['Year'], item.Data['Month'], item.Data['Day']) + ' ' + FormatHourly(item.Data['Hour']);
						break;
					case 2://daily
					    header = FormatDaily(item.Data['Year'], item.Data['Month'], item.Data['Day']);//item['Month'] + '/' + item['Day'] + '/' + item['Year'];
						break;
					case 3://weekly
					    var startDate = dateOfWeek(item.Data['Year'], item.Data['Week'], 0);
					    var endDate = dateOfWeek(item.Data['Year'], item.Data['Week'], 6);
						header = FormatDaily( startDate.getFullYear(), startDate.getMonth() + 1, startDate.getDate() ) + ' - ' + FormatDaily( endDate.getFullYear(), endDate.getMonth() + 1, endDate.getDate() );
						break;
					case 4://monthly
					    header = item.Data['Month'] + '/' + item.Data['Year'];
						break;
				}
				return header + sinename;
			}
			$scope.$on('changeView', function (e, data) {
			    if (vmd.data && vmd.data.DataResult) {
			        vmd.groupSite = data.groupSite;
			        reGroupdata();
			    }
			});

	        function reGroupdata() {
	            vmd.GroupData = Enumerable.From(vmd.AhocData).GroupBy('$.SiteKey', null, function (key, g) {
	                var data = g.ToArray();
	                return {
	                    SiteKey: key,
	                    SiteName: data[0].SiteName,
	                    Total: g.Sum("$.TotalTran"),
	                    Count: g.Count(),
	                    DataResult: data
	                };
	            }).ToArray();
	            vmd.SumOfCountGroup = vmd.GroupData.length;
	        }

	        vmd.definefield = angular.copy(filterSvc.getTransactionColumn());

	        function active() {

	            if (items) {

	                filterSvc.reset();
	                var filtergroup = buildfieldterGroup(vmd.reportSetting, items);
	                var metaAdhoc = new filterSvc.Metadata();
	                metaAdhoc.resetFilter();
	                metaAdhoc.resetSelect();
	                metaAdhoc.resetApply();

	                var apply = new filterSvc.Metadata.ApplyFilter();
	                apply.reset();
	                apply.applyFilter(filtergroup);
	                var hasChild = checkQueryAtDetail(vmd.definefield, vmd.reportSetting.ColumnSelect);
	                var groupPm = Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function(x) { return x.GroupBy === true; }).FirstOrDefault();
	                var groupStr, orderStr;
	                var group = angular.copy(groupPm);
	                if (group.DisplayName === "Payments") {
	                    group.DisplayName = "Name";
	                }
	                if (group) {
	                    vmd.GroupProperty = group.DisplayName;
	                    vmd.GroupFieldSelected = group;
	                    groupStr = getGroupField(group);
	                    orderStr = group.DisplayName;
	                    //orderStr = "PacId";
	                } else {
	                    groupStr = "PacId";
	                    orderStr = "PacId";
	                }


	                apply.applyGroup(groupStr);
	                if (hasChild === true) {
	                    apply.TotalAggregate("Amount", "TotalTran");
	                    //apply.CountAggregate("RetailId", "CountTran");
	                } else {
	                    apply.TotalAggregate("Total", "TotalTran");
	                    //apply.CountAggregate("TranId", "CountTran");
	                }

	                var query;
	                if (groupPm.DisplayName === "Payments") {
	                    query = metaAdhoc.filter(filtergroup).getQuery();
	                    query.groupPayment = "true";
	                } else {
	                    query = metaAdhoc.apply(apply).getQuery();
	                }

	                query.isPOSTransac = true;
	                if (items && items.SiteKeys) {
	                    query.keys = items.SiteKeys.join();
	                }

	                if (vmd.paramTran.flags) {
	                    query.flags = vmd.paramTran.flags.join();
	                }

	                if (vmd.paramTran.pays) {
	                    query.pays = vmd.paramTran.pays.join();
	                }

	                if (vmd.paramTran.taxs) {
	                    query.taxs = vmd.paramTran.taxs.join();
	                }
	                if (vmd.paramTran.DescId) {
	                    query.DescId = vmd.paramTran.DescId.join();
	                }
	                if (vmd.paramTran.ItemCodeId) {
	                    query.ItemCodeId = vmd.paramTran.ItemCodeId.join();
	                }
	                if (vmd.paramTran.FilterAmount) {
	                    query.FilterAmount = vmd.paramTran.FilterAmount;
	                }
	                if (vmd.paramTran.FilterPaymentAmount) {
	                    query.FilterPaymentAmount = vmd.paramTran.FilterPaymentAmount;
	                }
	                if (vmd.paramTran.FilterTaxAmount) {
	                    query.FilterTaxAmount = vmd.paramTran.FilterTaxAmount;
	                }
	                if (vmd.paramTran.FilterQty) {
	                    query.FilterQty = vmd.paramTran.FilterQty;
	                }
	                vmd.Nodata = "";

	                
	                if (hasChild === true) {
	                    rebarDataSvc.getAhocViewer(query, function (data) {
	                        query = null;
	                        applyQueryResult(data, metaAdhoc, apply, group);
	                    }, function (error) {
	                        vmd.Nodata = "Report setting is wrong.";
	                    });
	                } else {
	                    rebarDataSvc.getTransactionViewer(query, function (data) {
	                        query = null;
	                        applyQueryResult(data, metaAdhoc, apply, group);
	                    }, function (error) {
	                        vmd.Nodata = "Report setting is wrong.";
	                    });
	                }
	            }

	        }

	        function getGroupField(group) {
	            var   groupStr = "";
	            if (group.DisplayName === 'TranDate') {
	                switch (group.ColWidth) {
	                    case 1:
	                        groupStr = "PacId,Year, Month, Day, Hour";
	                        break;
	                    case 2:
	                        groupStr = "PacId,Year, Month, Day";
	                        break;
	                    case 3:
	                        groupStr = "PacId,Year,Week";
	                        break;
	                    case 4:
	                        groupStr = "PacId,Year,Month";
	                        break;
	                }
	                
	            } else {

	                var groupitem = Enumerable.From(vmd.definefield).Where(function (x) { return x.fieldName === group.DisplayName; }).FirstOrDefault();
	                if (groupitem) {
	                    if (groupitem.key !== '') {
	                        groupStr = "PacId," + group.DisplayName + "," + groupitem.key;
	                    } else {
	                        groupStr = "PacId," + group.DisplayName;
	                    }
	                    
	                } else {
	                    groupStr = "PacId," + group.DisplayName;
	                }
	            }
	            return groupStr;
	        }

	        function applyQueryResult(data, metaAdhoc, apply, group) {
	            vmd.data = data;
	            console.log(data);

	            metaAdhoc.resetFilter();
	            metaAdhoc.resetSelect();
	            metaAdhoc.resetApply();
	            apply.reset();
	            filterSvc.reset();
	            if (vmd.data == "") {
	                vmd.Nodata = "REPORT_SETTING_MSG";
	                return;
	            }

	            if (vmd.data && vmd.data.DataResult && vmd.data.DataResult.length == 0) {
	                vmd.Nodata = "NO_DATA_STRING";
	                return;
	            }

	            if (vmd.data.DataResult) {
	                vmd.SumOfTotal = Enumerable.From(vmd.data.DataResult).Select("$.TotalTran").Sum();

	                vmd.data.DataResult.forEach(function (x) {
	                    var site = Enumerable.From(items.pacInfo).Where(function (p) { return p.KDVR === x.PacId; }).FirstOrDefault();
	                    if (site) {
	                        x.SiteKey = site.SiteKey;
	                        x.SiteName = site.SiteName;
	                    } else {
	                        x.SiteKey = null;
	                        x.SiteName = "";
	                    }
	                });


	                if (vmd.GroupProperty === 'TranDate') {
	                    vmd.data.DataResult = Enumerable.From(vmd.data.DataResult)
	                        .OrderByDescending("$.Year")
	                        .ThenByDescending("$.Month")
	                        .ThenByDescending("$.Week")
	                        .ThenByDescending("$.Day")
	                        .ThenByDescending("$.Hour")
	                        .ToArray();
	                } else {
	                    vmd.data.DataResult = Enumerable.From(vmd.data.DataResult)
	                        .OrderBy("$." + vmd.GroupProperty).ToArray();
	                }

	                groupByFilter(group);

	                if (vmd.groupSite) {
	                    reGroupdata();
	                }

	                vmd.SumOfCount = Enumerable.From(vmd.AhocData).Count();
	            }
	        }

	        function getGroupFieldforSite(group) {
	            var groupStr = "";
	            if (group.DisplayName === 'TranDate') {
	                switch (group.ColWidth) {
	                    case 1:
	                        groupStr = "{SiteKey: $.SiteKey,Year:$.Year, Month:$.Month, Day:$.Day, Hour:$.Hour}";
	                        break;
	                    case 2:
	                        groupStr = "{SiteKey: $.SiteKey,Year:$.Year, Month:$.Month, Day:$.Day}";
	                        break;
	                    case 3:
	                        groupStr = "{SiteKey: $.SiteKey,Year:$.Year,Week:$.Week}";
	                        break;
	                    case 4:
	                        groupStr = "{SiteKey: $.SiteKey,Year:$.Year, Month:$.Month}";
	                        break;
	                }

	            } else {

	                var groupitem = Enumerable.From(vmd.definefield).Where(function (x) { return x.fieldName === group.DisplayName; }).FirstOrDefault();
	                if (groupitem) {
	                    if (groupitem.key !== '') {
	                        groupStr = "{SiteKey:$.SiteKey," + group.DisplayName + ":$." + group.DisplayName + "," + groupitem.key + ":$." + groupitem.key + "}";
	                    } else {
	                        groupStr = "{SiteKey:$.SiteKey," + group.DisplayName + ":$." + group.DisplayName + "}";
	                    }

	                } else {
	                    groupStr = "{SiteKey: $.SiteKey," + group.DisplayName + ":$." + group.DisplayName + "}";
	                }
	            }
	            return groupStr;
	        }

	        function getKeyforGroup(group) {
	            var groupStr = "";
	            if (group.DisplayName === 'TranDate') {
	                switch (group.ColWidth) {
	                    case 1:
	                        groupStr = "$.SiteKey + '-' + $.TranDate + '-' + $.Year + '-' + $. Month + '-' + $.Day + '-' + $.Hour";
	                        break;
	                    case 2:
	                        groupStr = "$.SiteKey + '-' + $.Year + '-' + $. Month + '-' + $. Day";
	                        break;
	                    case 3:
	                        groupStr = "$.SiteKey + '-' + $.Year + '-' + $.Week";
	                        break;
	                    case 4:
	                        groupStr = "$.SiteKey + '-' + $.Year + '-' + $.Month";
	                        break;
	                }

	            } else {

	                var groupitem = Enumerable.From(vmd.definefield).Where(function (x) { return x.fieldName === group.DisplayName; }).FirstOrDefault();
	                if (groupitem) {
	                    if (groupitem.key !== '') {
	                        groupStr = "$.SiteKey + '-' + $." + group.DisplayName + " + '-' + $." + groupitem.key;
	                    } else {
	                        groupStr = "$.SiteKey + '-' + $." + group.DisplayName;
	                    }

	                } else {
	                    groupStr = "$.SiteKey + '-' + $." + group.DisplayName;
	                }
	            }
	            return groupStr;
	        }

	        function groupByFilter(group) {
	            var groupStr = getGroupFieldforSite(group);
	            var keyGen = getKeyforGroup(group);
	            vmd.AhocData = Enumerable.From(vmd.data.DataResult).GroupBy(groupStr, null, function (key, g) {
	                var arrayR = g.ToArray();
	                return {
	                    Data: key,
	                    SiteKey: arrayR[0].SiteKey,
	                    SiteName: arrayR[0].SiteName,
	                    TotalTran: g.Sum("$.TotalTran"),
	                    DataResult: arrayR
	                };
	            }, keyGen).ToArray();
	            console.log(vmd.AhocData);
	        }

	        function checkQueryAtDetail(definefield, colSelect) {
	            var array = Enumerable.From(definefield).Where(function (w) { return w.isDetail === true; }).ToArray(); //['Qty', 'Amount', 'DescId', 'DescName', 'ItemCodeId'];
	            var result = false;
	            for (var i = 0; i < array.length; i++) {
	                var hasDetail = Enumerable.From(colSelect).Where(function (x) { return x.DisplayName === array[i].fieldName; }).FirstOrDefault();
	                if (hasDetail) {
	                    result = true;
	                    break;
	                }
	            }
	            return result;
	        }

	        function buildfieldterGroup(report, item) {
	            item.dateInfo.EndTranDate.setHours(23);
	            item.dateInfo.EndTranDate.setMinutes(59);
	            item.dateInfo.EndTranDate.setSeconds(59);
	            item.dateInfo.EndTranDate.setMilliseconds(999);
	            item.dateInfo.StartTranDate.setHours(0);
	            item.dateInfo.StartTranDate.setMinutes(0);
	            item.dateInfo.StartTranDate.setSeconds(0);
	            item.dateInfo.StartTranDate.setMilliseconds(0);
	            var enddate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')(item.dateInfo.EndTranDate, "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "le"); 
	            var groupdate = new filterSvc.Metadata.PrecedenceGroup(enddate);
			    var startdate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')(item.dateInfo.StartTranDate, "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "ge");
			    groupdate = groupdate.andFilter(startdate);

	            //var filter1 = new filterSvc.Metadata.FilterClause("PacId").ge(0);
	            //var group = new filterSvc.Metadata.PrecedenceGroup(filter1);
			    report.ColumnFilter = Enumerable.From(report.ColumnFilter).OrderByDescending("$.AND_OP").ToArray();
			    var group = groupdate, group1, firtColFitler = report.ColumnFilter.length > 0 ? report.ColumnFilter[0] : null;

			    var removeOneOr = Enumerable.From(report.ColumnFilter).Where(function(x) { return x.AND_OP === false; }).FirstOrDefault();
			    if (report.ColumnFilter.length === 1 && removeOneOr) {
	                return group;
			    }

			    vmd.paramTran.FilterQty = "";
			    vmd.paramTran.FilterAmount = "";
			    vmd.paramTran.FilterPaymentAmount = "";
	            vmd.paramTran.FilterTaxAmount = "";

	            report.ColumnFilter.forEach(function (x) {

	                var filte2 = null;
	                var setlisttype = false, i = 0;
	                x.ColumnValue.forEach(function(t) {

	                    switch (t.Column) {
	                        case 'Qty':
	                            buildfilterNumberType("FilterQty", t, x);
	                        case 'Amount':
	                            buildfilterNumberType("FilterAmount", t, x);
	                            break;
	                        case 'PaymentAmount':
	                            buildfilterNumberType("FilterPaymentAmount", t, x);
	                            break;
	                        case 'TaxAmount':
	                            buildfilterNumberType("FilterTaxAmount", t, x);
	                            break;
	                    default:
	                        var col = Enumerable.From(vmd.definefield).Where(function(s) { return s.fieldName === t.Column; }).FirstOrDefault();
	                        if (col) {
	                            if (i == 0) {
	                                switch (col.fieldType) {
										case 'number':
											filte2 = new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator);
											break;
										case 'string':
											var text = "'" + t.CriteriaValue_1 + "'";
											filte2 = new filterSvc.Metadata.FilterClause(t.Column).operString(text, t.Operator);
											break;
										case 'money':
											filte2 = new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator);
											break;
										case 'datetime':
											filte2 = new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator);
											break;
										case 'childid':
											if (t.CriteriaValue_1.length <= 0) //ignore when it is 'Any'
												break;
											var ids = t.CriteriaValue_1.split(',');
											var fiLen = ids.length;
											for (var j = 0; j < fiLen; j++) {
												if (j === 0) {
													filte2 = new filterSvc.Metadata.FilterClause(t.Column).eq(ids[j]);
												} else {
													var filer = new filterSvc.Metadata.FilterClause(t.Column).eq(ids[j]);
													filte2 = filte2.or(filer);
												}
											}
											break;
										case 'list':
											if (t.CriteriaValue_1.length <= 0) //ignore when it is 'Any'
												break;
											var ids = t.CriteriaValue_1.split(',');
											if (t.Column === 'ExceptionTypes') {
												vmd.paramTran.flags = ids;
											}

											if (t.Column === 'Payments') {
												vmd.paramTran.pays = ids;
											}

											if (t.Column === 'Taxs') {
												vmd.paramTran.taxs = ids;
											}
											if (t.Column === 'DescId') {
												vmd.paramTran.DescId = ids;
											}

											if (t.Column === 'ItemCodeId') {
												vmd.paramTran.ItemCodeId = ids;
											}
											setlisttype = true;
											break;
										default:
											filte2 = new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator);
											break;
	                                }

	                            } else {
	                                filte2.andFilter(new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator));
	                            }
	                        }
	                        break;
	                    }
	                    i++;
	                });

	                if (!setlisttype && filte2) {
	                    if (!group1) {
	                        group1 = new filterSvc.Metadata.PrecedenceGroup();
	                    }
	                    if (firtColFitler.CriteriaID === x.CriteriaID) {
	                        if (report.ColumnFilter.length > 1 || x.AND_OP === true) {
	                            group1 = new filterSvc.Metadata.PrecedenceGroup(filte2);
	                        } else {
	                            if (x.AND_OP === true) {
	                                group1.andGroupFilter(filte2);
	                            } else {
	                                if (report.ColumnFilter.length > 1) {
	                                    group1.orGroupFilter(filte2);
	                                }
	                            }
	                        }
	                    } else {
	                        var groupchild = new filterSvc.Metadata.PrecedenceGroup(filte2);
	                        if (x.AND_OP === true) {
	                            if (group1.clauses.length > 0 || group1.groupClauses.length > 0) {
	                                group1.andGroupFilter(groupchild);
	                            } else {
	                                group1.filter(filte2);
	                            }
	                            
	                        } else {
	                            if (report.ColumnFilter.length > 1) {
	                                group1.orGroupFilter(groupchild);
	                            }
	                        }
	                    }
	                }
	                
	            });
	            if (group1 && (group1.clauses.length > 0 || group1.groupClauses.length > 0)) {
	                group.andGroupFilter(group1);
	            }

	            return group;
	        }

	        var dateOfWeek = function (year, wn, dayNb) {
	            var j10 = new Date(year, 0, 0, 0, 0, 0),
                    j4 = new Date(year, 0, 0, 0, 0, 0),
                    mon1 = j4.getTime() - j10.getDay() * 86400000;
	            return new Date(mon1 + ((wn - 1) * 7 + dayNb) * 86400000);
	        };

	        function buildfilterNumberType(columnName, t, field) {
	            if (vmd.paramTran[columnName] === "") {
	                vmd.paramTran[columnName] = t.Column + ' ' + filterSvc.getForDynamicQuery(t.Operator) + ' ' + t.CriteriaValue_1;
	            } else {
	                if (field.AND_OP === true) {
	                    vmd.paramTran[columnName] += ' && ' + t.Column + ' ' + filterSvc.getForDynamicQuery(t.Operator) + ' ' + t.CriteriaValue_1;
	                } else {
	                    vmd.paramTran[columnName] += ' || ' + t.Column + ' ' + filterSvc.getForDynamicQuery(t.Operator) + ' ' + t.CriteriaValue_1;
	                }
	            }
	        }

	        function buildparamTransactionDetail(exportFilter) {
			    vmd.definefield = angular.copy(filterSvc.getTransactionColumn());
	                vmd.paramTran.isPOSTransac = true;
	               

	                vmd.paramTran.calbackFn = vmd.calbackFn;
	            //var pac = {
	            //    SelectAnd: true,
	            //    firstvalue: vmd.selectRow.PacId,
	            //    endvalue: null,
	            //    selectfirst: { id: 1, key: 'eq', name: 'Equal' },
	            //    selectend: { id: 0, key: '', name: '-Select filter-' }
	            //};
	            //var pacitem = Enumerable.From(vmd.definefield).Where(function (x) { return x.fieldName === 'PacId'; }).FirstOrDefault();
	            //pacitem.groupFilter = pac;
	            if (exportFilter) {
	                vmd.paramTran.SiteKeys = items.SiteKeys;
	            } else {
	                var rowselect = vmd.selectRow ? vmd.selectRow.Data : null;
	                if (rowselect) {
	                    vmd.paramTran.SiteKeys = [vmd.selectRow.SiteKey];
	                }
	            }


	            var group = Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function (x) { return x.GroupBy === true; }).FirstOrDefault();
	            if (group && !exportFilter && rowselect) {
	                var col = Enumerable.From(vmd.definefield).Where(function (c) { return c.fieldName === group.DisplayName; }).FirstOrDefault();
	              
	                if (col) {
	                    var fil = {
	                        firstvalue: rowselect[group.DisplayName],
	                        SelectAnd: true,
	                        selectfirst: { id: 1, key: 'eq', name: 'Equal' },
	                        endvalue: null,
	                        selectend: { id: 0, key: '', name: '-Select filter-' }
	                    }
	                    if (col.key !== '') {
	                        col = Enumerable.From(vmd.definefield).Where(function (c) { return c.fieldName === col.key; }).FirstOrDefault();
	                        fil.firstvalue = rowselect[col.fieldName] !== null ? rowselect[col.fieldName] : null;
	                    }

	                    if (group.DisplayName === "Payments") {
	                        fil = vmd.selectRow.DataResult[0].Id !== null ? [vmd.selectRow.DataResult[0].Id] : null;
	                    }

	                    if (group.DisplayName === 'TranDate') {
	                        var fData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], 0, 0, 0, 0);
	                        var sData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], 23, 59, 59, 999);
	                                switch (group.ColWidth) {
	                                    case 1:
	                                        fData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], rowselect['Hour'], 0, 0, 0);
	                                        sData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], rowselect['Hour'], 59, 59, 999);
	                                        break;
	                                    case 2:
	                                        fData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], 0, 0, 0, 0);
	                                        sData = new Date(rowselect['Year'], rowselect['Month'] - 1, rowselect['Day'], 23, 59, 59, 999);
	                                        break;
	                                    case 3:
	                                        var startDate = dateOfWeek(rowselect['Year'], rowselect['Week'], 0);
	                                        var endDate = dateOfWeek(rowselect['Year'], rowselect['Week'], 6);
	                                        fData = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate(), 0, 0, 0, 0);
	                                        sData = new Date(endDate.getFullYear(), endDate.getMonth(), endDate.getDate(), 23, 59, 59, 999);
	                                        break;
	                                    case 4:
	                                        fData = new Date(rowselect['Year'], rowselect['Month'] - 1, 1, 0, 0, 0, 0);
	                                        sData = new Date(rowselect['Year'], rowselect['Month'] , 0, 23, 59, 59, 999);
	                                        break;
	                                }
	                        var firstvalue = $filter('date')(fData, "yyyy-MM-ddTHH:mm:ss.sss", 'UTC') + "Z";
	                        var secondvalue = $filter('date')(sData, "yyyy-MM-ddTHH:mm:ss.sss", 'UTC') + "Z";
	                         fil = {
	                             firstvalue: firstvalue,
	                            SelectAnd: true,
	                            selectfirst: { id: 1, key: 'ge', name: 'Equal' },
	                            endvalue: secondvalue,
	                            selectend: { id: 6, key: 'le', name: 'Less than or equal', className: 'icon-less-than-or-equal', symbol: '<=' }
	                        }
	                    }

	                    col.groupFilter = fil;
	                }
	            } 

	            //var trandatefilter = {
	            //    SelectAnd: true,
	            //    firstvalue: $rootScope.rebarSearch.DateFrom,
	            //    endvalue: $rootScope.rebarSearch.DateTo,
	            //    selectfirst: { id: 4, key: 'ge', name: "Greater than or equal" },
	            //    selectend: { id: 6, key: 'le', name: "Less than or equal" }
	            //};
	            //var trandate = Enumerable.From(vmd.definefield).Where(function (x) { return x.fieldName === 'TranDate'; }).FirstOrDefault();
	            //trandate.filter = trandatefilter;

	            //var enddate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')($rootScope.rebarSearch.DateTo, "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "le");
			    //var groupdate = new filterSvc.Metadata.PrecedenceGroup(enddate);
	            $rootScope.rebarSearch.DateTo.setHours(23);
	            $rootScope.rebarSearch.DateTo.setMinutes(59);
	            $rootScope.rebarSearch.DateTo.setSeconds(59);
	            $rootScope.rebarSearch.DateTo.setMilliseconds(999);
	            $rootScope.rebarSearch.DateFrom.setHours(0);
	            $rootScope.rebarSearch.DateFrom.setMinutes(0);
	            $rootScope.rebarSearch.DateFrom.setSeconds(0);
	            $rootScope.rebarSearch.DateFrom.setMilliseconds(0);
	            var startdate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')($rootScope.rebarSearch.DateFrom, "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "ge");
	            var enddate = new filterSvc.Metadata.FilterClause("TranDate").oper($filter('date')($rootScope.rebarSearch.DateTo, "yyyy-MM-ddTHH:mm:ss.sss") + "Z", "le");
			    startdate.and(enddate);
	            //groupdate = groupdate.andFilter(startdate);
			    vmd.paramTran.filter = startdate;//groupdate;

			    var removeOneOr = Enumerable.From(vmd.reportSetting.ColumnFilter).Where(function (x) { return x.AND_OP === false; }).FirstOrDefault();
			    if (vmd.reportSetting.ColumnFilter.length === 1 && removeOneOr) {
			    } else {
			        vmd.reportSetting.ColumnFilter.forEach(function (x) {
			            var i = 0;
			            x.ColumnValue.forEach(function (t) {
			                if (i == 0) {
			                    var col = Enumerable.From(vmd.definefield).Where(function (c) { return c.fieldName === t.Column && c.fieldName !== 'PaymentAmount' && c.fieldName !== 'TaxAmount'; }).FirstOrDefault();
			                    if (col) {
			                        var cusfilter = filterSvc.getfilterDef(col.fieldType);
			                        var fil = {
			                            AndGroup: x.AND_OP,
			                            firstvalue: x.ColumnValue[0].CriteriaValue_1,
			                            SelectAnd: x.AND_OP,
			                            selectfirst: Enumerable.From(cusfilter).Where(function (u) { return u.key === x.ColumnValue[0].Operator; }).FirstOrDefault(),
			                            endvalue: null,
			                            selectend: { id: 0, key: '', name: '-Select filter-' }
			                        }
                                    //For more column filter
			                        var newFilter = angular.copy(col);
			                        var newId = vmd.definefieldlength + 1;
			                        newFilter.filter = fil;
			                        newFilter.id = newId;
			                        newFilter.checked = false;
			                        newFilter.isShow = false;
			                        vmd.definefield.push(newFilter);
			                    }
			                } else {
			                    // filte2.andFilter(new filterSvc.Metadata.FilterClause(t.Column).oper(t.CriteriaValue_1, t.Operator));
			                }

			                i++;
			            });
			        });
			    }

	            if (vmd.reportSetting.ColumnSelect.length > 0) {
	                vmd.definefield.forEach(function (x) {
	                    x.checked = false;
	                });
			        vmd.reportSetting.ColumnSelect.forEach(function (x) {
			            var col = Enumerable.From(vmd.definefield).Where(function (c) { return c.fieldName === x.DisplayName; }).FirstOrDefault();
			            if (col) {
			                col.checked = true;
			                col.isShow = true;
			            }
			        });
			    }
			}

	        //function getDetailColumn() {
	        //    return Enumerable.From(vmd.definefield).Where(function (d) {  return d.isDetail === true;}).Select(function(d) { return d.fieldName; }).ToArray();
	        //}

	        vmd.calbackFn = function (count) {
	            if (vmd.isrowselect === true) {
	                vmd.itemTotalRecord = angular.copy(count);
	            }
	            vmd.isrowselect = false;
			}

	        vmd.showContent = false;
	        vmd.SelectRow = function (index, row) {

	            //vmd.itemTotalRecord = 0;
	            
	            if (vmd.selectIndex === index && vmd.selectRow && vmd.selectRow.Id === row.Id) {
	                vmd.showContent = !vmd.showContent;
	            }

	            if (vmd.selectIndex !== index) {
	                vmd.showContent = true;
	                vmd.isrowselect = true;
	            }

	            vmd.selectIndex = index;
	            vmd.selectRow = row;

	            buildparamTransactionDetail();
	        }


			vmd.nextPage = function (data) {

			    if (vmd.currentPage >= $scope.totalPages) {
			        return;
			    }

			    if (data) {
			        vmd.currentPage = data;
			    } else {
			        vmd.currentPage = vmd.currentPage + 1;
			    }
			    active();
			}

			vmd.gotoPage = function () {

			    if (vmd.currentPage > 0 && vmd.currentPage <= $scope.totalPages) {
			        active();
			    }
			}

			vmd.prevPage = function (data) {

			    if (vmd.currentPage <= 1) {
			        return;
			    }

			    if (data) {
			        vmd.currentPage = 1;
			    } else {
			        vmd.currentPage = vmd.currentPage - 1;
			    }
			    active();
			}

			$scope.$on(AppDefine.Events.EXPORTEVENT, function (event, data) {
			    if (!vmd.AhocData) return;

			    buildparamTransactionDetail(false);
			    exportData(vmd.paramTran, vmd.definefield);
			});

			function exportData(param, defines) {

			    if (!vmd.selectRow || !vmd.showContent || vmd.showContent === false) {
			        exportServer([]);
			        return;
			    }

			    var meta = new filterSvc.Metadata();
			    var definefield = defines;
			    var selectsDef = Enumerable.From(definefield).Where(function (x) { return x.checked === true && x.isShow === true; }).ToArray();
			    var tranId = Enumerable.From(definefield).Where(function (x) { return x.fieldName === 'TranId'; }).FirstOrDefault();
			    var pacId = Enumerable.From(definefield).Where(function (x) { return x.fieldName === 'PacId'; }).FirstOrDefault();
			    tranId.checked = true;
			    pacId.checked = true;

			    var showdetail = checkQueryAtDetail(definefield, vmd.reportSetting.ColumnSelect);
			    if (showdetail === true) {
			        $scope.selects = Enumerable.From(definefield).Where(function (x) { return x.checked === true; }).Select(function (x) { return x.fieldName; }).ToArray().join();
			        $scope.selects = $scope.selects + ",RetailId";
			    } else {
			        definefield = Enumerable.From(definefield).Where(function (x) { return x.isDetail !== true; }).ToArray();
			        $scope.selects = Enumerable.From(definefield).Where(function (x) { return x.checked === true && x.isDetail !== true; }).Select(function (x) { return x.fieldName; }).ToArray().join();
			    }

			    var expanfield = Enumerable.From(definefield).Where(function (x) { return x.checked === true && x.Expand !== undefined; }).Select(function (x) { return x.fieldName; }).ToArray().join();


			    meta.resetFilter();
			    meta.resetSelect();


			    var groupfilter;
			    var group1 = new filterSvc.Metadata.PrecedenceGroup();


			    if (param && param.filter) {
			        groupfilter = new filterSvc.Metadata.PrecedenceGroup(param.filter);
			    }

			    group1 = buildfieldter(group1, definefield);

			    if (group1.clauses.length > 0 || group1.groupClauses.length > 0) {
			        if (groupfilter) {
			            groupfilter.andGroupFilter(group1);
			        } else {
			            groupfilter = group1;
			        }
			    }

			    var group2 = new filterSvc.Metadata.PrecedenceGroup();
			    group2 = buildgroupFilter(group2, definefield);
			    if (group2.clauses.length > 0 || group2.groupClauses.length > 0) {
			        groupfilter.andGroupFilter(group2);
			    }

			    meta.filter(groupfilter);


			    $scope.query = meta.select($scope.selects).expand(expanfield).getQuery();

			    if (param && param.SiteKey) {
			        var keys = [];
			        keys.push(param.SiteKey);
			        $scope.query.keys = keys.join();
			    }


			    if (param && param.flags) {
			        $scope.query.flags = param.flags.join();
			    }

			    if (param && param.pays) {
			        $scope.query.pays = param.pays.join();
			    }

			    if (param && param.taxs) {
			        $scope.query.taxs = param.taxs.join();
			    }
			    if (param && param.DescId) {
			        $scope.query.DescId = param.DescId.join();
			    }
			    if (param && param.ItemCodeId) {
			        $scope.query.ItemCodeId = param.ItemCodeId.join();
			    }
			    if (param && param.FilterAmount) {
			        $scope.query.FilterAmount = param.FilterAmount;
			    }
			    if (param.FilterPaymentAmount) {
			        $scope.query.FilterPaymentAmount = param.FilterPaymentAmount;
			    }
			    if (param.FilterTaxAmount) {
			        $scope.query.FilterTaxAmount = param.FilterTaxAmount;
			    }
			    if (param && param.FilterQty) {
			        $scope.query.FilterQty = param.FilterQty;
			    }
			    if (param && param.SiteKeys) {
			        $scope.query.keys = param.SiteKeys.join();
			        //query.keys = param.SiteKeys;
			    }

			    if (param && param.isPOSTransac) {
			        $scope.query.isPOSTransac = param.isPOSTransac;
			    } else {
			        $scope.query.isPOSTransac = false;
			    }

			    if (showdetail === true) {
			        rebarDataSvc.getAhocViewer($scope.query, function (data) {
			            console.log(data);
			            exportServer(data.DataResult);
			        }, function (error) {

			        });
			    } else {
			        rebarDataSvc.getTransactionViewer($scope.query, function (data) {
			            console.log(data);
			            exportServer(data.DataResult);
			        }, function (error) {

			        });
			    }
			}

			function buildgroupFilter(query, definefield) {
			    definefield.forEach(function (x) {
			        if (x.groupFilter) {
			            buildfilterdetail(query, x.groupFilter, x);
			        }
			    });
			    return query;
			}

			function buildfieldter(query, definefield) {
			    var buildfilters = Enumerable.From(definefield).Where(function (x) { return x.filter; }).OrderByDescending("$.filter.AndGroup").ToArray();
			    buildfilters.forEach(function (x) {
			        if (x.filter) {
			            buildfilterdetail(query, x.filter, x);
			        }

			    });
			    return query;
			}
            
			function buildfilterdetail(query, filter, col) {
			    var group1 = null, group2 = null, firVal, endVal;
			    var x = col;
			    if (x.fieldType === 'list' && x.Expand && x.Expand.fieldType === "number") {
			        if (filter.length <= 0) return;
			        if (filter.selectfirst) return;
			        var filter1;
			        var fiLen = filter.length;

			        filter1 = new filterSvc.Metadata.FilterClause(x.Expand.key).eq(filter[0]);
			        var query1 = new filterSvc.Metadata.ExpandFilter(x.fieldName, filter1).anyFilter();;
			        for (var i = 1; i < fiLen; i++) {
			            var filer2 = new filterSvc.Metadata.FilterClause(x.Expand.key).eq(filter[i]);
			            query1.orFilter(filer2);
			        }

			        if (query.clauses.length > 0 || query.groupClauses.length > 0) {
			            query.andFilter(query1);
			        } else {
			            query.filter(query1);
			        }
			    } else {
			        if (filter.selectfirst.id > 0 || (x.fieldType === 'childid' && filter.firstvalue)) {

			            switch (x.fieldType) {
			                case 'datetime':
			                    firVal = "" + $filter('date')(filter.firstvalue, "yyyy-MM-ddTHH:mm:ss.sss") + "Z";
			                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
			                    break;
			                case 'money':
			                    firVal = filter.firstvalue + 'm';
			                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
			                    break;
			                case 'string':
			                    firVal = "'" + filter.firstvalue + "'";
			                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).operString(firVal, filter.selectfirst.key));
			                    break;
			                case 'list':
			                    firVal = "'" + filter.firstvalue + "'";
			                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).operString(firVal, filter.selectfirst.key));
			                    break;
			                case 'childid':
			                    if (filter.selectfirst.id > 0) {
			                        firVal = filter.firstvalue;
			                        group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
			                    } else {
			                        var filterlist;
			                        var ids = filter.firstvalue.split(',');
			                        var leng = ids.length;
			                        for (var j = 0; j < leng; j++) {
			                            if (j === 0) {
			                                filterlist = new filterSvc.Metadata.FilterClause(x.fieldName).eq(ids[j]);
			                            } else {
			                                var filer = new filterSvc.Metadata.FilterClause(x.fieldName).eq(ids[j]);
			                                filterlist = filterlist.or(filer);
			                            }
			                        }
			                        if (filterlist) {
			                            group1 = new filterSvc.Metadata.PrecedenceGroup(filterlist);
			                        }
			                    }

			                    break;
			                default:
			                    firVal = filter.firstvalue;
			                    group1 = new filterSvc.Metadata.PrecedenceGroup(new filterSvc.Metadata.FilterClause(x.fieldName).oper(firVal, filter.selectfirst.key));
			                    break;
			            }


			            if (filter.selectend.id > 0) {
			                switch (x.fieldType) {
			                    case 'datetime':
			                        endVal = "" + $filter('date')(filter.endvalue, "yyyy-MM-ddTHH:mm:ss.sss") + "Z";
			                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
			                        break;
			                    case 'money':
			                        endVal = filter.endvalue + 'm';
			                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
			                        break;
			                    case 'string':
			                        endVal = "'" + filter.endvalue + "'";
			                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).operString(endVal, filter.selectend.key);
			                        break;
			                    case 'list':
			                        endVal = "'" + filter.endvalue + "'";
			                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).operString(endVal, filter.selectend.key);
			                        break;
			                    default:
			                        endVal = filter.endvalue;
			                        group2 = new filterSvc.Metadata.FilterClause(x.fieldName).oper(endVal, filter.selectend.key);
			                        break;
			                }


			                if (filter.SelectAnd) {
			                    group1 = group1.andFilter(group2);
			                } else {
			                    group1 = group1.orFilter(group2);
			                }
			            }

			            if (query.clauses.length > 0 || query.groupClauses.length > 0) {
			                //query.andFilter();
			                if (filter.AndGroup !== undefined && filter.AndGroup === false) {
			                    query.orFilter(group1);
			                } else {

			                    query.andFilter(group1);

			                }
			            } else {

			                query.filter(group1);
			            }

			            //query.andFilter(group1);

			        } else if (x.fieldType === 'string' && x.key !== '' && filter.firstvalue && angular.isArray(filter.firstvalue)) {
			            var filterlist;
			            var ids = filter.firstvalue;
			            var leng = ids.length;
			            for (var j = 0; j < leng; j++) {
			                if (j === 0) {
			                    filterlist = new filterSvc.Metadata.FilterClause(x.key).eq(ids[j]);
			                } else {
			                    var filer = new filterSvc.Metadata.FilterClause(x.key).eq(ids[j]);
			                    filterlist = filterlist.or(filer);
			                }
			            }
			            if (filterlist) {
			                group1 = new filterSvc.Metadata.PrecedenceGroup(filterlist);
			                if (query.clauses.length > 0 || query.groupClauses.length > 0) {
			                    query.andFilter(group1);
			                } else {
			                    query.filter(group1);
			                }

			            }
			        }
			    }
			}

			function exportServer(data) {
			    var userLogin = AccountSvc.UserModel();

			    var sumtable = vmd.groupSite ? buildExportGroupbySite(data) : buildExportSummaryTable(data);

                var reportInfo = {
                	//TemplateName: "Adhoc" + $filter('date')(new Date(), "yy-MM-dd HHmmss"),
                	TemplateName: "Adhoc" + "_" + cmsBase.translateSvc.getTranslate($rootScope.title).replace(/[\s]/g, ''),
                    ReportName: cmsBase.translateSvc.getTranslate($rootScope.title),
                    CompanyID: userLogin.CompanyID,
                    RegionName: '',
                    Location: '',
                    WeekIndex: $rootScope.GWeek,
                    Footer: '',
                    CreatedBy: userLogin.FName + ' ' + userLogin.LName,
                    CreateDate: $filter('date')(new Date(), AppDefine.ParamDateFormat) // MM/dd/yyyy
                };

                var tables = [sumtable];
                var charts = [];
                exportSvc.exportXlsFromServer(reportInfo, tables, charts);
			}


	        function compareGroupDate(yyyy, mm, ww, dd, hh, dataDate) {
	            var date1 = new Date();
	            var date2 = new Date(dataDate.substring(0,10));

	            if (ww) {
	                var startDate = dateOfWeek(yyyy, ww, 0);
	                startDate.setHours(0);
	                startDate.setMinutes(0);
	                startDate.setSeconds(0);
	                date2.setHours(0);
	                date2.setMinutes(0);
	                date2.setSeconds(0);
	                var endDate = dateOfWeek(yyyy, ww, 6);
	                endDate.setHours(23);
	                endDate.setMinutes(59);
	                endDate.setSeconds(59);
	                return date2 >= startDate && date2 <= endDate;
	            }

	            if (yyyy) {
	                date1.setFullYear(yyyy);
	            } else {
	                date1.setFullYear(0);
	                date2.setFullYear(0);
	            }

	            if (mm) {
	                date1.setMonth(mm -1 );
	            } else {
	                date1.setMonth(0);
	                date2.setMonth(0);
	            }

	            if (dd) {
	                date1.setDate(dd);
	            } else {
	                date1.setDate(1);
	                date2.setDate(1);
	            }

	            if (hh) {
	                date1.setHours(hh);
	            } else {
	                date1.setHours(0);
	                date2.setHours(0);
	            }

	            date1.setMinutes(0);
	            date1.setSeconds(0);
	            date1.setMilliseconds(0);
	            date2.setMinutes(0);
	            date2.setSeconds(0);
	            date2.setMilliseconds(0);

	            return date1.getTime() === date2.getTime();
	        }

	        function buildExportGroupbySite(data) {
	            var rows = [];
	            var headers = [];
	            var colIndex = 1, rowIndex = 1;
	            headers.push({
	                Value: "", Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 15
	            });
	            colIndex += 1
	            headers.push({
	                Value: "", Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 15
	            });

	            var columns = Enumerable.From(vmd.definefield).Where(function (x) {
	                return Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function (c) { return c.DisplayName === x.fieldName; }).FirstOrDefault() !== undefined;
	            }).ToArray();

	            columns.forEach(function (x) {
	                colIndex += 1;
	                headers.push({ Value: cmsBase.translateSvc.getTranslate(x.fieldName), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
	            });

	            rows.push({
	                Type: AppDefine.tableExport.Header,
	                ColDatas: headers,
	                GridModels: null
	            });

	            vmd.GroupData.forEach(function (s) {
	                colIndex = 1;
	                rowIndex += 1;
	                var rowsite = {
	                    Type: AppDefine.tableExport.Body,
	                    ColDatas: [{ Value: s.SiteName + " : " + $filter('number')(s.Total, 2), Color: AppDefine.ExportColors.GridGroupHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 2, Rows: 1 } }],
	                    GridModels: null
	                }
	                rows.push(rowsite);
	                s.DataResult.forEach(function (ah) {
	                    colIndex = 2;
	                    rowIndex += 1;
	                    var groupPm = Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function(x) { return x.GroupBy === true; }).FirstOrDefault();

	                    var row = {
	                        Type: AppDefine.tableExport.Body,
	                        ColDatas: [{ Value: cmsBase.translateSvc.getTranslate(vmd.RowHeaderGroup(ah)) + " : " + $filter('number')(ah.TotalTran, 2), Color: AppDefine.ExportColors.GridGroupHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 1, Rows: 1 } }],
	                        GridModels: null
	                    }

	                    rows.push(row);


	                    ah.DataResult.forEach(function(ahdata) {
	                        var datadetail;
	                        if (vmd.GroupProperty !== 'TranDate') {

	                            if (groupPm.DisplayName === "Payments") {
	                                datadetail = Enumerable.From(data).Where(function(x) {
	                                    return ahdata['PacId'] == x['PacId'] && Enumerable.From(x.Payments).Where(function(pm) { return ahdata.Id === pm.Id; }).FirstOrDefault();
	                                }).ToArray();
	                            } else {
	                                datadetail = Enumerable.From(data).Where(function(x) { return ahdata['PacId'] == x['PacId'] && ahdata[groupPm.DisplayName] === x[groupPm.DisplayName]; }).ToArray();
	                            }

	                        } else {
	                            switch (vmd.GroupFieldSelected.ColWidth) {
	                            case 1: //hourly
	                                datadetail = Enumerable.From(data).Where(function(x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, ahdata['Day'], ahdata['Hour'], x.TranDate); }).ToArray();
	                                break;
	                            case 2: //daily
	                                datadetail = Enumerable.From(data).Where(function(x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, ahdata['Day'], null, x.TranDate); }).ToArray();
	                                break;
	                            case 3: //weekly

	                                datadetail = Enumerable.From(data).Where(function(x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], null, ahdata['Week'], null, null, x.TranDate); }).ToArray();
	                                //header = FormatDaily(startDate.getFullYear(), startDate.getMonth() + 1, startDate.getDate()) + ' - ' + FormatDaily(endDate.getFullYear(), endDate.getMonth() + 1, endDate.getDate());
	                                break;
	                            case 4: //monthly
	                                datadetail = Enumerable.From(data).Where(function(x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, null, null, x.TranDate); }).ToArray();
	                                break;
	                            }
	                        }

	                        datadetail.forEach(function(r) {
	                            var row = {
	                                Type: AppDefine.tableExport.Body,
	                                ColDatas: [],
	                                GridModels: null
	                            }

	                            colIndex = 2;
	                            rowIndex += 1;
	                            var cols = [{ Value: "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex }];
	                            columns.forEach(function(x) {
	                                colIndex += 1;
	                                if (x.fieldName === 'Payments' || x.fieldName === 'Taxs' || x.fieldName === 'ExceptionTypes' || x.fieldName === 'Notes') {
	                                    var pay = Enumerable.From(r[x.fieldName]).Select(function(f) { return f.Name; }).ToArray().join();
	                                    cols.push({ Value: pay ? pay : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
	                                } else {
	                                    if (x.fieldName === 'TranDate' || x.fieldName === 'DvrDate') {
	                                        cols.push({ Value: r[x.fieldName] ? $filter('date')(r[x.fieldName], "MM/dd/yyyy HH:mm:ss") : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
	                                    } else {
	                                        cols.push({ Value: r[x.fieldName] ? r[x.fieldName] : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
	                                    }
	                                }
	                            });
	                            row.ColDatas = cols;
	                            rows.push(row);
	                        });
	                    });
	                });
	            });

	            rowIndex += 1;
	            rows.push({
	                Type: AppDefine.tableExport.Header,
	                ColDatas: [{ Value: cmsBase.translateSvc.getTranslate("TOTAL_AMOUNT") + ":" + $filter('number')(vmd.SumOfCountGroup, 2), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: 1, RowIndex: rowIndex, MergeCells: {  Cells: vmd.reportSetting.ColumnSelect.length + 2, Rows: 1 } }],
	                GridModels: null
	            });
	            rowIndex += 1;
	            rows.push({
	                Type: AppDefine.tableExport.Header,
	                ColDatas: [{ Value: cmsBase.translateSvc.getTranslate("TOTAL_COUNT") + ":" + vmd.GroupData.length, Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: 1, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 2, Rows: 1 } }],
	                GridModels: null
	            });

	            var result = {
	                Name: 'Result',
	                RowDatas: rows,
	                Format: { ColumnFirstSpace: 4, ColumnSpace: 3, ColumnEndSpace: 3 }
	            }
	            return result;
	        }

	        function buildExportSummaryTable(data) {
                var rows = [];
                var headers = [];
	            var colIndex = 1, rowIndex = 1;
                headers.push({
                    Value: "", Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 10
                });

	            var columns = Enumerable.From(vmd.definefield).Where(function(x) {
	                return Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function(c) { return c.DisplayName === x.fieldName; }).FirstOrDefault() !== undefined;
	            }).ToArray();

	            columns.forEach(function (x) {
                    colIndex += 1;
                    headers.push({ Value: cmsBase.translateSvc.getTranslate(x.fieldName), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: colIndex, RowIndex: rowIndex, CustomerWidth: true, Width: 20 });
                });

                rows.push({
                    Type: AppDefine.tableExport.Header,
                    ColDatas: headers,
                    GridModels: null
                });

                vmd.AhocData.forEach(function (ah) {
                    colIndex = 1;
                    rowIndex += 1;
                    var groupPm = Enumerable.From(vmd.reportSetting.ColumnSelect).Where(function (x) { return x.GroupBy === true; }).FirstOrDefault();

                    var row = {
                        Type: AppDefine.tableExport.Body,
                        ColDatas: [{ Value: cmsBase.translateSvc.getTranslate(vmd.RowHeaderGroup(ah)) + " : " +$filter('number')(ah.TotalTran, 2), Color: AppDefine.ExportColors.GridHeaderFirstCell, ColIndex: colIndex, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 1, Rows: 1 } }],
                        GridModels: null
                    }
                    //vmd.reportSetting.ColumnSelect.forEach(function (x) {
                    //    colIndex += 1;
                    //    row.ColDatas.push({ Value: "", Color: AppDefine.ExportColors.GridHeaderFirstCell, ColIndex: colIndex, RowIndex: rowIndex });
                    //});

                    rows.push(row);


                    ah.DataResult.forEach(function(ahdata) {
                        var datadetail;
                        if (vmd.GroupProperty !== 'TranDate') {

                            if (groupPm.DisplayName === "Payments") {
                                datadetail = Enumerable.From(data).Where(function(x) {
                                    return ahdata['PacId'] == x['PacId'] && Enumerable.From(x.Payments).Where(function(pm) { return ahdata.Id === pm.Id; }).FirstOrDefault(); 
                                }).ToArray();
                            } else {
                                datadetail = Enumerable.From(data).Where(function (x) { return ahdata['PacId'] == x['PacId'] && ahdata[groupPm.DisplayName] === x[groupPm.DisplayName]; }).ToArray();
                            }
                            
                        } else {
                            switch (vmd.GroupFieldSelected.ColWidth) {
                                case 1: //hourly
                                    datadetail = Enumerable.From(data).Where(function (x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, ahdata['Day'], ahdata['Hour'], x.TranDate); }).ToArray();
                                    break;
                                case 2: //daily
                                    datadetail = Enumerable.From(data).Where(function (x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, ahdata['Day'], null, x.TranDate); }).ToArray();
                                    break;
                                case 3: //weekly
                                    
                                    datadetail = Enumerable.From(data).Where(function (x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], null, ahdata['Week'], null, null, x.TranDate); }).ToArray();
                                    //header = FormatDaily(startDate.getFullYear(), startDate.getMonth() + 1, startDate.getDate()) + ' - ' + FormatDaily(endDate.getFullYear(), endDate.getMonth() + 1, endDate.getDate());
                                    break;
                                case 4: //monthly
                                    datadetail = Enumerable.From(data).Where(function (x) { return ahdata['PacId'] == x['PacId'] && compareGroupDate(ahdata['Year'], ahdata['Month'], null, null, null, x.TranDate); }).ToArray();
                                    break;
                            }
                        }

                        datadetail.forEach(function(r) {
                            var row = {
                                Type: AppDefine.tableExport.Body,
                                ColDatas: [],
                                GridModels: null
                            }

                            colIndex = 1;
                            rowIndex += 1;
                            var cols = [{ Value: "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex }];
                            columns.forEach(function (x) {
                                colIndex += 1;
                                if (x.fieldName === 'Payments' || x.fieldName === 'Taxs' || x.fieldName === 'ExceptionTypes' || x.fieldName === 'Notes') {
                                    var pay = Enumerable.From(r[x.fieldName]).Select(function(f) { return f.Name; }).ToArray().join();
                                    cols.push({ Value: pay ? pay : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
                                } else {
                                    if (x.fieldName === 'TranDate' || x.fieldName === 'DvrDate') {
                                        cols.push({ Value: r[x.fieldName] ? $filter('date')(r[x.fieldName], "MM/dd/yyyy HH:mm:ss") : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
                                    } else {
                                        cols.push({ Value: r[x.fieldName] ? r[x.fieldName] : "", Color: AppDefine.ExportColors.GridSumCell, ColIndex: colIndex, RowIndex: rowIndex });
                                    }
                                }
                            });
                            row.ColDatas = cols;
                            rows.push(row);
                        });
                    });
               });

                rowIndex += 1;
                rows.push({
                    Type: AppDefine.tableExport.Header,
                    ColDatas: [{ Value: cmsBase.translateSvc.getTranslate("TOTAL_AMOUNT") + ":" + $filter('number')(vmd.SumOfTotal, 2), Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: 1, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 1, Rows: 1 } }],
                    GridModels: null
                });
                rowIndex += 1;
                rows.push({
                    Type: AppDefine.tableExport.Header,
                    ColDatas: [{ Value: cmsBase.translateSvc.getTranslate("TOTAL_COUNT") + ":" + vmd.SumOfCount, Color: AppDefine.ExportColors.GridHeaderCell, ColIndex: 1, RowIndex: rowIndex, MergeCells: { Cells: vmd.reportSetting.ColumnSelect.length + 1, Rows: 1 } }],
                    GridModels: null
                });

                var result = {
                    Name: 'Result',
                    RowDatas: rows,
                    Format: { ColumnFirstSpace: 4, ColumnSpace: 3, ColumnEndSpace: 3 }
                }
                return result;
            }
		}
	});
})();

