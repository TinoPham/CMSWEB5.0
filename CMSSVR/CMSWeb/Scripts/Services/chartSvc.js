(function () {
	define(['cms', 'Scripts/Services/FileSaver'], function (cms) {
		cms.register.service('chartSvc', chartSvc);
		chartSvc.$inject = ['AppDefine', '$modal', '$filter', 'cmsBase', 'Utils'];

		function chartSvc(AppDefine, $modal, $filter, cmsBase, utils) {
			function DateToString(dtime) {
				var retDate = null;
				if (typeof (dtime) == 'string') {
					retDate = new Date(dtime);
				}
				else {
					retDate = dtime;
				}
				return retDate.getFullYear().toString() + '/' + (retDate.getMonth() + 1).toString() + '/' + retDate.getDate().toString();
			}
			function createParam(keys, date, type, period) {
				var param = {};
				param.siteKeys = keys;
				param.type = type;
				param.date = date;
				param.period = period;
				param.severity = '';

				return param;
			}
			function CreateAlertParam(keys, date, type, period, severity) {
				var param = {};
				param.siteKeys = keys;
				param.type = type;
				param.date = date;
				param.period = period;
				param.severity = severity;

				return param;
			}
			function GetColor(curVal, minVal, maxVal) {
				if (curVal < minVal) {
					return AppDefine.ChartColor.Red;
				}
				else if (curVal > maxVal) {
					return AppDefine.ChartColor.Green;
				}
				else {
					return AppDefine.ChartColor.Yellow;
				}
			}

			function download(filename, text) {
				/*
				var pom = document.createElement('a');
				pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
				pom.setAttribute('download', filename);
				pom.style.display = 'none';
				document.body.appendChild(pom);
				pom.click();
				document.body.removeChild(pom);
				*/
				var blob = new Blob([text], { type: "text/plain;charset=utf-8" });
				saveAs(blob, filename);
			}

			function GetTypes(strID) {
				var retVal = '';
				if (strID.lastIndexOf('exportpdf') === 0) {
					retVal = "pdf";
				}
				else if (strID.lastIndexOf('exportcsv') === 0) {
					retVal = "csv";
				}
				else if (strID.lastIndexOf('exportpng') === 0) {
					retVal = "png";
				}
				return retVal;
			}
			/*
			//var types = {
							//	"exportpdf": "pdf",
							//	"exportcsv": "csv",
							//	"exportpng": "png"
							//};
			*/
			var arrItemIDs = [];
			function CreateExportEvent(extStr, fname) {
				var exportEvt = {
					renderComplete: function (e, a) {
						// Cross-browser event listening
						var addListener = function (elem, evt, fn) {
							if (elem && elem.addEventListener) {
								//elem.removeEventListener(evt, fn);
								elem.addEventListener(evt, fn);
							}
							else if (elem && elem.attachEvent) {
								elem.attachEvent("on" + evt, fn);
							}
							else {
								elem["on" + evt] = fn;
							}
						};
						var removeListener = function (elem, evt, fn) {
							if (elem && elem.addEventListener) {
								elem.removeEventListener(evt, fn);
							}
							else if (elem && elem.detachEvent) {
								elem.detachEvent("on" + evt, fn);
							}
							else {
								elem["on" + evt] = '';
							}
						};

						// Export chart method
						var exportFC = function () {
							var expType = GetTypes(this.id);
							if (e && e.sender && expType === "csv") {
								var csvData = e.sender.getCSVData();
								download(fname + ".csv", csvData);
							}
							else if (e && e.sender && e.sender.exportChart) {
								e.sender.exportChart({
									exportFileName: fname,
									exportFormat: expType //types[this.id]
								});
							}
						};
						// Attach events
						var exppdf_id = "exportpdf" + extStr;
						var expcsv_id = "exportcsv" + extStr;
						var exppng_id = "exportpng" + extStr;
						if (arrItemIDs.indexOf(exppdf_id) == -1) {
							arrItemIDs.push(exppdf_id);
							//addListener(document.getElementById(exppdf_id), "click", exportFC);
						}
						else {
							removeListener(document.getElementById(exppdf_id), "click", exportFC);
						}
						addListener(document.getElementById(exppdf_id), "click", exportFC);

						if (arrItemIDs.indexOf(expcsv_id) == -1) {
							arrItemIDs.push(expcsv_id);
							//addListener(document.getElementById(expcsv_id), "click", exportFC);
						}
						else {
							removeListener(document.getElementById(expcsv_id), "click", exportFC);
						}
						addListener(document.getElementById(expcsv_id), "click", exportFC);

						if (arrItemIDs.indexOf(exppng_id) == -1) {
							arrItemIDs.push(exppng_id);
							//addListener(document.getElementById(exppng_id), "click", exportFC);
						}
						else {
							removeListener(document.getElementById(exppng_id), "click", exportFC);
						}
						addListener(document.getElementById(exppng_id), "click", exportFC);
					}
					, dataPlotClick: function (eventObj, dataObj) {
						if (eventObj.sender.options.dataSource.chartName === "TRAFFICSTATISTIC") {
							var currentCategory = dataObj.categoryLabel;
							var dataToolTip = $.grep(eventObj.sender.options.dataSource.toolTipData, function (e, i) { return e.time == currentCategory });
							if (dataToolTip.length > 0) {
								$("#fusioncharts-tooltip-element").hide();
								showModalDetail(dataToolTip[0]);
							}
						}
					}
					, dataplotRollOver: function (eventObj, dataObj) {
						//$("#TrafficPlotDetail").empty().append(dataObj.toolText)
						//		.append("<span id=\"closeTrafficDetail\"><i class=\"icon-cancel-1\"></i></span>")
						//		.css("width", "315px").css("overflow-x", "auto").css("position", "absolute")
						//		.css("background-color", "#fff").css("border", "1px solid #ccc")
						//		.css("box-shadow", "3px 3px 3px #eee").css("padding", "5px")
						//		.css("top", dataObj.chartY).css("left", dataObj.chartX)
						//		.show();

						//$("#closeTrafficDetail").css("cursor", "pointer").bind("click", function (e) {
						//	$("#TrafficPlotDetail").hide();
						//});
					}
					, dataplotRollOut: function (eventObj, dataObj) {
						//$("#TrafficPlotDetail").hide();
					}
				};
				return exportEvt;
			}
			function GetConvColor(val) {
				var color = Enumerable.From(AppDefine.ConvMapRanges)
						.Where(function (i) { return (i.min <= val) && (i.max > val || i.max == 100) })
						.Select(function (x) { return x.color; })
						.First();
				return color;
			}

			function FYFormatDate(_date) {
				return ("0" + (_date.getMonth() + 1).toString()).slice(-2) + "/" + ("0" + _date.getDate().toString()).slice(-2) + "/" + (_date.getFullYear().toString());
			}
			function GetUTCDate(date) {
				var dateUTC = new Date(date);
				var retDate = new Date(dateUTC.getUTCFullYear(), dateUTC.getUTCMonth(), dateUTC.getUTCDate(), dateUTC.getUTCHours(), dateUTC.getUTCMinutes(), dateUTC.getUTCSeconds(), dateUTC.getUTCMilliseconds());
				return retDate;
			}
			//********************* FISCAL YEAR - Begin **********************/
			function dateAdds(date, days) {
				var result = new Date(date);
				result.setDate(date.getDate() + days);
				return result;
			}
			function dateDiff(date1, date2) {
				var datediff = clearTime(date1).getTime() - clearTime(date2).getTime();
				return Math.round(datediff / (24 * 60 * 60 * 1000));
			}
			function clearTime(d) {
				d.setHours(0);
				d.setMinutes(0);
				d.setSeconds(0);
				d.setMilliseconds(0);
				return d;
			}
			function GetFiscalWeek(fyInfo, _date) {
				if (fyInfo == null || fyInfo.FYDateStart == null)
					return null;
				var fiscalWeek = {};
				var fyStart = GetUTCDate(fyInfo.FYDateStart);
				var fyEnd = GetUTCDate(fyInfo.FYDateEnd);

				var totalDays = dateDiff(_date, fyStart);
				var numDayofFYear = dateDiff(fyEnd, fyStart) + 1;//fyInfo.FYNoOfWeeks * 7;
				while (totalDays < 0) {
					fyStart = dateAdds(fyStart, 0 - numDayofFYear);
					totalDays = dateDiff(_date, fyStart);
				}
				while (totalDays > numDayofFYear) {
					fyStart = dateAdds(fyStart, numDayofFYear);
					totalDays = dateDiff(_date, fyStart);
				}
				if (totalDays >= 0) {
					fiscalWeek.WeekNo = ~~(totalDays / 7); //Start from Zero (0)
					fiscalWeek.StartDate = dateAdds(fyStart, fiscalWeek.WeekNo * 7);//fyStartDate.AddDays(fw.WeekNo * 7);
					fiscalWeek.EndDate = dateAdds(fiscalWeek.StartDate, 7 - 1);//fw.StartDate.AddDays(7);
					if (fiscalWeek.EndDate > fyEnd) {
						fiscalWeek.EndDate = fyEnd;
					}
				}
				return fiscalWeek;
			}
			function GetFiscalPeriod(_weekNo, calStyle, _startDateFY, _endDateFY, _isLast, _totalWeek) {
				var wNumber = [];
				if (calStyle == '445') {
					wNumber[0] = 4;
					wNumber[1] = 4;
					wNumber[2] = 5;
				}
				else if (calStyle == '544') {
					wNumber[0] = 5;
					wNumber[1] = 4;
					wNumber[2] = 4;
				}
				else {
					//if (calStyle.CompareTo(FiscalCalendarConst.CStyle_454) == 0)
					wNumber[0] = 4;
					wNumber[1] = 5;
					wNumber[2] = 4;
				}
				var fiscalPeriod = {};
				var iPeriod = 0;
				var quarterStart = 0;
				var _startDate = null;//_startDateFY;
				var _endDate = null;//_endDateFY;
				var weekNo = _weekNo;

				if (_isLast) {
					if (weekNo < wNumber[0]) {
						weekNo = _totalWeek - 1;
						var totalDays = dateDiff(_endDateFY, _startDateFY) + 1;
						_startDate = dateAdds(_startDateFY, 0 - totalDays);
						_endDate = dateAdds(_startDateFY, -1);
					}
					else {
						_startDate = _startDateFY;
						_endDate = _endDateFY;

						quarterStart = 0;
						var bBreak = false;
						for (var qt = 0; qt < 4; qt++) {
							if (weekNo < wNumber[0] + quarterStart) {
								weekNo = quarterStart - 1;
								bBreak = true;
							}
							else if (weekNo < wNumber[0] + wNumber[1] + quarterStart) {
								weekNo = quarterStart + wNumber[0] - 1;
								bBreak = true;
							}
							else if (weekNo < wNumber[0] + wNumber[1] + wNumber[2] + quarterStart) {
								weekNo = quarterStart + wNumber[0] + wNumber[1] - 1;
								bBreak = true;
							}
							if (bBreak)
								break;
							quarterStart += wNumber[0] + wNumber[1] + wNumber[2];
						}
					}
				}
				else {
					_startDate = _startDateFY;
					_endDate = _endDateFY;
				}

				/*
					for (var qt = 0; qt < 4; qt++) {
						if (weekNo < wNumber[0] + quarterStart) {
							if (quarterStart == 0 && qt == 0) {
								var daysPerFY = _totalWeek * 7;
								var lastStartDate = dateAdds(_startDate, 0 - daysPerFY);
								var lastEndDate = dateAdds(_endDate, 0 - daysPerFY);

								iPeriod = 12;
								fiscalPeriod.StartDate = dateAdds(lastStartDate, (26 + wNumber[0] + wNumber[1]) * 7);
								fiscalPeriod.EndDate = new Date(lastEndDate);
							}
							else {
								iPeriod = (qt * 3);
								fiscalPeriod.StartDate = dateAdds(_startDate, (quarterStart - 13 + wNumber[0] + wNumber[1]) * 7);
								fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart - 13 + wNumber[0] + wNumber[1] + wNumber[2]) * 7 - 1);
							}
						}
						else if (weekNo < wNumber[0] + wNumber[1] + quarterStart) {
							iPeriod = 1 + (qt * 3);
							fiscalPeriod.StartDate = dateAdds(_startDate, quarterStart * 7);
							fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart + wNumber[0]) * 7 - 1);
						}
						else if (weekNo < wNumber[0] + wNumber[1] + wNumber[2] + quarterStart) {
							iPeriod = 2 + (qt * 3);
							fiscalPeriod.StartDate = dateAdds(_startDate, (quarterStart + wNumber[0]) * 7);
							fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart + wNumber[0] + wNumber[1]) * 7 - 1);
						}
						if (iPeriod > 0)
							break;
						quarterStart += wNumber[0] + wNumber[1] + wNumber[2];
					} //for
					if (iPeriod == 0) {
						iPeriod = 11;
						fiscalPeriod.StartDate = dateAdds(_startDate, (26 + wNumber[0]) * 7);
						fiscalPeriod.EndDate = dateAdds(_startDate, (26 + wNumber[0] + wNumber[1]) * 7 - 1);
					}
				}*/
				//else {
				quarterStart = 0;
					for (var qt = 0; qt < 4; qt++) {
						if (weekNo < wNumber[0] + quarterStart) {
							iPeriod = 1 + (qt * 3);
							fiscalPeriod.StartDate = dateAdds(_startDate, quarterStart * 7);
							fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart + wNumber[0]) * 7 - 1);
						}
						else if (weekNo < wNumber[0] + wNumber[1] + quarterStart) {
							iPeriod = 2 + (qt * 3);
							fiscalPeriod.StartDate = dateAdds(_startDate, (quarterStart + wNumber[0]) * 7);
							fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart + wNumber[0] + wNumber[1]) * 7 - 1);
						}
						else if (weekNo < wNumber[0] + wNumber[1] + wNumber[2] + quarterStart) {
							iPeriod = 3 + (qt * 3);
							fiscalPeriod.StartDate = dateAdds(_startDate, (quarterStart + wNumber[0] + wNumber[1]) * 7);
							fiscalPeriod.EndDate = dateAdds(_startDate, (quarterStart + wNumber[0] + wNumber[1] + wNumber[2]) * 7 - 1);
						}
						if (iPeriod > 0)
							break;
						quarterStart += wNumber[0] + wNumber[1] + wNumber[2];
					} //for
					if (iPeriod == 0) {
						iPeriod = 12;
						fiscalPeriod.StartDate = dateAdds(_startDate, (39 + wNumber[0] + wNumber[1]) * 7);
						fiscalPeriod.EndDate = new Date(_endDate);
					}
				//}
				fiscalPeriod.Period = iPeriod;
				return fiscalPeriod;
			}
			function GetFiscalQuarter(weekNo, _startDate, _endDate, _isLast, _totalWeek) {
				var fiscalQuarter = {};

				if (_isLast) {
					if (weekNo < 13) {
						var daysPerFY = _totalWeek * 7;
						var lastStartDate = dateAdds(_startDate, 0 - daysPerFY);
						var lastEndDate = dateAdds(_endDate, 0 - daysPerFY);

						fiscalQuarter.QuarterNo = 4;
						fiscalQuarter.StartDate = dateAdds(lastEndDate, (39 * 7) - daysPerFY + 1);
						fiscalQuarter.EndDate = new Date(lastEndDate);//dateAdds(_startDate, 13 * 7);
					}
					else if (weekNo < 26) {
						fiscalQuarter.QuarterNo = 1;
						fiscalQuarter.StartDate = new Date(_startDate);
						fiscalQuarter.EndDate = dateAdds(_startDate, 13 * 7 - 1);
					}
					else if (weekNo < 39) {
						fiscalQuarter.QuarterNo = 2;
						fiscalQuarter.StartDate = dateAdds(_startDate, 13 * 7);
						fiscalQuarter.EndDate = dateAdds(_startDate, 26 * 7 - 1);
					}
					else {
						fiscalQuarter.QuarterNo = 3;
						fiscalQuarter.StartDate = dateAdds(_startDate, 26 * 7);
						fiscalQuarter.EndDate = dateAdds(_startDate, 39 * 7 - 1);
					}
				}
				else {
					if (weekNo < 13) {
						fiscalQuarter.QuarterNo = 1;
						fiscalQuarter.StartDate = new Date(_startDate);
						fiscalQuarter.EndDate = dateAdds(_startDate, 13 * 7 - 1);
					}
					else if (weekNo < 26) {
						fiscalQuarter.QuarterNo = 2;
						fiscalQuarter.StartDate = dateAdds(_startDate, 13 * 7);
						fiscalQuarter.EndDate = dateAdds(_startDate, 26 * 7 - 1);
					}
					else if (weekNo < 39) {
						fiscalQuarter.QuarterNo = 3;
						fiscalQuarter.StartDate = dateAdds(_startDate, 26 * 7);
						fiscalQuarter.EndDate = dateAdds(_startDate, 39 * 7 - 1);
					}
					else {
						fiscalQuarter.QuarterNo = 4;
						fiscalQuarter.StartDate = dateAdds(_startDate, 39 * 7);
						fiscalQuarter.EndDate = new Date(_endDate);//dateAdds(_startDate, 13 * 7);
					}
				}
				return fiscalQuarter;
			}
			function GetCalendarPeriod(_date, _isLast) {
				var month = _date.getMonth();
				var year = _date.getFullYear();
				var fiscalPeriod = {};
				if (_isLast) {
					month = month - 1;
					if (month < 0) {
						month = 11;
						year = year - 1;
					}
				}
				fiscalPeriod.StartDate = new Date(year, month, 1);
				fiscalPeriod.EndDate = (month == 11) ? new Date(year, month, 31) : dateAdds(new Date(year, month + 1, 1), -1);

				fiscalPeriod.Period = month;
				return fiscalPeriod;
				//return new Date(year, month, 1);
			}
			function GetFiscalPeriodInfo(fyInfo, _weekNo, _date, _isLast) {
				if (fyInfo == null || fyInfo == undefined) { // || fyInfo.FYTypesID == AppDefine.FiscalTypes.NORMAL) {
					return GetCalendarPeriod(_date, _isLast);
				}
				else {
					var weekNo = _weekNo;
					if (weekNo < 0) {
						var fw = GetFiscalWeek(fyInfo, _date);
						weekNo = fw.WeekNo;
					}
					var fyStartDate = GetUTCDate(fyInfo.FYDateStart);//new Date(fyInfo.FYDateStart);
					var fyEndDate = GetUTCDate(fyInfo.FYDateEnd);

					return GetFiscalPeriod(weekNo, fyInfo.CalendarStyle, fyStartDate, fyEndDate, _isLast, fyInfo.FYNoOfWeeks);
				}
			}
			//********************* FISCAL YEAR - End ************************/
			//********************* SITETREE - Begin ************************/
			function UpdateSiteChecked(treeData, userSelected, checkAll) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (checkAll) {
					treeData.Checked = true; //Checked on root node
				}
				if ($.isEmptyObject(treeData.Sites)) {
					angular.forEach(treeData, function (n) {
						if (n != null && n != undefined) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites, userSelected, checkAll);
							}
							if (checkAll == false && (userSelected == null || userSelected == undefined
								|| userSelected.SiteIDs == null || userSelected.SiteIDs == undefined
								|| userSelected.SiteIDs.indexOf(n.ID) < 0)) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
				else {
					angular.forEach(treeData.Sites, function (n) {
						if (n != null && n != undefined) {
							if (n.Sites && n.Sites.length > 0) {
								UpdateSiteChecked(n.Sites, userSelected, checkAll);
							}
							if (checkAll == false && (userSelected == null || userSelected == undefined
								|| userSelected.SiteIDs == null || userSelected.SiteIDs == undefined
								|| userSelected.SiteIDs.indexOf(n.ID) < 0)) {
								n.Checked = false;
							}
							else {
								n.Checked = true;
							}
						}
					});
				}
			}

			function SetNodeSelected(treeData, siteIds) {
				if (treeData == null || treeData == undefined || !siteIds) {
					return;
				}

				if (angular.isArray(treeData)) {
					angular.forEach(treeData, function (n) {
						if (n.Type === AppDefine.NodeType.Site && siteIds.indexOf(n.ID) != -1) {
							n.Checked = true;
						}
						else {
							n.Checked = false;
							if (n.Type === AppDefine.NodeType.Region) {
								SetNodeSelected(n.Sites, siteIds);
							}
						}
					});
				}
				else {
					if (treeData.Type === AppDefine.NodeType.Site && siteIds.indexOf(treeData.ID) != -1) {
						treeData.Checked = true;
					}
					else {
						treeData.Checked = false;
						if (treeData.Type === AppDefine.NodeType.Region) {
							SetNodeSelected(treeData.Sites, siteIds);
						}
					}
				}
			}

			function GetSiteSelectedIDs(siteCheckedIDs, treeData) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (siteCheckedIDs == null || siteCheckedIDs == undefined) {
					siteCheckedIDs = new Array();
				}
				var hasChecked = false;
				angular.forEach(treeData, function (n) {
					if (n != null && n != undefined) {
						if (n.Sites && n.Sites.length > 0) {
							GetSiteSelectedIDs(siteCheckedIDs, n.Sites);
						}
						if (n.Checked == true) {
							if (n.Type == 1) {
								siteCheckedIDs.push(n.ID);
							}
							else if (!hasChecked) {
								hasChecked = true;
							}
						}
					}
				});
				if (hasChecked && siteCheckedIDs.length == 0) {
					siteCheckedIDs.push(-1);
				}
			}
			function GetSiteSelectedNames(siteCheckedIDs, siteNames, treeData) {
				if (treeData == null || treeData == undefined) {
					return;
				}
				if (siteCheckedIDs == null || siteCheckedIDs == undefined) {
					siteCheckedIDs = new Array();
				}
				var hasChecked = false;
				angular.forEach(treeData, function (n) {
					if (n != null && n != undefined) {
						if (n.Sites && n.Sites.length > 0) {
							GetSiteSelectedNames(siteCheckedIDs, siteNames, n.Sites);
						}
						if (n.Checked == true) {
							if (n.Type == 1) {
								siteCheckedIDs.push(n.ID);
								siteNames.push(n.Name);
							}
							else if (!hasChecked) {
								hasChecked = true;
							}
						}
					}
				});
				if (hasChecked && siteCheckedIDs.length == 0) {
					siteCheckedIDs.push(-1);
					siteNames.push('');
				}
			}
			//********************* SITETREE - End ************************/
			
			function showModalDetail(data) {
				var TrafficModalInstance = $modal.open({
					templateUrl: 'Widgets/Charts/TrafficDetail.html',
					controller: 'TrafficDetailCtrl',
					size: 'sm',
					backdrop: 'static',
					backdropClass: 'modal-backdrop',
					keyboard: false,
					resolve: {
						items: function () {
							return {
								data: data,
								titleString: AppDefine.Resx.CHART_TRAFFIC_STATISTICS_DETAIL
							};
						}
					}
				});

				TrafficModalInstance.result.then(function (data) {});
			}

			function formatHourBAM(value) {
				//if (angular.isNumber(value)) { return; }
				if (value < 10) {
					return "0" + value + ":00-0" + (value + 1) + ":00";
				}
				else {
					if (value == 23) { return value + ":00" + "-00:00"; }
					return value + ":00-" + (value + 1) + ":00";
				}
			}
			//********************* BAM Charts - Begin ************************/
			function formatHourChart(value) {
				var strVal = '';
				//if (value < 10) {
				//	strVal = "0" + value + " AM";
				//}
				if (value < 12) {
					strVal = value + " AM";
				}
				else {
					if (value == 12) {
						strVal = "12 PM";
					}
					else {
						strVal = (value - 12) + " PM";
					}
				}
				return strVal;
			}

			function formatChartLabel(rptType, data) {
				var label = '';
				switch (rptType) {
					case AppDefine.SaleReportTypes.Hourly:
						label = formatHourChart(data.TimeIndex);
						break;
					case AppDefine.SaleReportTypes.Weekly:
						label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.WEEK_STRING) + data.TimeIndex;
						break;
					case AppDefine.SaleReportTypes.Monthly:
						label = cmsBase.translateSvc.getTranslate(AppDefine.Resx.PERIOD_STRING) + data.TimeIndex;
						break;
					default:
						label = $filter('date')(GetUTCDate(data.Date), AppDefine.BAMDateFormat);
						break;
				}
				return label;
			}

			function CreateBAMChartTooltip(data, pname, type, label) {
				// Conversion CountTrans TrafficIn TrafficOut Count Dwell
				var strTooltip = '<div class="bam_tooltip"><ul>';//'<div class="bam_tooltip"><ul><li>sdsdsds</li><li>sdsdsds</li><li>sdsdsds</li><li> <span class="green bold">sd</span>sdsds</li></ul></div>';
				if (data.hasOwnProperty('Name')) {
					strTooltip += '<li><span class="yellow bold">' + pname + ' - ' + label + '</span></li><li><span> &nbsp;</span></li>';
				}
				else {
					strTooltip += '<li><span class="yellow bold">' + pname + '</span></li><li><span> &nbsp;</span></li>';
				}
				if (data.hasOwnProperty('Conversion') && type == AppDefine.BamDataTypes.CONVERSION) {
					strTooltip += '<li>Conversion: <span class="green bold">' + data.Conversion.toFixed(2) + ' %</span></li>';
				}
				if (data.hasOwnProperty('CountTrans') && type == AppDefine.BamDataTypes.POS) {
					strTooltip += '<li>POS: <span class="green bold">' + data.CountTrans + '</span></li>';
				}
				if (data.hasOwnProperty('TrafficIn') && type == AppDefine.BamDataTypes.TRAFFIC_IN) {
					strTooltip += '<li>Count In: <span class="green bold">' + data.TrafficIn + '</span></li>';
				}
				if (data.hasOwnProperty('TrafficOut') && type == AppDefine.BamDataTypes.TRAFFIC_OUT) {
					strTooltip += '<li>Count Out: <span class="green bold">' + data.TrafficOut + '</span></li>';
				}
				if (type == AppDefine.BamDataTypes.COUNT) {
					if (data.hasOwnProperty('Count')) {
						strTooltip += '<li>Count: <span class="green bold">' + data.Count + '</span></li>';
					}
					if (data.hasOwnProperty('DataYTD')) {
						if (data.DataYTD != null) {
							strTooltip += '<li>YTD Count: <span class="green bold">' + data.DataYTD.Count + '</span></li>';
						}
						else {
							var totalCount = Enumerable.From(data.Details).FirstOrDefault();
							//	  .Sum(function (x) { return x.DataYTD != null ? x.DataYTD.Count : 0; });

							var ave = (totalCount == null || totalCount.DataYTD == null) ? 0 : Math.round(totalCount.DataYTD.Count);// / Math.max(data.ItemCount, 1));
							strTooltip += '<li>YTD Count: <span class="green bold">' + ave + '</span></li>';
						}
					}
				}
				if (type == AppDefine.BamDataTypes.DWELL) {
					if (data.hasOwnProperty('Dwell')) {
						strTooltip += '<li>Dwell(s): <span class="green bold">' + data.Dwell + '</span></li>';
					}
					if (data.hasOwnProperty('DataYTD') && data.DataYTD != null) {
						strTooltip += '<li>YTD Dwell(s): <span class="green bold">' + data.DataYTD.Dwell + '</span></li>';
					}
				}

				if (data.hasOwnProperty('KDVR') && data.hasOwnProperty('ChannelNo')) {
					if (data.KDVR && data.KDVR > 0) {
						var chan = data.ChannelNo;// + 1;
						var strchan = chan.toString();
						if (chan < 10) strchan = '0' + strchan;
						var imgURL = AppDefine.Api.SiteChanImage + "name=C_" + strchan + ".jpg&kdvr=" + data.KDVR.toString();
						strTooltip += '<li><img src="' + imgURL + '" alt="Image" style="width:120px;" /></li>';
					}
				}
				strTooltip += '</ul></div>';
				return strTooltip;
			}
			//********************* BAM Charts - End ************************/

			return {
				dateToString: DateToString,
				createParam: createParam,
				getColor: GetColor,
				createAlertParam: CreateAlertParam,
				createExportEvent: CreateExportEvent,
				FYFormatDate: FYFormatDate,
				clearTime: clearTime,
				dateAdds: dateAdds,
				dateDiff: dateDiff,
				GetUTCDate: GetUTCDate,
				GetFiscalWeek: GetFiscalWeek,
				GetFiscalPeriod: GetFiscalPeriod,
				GetCalendarPeriod: GetCalendarPeriod,
				GetFiscalPeriodInfo: GetFiscalPeriodInfo,
				GetFiscalQuarter: GetFiscalQuarter,
				UpdateSiteChecked: UpdateSiteChecked,
				GetSiteSelectedIDs: GetSiteSelectedIDs,
				GetSiteSelectedNames : GetSiteSelectedNames,
				GetConvColor: GetConvColor,
				SetNodeSelected: SetNodeSelected,
				formatHourBAM: formatHourBAM,
				formatHourChart: formatHourChart,
				CreateBAMChartTooltip: CreateBAMChartTooltip,
				formatChartLabel: formatChartLabel,
				beginOfDate: function (dtval) { return utils.beginOfDate(dtval); }
			}
		}
	});
})();
