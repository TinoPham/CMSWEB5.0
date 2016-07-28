(function () {
	'use strict';

	define(['cms', 'widgets/rebar/select-compare'
        , 'widgets/rebar/select-tran-flag'
        , 'widgets/rebar/payment-filter'
	, 'DataServices/SiteSvc'], function (cms) {
		cms.register.controller('transactiondetailCtrl', transactiondetailCtrl);

		transactiondetailCtrl.$inject = ['$rootScope', '$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', '$modal', 'dialogSvc', 'SiteSvc','$filter'];

		function transactiondetailCtrl($rootScope, $scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, $modal, dialogSvc, SiteSvc,$filter) {
			$scope.showVideo = false;
			$scope.showVideoFn = showVideoFn;
			$scope.isMobile = cmsBase.isMobile;
			$scope.showNextTransaction = showNextTransaction;
			$scope.showPrevTransaction = showPrevTransaction;
			$scope.nextOrPrevFn = nextOrPrevFn;
			$scope.SaveException = SaveException;
			$scope.editTranFlag = editTranFlag;
			$scope.FlagHasChange = false;
			$scope.NoteHasChange = false;
			$scope.isMax = false;
			$scope.showNotes = false;
			$scope.tranChange = { TranId: 0, PacId: 0, RegisterId: 0, ExceptionTypes: [], Note: { UserId: 0, TranId: 0, Note: "", DateNotes: null } };
			//$scope.$watch('data.Note.Note', function () {
			//	if ($scope.data && $scope.data.Note && $scope.data.Note.Note && $scope.NoteHasChange === false) {
			//		$scope.NoteHasChange = true;
			//	}
			//});
		
			$scope.cancel = function () {
			    if ($scope.NoteHasChange || $scope.FlagHasChange) {
			        ShowDialogConfirm(0);
			    } else {
			        $modalInstance.close();
			    }
			};

			$scope.fullSize = function () {
				$scope.isMax = !$scope.isMax;
				cmsBase.modalfullscreen($scope.isMax, 'transactiondetailCtrl');
			};

			$scope.noteChanged = function () {
				$scope.NoteHasChange = true;
			};

			active();

			function nextOrPrevFn($event) {
				$scope.FlagHasChange = false;
				$scope.NoteHasChange = false;
				$scope.tranChange = { TranId: 0, PacId: 0, RegisterId: 0, ExceptionTypes: [], Note: { UserId: 0, TranId: 0, Note: "", DateNotes: null } };
				var e = $event;

				switch (e.keyCode) {
					case 37:
						showPrevTransaction();
						break;
					case 39:
						showNextTransaction();
						break;
				}
			}

			function showNextTransaction() {
				if ($scope.NoteHasChange || $scope.FlagHasChange) {
					var nextOrPrevNumber = 1;
					ShowDialogConfirm(nextOrPrevNumber);
				}
				else {
					var param = { tranId: $scope.data.TranId, PacId: $scope.data.PacId, registerId: $scope.data.RegisterId, nextOrPrev: 1 };
					rebarDataSvc.getTransactionInfo(param, function (data) {
						if (!data) {
						    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NONEXTMSG);
							cmsBase.cmsLog.warning(msg);
							return;
						}
						$scope.data = data;

					}, function (error) {
					    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NONEXTMSG);
						cmsBase.cmsLog.warning(msg);
					});
				}
			}

			function showPrevTransaction() {
				if ($scope.NoteHasChange || $scope.FlagHasChange) {
					var nextOrPrevNumber = -1;
					ShowDialogConfirm(nextOrPrevNumber);
				}
				else {
					var param = { tranId: $scope.data.TranId, PacId: $scope.data.PacId, registerId: $scope.data.RegisterId, nextOrPrev: -1 };
					rebarDataSvc.getTransactionInfo(param, function (data) {
						if (!data) {
						    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NOPREVMSG);
							cmsBase.cmsLog.warning(msg);
							return;
						}
						$scope.data = data;

					}, function (error) {
					    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NONEXTMSG);
						cmsBase.cmsLog.warning(msg);
					});
				}
			}
			var dvrInfo;

			function showVideoFn() {
			    SiteSvc.GetDVRInfoRebarTransact(AppDefine.TypeTimeLines.Ten_minute, $scope.data, cmsBase.GetDVRInfoSuccess, function (errr) {
			    });
			  

			//	$scope.showVideo = !$scope.showVideo;
			}

			function active() {
				$scope.FlagHasChange = false;
				$scope.NoteHasChange = false;

				var param = { tranId: items.TranId };
				rebarDataSvc.getTransactionInfo(param, function (data) {
					$scope.data = data;

					$scope.picSource = null;
					if ($scope.data.CompanyLogo) {
						$scope.picSource = AppDefine.ImageOptions.PrefixImageEmbedding + $scope.data.CompanyLogo;
					}

				}, function (error) {

				});
			}

			function editTranFlag() {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var showFlagModal = $modal.open({
						templateUrl: 'widgets/rebar/select-tran-flag.html',
						controller: 'selecttranflagCtrl',
						resolve: {
							items: function () {
								return {
									data: $scope.data.ExceptionTypes
								}
							}
						},
						size: 'md',
						backdrop: 'static',
						keyboard: false
					});

					showFlagModal.result.then(function (data) {
						if (data) {
							$scope.FlagHasChange = true;
							$scope.hasChanged = true;
							$scope.data.ExceptionTypes = data;
							$scope.tranChange.ExceptionTypes = data;
						}
						$scope.modalShown = false;
					});
				}
			}

			function ShowDialogConfirm(nextOrPrevNumber) {
				var modalOptions = {
					headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
					bodyText: AppDefine.Resx.CLOSE_FORM_CONFIRM_MSG
				};

				var modalDefaults = {
					backdrop: true,
					keyboard: true,
					modalFade: true,
					templateUrl: 'Widgets/ConfirmDialog.html'
				};

				dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
					if (result === "cancel") {
						$scope.FlagHasChange = false;
						$scope.NoteHasChange = false;

						if (nextOrPrevNumber == 0) {
						    //if case button x close modal
						    $modalInstance.close();
						} else {
						    changeFormTransaction(nextOrPrevNumber);
						}
					}
					else {
					    SaveException(nextOrPrevNumber);
					}
				});
			}

		    //2015-05-25 Tri Add function change form transaction
			function changeFormTransaction(nextOrPrevNumber)
			{
			    var param = { tranId: $scope.data.TranId, PacId: $scope.data.PacId, registerId: $scope.data.RegisterId, nextOrPrev: nextOrPrevNumber };
			    rebarDataSvc.getTransactionInfo(param, function (data) {
			        if (!data) {
			            var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NONEXTMSG);
			            cmsBase.cmsLog.warning(msg);
			            return;
			        }
			        $scope.data = data;

			    }, function (error) {
			        var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.NONEXTMSG);
			        cmsBase.cmsLog.warning(msg);
			    });
			}

			//START TRANSACTION DETAIL----------------------------------------------------------------------------------------------------------
			$scope.selectComparition = function () {
				if (!$scope.modalShown) {
					$scope.modalShown = true;
					var showAboutdModal = $modal.open({
						templateUrl: 'widgets/rebar/select-compare.html',
						controller: 'selectcompareCtrl',
						resolve: {
							items: function () {
								return {
									data: $scope.data,
									startdate: angular.copy($rootScope.rebarSearch.DateTo),
									enddate: angular.copy($rootScope.rebarSearch.DateTo)
								}
							}
						},
						size: 'md',
						backdrop: 'static',
						keyboard: false
					});

					showAboutdModal.result.then(function (data) {


						$scope.modalShown = false;
					});
				}
			}
			//END TRANSACTION DETAIL------------------------------------------------------------------------------------------------------------

			function SaveException(nextOrPrevNumber) {
				if ($scope.FlagHasChange === false && $scope.NoteHasChange === false) {
					$modalInstance.close();
					return;
				}

				if ($scope.FlagHasChange === false) {
					$scope.tranChange.ExceptionTypes = $scope.data.ExceptionTypes;
				}

				$scope.tranChange.TranId = $scope.data.TranId;
				$scope.tranChange.PacId = $scope.data.PacId;
				$scope.tranChange.RegisterId = $scope.data.RegisterId;
				if ($scope.data.Note) {
					$scope.tranChange.Note.Note = $scope.data.Note.Note;
				} else {
					$scope.tranChange.Note = null;
				}

				rebarDataSvc.saveTransactionNotes($scope.tranChange, function (data) {
					$scope.FlagHasChange = false;
					$scope.NoteHasChange = false;

				    // Tri fix if nextOrPrevNumber is 0 close form
				    // if nextOrPrevNumber is deferen 0 change form transaction
					if (nextOrPrevNumber == 0) {
					    $modalInstance.close($scope.tranChange);
					} else {
					    changeFormTransaction(nextOrPrevNumber);
					}
				}, function (error) {
				    var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVEERROR);
					cmsBase.cmsLog.error(msg);
				});
			}

		}
	});
})();