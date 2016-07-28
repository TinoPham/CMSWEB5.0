(function () {
	'use strict';

	define(['cms'], function (cms) {
		cms.register.controller('dynamicFilterCtrl', dynamicFilterCtrl);

		dynamicFilterCtrl.$inject = ['$scope', '$filter', '$modal', '$modalInstance', 'cmsBase', 'items', 'AppDefine', 'rebarDataSvc', 'filterSvc'];

		function dynamicFilterCtrl($scope, $filter, $modal, $modalInstance, cmsBase, items, AppDefine, rebarDataSvc, filterSvc) {
			$scope.comparationOperators = [];
			$scope.compOperatorSelected = {};
			$scope.searchboxType = "text";
			$scope.filterModel = {
				name: '',
				type: '',
				value: '',
				foreignField: '',
				operators: {}
			};
			$scope.CurrentPage = 1;
			$scope.searchData = [];
			$scope.dateOptions = {
				format: 'MM/DD/YYYY HH:mm'
			};

			active();

			function active() {
				if (!items) { return; }

				if (items.dataFilter) {
					$scope.filterModel = items.dataFilter;
				}
				else {
					if (items.colInfo) {
						$scope.filterModel.name = items.colInfo.fieldName;
						$scope.filterModel.type = items.colInfo.fieldType;
						$scope.filterModel.foreignField = items.colInfo.key;
					}
				}

				buildComparisons(items.colInfo.fieldType);
			}

			$scope.save = function () {
				var IdSelecteds = [];
				switch ($scope.filterModel.name) {
					case "TypeName":
						IdSelecteds = Enumerable.From($scope.dataList).Where(function (x) { return x.Checked === true; })
							.Select(function (x) { return x.Id; }).ToArray();
						$scope.filterModel.value = IdSelecteds.toString();
						break;
					case "PaymentName":
						IdSelecteds = Enumerable.From($scope.dataList).Where(function (x) { return x.Checked === true; })
							.Select(function (x) { return x.ID; }).ToArray();
						$scope.filterModel.value = IdSelecteds.toString();
						break;
				}

				$modalInstance.close($scope.filterModel);
			};

			$scope.clearSearch = function () {
				//$scope.filterModel.name = items.colInfo.fieldName;
				//$scope.filterModel.type = items.colInfo.fieldType;
				//$scope.filterModel.foreignField = items.colInfo.key;
				//$scope.filterModel.value = "";
				$scope.filterModel = null;
			};

			$scope.cancel = function () {
				$modalInstance.close();
			};

			$scope.selectOperator = function (item) {
				$scope.compOperatorSelected = item;
				$scope.filterModel.operators = $scope.compOperatorSelected;
			};

			$scope.scrollOption = {
				onScroll: function (y, x) {
					if (Math.ceil(y.scroll) >= Math.ceil(y.maxScroll) && $scope.filterText && $scope.filterText.length === 0) {
						getDataList($scope.CurrentPage);
					}
				}
			};

			function buildComparisons(dataType) {
				$scope.comparationOperators = [];
				var operators = [];
				switch (dataType) {
					case "string":
						operators = filterSvc.getfilterString();
						$scope.searchboxType = "text";
						getDataList($scope.CurrentPage);
						break;
					case "number":
						operators = filterSvc.getfilterNumber();
						$scope.searchboxType = "number";
						break;
					case "datetime":
						operators = filterSvc.getfilterDatetime();
						$scope.searchboxType = "text";
						break;
				}
				$scope.comparationOperators = Enumerable.From(operators).Where(function (w) { return w.id !== 0 }).ToArray();
				if (items.dataFilter && items.dataFilter.operators) {
					$scope.compOperatorSelected = Enumerable.From(operators).Where(function (w) { return w.id === items.dataFilter.operators.id }).FirstOrDefault();
				}
				else {
					$scope.compOperatorSelected = $scope.comparationOperators[0]; //default selected 'Equal' operator
				}
				$scope.filterModel.operators = $scope.compOperatorSelected;
			}

			function getDataList(pageNumber) {
				switch ($scope.filterModel.name) {
					case "TypeName":
						rebarDataSvc.getTransactionTypes(function (response) {
							$scope.dataList = response;
							if ($scope.filterModel.value) {
								var listId = $scope.filterModel.value.split(',');
								if (listId.length > 0) {
									$scope.dataList.forEach(function (item) {
										if (listId.indexOf(item.Id.toString()) !== -1) {
											item.Checked = true;
										}
										else {
											item.Checked = false;
										}
									});
								}
							}
						}, function (error) { });
						break;
					case "EmpName":
						if ($scope.TotalPage < pageNumber) { return; }
						var params = {
							PageSize: 100,
							PageNumber: pageNumber
						};
						getEmployeeList(params);
						break;
					case "RegisterName":
						if ($scope.TotalPage < pageNumber) { return; }
						var params = {
							PageSize: 100,
							PageNumber: pageNumber
						};
						getRegisterList(params);
						break;
					case "PaymentName":
						if ($scope.TotalPage < pageNumber) { return; }
						var params = {
							PageSize: 100,
							PageNumber: pageNumber
						};
						getPaymentList(params);
						break;
					case "DescriptionName":
						if ($scope.TotalPage < pageNumber) { return; }
						var params = {
							PageSize: 100,
							PageNumber: pageNumber
						};
						getDescriptionList(params);
						break;
					case "TerminalName":
						rebarDataSvc.getTerminal(function (response) {
							$scope.dataList = response;
							if ($scope.filterModel.value) {
								var listId = $scope.filterModel.value.split(',');
								if (listId.length > 0) {
									$scope.dataList.forEach(function (item) {
										if (listId.indexOf(item.Id.toString()) !== -1) {
											item.Checked = true;
										}
										else {
											item.Checked = false;
										}
									});
								}
							}
						}, function (error) { });
						break;
					case "StoreName":
						rebarDataSvc.getStore(function (response) {
							$scope.dataList = response;
							if ($scope.filterModel.value) {
								var listId = $scope.filterModel.value.split(',');
								if (listId.length > 0) {
									$scope.dataList.forEach(function (item) {
										if (listId.indexOf(item.Id.toString()) !== -1) {
											item.Checked = true;
										}
										else {
											item.Checked = false;
										}
									});
								}
							}
						}, function (error) { });
						break;
					case "CheckName":
						rebarDataSvc.getCheckID(function (response) {
							$scope.dataList = response;
							if ($scope.filterModel.value) {
								var listId = $scope.filterModel.value.split(',');
								if (listId.length > 0) {
									$scope.dataList.forEach(function (item) {
										if (listId.indexOf(item.Id.toString()) !== -1) {
											item.Checked = true;
										}
										else {
											item.Checked = false;
										}
									});
								}
							}
						}, function (error) { });
						break;
					case "ExtraString":
						break;
					
				}
			}

			function getPaymentList(params) {
				rebarDataSvc.GetPaymentList(params, function (response) {
					$scope.searchData = $scope.searchData.concat(response.Data);
					$scope.CurrentPage = response.CurrentPage;
					$scope.TotalPage = response.TotalPage;

					$scope.dataList = $scope.searchData;
					if ($scope.filterModel.value) {
						var listId = $scope.filterModel.value.split(',');
						if (listId.length > 0) {
							$scope.dataList.forEach(function (item) {
								if (listId.indexOf(item.ID.toString()) !== -1) {
									item.Checked = true;
								}
								else {
									item.Checked = false;
								}
							});
						}
					}
				}, function (error) { });
			}

			function getEmployeeList(params) {
				rebarDataSvc.GetOperatorList(params, function (response) {
					$scope.searchData = $scope.searchData.concat(response.Data);
					$scope.CurrentPage = response.CurrentPage;
					$scope.TotalPage = response.TotalPage;

					$scope.dataList = $scope.searchData;
					if ($scope.filterModel.value) {
						var listId = $scope.filterModel.value.split(',');
						if (listId.length > 0) {
							$scope.dataList.forEach(function (item) {
								if (listId.indexOf(item.ID.toString()) !== -1) {
									item.Checked = true;
								}
								else {
									item.Checked = false;
								}
							});
						}
					}
				}, function (error) { });
			}

			function getRegisterList(params) {
				rebarDataSvc.GetRegisterList(params, function (response) {
					$scope.searchData = $scope.searchData.concat(response.Data);
					$scope.CurrentPage = response.CurrentPage;
					$scope.TotalPage = response.TotalPage;

					$scope.dataList = $scope.searchData;
					if ($scope.filterModel.value) {
						var listId = $scope.filterModel.value.split(',');
						if (listId.length > 0) {
							$scope.dataList.forEach(function (item) {
								if (listId.indexOf(item.ID.toString()) !== -1) {
									item.Checked = true;
								}
								else {
									item.Checked = false;
								}
							});
						}
					}
				}, function (error) { });
			}

			function getDescriptionList(params) {
				rebarDataSvc.GetDescriptionList(params, function (response) {
					$scope.searchData = $scope.searchData.concat(response.Data);
					$scope.CurrentPage = response.CurrentPage;
					$scope.TotalPage = response.TotalPage;

					$scope.dataList = $scope.searchData;
					if ($scope.filterModel.value) {
						var listId = $scope.filterModel.value.split(',');
						if (listId.length > 0) {
							$scope.dataList.forEach(function (item) {
								if (listId.indexOf(item.ID.toString()) !== -1) {
									item.Checked = true;
								}
								else {
									item.Checked = false;
								}
							});
						}
					}
				}, function (error) { });
			}
		}
	});
})();