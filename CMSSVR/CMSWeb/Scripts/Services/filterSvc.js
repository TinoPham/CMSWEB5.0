(function () {
	define(['cms'], function (cms) {
		cms.register.service('filterSvc', filterSvc);
		filterSvc.$inject = [];

		function filterSvc() {

			var filterNumber = [
                { id: 0, key: '', name: '-Select filter-', symbol: '-', T_Resource: 'SELECT_FILTER' },
                { id: 1, key: 'eq', name: 'Equal', className: 'icon-equal', symbol: '=', T_Resource: 'EQUAL' },
                { id: 2, key: 'ne', name: 'Not equal', className: 'icon-not-equal', symbol: '<>', T_Resource: 'NOT_EQUAL' },
                { id: 3, key: 'gt', name: 'Greater than', className: 'icon-greater-than', symbol: '>', T_Resource: 'GREATER_THAN' },
                { id: 4, key: 'ge', name: 'Greater than or equal', className: 'icon-greater-than-or-equal', symbol: '>=', T_Resource: 'GREATER_THAN_OR_EQUAL' },
                { id: 5, key: 'lt', name: 'Less than', className: ' icon-less-than', symbol: '<', T_Resource: 'LESS_THAN' },
                { id: 6, key: 'le', name: 'Less than or equal', className: 'icon-less-than-or-equal', symbol: '<=', T_Resource: 'LESS_THAN_OR_EQUAL' }
			];

			var filterDatetime = [
                { id: 0, key: '', name: '-Select filter-', symbol: '-', T_Resource: 'SELECT_FILTER' },
                //{ id: 1, key: 'eq', name: 'Equal', className: 'icon-equal', symbol: '=', T_Resource: 'EQUAL' },
                //{ id: 2, key: 'ne', name: 'Not equal', className: 'icon-not-equal', symbol: '<>', T_Resource: 'NOT_EQUAL' },
                { id: 3, key: 'gt', name: 'Greater than', className: 'icon-greater-than', symbol: '>', T_Resource: 'GREATER_THAN' },
                { id: 4, key: 'ge', name: 'Greater than or equal', className: 'icon-greater-than-or-equal', symbol: '>=', T_Resource: 'GREATER_THAN_OR_EQUAL' },
                { id: 5, key: 'lt', name: 'Less than', className: ' icon-less-than', symbol: '<', T_Resource: 'LESS_THAN' },
                { id: 6, key: 'le', name: 'Less than or equal', className: 'icon-less-than-or-equal', symbol: '<=', T_Resource: 'LESS_THAN_OR_EQUAL' }
			];

			var filterString = [
                { id: 0, key: '', name: '-Select filter-', symbol: '-', T_Resource: 'SELECT_FILTER' },
                { id: 1, key: 'eq', name: 'Equal', className: 'icon-equal', symbol: '=', T_Resource: 'EQUAL' },
                { id: 2, key: 'ne', name: 'Not equal', className: 'icon-not-equal', symbol: '<>', T_Resource: 'NOT_EQUAL' },
                { id: 3, key: 'startswith', name: 'Starts With', className: 'icon-to-start-3', symbol: '<-', T_Resource: 'STARTS_WITH' },
                { id: 4, key: 'endswith', name: 'Ends With', className: 'icon-to-end-3', symbol: '->', T_Resource: 'ENDS_WITH' },
                { id: 5, key: 'contains', name: 'Contains', className: 'icon-search-3', symbol: '{}', T_Resource: 'CONTAINS_STRING' }
			];

			var definefield = [
                { id: 1, key: '', fieldName: 'TranId', preFix: '', subFix: '', checked: true, isShow: false, fieldType: 'number' },
                { id: 2, key: '', fieldName: 'PacId', preFix: '', subFix: '', checked: true, isShow: false, fieldType: 'number' },
                { id: 3, key: '', fieldName: 'TranNo', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'number' },
                { id: 4, key: '', fieldName: 'StoreId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 5, key: 'StoreId', fieldName: 'StoreName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 6, key: '', fieldName: 'CamId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 7, key: 'CamId', fieldName: 'CamName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 8, key: '', fieldName: 'CardId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 9, key: 'CardId', fieldName: 'CardName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 10, key: '', fieldName: 'CheckId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 11, key: 'CheckId', fieldName: 'CheckName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 12, key: '', fieldName: 'EmployeeId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 13, key: 'EmployeeId', fieldName: 'EmployeeName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 14, key: '', fieldName: 'RegisterId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 15, key: 'RegisterId', fieldName: 'RegisterName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 16, key: '', fieldName: 'ShiftId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 17, key: 'ShiftId', fieldName: 'ShiftName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 18, key: '', fieldName: 'TerminalId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'childid' },
                { id: 19, key: 'TerminalId', fieldName: 'TerminalName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
                { id: 20, key: '', fieldName: 'Total', preFix: '$', subFix: '', checked: true, isShow: true, fieldType: 'money' },
                { id: 21, key: '', fieldName: 'SubTotal', preFix: '$', subFix: '', checked: false, isShow: true, fieldType: 'money' },
                { id: 22, key: '', fieldName: 'ChangeAmount', preFix: '$', subFix: '', checked: false, isShow: true, fieldType: 'money' },
                { id: 23, key: '', fieldName: 'DvrDate', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'datetime' },
                { id: 24, key: '', fieldName: 'TranDate', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'datetime' },
                { id: 25, key: '', fieldName: 'ExceptionTypes', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'list', Expand: { id: 1, key: 'Id', fieldName: 'Name', checked: true, isShow: true, fieldType: 'number' } },
                { id: 26, key: '', fieldName: 'Taxs', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'list', Expand: { id: 1, key: 'Id', fieldName: 'Name', checked: true, isShow: true, fieldType: 'number' } },
                { id: 27, key: '', fieldName: 'Notes', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'list', Expand: { id: 1, key: 'Id', fieldName: 'Note', checked: true, isShow: true, fieldType: 'string' } },
                { id: 28, key: '', fieldName: 'Payments', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'list', Expand: { id: 1, key: 'Id', fieldName: 'Name', checked: true, isShow: true, fieldType: 'number' } },
				{ id: 29, key: '', fieldName: 'Year', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 30, key: '', fieldName: 'Quarter', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 31, key: '', fieldName: 'Month', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 32, key: '', fieldName: 'Week', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 33, key: '', fieldName: 'Day', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 34, key: '', fieldName: 'Hour', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
                { id: 35, key: '', fieldName: 'DescId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'list', isDetail: true },
                { id: 36, key: '', fieldName: 'ItemCodeId', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'list', isDetail: true },
                { id: 37, key: 'DescId', fieldName: 'DescName', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'string', isDetail: true },
                { id: 38, key: 'ItemCodeId', fieldName: 'ItemCode', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'string', isDetail: true },
                { id: 39, key: '', fieldName: 'Qty', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number', isDetail: true },
                { id: 40, key: '', fieldName: 'Amount', preFix: '$', subFix: '', checked: false, isShow: false, fieldType: 'money', isDetail: true },
                { id: 41, key: '', fieldName: 'PaymentAmount', preFix: '$', subFix: '', checked: false, isShow: false, fieldType: 'money', isDetail: true },
                { id: 42, key: '', fieldName: 'TaxAmount', preFix: '$', subFix: '', checked: false, isShow: false, fieldType: 'money', isDetail: true }
			];

			var FieldConsts = [
				{ id: 1, key: '', fieldName: 'TransID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 2, key: '', fieldName: 'T_0TransNB', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'number' },
				{ id: 3, key: '', fieldName: 'T_PACID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 4, key: '', fieldName: 'T_StoreID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 5, key: 'T_StoreID', fieldName: 'T_StoreName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Stores' },
				{ id: 6, key: '', fieldName: 'T_CheckID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 7, key: 'T_CheckID', fieldName: 'T_CheckName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'CheckIDs' },
				{ id: 8, key: '', fieldName: 'T_OperatorID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 9, key: 'T_OperatorID', fieldName: 'T_OperatorName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Operators' },
				{ id: 10, key: '', fieldName: 'T_RegisterID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 11, key: 'T_RegisterID', fieldName: 'T_RegisterName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Registers' },
				{ id: 12, key: '', fieldName: 'T_TerminalID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				{ id: 13, key: 'T_TerminalID', fieldName: 'T_TerminalName', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Terminals' },
				{ id: 14, key: '', fieldName: 'T_6TotalAmount', preFix: '$', subFix: '', checked: true, isShow: true, fieldType: 'number' },
				{ id: 15, key: '', fieldName: 'T_8ChangeAmount', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'number' },
				{ id: 16, key: '', fieldName: 'DVRDate', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'datetime' },
				{ id: 17, key: '', fieldName: 'TransDate', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'datetime' },
				//{ id: 18, key: '', fieldName: 'TypeID', preFix: '', subFix: '', checked: false, isShow: false, fieldType: 'number' },
				//{ id: 19, key: 'TypeID', fieldName: 'TypeName', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'string' },
				{ id: 20, key: '', fieldName: 'TaxID', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Taxes' },
				{ id: 21, key: '', fieldName: 'TaxAmount', preFix: '$', subFix: '', checked: false, isShow: true, fieldType: 'number' },
				{ id: 22, key: 'PaymentID', fieldName: 'PaymentName', preFix: '', subFix: '', checked: true, isShow: true, fieldType: 'string', CacheName: 'Payments' },
				{ id: 23, key: '', fieldName: 'PaymentAmount', preFix: '$', subFix: '', checked: false, isShow: true, fieldType: 'number' },
				{ id: 24, key: '', fieldName: 'Description', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string', CacheName: 'Descriptions' },
				//{ id: 25, key: '', fieldName: 'ExtraNumber', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'number' },
				//{ id: 26, key: 'ExtraNumber', fieldName: 'ExtraString', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' },
				//{ id: 27, key: '', fieldName: 'Tracking', preFix: '', subFix: '', checked: false, isShow: true, fieldType: 'string' }
			];

			var query = { "inlinecount": "allpages" };


			var cmsMetadata = function () {
                query = { "inlinecount": "allpages" };
				this.SelectSettings =
                {
                	Select: null,
                	reset: function () {
                		if (query['$select']) {
                			delete query['$select'];
                		}
                		this.Select = null;
                	},
                	isSet: function () {
                		return this.Select !== null;
                	},
                	buildQuery: function () {
                		if (!this.Select) return;
                		if (this.Select instanceof Array) {
                			query['$select'] = query['$select'] && query['$select'].length > 0 ? query['$select'] + ',' + this.Select.join(',') : this.Select.join(',');
                		} else {
                			query['$select'] = query['$select'] && query['$select'].length > 0 ? query['$select'] + ',' + this.Select : this.Select;
                		}
                	}
                },
                this.ApplySettings = {
                	Apply: null,
                	isSet: function () {
                		return this.Apply !== null;
                	},
                	reset: function () {
                		if (query['$apply']) {
                			delete query['$apply'];
                		}
                		this.Apply = null;
                	},
                	buildQuery: function () {
                		query['$apply'] = this.Apply.buildQuery();
                	}
                },
                this.FilterSetting = {
                	Filters: [],
                	isSet: function () {
                		return this.Filters.length > 0;
                	},
                	buildQuery: function () {
                		var allFilters, i, filter;

                		allFilters = [];
                		filter = '';

                		for (i = 0; i < this.Filters.length; i++) {
                			allFilters.push(this.Filters[i]);
                		}

                		for (i = 0; i < allFilters.length; i++) {
                			filter += allFilters[i].buildQuery(i);
                		}

                		query['$filter'] = filter;
                	},
                	reset: function () {
                		if (query['$filter']) {
                			delete query['$filter'];
                		}
                		this.Filters = [];
                	},
                	fullReset: function () {
                		if (query['$filter']) {
                			delete query['$filter'];
                		}
                		this.Filters = [];
                		this.CapturedFilter = [];
                	}
                },
                this.OrderBySettings = {
                	Property: null,
                	Order: null,
                	buildQuery: function () {
                		query['$orderby'] = this.Property;
                		if (this.Order !== null) {
                			query['$orderby'] += ' ' + this.Order;
                		}
                	},
                	reset: function () {
                		this.Property = null;
                		this.Order = null;
                	},
                	isSet: function () {
                		return this.Property !== null;
                	}
                },
                this.TopSettings = {
                	Top: null,
                	DefaultTop: null,
                	buildQuery: function () {
                		query['$top'] = (this.Top !== null ? this.Top : this.DefaultTop);
                	},
                	reset: function () {
                		this.Top = null;
                	},
                	isSet: function () {
                		return this.Top !== null || this.DefaultTop !== null;
                	}
                },
                this.SkipSettings = {
                	Skip: null,
                	DefaultSkip: null,
                	buildQuery: function () {
                		query['$skip'] = (this.Skip !== null ? this.Skip : this.DefaultSkip);
                	},
                	reset: function () {
                		this.Skip = null;
                	},
                	isSet: function () {
                		return this.Skip !== null || this.DefaultSkip !== null;
                	}
                },
                this.ExpandSettings = {
                	Expand: null,
                	DefaultExpand: null,
                	buildQuery: function () {
                		query['$expand'] = (this.Expand || this.DefaultExpand);
                	},
                	reset: function () {
                		this.Expand = null;
                	},
                	isSet: function () {
                		return this.Expand !== null || this.DefaultExpand !== null;
                	}
                }
			};

			cmsMetadata.prototype = {
				select: function (selects) {
					this.SelectSettings.Select = selects;
					return this;
				},
				apply: function (apply) {
					this.ApplySettings.Apply = apply;
					return this;
				},
				resetSelect: function () {
					this.SelectSettings.reset();
					return this;
				},
				resetApply: function () {
					this.ApplySettings.reset();
					return this;
				},
				resetFilter: function () {
					this.FilterSetting.fullReset();
					return this;
				},
				filter: function (filterClause) {
					this.FilterSetting.Filters.push(filterClause);
					return this;
				},
				andFilter: function () {
					var andf = new cmsMetadata.FilterClause(null, 'and');
					this.FilterSetting.Filters.push(andf);
					return this;
				},
				orFilter: function () {
					var andf = new cmsMetadata.FilterClause(null, 'or');
					this.FilterSetting.Filters.push(andf);
					return this;
				},
				orderBy: function (property) {
					this.OrderBySettings.Property = property;
					return this;
				},
				resetOrderBy: function () {
					this.OrderBySettings.reset();
					return this;
				},
				desc: function () {
					this.OrderBySettings.Order = 'desc';
					return this;
				},
				asc: function () {
					this.OrderBySettings.Order = 'asc';
					return this;
				},
				top: function (top) {
					this.TopSettings.Top = top;
					return this;
				},
				resetTop: function () {
					this.TopSettings.reset();
					return this;
				},
				expand: function (expands) {
					this.ExpandSettings.Expand = expands;
					return this;
				},
				resetExpand: function () {
					this.ExpandSettings.reset();
				},
				skip: function (skip) {
					this.SkipSettings.Skip = skip;
					return this;
				},
				resetSkip: function () {
					this.SkipSettings.reset();
					return this;
				},
				getQuery: function () {

					if (this.SelectSettings.isSet()) {
						this.SelectSettings.buildQuery();
					}
					if (this.ApplySettings.isSet()) {
						this.ApplySettings.buildQuery();
					}
					if (this.FilterSetting.isSet()) {
						this.FilterSetting.buildQuery();
					}

					if (this.OrderBySettings.isSet()) {
						this.OrderBySettings.buildQuery();
					}

					if (this.TopSettings.isSet()) {
						this.TopSettings.buildQuery();
					}

					if (this.SkipSettings.isSet()) {
						this.SkipSettings.buildQuery();
					}

					if (this.ExpandSettings.isSet()) {
						this.ExpandSettings.buildQuery();
					}

					return query;
				},
				toString: function () {

					if (this.SelectSettings.isSet()) {
						this.SelectSettings.buildQuery();
					}
					if (this.ApplySettings.isSet()) {
						this.ApplySettings.buildQuery();
					}
					if (this.FilterSetting.isSet()) {
						this.FilterSetting.buildQuery();
					}

					if (this.OrderBySettings.isSet()) {
						this.OrderBySettings.buildQuery();
					}

					if (this.TopSettings.isSet()) {
						this.TopSettings.buildQuery();
					}

					if (this.SkipSettings.isSet()) {
						this.SkipSettings.buildQuery();
					}

					if (this.ExpandSettings.isSet()) {
						this.ExpandSettings.buildQuery();
					}
					var result = '';
					Object.getOwnPropertyNames(query).forEach(function (val, idx, array) {
						result = result === '' ? val + "=" + query[val] : result + "&" + val + "=" + query[val];
					});
					return result;
				}
			}

			cmsMetadata.PrecedenceGroup = function (filterClause, andor) {
				this.clauses = [];
				this.groupClauses = [];
				this.AndOR = null;
				if (filterClause !== undefined) {
					this.clauses.push(filterClause);
				}

				if (andor) {
					this.AndOR = andor;
				}

				return this;
			}

			cmsMetadata.PrecedenceGroup.prototype = {
				clauses: [],

				groupClauses: [],
				isEmpty: function () {
					return this.clauses.length === 0;
				},
				andFilter: function (filterClause) {
					var andf = new cmsMetadata.FilterClause(null, 'and');
					this.clauses.push(andf);
					if (filterClause) this.clauses.push(filterClause);
					return this;
				},
				orFilter: function (filterClause) {
					var andf = new cmsMetadata.FilterClause(null, 'or');
					this.clauses.push(andf);
					if (filterClause) this.clauses.push(filterClause);
					return this;
				},
				andGroupFilter: function (filtergroup) {
					var andf = new cmsMetadata.PrecedenceGroup(null, 'and');
					this.groupClauses.push(andf);
					this.groupClauses.push(filtergroup);
					return this;
				},
				orGroupFilter: function (filtergroup) {
					var andf = new cmsMetadata.PrecedenceGroup(null, 'or');
					this.groupClauses.push(andf);
					this.groupClauses.push(filtergroup);
					return this;
				},
				groupFilter: function (filtergroup) {
					this.groupClauses.push(filtergroup);
					return this;
				},
				filter: function (filterClause) {
					if (filterClause) this.clauses.push(filterClause);
					return this;
				},
				buildQuery: function () {
					var filter = "", i;
					if (this.clauses.length > 0) {
						var queryResult = '';
						for (i = 0; i < this.clauses.length; i++) {
							if (this.clauses[i]) {
								queryResult += this.clauses[i].buildQuery(i);
							}
						}
						if (queryResult.length > 1) filter = '(' + queryResult + ')';
					}

					if (this.groupClauses.length > 0) {
						for (i = 0; i < this.groupClauses.length; i++) {
							if (this.groupClauses[i].AndOR) {
								filter += " " + this.groupClauses[i].AndOR + " ";
							} else {
								filter += '(';
								filter += this.groupClauses[i].buildQuery(i);
								filter += ')';
							}
						}
					}
					return filter;
				}
			}

			cmsMetadata.ApplyFilter = function (filterClause) {
				this.clauses = [];

				if (filterClause !== undefined) {
					this.clauses.push(filterClause);
				}

				return this;
			}

			cmsMetadata.ApplyFilter.prototype = {
				clauses: [],
				groups: null,
				groupClause: null,
				aggregates: [],
				reset: function () {
					this.clauses = [];
					this.groups = null;
					this.groupClause = null;
					this.aggregates = [];
				},
				applyFilter: function (filterGroupClause) {
					this.groupClause = filterGroupClause;
				},
				applyGroup: function (fieldGroup) {
					this.groups = fieldGroup;
				},
				AverageAggregate: function (fieldName, asName) {
					var queryStr = fieldName + " with average as " + asName;
					this.aggregates.push(queryStr);
					return this;
				},
				TotalAggregate: function (fieldName, asName) {
					var queryStr = fieldName + " with sum as " + asName;
					this.aggregates.push(queryStr);
					return this;
				},
				CountAggregate: function (fieldCount, asName) {
					var queryStr = fieldCount + " with countdistinct as " + asName;
					this.aggregates.push(queryStr);
					return this;
				},
				ManualAggregate: function (queryStr) {
					this.aggregates.push(queryStr);
					return this;
				},
				buildQuery: function () {
					var filter = '', i;

					if (this.groupClause) {
						filter += "filter(" + this.groupClause.buildQuery();
					}

					if (this.clauses.length > 0) {
						if (this.groupClause) {
							filter = filter + 'and (';
						} else {
							filter = filter + '(';
						}

						for (i = 0; i < this.clauses.length; i++) {
							filter += this.clauses[i].buildQuery(i);
						}
						filter += ')';
					}
					filter += ')';
					if (this.groups) {
						if (this.groupClause) {
							filter += "/groupby(";
						} else {
							filter += "groupby(";
						}

						filter += "(" + this.groups + ")";
						if (this.aggregates.length > 0) {
							filter += ",aggregate(" + this.aggregates.join(',') + ")";
						}
						filter += ")";
					} else {
						if (this.aggregates.length > 0) {
							if (this.groupClause) {
								filter += "/aggregate";
							} else {
								filter += "aggregate";
							}
							filter += "(" + this.aggregates.join(',') + ")";
						}
					}

					return filter;
				}
			}

			cmsMetadata.ExpandFilter = function (parentPropery, filterClause) {
				this.clauses = [];
				this.ParentPropery = null;
				this.ChildFilter = null;
				if (filterClause !== undefined) {
					this.clauses.push(filterClause);
				}

				if (parentPropery !== undefined) {
					this.ParentPropery = parentPropery;
				}
				return this;
			}

			cmsMetadata.ExpandFilter.prototype = {
				clauses: [],
				andFilter: function (filterClause) {
					var andf = new cmsMetadata.FilterClause(null, 'and');
					this.clauses.push(andf);
					this.clauses.push(filterClause);
					return this;
				},
				orFilter: function (filterClause) {
					var andf = new cmsMetadata.FilterClause(null, 'or');
					this.clauses.push(andf);
					this.clauses.push(filterClause);
					return this;
				},
				allFilter: function () {
					this.ChildFilter = "all";
					return this;
				},
				anyFilter: function () {
					this.ChildFilter = "any";
					return this;
				},
				buildQuery: function () {
					var filter, i;
					filter = this.ParentPropery + "/";
					filter += this.ChildFilter + "(x:";
					for (i = 0; i < this.clauses.length; i++) {
						var filterC = this.clauses[i];
						if (!filterC.logicalOperator) {
							filter += "x/";
						}

						filter += filterC.buildQuery(i);
					}
					filter += ")";
					return filter;
				}

			}

			cmsMetadata.FilterClause = function (property, logicalOperator) {
				this.Property = property ? property : "";
				if (logicalOperator !== undefined && logicalOperator !== null) {
					this.logicalOperator = logicalOperator;
				}
				this.Components = [];
				return this;
			}

			cmsMetadata.FilterClause.prototype = {
				Property: null,
				Value: null,
				PropertyIncluded: false,
				Components: [],
				oper: function (value, operator) {
					return addLogicOperator(this, operator, value);
				},
				eq: function (value) {
					return addLogicOperator(this, 'eq', value);
				},
				ne: function (value) {
					return addLogicOperator(this, 'ne', value);
				},
				gt: function (value) {
					return addLogicOperator(this, 'gt', value);
				},
				ge: function (value) {
					return addLogicOperator(this, 'ge', value);
				},
				lt: function (value) {
					return addLogicOperator(this, 'lt', value);
				},
				le: function (value) {
					return addLogicOperator(this, 'le', value);
				},
				contains: function (value) {
					return contains(this, value);
					//this.PropertyIncluded = true;
					//this.Components.push('contains(' + this.Property + ',\'' + value + '\')');
					//return this;
				},
				endswith: function (value) {
					return endswith(this, value);
					//this.PropertyIncluded = true;
					//this.Components.push('endswith(' + this.Property + ',\'' + value + '\')');
					//return this;
				},
				startswith: function (value) {
					return startswith(this, value);
					//this.PropertyIncluded = true;
					//this.Components.push('startswith(' + this.Property + ',\'' + value + '\')');
					//return this;
				},
				andFilter: function () {
					this.Components.push(' and ');
					return this;
				},
				orFilter: function () {
					this.Components.push(' or ');
					return this;
				},
				and: function (clause) {
					this.Components.push(' and ');
					this.Components.push(clause.buildQuery());
					return this;
				},
				or: function (clause) {
					this.Components.push(' or ');
					this.Components.push(clause.buildQuery());
					return this;
				},
				operString: function (value, operator) {
					var result = null;
					switch (operator) {
						case 'contains':
							result = contains(this, value);
							break;
						case 'endswith':
							result = endswith(this, value);
							break;
						case 'startswith':
							result = startswith(this, value);
							break;
						default:
							result = addLogicOperator(this, operator, value);
							break;
					}
					return result;
				},
				buildQuery: function () {

					var strComps, i, filterStr;
					strComps = [];

					if (this.logicalOperator) {
						return ' ' + this.logicalOperator + ' ';
					}

					if (!this.PropertyIncluded) {
						strComps.push(this.Property);
					}

					for (i = 0; i < this.Components.length; i++) {
						strComps.push(this.Components[i]);
					}
					filterStr = strComps.join(' ');
					return filterStr;
				}
			}

			function contains(filter, value) {
				filter.PropertyIncluded = true;
				filter.Components.push('contains(' + filter.Property + ',' + value + ')');
				return filter;
			}

			function endswith(filter, value) {
				filter.PropertyIncluded = true;
				filter.Components.push('endswith(' + filter.Property + ',' + value + ')');
				return filter;
			}

			function startswith(filter, value) {
				filter.PropertyIncluded = true;
				filter.Components.push('startswith(' + filter.Property + ',' + value + ')');
				return filter;
			}

			function addLogicOperator(clause, operator, value) {
				clause.Value = value;
				clause.Components.push(operator + ' ' + value);
				return clause;
			}

			function getfilterNumber() {
				return filterNumber;
			}

			function getfilterDatetime() {
				return filterDatetime;
			}

			function getTransactionColumn() {
				return definefield;
			}

			function getTransQSColumn() {
				return FieldConsts;
			}

			function getfilterString() {
				return filterString;
			}

			function parseParam(obj) {

				var str = "";

				if (!obj) {
					return str;
				}

				for (var key in obj) {
					if (str != "") {
						str += "&";
					}
					str += key + "=" + obj[key];
				}
				return str;
			}

			function reset() {
				query = {};
			}

			function getfilterDef(type) {
				switch (type) {
					case 'string':
						return getfilterString();
						break;
					case 'number':
					case 'money':
						return getfilterNumber();
						break;
					case 'datetime':
						return getfilterDatetime();
						break;
					default:
						return getfilterString();
						break;
				}
			}


			function getForDynamicQuery(operator) {
				var result = '==';
				switch (operator) {
					case 'eq':
						result = '==';
						break;
					case 'ne':
						result = '!=';
						break;
					case 'gt':
						result = '>';
						break;
					case 'ge':
						result = '>=';
						break;
					case 'lt':
						result = '<';
						break;
					case 'le':
						result = '<=';
						break;
				}
				return result;
			}

			var service = {
				Metadata: cmsMetadata,
				reset: reset,
				getfilterDef: getfilterDef,
				getfilterNumber: getfilterNumber,
				getfilterDatetime: getfilterDatetime,
				getfilterString: getfilterString,
				parseParam: parseParam,
				getTransactionColumn: getTransactionColumn,
				getTransQSColumn: getTransQSColumn,
				getForDynamicQuery: getForDynamicQuery
			};

			return service;
		}
	});

})();
