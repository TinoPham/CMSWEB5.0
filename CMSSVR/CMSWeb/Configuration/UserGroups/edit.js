(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('editaddusergroupCtrl', editaddusergroupCtrl);

		editaddusergroupCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'AccountSvc', '$filter'];

		function editaddusergroupCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, AccountSvc, $filter) {
			var vm = this;
			vm.userList = [];
			vm.userSelections = [];
			vm.functions = [];
			vm.functionSelections = [];
			vm.levelSelection = '';
			vm.inciSelected = false;
			vm.frmTitle = AppDefine.Resx.USERGROUP_ADD_HEADER;
			vm.valueGroup = {};
			vm.levels = [];
			vm.checkModules = false;
			vm.checkUsers = false;
			vm.status = {
				isCloseOther: false,
				isGeneralOpen: true,
				isModulesOpen: true,
				isUsersOpen: true
			};
			vm.userFilter = '';

	        active();

	        function active() {
	            if (items != null) {
					vm.valueGroup = angular.copy(items);
					vm.frmTitle = AppDefine.Resx.USERGROUP_EDIT_HEADER;
	            }
	            getUserList();
	            getFunctions();
	            getFuncLevel();
	        }

	        function getUserList() {
	            dataContext.usergroups.getUserList(function (data) {
					vm.userList = data;
	                if (items != null) {
						vm.userSelections = vm.valueGroup.Users;
	                }
	            },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });
                
	        }

			vm.getFunction = function (funcID) {
				var idx = vm.functionSelections.indexOf(funcID);
	            // is currently selected
	            if (idx > -1) {
					vm.functionSelections.splice(idx, 1);
	            }
	                // is newly selected
	            else {
					vm.functionSelections.push(funcID);
	            }

	            if (vm.functionSelections.indexOf(9) > -1)
	                vm.inciSelected = true;
	            else {
	                vm.inciSelected = false;
	            }
	            vm.checkModules = (vm.functions.length > 0 && vm.functionSelections.length == vm.functions.length);
	            if (funcID == 11)
	            {
	                idx = vm.functionSelections.indexOf(3);
	                if(idx < 0)
	                {
	                    vm.functionSelections.push(3);
	                }
	            }
	            if (funcID == 3)
	            {
	                idx = vm.functionSelections.indexOf(3);
	                if (idx < 0)
	                {
	                    idx = vm.functionSelections.indexOf(11);
	                    vm.functionSelections.splice(idx, 1);
	                }

	            }
	            
	        }

			vm.getUser = function (userID) {
				var idx = vm.userSelections.indexOf(userID);
	            // is currently selected
	            if (idx > -1) {
					vm.userSelections.splice(idx, 1);
	            }
	                // is newly selected
	            else {
					vm.userSelections.push(userID);
	            }

	            vm.checkUsers = (vm.userList.length > 0 && vm.userSelections.length == vm.userList.length);
				//vm.checkUsers = vm.userList.every(function (itm) {
	            //	return itm.selected;
	            //})
	        }

			vm.ddLevelChange = function () {
				vm.levelSelection = vm.levelModel.LevelID;
	        }

			vm.checkDisplayLevel = function (item) {
	            return (item==9)
	        }

	        function getFunctions() {
	            dataContext.usergroups.getFunctions(function (data) {
					vm.functions = data;
	                if (items != null) {
						angular.forEach(vm.valueGroup.FuncLevels, function (n) {
						    vm.functionSelections.push(n.FunctionID);
						    if (n.FunctionID== 9)
						        vm.inciSelected = true;
	                    });	                 
	                }
	            },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });
	        }

	        function getFuncLevel() {
	            dataContext.usergroups.getFuncLevel(function (data) {
					vm.levels = data;
	                if (items != null) {
						angular.forEach(vm.valueGroup.FuncLevels, function (n) {
	                        if (n.FunctionID == 9) {
								for (var i = 0; i < vm.levels.length; i++) {
									if (vm.levels[i].LevelID == n.LevelID) {
										vm.levelModel = vm.levels[i];
	                                }                                    
	                            }
	                        }
	                    });
	                }
					if ($.isEmptyObject(vm.levelModel) == true) {
						vm.levelModel = vm.levels[0];
	                }
					vm.levelSelection = vm.levelModel.LevelID;
	            },
                function (error) {
                    cmsBase.cmsLog.error('error');
                });
	        }

	        vm.SelectAllModule = function () {
	            vm.checkModules = (vm.checkModules == false);
	            vm.CheckAllModule();
	        }

	        vm.SelectAllUser = function () {
	            vm.checkUsers = (vm.checkUsers == false);
	            vm.CheckAllUsers();
	        }

			vm.CheckAllUsers = function () {
				var toggleStatus = vm.checkUsers;
				vm.userSelections = [];
	            if (toggleStatus == true) {
					angular.forEach(vm.userList, function (itm) {
						vm.userSelections.push(itm.UserID);
	                });
	            } 
			}			

			vm.CheckAllModule = function () {
				var toggleStatus = vm.checkModules;
				vm.functionSelections = [];
	            if (toggleStatus == true) {	                
					angular.forEach(vm.functions, function (itm) {
						vm.functionSelections.push(itm.FunctionID);
	                });
	            }

	            if (vm.functionSelections.indexOf(9) > -1)
	                vm.inciSelected = true;
	            else {
	                vm.inciSelected = false;
	            }
	        }

			vm.filterFunction = function (item) {
				var fullName = item.FName + item.LName;
				if (!$.isEmptyObject(fullName) && fullName.toLowerCase().replace(/\n/g, '').replace(/\s+/g, '').indexOf(vm.userFilter.toLowerCase().replace(/\n/g, "").replace(/\s+/g, '')) > -1) {
			        return true;
			    }		    
			    return false;
			}

			vm.CloseEdit = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
	        }

			function AddFunCLevel() {
	            var FuncLV = [];
				angular.forEach(vm.functionSelections, function (n) {
	                if (n == 9) {
						FuncLV.push({ FunctionID: n, LevelID: vm.levelSelection });
	                } else {
	                    FuncLV.push({ FunctionID: n, LevelID: 0 });
	                }
	            });
	            return FuncLV;
	        }

			vm.SaveUserGroup = function () {
				vm.valueGroup.Users = vm.userSelections;
				vm.valueGroup.FuncLevels = AddFunCLevel();
				vm.valueGroup.CreatedBy = AccountSvc.UserModel().UserID;
				dataContext.usergroups.updateUserGroup(vm.valueGroup, function (response) {
					if (response.ReturnStatus) {
						$modalInstance.close(response.Data);
	                    var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
						cmsBase.cmsLog.success(msg);
	                }
	                else {
						angular.forEach(response.ReturnMessage, function (message) {
							var msg = cmsBase.translateSvc.getTranslate(message);
							cmsBase.cmsLog.warning(msg);
						});
	                }
				}
				, function (error) {
					if (vm.valueGroup.GroupId != 0) {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
                    cmsBase.cmsLog.error(msg);
					}
					else {
						var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					}
                });

	        }
		}
	});
})();