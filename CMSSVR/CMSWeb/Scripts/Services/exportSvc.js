(function () {
	'use strict';

	define([
        'cms',
        'scripts/services/jsPdf',
        'scripts/services/FileSaver',
        'scripts/DataServices/Export/ExportAPIService'
	], function (cms) {
		cms.register.service('exportSvc', exportSvc);
		exportSvc.$inject = ['Base64', '$filter', 'ExportAPISvc', 'AppDefine', 'cmsBase'];

		function exportSvc(Base64, $filter, ExportAPISvc, AppDefine, cmsBase) {

			var defaults = {
				separator: ',',
				ignoreColumn: [],
				tableName: 'yourTableName',
				type: 'csv',
				pdfFontSize: 11,
				pdfLeftMargin: 10,
				escape: 'false',
				htmlContent: 'false',
				consoleLog: 'false'
			};
			var textLine = '                                                                       ';

			var service = {
				ToXls: exportToExcel,
				ToCSV: exportToCSV,
				ToSensorCSV: exportSensorToCsv,
				ToPdf: exportToPdf,
				ToSensorPdf: exportSensorToPdf,
				ToImagePdf: exportToSimplePdf,
				exportXlsFromServer: exportXlsFromServer,
				lineChartValue: lineChartValue,
				getContentByClass: getContentByClass,
				getContent: getContent,
				getTableColorCompareGoal: getTableColorCompareGoal,
				getTableBGColorCompareGoal: getTableBGColorCompareGoal,
				formatData: formatData,
				buildTableTemplate: buildTableTemplate,
				getColorChart: getColorChart,
				parseString: parseString
			};

			function buildReportInfo(data) {
				if (!data.TemplateName) data.TemplateName = "ExportFile";
				if (!data.ReportName) data.ReportName = "ExportFile";
				if (!data.CompanyID) data.CompanyID = 0;
				return data;
			}

			function exportXlsFromServer(reportInfo, tableDataList, chartDataList) {
				var model = {
					ReportInfo: buildReportInfo(reportInfo),
					GridModels: [],
					ChartModels: []
				}
				if (tableDataList) {
					model.GridModels = tableDataList;
				}

				if (chartDataList) {
					model.ChartModels = chartDataList;
				}

				console.log(model);

				ExportAPISvc.exportExcel(model).then(
					function (response) {
						location.href = ExportAPISvc.DownloadExport(response);
					});
			}

			function exportToSimplePdf(el, sheetName) {
				var pdf = new jsPDF('l', 'pt', 'a3');
				var options = {
					pagesplit: true,
					overflow: 'none',
					width: 1200,
					background: '#fff'
				};

				pdf.addHTML(el, 0, 0, options, function () {
					pdf.save(sheetName + ".pdf");
				});
			}

			function exportToPdf(el, sheetName) {
				var doc = new jsPDF('l', 'pt', 'a3');
				var rowCalc = 0;
				var rotateHeader = -35;
				var lineweight = 0.1;
				var marginLeft = 10;
				var pageWidth = 1140;
				var heightRotate = 130;
				var deltaWidthHeader = 165;
				var lineheightheader = 40;
				var lineheight = 30;
				var startHeight = 40;
				var deltaLine = 10;
				var colPosition = 0;
				var colorline = 204;

				doc.setFontSize(defaults.pdfFontSize);
				setLine(doc, marginLeft, marginLeft, pageWidth, marginLeft, lineweight, colorline);

				var tablewidth = el[0].clientWidth;

				var widthpercent = pageWidth / tablewidth;
				// Header

				$(el).find('thead').find('tr').each(function (ind) {
					if (ind === 0) {
						$(this).filter(':visible').find('th').each(function (index, data) {
							if ($(this).css('display') != 'none') {
								if (defaults.ignoreColumn.indexOf(index) == -1) {

									if (index === 0) {
										colPosition += data.clientWidth * widthpercent - deltaWidthHeader;
									} else {
										var textout = parseString($(this));
										var pdftext = " ";
										if (textout.length > 0) {
											var txtWidth = doc.getStringUnitWidth(textout) * defaults.pdfFontSize / doc.internal.scaleFactor;
											var txtline = doc.getStringUnitWidth(textLine) * defaults.pdfFontSize / doc.internal.scaleFactor;
											var txt = doc.getStringUnitWidth(" ") * defaults.pdfFontSize / doc.internal.scaleFactor;
											var nowhite = (txtline - txtWidth) / txt;
											pdftext = textLine.substr(0, nowhite) + textout;
										}
										doc.text(colPosition, startHeight, pdftext, null, rotateHeader);
										colPosition += data.clientWidth * widthpercent;
									}
								}
							}
						});
						startHeight += heightRotate;
						setLine(doc, marginLeft, startHeight + deltaLine, pageWidth, startHeight + deltaLine, lineweight, colorline);
					} else {
						startHeight += lineheightheader;
						colPosition = marginLeft;
						$(this).filter(':visible').find('th').each(function (index, data) {
							if ($(this).css('display') != 'none') {
								if (defaults.ignoreColumn.indexOf(index) == -1) {
									doc.text(colPosition, startHeight, parseString($(this)));
									colPosition += data.clientWidth * widthpercent;
								}
							}
						});
						setLine(doc, marginLeft, startHeight + deltaLine, pageWidth, startHeight + deltaLine, lineweight, colorline);
					}
				});


				// Row Vs Column
				var startRowPosition = startHeight + 5;
				var page = 1;
				var rowPosition = 0;
				var page0size = 15;
				var pagesize = 25;

				setLine(doc, marginLeft, startRowPosition + deltaLine, pageWidth, startRowPosition + deltaLine, lineweight, colorline);

				$(el).find('tbody').find('tr').each(function (index, data) {
					rowCalc += 1;

					if ((page === 1 && rowCalc % page0size === 0) || (page > 1 && rowCalc % pagesize === 0)) {
						page++;
						rowPosition = 0;
						rowCalc = 0;
						startRowPosition = 50;
						doc.addPage();
						setLine(doc, marginLeft, startRowPosition - lineheightheader / 2, pageWidth, startRowPosition - lineheightheader / 2, lineweight, colorline);
					}
					rowPosition = (startRowPosition + (rowCalc * lineheight));
					colPosition = marginLeft;
					$(this).filter(':visible').find('td').each(function (index, data) {
						if ($(this).css('display') != 'none') {
							if (defaults.ignoreColumn.indexOf(index) == -1) {
								var str;
								if ($(this).find('span').length > 0) {
									str = 'x';
								} else {

									if ($(this).find('a').length > 0) {
										str = parseString($(this));
									} else {
										str = parseString($(this));
									}
								}

								if (str && str.length > 0) {
									doc.text(colPosition, rowPosition, str);
									colPosition += data.clientWidth * widthpercent;
								} else {
									doc.text(colPosition, rowPosition, '');
									colPosition += data.clientWidth * widthpercent;
								}
								setLine(doc, marginLeft, rowPosition + deltaLine, pageWidth, rowPosition + deltaLine, lineweight, colorline);
							}
						}

					});

				});

				doc.save(sheetName + '.pdf');
			}

			function setLine(doc, x1, y1, x2, y2, weight, c1, c2, c3) {
				if (c1) {
					if (!c2) c2 = c1;
					if (!c3) c3 = c1;
					doc.setDrawColor(c1, c2, c3);
				}
				if (weight) {
					doc.setLineWidth(weight);
				}
				doc.line(x1, y1, x2, y2);
			}

			function polygon(doc, points, scale, style, closed) {
				var x1 = points[0][0];
				var y1 = points[0][1];
				var cx = x1;
				var cy = y1;
				var acc = [];
				for (var i = 1; i < points.length; i++) {
					var point = points[i];
					var dx = point[0] - cx;
					var dy = point[1] - cy;
					acc.push([dx, dy]);
					cx += dx;
					cy += dy;
				}
				doc.lines(acc, x1, y1, scale, style, closed);
			}

			function exportSensorToCsv(SensorsTable, dateExpand, sheetName) {
				var tdData = "";
				// Row Vs Column
				var rowCount = 1;

				SensorsTable.forEach(function (date) {
					tdData += "\n";

					tdData += '"' + $filter('date')(date.Time, "dd/MM/yyyy") + '"' + defaults.separator;
					tdData += '"' + "Total: " + date.Total + '"' + defaults.separator;

					if (date.Sensors.length > 0) {
						date.Sensors.forEach(function (sen) {
							tdData += "\n";
							tdData += '""' + defaults.separator;
							tdData += '"' + "Site: " + sen.SiteName + "(" + sen.TotalAlert + ")" + '"' + defaults.separator;
							tdData += '"' + "DVR Id: " + sen.DVRID + '"' + defaults.separator;

							if (sen.Details.length > 0 && date.reserve === true && sen.reserve === true) {
								sen.Details.forEach(function (det) {
									tdData += "\n";
									tdData += '""' + defaults.separator;
									tdData += '""' + defaults.separator;
									tdData += '"' + "Channel: " + det.ChannelName + '"' + defaults.separator;
									tdData += '"' + "Time: " + det.Time + '"' + defaults.separator;
									tdData += '"' + "Description: " + det.Description + '"' + defaults.separator;
								});
							}
						});
					}

				});

				tdData = $.trim(tdData).substring(0, tdData.length - 1);
				var base64data = "" + tdData; //$.base64.encode(tdData);

				var blob = new Blob([base64data]);
				saveAs(blob, sheetName + '.csv');
			}

			function exportSensorToPdf(SensorsTable, dateExpand, sheetName) {
				var doc = new jsPDF('l', 'pt', 'a3');
				var rowCalc = 0;
				var lineweight = 0.1;
				var marginLeft = 10;
				var pageWidth = 1140;
				var lineHeight = 20;
				var colorline = 204;

				doc.setFontSize(defaults.pdfFontSize);

				//   setLine(doc, marginLeft, marginLeft, pageWidth, marginLeft, lineweight, colorline);


				var startRowPosition = 5;
				var page = 1;
				var rowPosition = 0;
				var page0size = 25;
				var marginExpand = 30;
				var lineheight = 30;
				SensorsTable.forEach(function (date) {
					startRow();
					doc.text(marginLeft, rowPosition, "Date: " + $filter('date')(date.Time, "dd/MM/yyyy"));
					doc.text(pageWidth * 0.46, rowPosition, "Total: " + date.Total);
					// setLine(doc, marginLeft,  marginLeft, pageWidth, marginLeft, lineweight, colorline);
					if (date.Sensors.length > 0) {
						date.Sensors.forEach(function (sen) {

							startRow();
							doc.text(marginLeft + 30, rowPosition, "Site: " + sen.SiteName + "(" + sen.TotalAlert + ")");
							doc.text(pageWidth * 0.46, rowPosition, "DVR Id: " + sen.DVRID);
							// setLine(doc, marginLeft, marginLeft, pageWidth, marginLeft, lineweight, colorline);


							if (sen.Details.length > 0 && date.reserve === true && sen.reserve === true) {
								sen.Details.forEach(function (det) {
									startRow();
									doc.text(marginLeft + 60, rowPosition, "Channel: " + det.ChannelName);
									doc.text(pageWidth * 0.46, rowPosition, "Time: " + det.Time);
									doc.text(pageWidth * 0.46 + 150, rowPosition, "Description: " + det.Description);
									// setLine(doc, marginLeft, marginLeft, pageWidth, marginLeft, lineweight, colorline);
								});
							}
						});
					}

				});

				function startRow() {
					rowCalc += 1;

					if (rowCalc % page0size == 0) {
						page++;
						rowPosition = 0;
						rowCalc = 0;
						startRowPosition = 50;
						doc.addPage();
						//setLine(doc, marginLeft, rowPosition + marginLeft, pageWidth, rowPosition + marginLeft, lineweight, colorline);
					}
					rowPosition = (startRowPosition + (rowCalc * lineheight));

				}

				doc.save(sheetName + '.pdf');

			}

			function parseString(data) {

				var content_data;
				if (defaults.htmlContent == 'true') {
					content_data = data.html().trim();
				} else {
					content_data = data.text().trim();
				}

				if (defaults.escape == 'true') {
					content_data = escape(content_data);
				}


				return content_data;
			}

			function exportToCSV(elem, fileName) {
				// Header
				var tdData = "";
				$(elem).find('thead').find('tr').each(function () {
					tdData += "\n";
					$(this).filter(':visible').find('th').each(function (index, data) {
						if ($(this).css('display') != 'none') {
							if (defaults.ignoreColumn.indexOf(index) == -1) {
								tdData += '"' + parseString($(this)) + '"' + defaults.separator;
								var colpan = $(this).attr('colspan');
								if (colpan) {
									var colpanno = parseInt(colpan);
									for (var i = 0; i < colpanno - 1; i++) {
										tdData += '""' + defaults.separator;
									}
								}
							}
						}

					});
					tdData = $.trim(tdData);
					tdData = $.trim(tdData).substring(0, tdData.length - 1);
				});

				// Row vs Column
				$(elem).find('tbody').find('tr').each(function () {
					tdData += "\n";
					$(this).filter(':visible').find('td').each(function (index, data) {
						if ($(this).css('display') != 'none') {
							if (defaults.ignoreColumn.indexOf(index) == -1) {
								if ($(this).find('ul').length > 0) {
									tdData += '"x"' + defaults.separator;
								} else {

									if ($(this).find('a').length > 0) {
										tdData += '"' + parseString($(this)) + '"' + defaults.separator;
									} else {
										tdData += '"' + parseString($(this)) + '"' + defaults.separator;
									}
								}

								var colpan = $(this).attr('colspan');
								if (colpan) {
									var colpanno = parseInt(colpan);
									for (var i = 0; i < colpanno - 1; i++) {
										tdData += '""' + defaults.separator;
									}
								}
							}
						}
					});
					//tdData = $.trim(tdData);
					tdData = $.trim(tdData).substring(0, tdData.length - 1);
				});

				//output
				if (defaults.consoleLog == 'true') {
					console.log(tdData);
				}
				var base64data = "" + tdData; //$.base64.encode(tdData);

				var blob = new Blob([base64data]);
				saveAs(blob, fileName + '.csv');
				//window.open('data:application/' + defaults.type + ';filename=exportData;' + base64data);
			}

			function exportToExcel(elem, sheetName) {

				var defaults = {
					type: 'excel'
				}

				var excel = elem;

				var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:" + defaults.type + "' xmlns='http://www.w3.org/TR/REC-html40'>";
				excelFile += "<head>";
				excelFile += "<!--[if gte mso 9]>";
				excelFile += "<xml>";
				excelFile += "<x:ExcelWorkbook>";
				excelFile += "<x:ExcelWorksheets>";
				excelFile += "<x:ExcelWorksheet>";
				excelFile += "<x:Name>";
				excelFile += sheetName;
				excelFile += "</x:Name>";
				excelFile += "<x:WorksheetOptions>";
				excelFile += "<x:DisplayGridlines/>";
				excelFile += "</x:WorksheetOptions>";
				excelFile += "</x:ExcelWorksheet>";
				excelFile += "</x:ExcelWorksheets>";
				excelFile += "</x:ExcelWorkbook>";
				excelFile += "</xml>";
				excelFile += "<![endif]-->";
				excelFile += "</head>";
				excelFile += "<body>";
				excelFile += excel;
				excelFile += "</body>";
				excelFile += "</html>";

				var base64data = "," + excelFile;

				var blob = new Blob(['data:application/vnd.ms-' + defaults.type + ';fileName=' + sheetName + '.xls;' + base64data], { type: 'application/vnd.ms-' + defaults.type });
				saveAs(blob, sheetName + '.xls');


			}

			function lineChartValue(data, fieldKey, fieldValue) {
				var ret = "";
				if (!data) { return ret; }
				angular.forEach(data, function (item) {
					ret += item[fieldKey] + "," + item[fieldValue] + "|";
				});
				ret = ret.substr(0, ret.length - 1);
				return ret;
			}

			function getContentByClass(className) {
				var html = document.getElementsByClassName(className).innerHTML;
				return html.replace(/<[^>]*>/g, "");
			}

			function getContent(elem) {
				var html = elem[0].innerHTML;
				return html.replace(/<[^>]*>/g, "");
			}

			function getTableColorCompareGoal(val, goalValue) {
				var color = AppDefine.ExportColors.Default;

				if (val > goalValue.MaxGoal) {
					color = AppDefine.ExportColors.TextGreaterGoal;
				}
				else if (val < goalValue.MinGoal) {
					color = AppDefine.ExportColors.TextLessGoal;
				}
				else if (val >= goalValue.MinGoal && val <= goalValue.MaxGoal) {
					color = AppDefine.ExportColors.TextInGoal;
				}

				return color;
			}

			function getTableBGColorCompareGoal(val, goalValue) {
				var color = AppDefine.ExportColors.Default;

				if (val > goalValue.MaxGoal) {
					color = AppDefine.ExportColors.GreaterGoalCell;
				}
				else if (val < goalValue.MinGoal) {
					color = AppDefine.ExportColors.LessGoalCell;
				}
				else if (val >= goalValue.MinGoal && val <= goalValue.MaxGoal) {
					color = AppDefine.ExportColors.InGoalCell;
				}

				return color;
			}

			function getFormatNumber(unitName, unitRound, unitType) {
				var format = "0,0";
				if (unitType == 2) {
					format = "0";
				}

				if (unitRound) {
					format = format + ".";
					for (var i = 0; i < unitRound; i++) {
						format = format + "0";
					}
				}

				return format;
			}

			function formatData(val, unitName, unitRound, unitType) {
				switch (unitName) {
					case AppDefine.NumberFormat.Dollar:
						return unitName + $filter('salenumber')(val, getFormatNumber(unitName, unitRound, unitType));
					case AppDefine.NumberFormat.Percent:
						return $filter('salenumber')(val, getFormatNumber(unitName, unitRound, unitType)) + unitName;
					default:
						return $filter('salenumber')(val, getFormatNumber(unitName, unitRound, unitType));
						break;
				}
			}

			function buildTableTemplate(dtSource, options) {
				var table = {
					Name: options.tableName,
					RowDatas: [],
					Format: { ColIndex: options.ColIndex, RowIndex: options.RowIndex }
				};

				buildTableHeader(dtSource, table, options);
				options.ColIndex = 1;
				options.RowIndex = options.cellNames.length === 1 ? 2 : 3;
				buildTableBody(dtSource, table, options);

				return table;
			}

			function buildTableHeader(dtSource, table, options) {
				var startCol = 1;
				var startRow = options.RowIndex;
				var colIndex = 1;
				var mergeCellNumber = options.cellNames.length > 0 ? options.cellNames.length : 1;

				var RowData = {
					Type: AppDefine.tableExport.Header,
					ColDatas: []
				};
				var ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.LOCATION_STRING),
					Color: AppDefine.ExportColors.GridHeaderFirstCell,
					CustomerWidth: true,
					Width: 30,
					MergeCells: { Cells: 1, Rows: mergeCellNumber > 1 ? 2 : 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);
				colIndex = ColData.MergeCells.Cells + colIndex;

				angular.forEach(options.dateList, function (item) {
					ColData = {
						Value: item,
						Color: AppDefine.ExportColors.GridHeaderCell,
						CustomerWidth: options.cellNames.length === 1 ? true : false,
						Width: options.cellNames.length === 1 ? 15 : 0,
						MergeCells: { Cells: mergeCellNumber, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: startRow
					};
					RowData.ColDatas.push(ColData);
					colIndex = ColData.MergeCells.Cells + colIndex;
				});

				//Summary column
				ColData = {
					Value: cmsBase.translateSvc.getTranslate(AppDefine.Resx.SUMMARY_STRING),
					Color: AppDefine.ExportColors.GridHeaderEndCell,
					CustomerWidth: options.cellNames.length === 1 ? true : false,
					Width: options.cellNames.length === 1 ? 15 : 0,
					MergeCells: { Cells: mergeCellNumber, Rows: 1 },
					ColIndex: colIndex,
					RowIndex: startRow
				};
				RowData.ColDatas.push(ColData);

				//Region: insert row header
				table.RowDatas.push(RowData);

				//Sub header - dates column
				if (options.cellNames.length > 1) {
					colIndex = 2;
					RowData = {
						Type: AppDefine.tableExport.Header,
						ColDatas: []
					};
					angular.forEach(options.dateList, function (item) {
						angular.forEach(options.cellNames, function (cName) {
							ColData = {
								Value: cName.value,
								Color: AppDefine.ExportColors.GridSubHeaderCell,
								CustomerWidth: true,
								Width: 10,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: startRow + 1
							};
							RowData.ColDatas.push(ColData);
							colIndex = ColData.MergeCells.Cells + colIndex;
						});
					});

					//Sub header - Summary column
					angular.forEach(options.cellNames, function (cName) {
						ColData = {
							Value: cName.value,
							Color: AppDefine.ExportColors.GridHeaderEndCell,
							CustomerWidth: true,
							Width: 10,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: startRow + 1
						};
						RowData.ColDatas.push(ColData);
						colIndex = ColData.MergeCells.Cells + colIndex;
					});

					table.RowDatas.push(RowData);
				}
			}

			function buildTableBody(dtSource, table, options) {
				//Used to: Sale, Queue Time, Drive Through
				var rowData = {};
				var colData = {};
				var startCol = options.ColIndex;
				var startRow = options.RowIndex;
				var cellNames = Enumerable.From(options.cellNames).Select(function (s) { return s.key; }).ToArray();
				var rowIndex = startRow;

				angular.forEach(dtSource, function (item, index) {
					var colIndex = startCol;

					rowData = {
						Type: AppDefine.tableExport.Body,
						ColDatas: []
					};
					colData = {
						Value: item.Name,
						Color: AppDefine.ExportColors.GridRegionCell,
						CustomerWidth: false,
						Width: 0,
						MergeCells: { Cells: 1, Rows: 1 },
						ColIndex: colIndex,
						RowIndex: rowIndex
					};
					rowData.ColDatas.push(colData);
					colIndex = colData.MergeCells.Cells + colIndex;

					angular.forEach(item[options.Fields.DetailData], function (detail) {
						if (cellNames.indexOf('COUNT') !== -1) {
							colData = {
								Value: detail[options.Fields.Count],
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('DWELL') !== -1) {
							colData = {
								Value: detail[options.Fields.Dwell],
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('COUNTIN') !== -1) {
							colData = {
								Value: $filter('salenumber')(detail[options.Fields.TrafficIn], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('COUNTOUT') !== -1) {
							colData = {
								Value: $filter('salenumber')(detail[options.Fields.TrafficOut], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('POS') !== -1) {
							colData = {
								Value: $filter('salenumber')(detail[options.Fields.CountTrans], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('CONVERSION') !== -1) {
							colData = {
								Value: $filter('salenumber')(detail[options.Fields.Conversion], '0,0.00') + "%",
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
					});

					//Region: Summary Column 
					if (cellNames.indexOf('COUNT') !== -1) {
						colData = {
							Value: item[options.Fields.Count],
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}
					if (cellNames.indexOf('DWELL') !== -1) {
						colData = {
							Value: item[options.Fields.Dwell],
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}
					if (cellNames.indexOf('COUNTIN') !== -1) {
						colData = {
							Value: $filter('salenumber')(item[options.Fields.TrafficIn], '0,0'),
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}
					if (cellNames.indexOf('COUNTOUT') !== -1) {
						colData = {
							Value: $filter('salenumber')(item[options.Fields.TrafficOut], '0,0'),
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}
					if (cellNames.indexOf('POS') !== -1) {
						colData = {
							Value: $filter('salenumber')(item[options.Fields.CountTrans], '0,0'),
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}
					if (cellNames.indexOf('CONVERSION') !== -1) {
						colData = {
							Value: $filter('salenumber')(item[options.Fields.Conversion], '0,0.00') + "%",
							Color: AppDefine.ExportColors.GridNormalCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;
					}

					//Insert Row region
					table.RowDatas.push(rowData);

					angular.forEach(item[options.Fields.Regions], function (site) {
						colIndex = 1;
						rowIndex++;
						rowData = {
							Type: AppDefine.tableExport.Body,
							ColDatas: []
						};
						colData = {
							Value: site.Name,
							Color: AppDefine.ExportColors.GridSiteCell,
							CustomerWidth: false,
							Width: 0,
							MergeCells: { Cells: 1, Rows: 1 },
							ColIndex: colIndex,
							RowIndex: rowIndex
						};
						rowData.ColDatas.push(colData);
						colIndex = colData.MergeCells.Cells + colIndex;

						angular.forEach(site[options.Fields.DetailData], function (siteDetail) {
							if (cellNames.indexOf('COUNT') !== -1) {
								colData = {
									Value: siteDetail[options.Fields.Count],
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
							if (cellNames.indexOf('DWELL') !== -1) {
								colData = {
									Value: siteDetail[options.Fields.Dwell],
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
							if (cellNames.indexOf('COUNTIN') !== -1) {
								colData = {
									Value: $filter('salenumber')(siteDetail[options.Fields.TrafficIn], '0,0'),
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
							if (cellNames.indexOf('COUNTOUT') !== -1) {
								colData = {
									Value: $filter('salenumber')(siteDetail[options.Fields.TrafficOut], '0,0'),
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
							if (cellNames.indexOf('POS') !== -1) {
								colData = {
									Value: $filter('salenumber')(siteDetail[options.Fields.CountTrans], '0,0'),
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
							if (cellNames.indexOf('CONVERSION') !== -1) {
								colData = {
									Value: $filter('salenumber')(siteDetail[options.Fields.Conversion], '0,0.00') + "%",
									Color: AppDefine.ExportColors.GridNormalCell,
									CustomerWidth: false,
									Width: 0,
									MergeCells: { Cells: 1, Rows: 1 },
									ColIndex: colIndex,
									RowIndex: rowIndex
								};
								rowData.ColDatas.push(colData);
								colIndex = colData.MergeCells.Cells + colIndex;
							}
						});

						//Site: Summary column
						if (cellNames.indexOf('COUNT') !== -1) {
							colData = {
								Value: site[options.Fields.Count],
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('DWELL') !== -1) {
							colData = {
								Value: site[options.Fields.Dwell],
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('COUNTIN') !== -1) {
							colData = {
								Value: $filter('salenumber')(site[options.Fields.TrafficIn], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('COUNTOUT') !== -1) {
							colData = {
								Value: $filter('salenumber')(site[options.Fields.TrafficOut], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('POS') !== -1) {
							colData = {
								Value: $filter('salenumber')(site[options.Fields.CountTrans], '0,0'),
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}
						if (cellNames.indexOf('CONVERSION') !== -1) {
							colData = {
								Value: $filter('salenumber')(site[options.Fields.Conversion], '0,0.00') + "%",
								Color: AppDefine.ExportColors.GridNormalCell,
								CustomerWidth: false,
								Width: 0,
								MergeCells: { Cells: 1, Rows: 1 },
								ColIndex: colIndex,
								RowIndex: rowIndex
							};
							rowData.ColDatas.push(colData);
							colIndex = colData.MergeCells.Cells + colIndex;
						}

						//Insert Row site
						table.RowDatas.push(rowData);
					});

					rowIndex++;
				});
			}

			function getColorChart(val, goal) {
				var color = AppDefine.ChartExportColor.Default;
				if (!goal) { return color; }

				if (val < goal.Min) {
					color = AppDefine.ChartExportColor.Red;
				}
				else if (val >= goal.Min && val <= goal.Max) {
					color = AppDefine.ChartExportColor.Orange;
				}
				else if (val > goal.Max) {
					color = AppDefine.ChartExportColor.Green;
				}
				return color;
			}

			return service;
		}
	});
})();