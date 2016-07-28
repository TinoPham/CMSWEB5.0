(function () {
	'use strict';

	define(['cms', 'Services/dialogService'], function (cms) {
	    cms.register.controller('configtranflagCtrl', configtranflagCtrl);

	    configtranflagCtrl.$inject = ['$scope', 'AccountSvc', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc', 'dialogSvc', 'colorSvc', '$timeout'];

	    function configtranflagCtrl($scope, AccountSvc, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc, dialogSvc, colorSvc, $timeout) {

	        $scope.optionColor = {
	            showPaletteOnly: true,
	            hideAfterPaletteSelect: true,
	            className: 'mobile-picker',
	            palette: [
	                [
	                    '#000', '#444', '#666', '#999', '#ccc', '#eee', '#f3f3f3', '#fff',
	                    '#f00', '#f90', '#ff0', '#0f0', '#0ff', '#00f', '#90f', '#f0f',
	                    '#f4cccc', '#fce5cd', '#fff2cc', '#d9ead3', '#d0e0e3', '#cfe2f3', '#d9d2e9', '#ead1dc',
	                    '#ea9999', '#f9cb9c', '#ffe599', '#b6d7a8', '#a2c4c9', '#9fc5e8', '#b4a7d6', '#d5a6bd',
	                    '#e06666', '#f6b26b', '#ffd966', '#93c47d', '#76a5af', '#6fa8dc', '#8e7cc3', '#c27ba0',
	                    '#c00', '#e69138', '#f1c232', '#6aa84f', '#45818e', '#3d85c6', '#674ea7', '#a64d79',
	                    '#900', '#b45f06', '#bf9000', '#38761d', '#134f5c', '#0b5394', '#351c75', '#741b47',
	                    '#600', '#783f04', '#7f6000', '#274e13', '#0c343d', '#073763', '#20124d', '#4c1130'
	                ]
	            ]
	        };

	        $scope.selectItem = {};
	        $scope.editMode = false;
	        $scope.hasChanged = false;
	        $scope.userLogin = AccountSvc.UserModel();
	        active();

	        $scope.addFlag = function () {
	            $scope.error = false;
	            $scope.error2 = false;
	            if (!$scope.Name || ($scope.Name && $scope.Name.length <= 0) || $scope.TypeWeight === undefined || $scope.TypeWeight === null || ($scope.TypeWeight && $scope.TypeWeight.length <= 0)) {
	                $scope.error = true;
	                return;
	            }
	            if (parseInt($scope.TypeWeight) == 0) {
	                $scope.error2 = true;
	                return;
	            }

	            if ($scope.Name !== null && $scope.TypeWeight !== null) {
	            	var oldColor = '';
	            	var oldName = '';
	            	var oldWeight = '';
	            	if (!$scope.newMode) {
	            		oldColor = $scope.selectItem.Color;
	            		oldName = $scope.selectItem.Name;
	            		oldWeight = $scope.selectItem.TypeWeight;
	            	}
	            	var color = $scope.Color == null ? "000" : colorSvc.rgbtoHex($scope.Color);
	            	if (color.charAt(0) != "#") {
	            		color = "#" + color;
	            	}
	                $scope.selectItem.Color = color;
	                $scope.selectItem.Name = $scope.Name;
	                $scope.selectItem.TypeWeight = parseInt($scope.TypeWeight);
	                if ($scope.newMode === true) {
	                    $scope.selectItem.Id = 0;
	                    $scope.selectItem.Desc = "";
	                    $scope.selectItem.IsSystem = false;

	                    var isExist = Enumerable.From($scope.data)
							.Where(function (i) { return i.Name.toLowerCase() == $scope.Name.toLowerCase() })
							.Select(function (x) { return x; })
							.FirstOrDefault();
	                    if (!isExist) {
	                    	rebarDataSvc.addTransactionFlagType($scope.selectItem, function (result) {
	                    		if (result.data <= 0) {
	                    			var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
	                    			cmsBase.cmsLog.error(msg);
	                    			return;
	                    		}
	                    		$scope.hasChanged = true;
	                    		active();
	                    		var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_SUCCESS_MSG);
	                    		cmsBase.cmsLog.success(msg);
	                    	}, function () {
	                    		var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
	                    		cmsBase.cmsLog.error(msg);
	                    	});
	                    }
	                    else {
	                    	var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG) + ": " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.NAME_EXIST_MSG);
	                    	cmsBase.cmsLog.error(msg);
	                    }
	                } else {
	                	var isExist = Enumerable.From($scope.data)
							.Where(function (i) { return ($scope.selectItem !== i && i.Name.toLowerCase() == $scope.Name.toLowerCase()) })
							.Select(function (x) { return x; })
							.FirstOrDefault();
	                	if (!isExist) {
	                		rebarDataSvc.updateTransactionFlagType($scope.selectItem, function (result) {
	                			if (result.data <= 0) {
	                				var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
	                				cmsBase.cmsLog.error(msg);
	                				return;
	                			}
	                			$scope.hasChanged = true;
	                			var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_SUCCESS_MSG);
	                			cmsBase.cmsLog.success(msg);
                                // 2014-05-24: Tri fix bug no add new after edit success flag.
	                			$scope.selectItem = {};
	                		}, function () {
	                			var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
	                			cmsBase.cmsLog.success(msg);
	                		});
	                	}
	                	else {
	                		$scope.selectItem.Color = oldColor;
	                		$scope.selectItem.Name = oldName;
	                		$scope.selectItem.TypeWeight = oldWeight;

	                		var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG) + ": " + cmsBase.translateSvc.getTranslate(AppDefine.Resx.NAME_EXIST_MSG);
	                		cmsBase.cmsLog.error(msg);
	                	}
	                }

	            } else {
	                var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ERR_EMPTY_MSG);
	                cmsBase.cmsLog.error(msg);
	            }
	            $scope.editMode = false;
	            $scope.newMode = false;
	        }

	        $scope.removeFlag = function(flag) {

	            if (flag.IsSystem === true) {
	                //var msg = cmsBase.translateSvc.getTranslate("Do not allow remove system flag.");
	                //cmsBase.cmsLog.warn(msg);
	                return;
	            }


	            var title = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELCONF_MSG);

	            var modalOptions = {
	                headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
	                bodyText: title + " " +flag.Name
	            };

	                var modalDefaults = {
	                    backdrop: true,
	                    keyboard: true,
	                    modalFade: true,
	                    size: 'sm'
	                }

	                dialogSvc.showModal(modalDefaults, modalOptions).then(function (data) {
	                    if (data === 'ok') {
	                        $scope.selectItem = flag;
	                        rebarDataSvc.delTransactionFlagType($scope.selectItem, function (result) {
	                            if (result === false) {
	                                var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.COFIRM_DELETE_MSG);
	                                cmsBase.cmsLog.error(msg);
	                                return;
	                            }
	                            $scope.hasChanged = true;
	                            var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_SUCCESS_MSG);
	                            cmsBase.cmsLog.success(msg);
	                            active();
	                        }, function (error) {
	                            var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.DELETE_FAIL_MSG);
	                            cmsBase.cmsLog.success(msg);
	                        });
	                    }
	                });
	        }

	        $scope.addNewFlags = function() {
	            $scope.editMode = true;
	            $scope.newMode = true;

	            $scope.Color = null;
	            $scope.Name = null;
	            $scope.TypeWeight = null;
	        }

	        $scope.editFlag = function(flag) {
	            
	            if (flag.IsSystem === true && $scope.userLogin.IsAdmin !== true) {
	                return;
	            }
	            $scope.editMode = true;
	            $scope.newMode = false;
	            $scope.selectItem = flag;

	            var color = colorSvc.hextoNum($scope.selectItem.Color);
	            $scope.Color = colorSvc.numtoRGB(color);
	            $scope.Name = $scope.selectItem.Name;
	            $scope.TypeWeight = $scope.selectItem.TypeWeight;
	        }

	        function active() {

	            rebarDataSvc.getTransactionTypes(function (data) {
	                $scope.data = data;
	            }, function (error) {

	            });

	        }
	        $scope.cancel = function () {
	            if ($scope.hasChanged === true) {
	                $modalInstance.close($scope.hasChanged);
	            }
	            $modalInstance.close();
	        }
	       
	        //$timeout(function () {
	        //    if ($(".numberic").length > 0) {
	        //        $(".numberic").keydown(function (e) {
	        //            if (!((e.keyCode > 95 && e.keyCode < 106)
            //              || (e.keyCode > 47 && e.keyCode < 58)
            //              || e.keyCode == 8 || e.keyCode == 46
            //              || e.keyCode == 37 || e.keyCode == 39 || e.keyCode == 9)) {
	        //                        return false;
	        //              }
	        //        });
	        //    }
	        //}, 1000)
	        
            $scope.pasteRegx = AppDefine.RegExp.PasteExp;
			$scope.inputRex = AppDefine.RegExp.NumberRestriction;
			$scope.PreventPaste = cmsBase.PreventPaste;
			$scope.PreventLetter = cmsBase.PreventInputKeyPress;

	        // Listen for input event on numInput.
			$scope.onkeydown = function (e) {
			    if (!((e.keyCode > 95 && e.keyCode < 106)
                  || (e.keyCode > 47 && e.keyCode < 58)
                  || e.keyCode == 8)) {
			        return false;
			    }
			}
			$scope.maxLengthCheck = function (e) {
			    var a = document.getElementById("typeWeight").value;
			    if (parseFloat(a) < 0) {
			        $scope.TypeWeight = 1;
			    }
			    if (parseFloat(a) > 999) {
			        //$scope.TypeWeight = 999;
			        $scope.TypeWeight = parseInt($scope.TypeWeight.toString().substr(0, 3));
			    }
			}
	    }
	});
})();