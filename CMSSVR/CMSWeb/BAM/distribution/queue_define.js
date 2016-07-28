(function () {

	define(['cms',
		'DataServices/Bam/DistributionSvc', 'Services/dialogService'], function (cms) {
			cms.register.controller('queuedefineCtrl', queuedefineCtrl);

			queuedefineCtrl.$inject = ['$scope', '$modalInstance', '$timeout', 'cmsBase', 'items', 'AppDefine', 'DistributionSvc', 'chartSvc', 'dialogSvc'];

			function queuedefineCtrl($scope, $modalInstance, $timeout, cmsBase, items, AppDefine, DistributionSvc, chartSvc, dialogSvc) {
				var vm = this;

				vm.closePopup = function (data) {
					$modalInstance.close(data);
				};

				active();
				function active() {
					//console.log(items.selectedSites);
				}

				vm.arrQueue = [];
				$scope.areaitem = [{
					id: '0',
					name: cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT)
				}];
				var QueueID = 1;
				var AreaID = 1;
				

				vm.AddQueue = function () {
					var obj = {};
					obj.name = '';
					obj.id = QueueID;
					obj.cid = '0';
					obj.areas = [
						//{
						//	id: '0',
						//	name: 'Please select a region'
						//}
					]

					vm.arrQueue.push(obj);
					vm.AddArea(QueueID);
					QueueID++;

					//auto scroll to the bottom
					//var height = 0;
					//$('.queue-block').each(function (i, value) {
					//	height += parseInt($(this).height());
					//});

					//height += '50';

					//$('.scroll-body').animate({ scrollTop: height });
					$('#queuebox').animate({
						scrollTop: $('#queuebox').get(0).scrollHeight
					}, 2000);
				}
				vm.RemoveQueue = function (id) {
					//alert(id);
					//var Queue = vm.arrQueue[id];
					//var Queue = $.grep(vm.arrQueue, function (e) { return e.id == id; }).FirstOrDefault();
					var Queue = Enumerable.From(vm.arrQueue)
				  .Where(function (i) { return (i.id == id) })
				  .Select(function (x) { return x; })
				  .FirstOrDefault();
					//vm.aaa = {
					//	name: 'DD',
					//	id: 7,
					//	areas: [
					//	 {
					//	 	name: '',
					//	 	id: '0'
					//	 }
					//	]
					//};
					//	DistributionSvc.DeleteQueue(vm.aaa, function (data) {


					//	},
					//function (error) {
					//	cmsBase.cmsLog.error('error');
					//});

					vm.arrQueue.splice(vm.arrQueue.indexOf(Queue), 1);
					//vm.disableSelect = false;
					vm.checkAllSelect();
				}

				vm.checkSelect = function (area) {
					var aa = Enumerable.From(area)
							  .Where(function (i) { return (i.id == 0) })
							  .Select(function (x) { return x; })
							  .FirstOrDefault();
					if (aa != null) {
						vm.disableSelect = true;
						return;
					}
					else {
						vm.disableSelect = false;
					}
				}
				
				vm.AddArea = function (id) {
					var Queue = Enumerable.From(vm.arrQueue)
								.Where(function (i) { return (i.id == id) })
								.Select(function (x) { return x; })
								.FirstOrDefault();

					var arrIDs = Enumerable.From(Queue.areas)
								.Select(function (x) { return x.id; }).ToArray();

					//var count = Enumerable.From($scope.areaitem)
					//		  .Where(function (i) { return (arrIDs.indexOf(i.id) < 0 && i.id != 0) })
					//		  .Select(function (x) { return x; })
					//		  .FirstOrDefault();
					

					if (arrIDs.length < $scope.areaitem.length-1) {
						var first = Enumerable.From($scope.areaitem).FirstOrDefault();

						var reg = {};
						reg.name = first.name;
						reg.id = first.id;
						Queue.areas.push(reg);
						//vm.checkSelect(Queue.areas);
						vm.checkAllSelect();

					}
					else {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.REGION_MAXIMUM);//"Cannot add more than maximum region number!";
						cmsBase.cmsLog.warning(msg);
					}
					//AreaID++;
				}
				vm.RemoveArea = function (Queueid, AreaId) {
					var QueueAreas = Enumerable.From(vm.arrQueue)
								.Where(function (i) { return (i.id == Queueid) })
								.Select(function (x) { return x.areas; })
								.FirstOrDefault();

					var area = Enumerable.From(QueueAreas)
							  .Where(function (i) { return (i.id == AreaId) })
							  .Select(function (x) { return x; })
							  .FirstOrDefault();

					QueueAreas.splice(QueueAreas.indexOf(area), 1);
					//vm.checkSelect(QueueAreas);
					vm.checkAllSelect();

					angular.forEach(vm.arrQueue, function (_data) {
						if (_data.areas.length == 0) {
							vm.disableSelect = true;
						}
					});
				}

				vm.checkAllSelect = function () {
					if (vm.arrQueue.length == 0) { //Show Save button when there is no queue
						vm.disableSelect = false;
					}
					else {
						var keepGoing = true;
						angular.forEach(vm.arrQueue, function (d) {
							if (keepGoing) {
								vm.checkSelect(d.areas);
								if (vm.disableSelect == true) {
									keepGoing = false;
								}
							}
						});
					}
					
				}

				vm.disableSelect = false;
				vm.onchangeArea = function (area, item) {
					vm.checkAllSelect();

					angular.forEach(item.areas, function (d) {
						if (d === area) {
							//continue;

						}
						else if (parseInt(d.id) === parseInt(area.id) &&d.id!=0) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.REGION_CHOSEN);//"This region has been chosen";
							cmsBase.cmsLog.warning(msg);
							area.id = '0';
							area.name = cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT);
							vm.disableSelect = true;
							return;
						}
					});
				}
				var pram = {
					siteKeys: items.selectedSites.toString(),
					QueueData: vm.arrQueue
				}
				var flag = 0;
				var name = "";
				vm.checkDup = function (aa) {
					if (aa.length > 1) {
						for (i = 0; i < aa.length - 1; i++) {
							for (j = i + 1; j < aa.length; j++) {
								if (aa[i].name == aa[j].name) {
									name = aa[i].name;
									flag = 1;
								}
							}
						}
					}
				}

				vm.Save = function () {
					vm.checkDup(vm.arrQueue);
					if (flag == 0) {
						DistributionSvc.AddQueue(pram, function (data) {
							$timeout(function () {
								vm.closePopup(true);
							}, 1000);
						},
						function (error) {
							$modalInstance.close(false);
							cmsBase.cmsLog.error('error');
						});
					}
					else {
						//var msg = "This Queue " + name + " already exists";
						var msg = formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.QUEUE_NAME_EXIST), name);
						cmsBase.cmsLog.warning(msg);
						flag = 0;
						$modalInstance.close(false);
						//return;
					}
					//$modalInstance.close(false);
				}

				function showApplyConfirm() {
					var modalOptions = {
						closeButtonText: AppDefine.Resx.BTN_CANCEL,
						actionButtonText: AppDefine.Resx.BTN_OK,
						headerText: AppDefine.Resx.QUEUE_APPLY_HEADER,
						bodyText: AppDefine.Resx.QUEUE_APPLY_ALL_STORES_MSG
					};
					var modalDefaults = {
						size: 'sm'
					};

				dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
					if (result === AppDefine.ModalConfirmResponse.OK) {
				   
					    DistributionSvc.applyStore(pram, function (data) {
						    $timeout(function () {
							    vm.closePopup();
						    }, 1000);
					    },
					    function (error) {
						    cmsBase.cmsLog.error('error');
					    });
				  
					}
				    });
				}
				vm.Apply = function () {
					vm.checkDup(vm.arrQueue);
					if (flag == 0) {
						showApplyConfirm();
					}
					else {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.QUEUE_NAME_THE_SAME);//"CAN NOT INPUT THE SAME QUEUE'S NAME";
						cmsBase.cmsLog.warning(msg);
						flag = 0;
						return;
					}
				}
				//var siteKeys = {
				//	siteKeys: items.selectedSites,
				//};

				vm.AllRegionsData = false;
				DistributionSvc.GetArea(pram, function (data) {
					vm._aaa = [];
					if (data.AllRegions.length) {
						vm.AllRegionsData = true;
					}
					if (data) {
						vm.myData = data;
						angular.forEach(data.QueueData, function (_data) {
							angular.forEach(_data.areas, function (dd) {
								var bbb = {
									id: dd.id,
									name: dd.Name
								};
								vm._aaa.push(bbb);
							});
							var _queue = {
								id: _data.id,
								cid: _data.id,
								name: _data.Name,
								areas: vm._aaa
							};
							vm.arrQueue.push(_queue);
							vm._aaa = [];

						});
						angular.forEach(data.AllRegions, function (e) {
							var bbb = {
								id: e.id,
								name: e.Name
							};
							$scope.areaitem.push(bbb);

						});
						if (vm.arrQueue.length > 0) {
							QueueID = vm.arrQueue[vm.arrQueue.length - 1].id;
							QueueID++;
						}
						//console.log("vm.arrQueuevm.arrQueuevm.arrQueuevm.arrQueue");
						//console.log(vm.arrQueue);
						//console.log($scope.areaitem);

					}
				},
				function (error) {
					//cmsBase.cmsLog.error('error');
				});

				function formatString(format) {
					var args = Array.prototype.slice.call(arguments, 1);
					return format.replace(/{(\d+)}/g, function (match, number) {
						return typeof args[number] != 'undefined' ? args[number] : match;
					});
				};
			}
		});

})();