(function () {
	'use strict';

	define(['cms',
            'DataServices/Configuration/jobtitleSvc',
            'Scripts/Directives/treeComponent',
            'Scripts/Directives/CmsTreeView',
            'configuration/jobtitle/edit.js',
            'configuration/usergroups/edit.js',
            'configuration/sites/helpers'
	    ], function (cms) {
		cms.register.controller('editadduserCtrl', editadduserCtrl);

		editadduserCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'AccountSvc', 'JobTitleSvc', 'colorSvc', '$timeout', '$filter', '$q', '$modal', 'siteadminService'];

		function editadduserCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, AccountSvc, JobTitleSvc, colorSvc, $timeout, $filter, $q, $modal, siteadminService) {
		    var passdefault = "password";		    
			var IMAGE_TYPE_STRING = "image";
		    $scope.def = {
		        Id: 'ID',
		        Name: 'Name',
		        Type: 'Type',
		        Checked: 'Checked',
		        Childs: 'Sites',
		        Count: 'SiteCount',
		        Model: {}
		    }
		    var vm = this;
		    vm.Password = null;
		    vm.PasswordConfirm = null;
			vm.userLogin = AccountSvc.UserModel();
			vm.frmTitle = "USER_ADD_HEADER";
			vm.fileAccept = AppDefine.FileUploadTypes.Images;
			
			vm.options = {
			    Node: {
			        IsShowIcon: true,
			        IsShowCheckBox: true,
			        IsShowNodeMenu: false,
			        IsShowAddNodeButton: false,
			        IsShowAddItemButton: false,
			        IsShowEditButton: true,
			        IsShowDelButton: true,
			        IsDraggable: false
			    },
			    Icon: {
			        Item: ' icon-home'
			    },
			    Item: {
			        IsShowItemMenu: false
				},
				Type: {
			        Folder: 0,
			        Group: 2,
			        File: 1
				},
				CallBack: {
					SelectedFn: selectedFn
				},
			}

			vm.minDate = new Date('01/01/1900');
			vm.dateFormat = "MM/dd/yyyy";
			vm.dateOptions = {
				formatYear: 'yy',
				startingDay: 1
			};
			vm.isAddForm = true;
			vm.status = {
				isCloseOther: false,
				isInfoOpen: true,
				isDetailOpen: true
			};
			vm.picSource = null;

			active();

			function active() {
				if (items != null) {
					getUserDetail(items.UID);
				}
				else {
					vm.isAddForm = true;
					getMaxEmployeeID();
				}

				//Load resources referrences
				cmsBase.translateSvc.partLoad('Configuration/JobTitle');
				cmsBase.translateSvc.partLoad('Configuration/UserGroups');
			}

			function InitData() {
				var curDate = new Date();
				var nextYearDate = new Date(curDate.getFullYear() + 1, curDate.getMonth(), curDate.getDate());
				vm.userSelected = {
				    UserID: 0
					, EmployeeID: vm.maxEmployeeId
					, GroupID: 0
					, FName: ""
					, LName: ""
					, Telephone: ""
					, Email: ""
					, EmailDaily: false
					, Notes: ""
					, PositionID: 0
					, PositionName: ''
					, PositionColor: 0
					, UserName: ''
					, Password: null
					, ExpiredDate: nextYearDate
					, CreatedDate: new Date()
					, CreatedBy: null
					, CompanyID: vm.userLogin.CompanyID
					, IsAdmin: false
					, isExpired: false
                    , SiteIDs: []
                    , UPhoto: null
                    , ImageSrc: ''
				};
			}

			function getUserDetail(userId) {
				dataContext.user.getUserDetail(userId).then(
					function (response) {
					    vm.userSelected = response;
					    vm.Password = passdefault;
					    vm.PasswordConfirm = vm.Password;
					    vm.UserName = vm.userSelected.UserName;
						if (vm.userSelected.UPhoto != null) {
							vm.picSource = dataContext.user.GetUserImage(vm.userSelected.UserID, vm.userSelected.UPhoto );// AppDefine.ImageOptions.PrefixImageEmbedding + vm.userSelected.ImageSrc;
						}						    
						vm.frmTitle = "USER_EDIT_HEADER";
						vm.isAddForm = false;
						dataContext.injectRepos(['configuration.user', 'configuration.usergroups', 'configuration.siteadmin']).then(getExtendData);
					},
					function (error) {
						cmsBase.cmsLog.error(msg);
					}
				);
			}

			function getMaxEmployeeID() {
				dataContext.user.GetMaxEmployeeID().then(
						function (response) {
							vm.maxEmployeeId = response;
							InitData();
							dataContext.injectRepos(['configuration.user', 'configuration.usergroups', 'configuration.siteadmin']).then(getExtendData);
						}
						, function (error) {
							console.log(error);
						}
					);
			}

			function getExtendData() {
				GetAllJobTitle();
				GetAllUserGroups();
				GetAllRegionSites();
			}

			function GetAllJobTitle() {
				JobTitleSvc.GetJobTitle(vm.userLogin.UserID).then(function (data) {
					vm.JobTitleData = data;
					SetJobTitleSelected(data);
				});
			}

			function GetAllUserGroups() {
				dataContext.usergroups.getUserGroup().then(
					function (data) {
					vm.UserGroupsData = data;
						SetUserGroupSelected(data);
					},
					function (error) {
						console.log(error);
					});
						}

			function SetJobTitleSelected(jobList) {
				for (var i = 0; i < jobList.length; i++) {
					if (vm.userSelected.PositionID === jobList[i].PositionID) {
						vm.jobtitleSelected = jobList[i];
						break;
						}
					}
					}

			function SetUserGroupSelected(goalTypeList) {
				for (var i = 0; i < goalTypeList.length; i++) {
					if (vm.userSelected.GroupID === goalTypeList[i].GroupId) {
						vm.userGroupSelected = goalTypeList[i];
						break;
					}
					}
			}

			function GetAllRegionSites() {
			    dataContext.siteadmin.GetSiteByUserId(vm.userLogin.UserID, function (data) {
					if (!data.Sites) {
					    vm.sitetree = {
					        ID: 0,
					        Name: "",
					        MACAddress: 0,
					        UserID: 0,
					        ParentKey: null,
					        ImageSite: 0,
					        PACData: 0,
					        Sites: [],
					        SiteCount: 0,
					        Type: 0,
					        Checked: false
					    };
						return;
					}
					UpdateSiteChecked(data);
					vm.sitetree = data;
			        reCheckTree(vm.sitetree.Sites);
					vm.treeSiteFilter = angular.copy(vm.sitetree);
				}, function (error) {
					var msg = cmsBase.translateSvc.getTranslate(error.data.ReturnMessage[0]);
					cmsBase.cmsLog.error(msg);
				});

			}

			function UpdateSiteChecked(treeData) {
				if (!treeData) {
					return;
				}

			    if ($.isEmptyObject(treeData.Sites)) {
			        angular.forEach(treeData, function (n) {
						if (angular.isObject(n)) {
			            if (n.Sites && n.Sites.length > 0) {
			                UpdateSiteChecked(n.Sites);
			            }

			            if (vm.userSelected == null || vm.userSelected == undefined
                            || vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
                            || vm.userSelected.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
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
						if (angular.isObject(n)) {
			            if (n.Sites && n.Sites.length > 0) {
			                UpdateSiteChecked(n.Sites);
			            }
			            if (vm.userSelected == null || vm.userSelected == undefined
                            || vm.userSelected.SiteIDs == null || vm.userSelected.SiteIDs == undefined
								|| vm.userSelected.SiteIDs.indexOf(n.ID) < 0 || n.Type === 0) {
			                n.Checked = false;
			            }
			            else {
			                n.Checked = true;
			            }
						}
			        });
			    }				
			}

			function GetSiteSelectedIDs(siteCheckedIDs, treeData) {
				angular.forEach(treeData, function (n) {
					if (n.Sites && n.Sites.length > 0) {
						GetSiteSelectedIDs(siteCheckedIDs, n.Sites);
					}
					if (n.Type === 1 && n.Checked === true) {
						siteCheckedIDs.push(n.ID);
					}
				});
			}

			vm.open = function ($event) {
				$event.preventDefault();
				$event.stopPropagation();

				vm.opened = true;
			};

			vm.ddlJobTitleChanged = function () {
				if (!$.isEmptyObject(vm.jobtitleSelected)) {
					vm.userSelected.PositionID = vm.jobtitleSelected.PositionID;
			    }			    
			}

			vm.ddlUserGroupChanged = function () {
				if (!$.isEmptyObject(vm.userGroupSelected)) {
					vm.userSelected.GroupID = vm.userGroupSelected.GroupId;
			    }				
			}

			vm.SaveUser = function () {
				var siteCheckedIDs = new Array();
				GetSiteSelectedIDs(siteCheckedIDs, vm.treeSiteFilter.Sites);
				if (vm.Password == passdefault)
				    vm.userSelected.Password = null;
				else
				    vm.userSelected.Password = vm.Password;
				vm.userSelected.UserName = vm.UserName;
				vm.userSelected.CreatedDate = new Date();
				vm.userSelected.SiteIDs = siteCheckedIDs;
				if (!vm.picSource) {
				    vm.userSelected.ImageSrc = null;
					vm.userSelected.UPhoto = null;
				}
				ExcAddUser();
			}

			vm.CloseUserAdmin = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			function ExcAddUser() {
				dataContext.user.AddUser(vm.userSelected).then(
					function (response) {
						if (response.ReturnStatus) {
					        var msg = cmsBase.translateSvc.getTranslate(response.ReturnMessage[0]);
							cmsBase.cmsLog.success(msg);
							$modalInstance.close(response.Data);
						}
						else {
							angular.forEach(response.ReturnMessage, function (message) {
								var msg = cmsBase.translateSvc.getTranslate(message);
								cmsBase.cmsLog.warning(msg);
							});
						}
					}
					, function (response) {
						console.log(response);
						if (vm.userSelected.UserID != 0) {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
						cmsBase.cmsLog.error(msg);
					}
						else {
							var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
							cmsBase.cmsLog.error(msg);
						}
					}
				);
			}

			function reCheckTree(sites) {
			    angular.forEach(sites, function (nodes) {
			        checkinglist(nodes);
			    });
			}

			function checkinglist(node) {

			    var result = AppDefine.treeStatus.Uncheck;
			    var intermid = false;
			    var interval = AppDefine.treeStatus.Uncheck;
			    var i = 0;
			    if (node.Sites && node.Sites.length > 0) {
			        angular.forEach(node.Sites, function (n) {
			            var ch = n.Checked;
			            if (n.Sites && n.Sites.length > 0) {
			                ch = checkinglist(n);
			            }

			            if (i === 0) {
			                interval = ch;
			            }

			            if (i > 0 && ch !== interval) {
			                intermid = true;
			            }

			            if (intermid === true && i > 0) {
			                ch = AppDefine.treeStatus.Indeterm;
			            }

			            result = ch;
			            i++;
			        });
				}
				else {
			        result = node.Checked;
			    }
			    node.Checked = result;
			    return result;
			}

			vm.AddUserGroup = function () {
			    if (!vm.modalShown) {
			        vm.modalShown = true;
					var userGroupInstance = $modal.open({
			            templateUrl: 'configuration/usergroups/edit.html',
			            controller: 'editaddusergroupCtrl as vm',
			            size: 'md',
			            backdrop: 'static',
			            backdropClass: 'modal-backdrop',
			            keyboard: false,
			            resolve: {
			                items: function () {
			                    return null;
			                }
			            }
			        });

					userGroupInstance.result.then(function (data) {
			            vm.modalShown = false;
						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
							vm.userSelected.GroupID = data.GroupId;
			                GetAllUserGroups();
			            }
			        });
			    }
			}

			vm.AddJobTitle = function () {
			    if (!vm.modalShown) {
			        vm.modalShown = true;
			        var jobInstance = $modal.open({
			            templateUrl: 'Configuration/JobTitle/edit.html',
			            controller: 'editaddjobtitleCtrl',
			            size: 'md',
			            backdrop: 'static',
			            backdropClass: 'modal-backdrop',
			            keyboard: false,
			            resolve: {
			                items: function () {
			                    return null;
			                }
			            }
			        });

			        jobInstance.result.then(function (data) {
			            vm.modalShown = false;
						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
							vm.userSelected.PositionID = data.PositionID;
			                GetAllJobTitle();
			            }
			        });
			    }
			}
            			
			$scope.convertColor = function (data) {
				return colorSvc.numtoRGB(data);
			};
            			
			$scope.$on(AppDefine.Events.FILESELECTEDCHANGE, function (event, args) {
				var file = args.file;
				angular.forEach(file, function (f) {
					if (f.type.indexOf(IMAGE_TYPE_STRING) > -1) {
						$timeout(function () {
							var fileReader = new FileReader();
							fileReader.readAsDataURL(f);
							fileReader.onload = function (image) {
								$timeout(function () {
									ChangeImageSize(image.target.result);
								$scope.$apply(function () {
									vm.userSelected.UPhoto = f.name;
								});
								});
							}
						});
					}
				});
			});

			function ChangeImageSize(image) {
				var def = $q.defer();
				createImage(image).then(resizeImage, function (data) {
					def.resolve();
					console.log('error')
				});
				return def.promise;
			}

			function createImage(src) {
				var deferred = $.Deferred();
				var img = new Image();
				img.onload = function () {
					deferred.resolve(img);
				};
				img.src = src;
				return deferred.promise();
			};

			function resizeImage(image) {
				var mainCanvas = document.createElement("canvas");
				mainCanvas.width = 100;
				mainCanvas.height = 100;
				$scope.$apply(function () {
					var ctx = mainCanvas.getContext("2d");
					ctx.drawImage(image, 0, 0, mainCanvas.width, mainCanvas.height);
					vm.picSource = mainCanvas.toDataURL(AppDefine.ImageOptions.ImageJPEG, AppDefine.ImageOptions.ImageFullQuality);
					vm.userSelected.ImageSrc = getImageDataBase64(vm.picSource);
				});
			};

			function getImageDataBase64(data) {
				var source = data.substring(AppDefine.ImageOptions.PrefixImageEmbedding.length - 1);
				if (source.indexOf(",") > -1) {
					source = source.substring(1);
				}
				return source;
			}

			vm.removeFileFn = function () {
				vm.picSource = null;
			}

			function selectedFn(node, scope) {
				scope.checkFn(node, scope.props.parentNode, scope);
			}

			/* Validator Form Data, start */
			vm.EmployeeIDValidFunc = function (employeeId) {
				if (!employeeId) { return; }
				if (!validNumber(employeeId)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.IS_NUMBER_REQUIRED);
				}
				return true;
			}

			vm.UserNameValidFunc = function (userName) {
				if (!userName) { return; }
				var reg = AppDefine.RegExp.LoginRestriction;
				if (!reg.test(userName)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.RESTRICTION_MSG);
				}
				if (userName.length < 4) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.USER_NAME_MINLENGTH_INVALID);
				}
				return true;
			}

			vm.PasswordValidFunc = function (Password) {
			    if (!Password) { return; }
                //2015-05-27 Tri fix contain special character in password
				//var reg = AppDefine.RegExp.InputRestriction;
				//if (!reg.test(Password)) {
				//	return cmsBase.translateSvc.getTranslate(AppDefine.Resx.RESTRICTION_MSG);
				//}
				if (Password.length < 6) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASSWORD_MINLENGTH_INVALID);
				}
				return true;
			}

			vm.TriggerValidPasswordConfirm = function () {
				var passwordConfirm = $scope.myForm["txtPasswordConfirm"];
				passwordConfirm.$setDirty();
			}

			$scope.$watch('vm.Password', function(data){
			    vm.PasswordConfirmValidFunc(vm.passwordConfirm);
			})

			vm.PasswordConfirmValidFunc = function (passwordConfirm) {
				if (!passwordConfirm) { return; }
				if (passwordConfirm !== vm.userSelected.Password) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.PASSWORD_MATCH_MSG);
				}
				return true;
			}

			vm.EmailValidFunc = function (email) {
				if (!email) { return; }
				var reg = AppDefine.RegExp.EmailRestriction;
				if (!reg.test(email)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.EMAIL_INVALID_MSG);
				}
				return true;
			}

			vm.PhoneValidFunc = function (phone) {
				if (!phone) { return true; }
				if (!validNumber(phone)) {
					return cmsBase.translateSvc.getTranslate(AppDefine.Resx.IS_NUMBER_REQUIRED);
				}
				return true;
			}

			function validNumber(value) {
				var reg = AppDefine.RegExp.NumberRestriction;
				return (reg.test(value));
			}

			/* Validator Form Data, end */

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			};
		}
	});
})();