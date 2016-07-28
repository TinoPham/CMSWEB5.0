(function () {
    'use strict';

    define(['cms',
        'Configuration/Sites/workinghours',
        'Configuration/GoalType/edit',
        'Configuration/Calendar/edit',
        'Configuration/Users/edit',
        'Configuration/Sites/helpers',
        'Scripts/Directives/metricComponent',
        'DataServices/Configuration/goaltypeSvc'], function (cms) {
        cms.register.controller('addsiteCtrl', addsiteCtrl);
        	addsiteCtrl.$inject = ['$scope', 'cmsBase', 'dataContext', 'AppDefine', '$modal', 'siteadminService', '$upload', 'GoalTypeSvc', '$timeout', 'colorSvc', 'AccountSvc', '$filter'];

        	function addsiteCtrl($scope, cmsBase, dataContext, AppDefine, $modal, siteadminService, $upload, goalSvc, $timeout, colorSvc, AccountSvc, $filter) {
            var vm = this;
            vm.tree = [];
            var addNew = true;
            vm.isLoadData = false;
            var items = null;
            $scope.SiteOption = {
                CheckEnable: true
            }
            vm.FixturePlan = [];
            vm.site = {
        			SiteKey: 0,
                WorkingHours: [],
                CalendarEvent: [],
                DvrMetrics: [],
                DvrUsers: [],
        			Macs: [{ Id: 0, MacAddress: '', DvrName: '', Image: '', Files: [] }],
        			Files: [],
        			HaspLicense: []
            };
        		vm.HaspLicenseModel = function () {
        			return { KDVR: 0, SerialNumber: '', ServerID: 'Virtual DVR' };
        		};
        		vm.HaspLicenseData = [];
            $scope.def = {
                Id: 'Id',
                Name: 'Name',
                Type: 'MetricType',
                Checked: 'Checked',
                ParentId: 'ParentId',
                Childs: 'Childs',
                Model: {}
            }
        		vm.defField = {
        			ActualBudget: 'ActualBudget',
        			ActualBudgetBytes: 'ActualBudgetBytes',
        			FixturePlan: 'FixturePlan',
        			FixtureBytes: 'FixtureBytes',
        			ImageSite: 'ImageSite',
        			ImageSiteBytes: 'ImageSiteBytes'
        		};
        		vm.defUpload = {
        			ActualBudgetOption: { modelName: vm.defField.ActualBudget, modelData: vm.defField.ActualBudgetBytes, maxSize: 4, maxLength: 50 },
        			ActualBudgetAccept: '.pdf',
        			FixturePlanOption: { modelName: vm.defField.FixturePlan, modelData: vm.defField.FixtureBytes, maxSize: 4, maxLength: 50 },
        			FixturePlanAccept: AppDefine.FileUploadTypes.Images + ',' + AppDefine.FileUploadTypes.Documents + ',' + AppDefine.FileUploadTypes.Audios + ',' + AppDefine.FileUploadTypes.Videos,
        			ImageSiteOption: { modelName: vm.defField.ImageSite, modelData: vm.defField.ImageSiteBytes, maxSize: 2, maxLength: 50 },
        			ImageSiteAccept: AppDefine.FileUploadTypes.Images
        		};
        		vm.siteFieldAs = {
        			ImageSiteUrl: "",
					ImageSiteAs : '',
					FixturePlanUrl: "",
					FixturePlanAs: '',
					ActualBudgetUrl: "",
					ActualBudgetAs: ''
        		};
        		vm.serialListToRender = [];
			
        		vm.ApiImageUrl = function (userId, fileName) {
        			return dataContext.user.GetUserImage(userId, fileName);
            };
            vm.isActived = false;
            vm.query = '';
            vm.userquery = '';
        		vm.countrySelected = {};
        		vm.stateSelected = {};
        		vm.goalSelected = {};
            var watchCountry = 'vm.countrySelected';
        		vm.urlForDownloadSite = AppDefine.Api.Sites + AppDefine.SiteAPI.GET_FILE;
        		vm.calenderEventsDB = [];
        		vm.CalEventSelected = [];
        		vm.filterZipCodeUrl = AppDefine.Api.Sites + AppDefine.SiteAPI.ZIP_CODE;

        		vm.filterZipCodeFormatFn = function (search){
        			return { filter: search };
        		};

        		vm.ZipCodeSelectedFn = function (value) {
        			if (value) {
        				vm.site.PostalZipCode = value.originalObject.ZipCode;
        			}
        		};

        		vm.ZipCodeChangedFn = function (value) {
        			if (value) {
        				//Validate zipcode input
        				if (!vm.RegExp.ZipCodeRestriction.test(value)) {
        					$scope.$$childHead.$$childHead.formsite.ZipcodeID.$setValidity('zipcode', false);
        					$scope.hasError = true;
        					vm.site.PostalZipCode = null;
        				}
        				else {
        					$scope.$$childHead.$$childHead.formsite.ZipcodeID.$setValidity('zipcode', true);
        					$scope.hasError = false;
        					vm.site.PostalZipCode = value;
        				}
        			}
        			else {
						//if you want to check required validate here
        				$scope.$$childHead.$$childHead.formsite.ZipcodeID.$setValidity('zipcode', true);
        				$scope.hasError = false;
        				vm.site.PostalZipCode = null;
        			}
        		};

            vm.datepickerStatus = {
                opencloseStart: { index: 0, value: false },
                openClaseEnd: { index: 1, value: false },
                remodelStart: { index: 2, value: false },
                remodelEnd: { index: 3, value: false },
                leaseStart: { index: 4, value: false },
                leaseEnd: { index: 5, value: false },
                serviceStart: { index: 6, value: false },
                serviceEnd: { index: 7, value: false },
                install: { index: 8, value: false }
            }
        		vm.status = {
        			isCloseOther: false,
        			isInfoOpen: true,
        			isAddressOpen: false,
        			isManagerOpen: false,
        			isSiteMetricOpen: false,
        			isFinacialOpen: false,
        			isSecurityOpen: false,
        			isUsersOpen: true
        		};

        		vm.RegExp = AppDefine.RegExp;

            vm.init = function (node, add) {
                active(node, add);
            }

            var callfn;

        		$scope.$on(AppDefine.Events.SAVESITE, function (e, callFn) {
                e.preventDefault();
                callfn = callFn;

                if (callfn.close) {
                    var formInScope = $scope.$$childHead.$$childHead;
                    if (formInScope && formInScope.$$nextSibling && formInScope.$$nextSibling.formsite && formInScope.$$nextSibling.formsite.$dirty === true) {
                        callfn.close(true);
        				}
        				else {
                        callfn.close(false);
                    }
        			}
        			else {
                    $timeout(function() {
                        angular.element('form').find('[type=submit]').trigger('click');
                    }, 50, false);
                }
            });

        		$scope.$on(AppDefine.Events.GENERATESERIALNUMBER, function (e, arg) {
        			if (!arg) { return; }
        			angular.forEach(arg, function (serialItem) {
        				vm.serialListToRender.push(serialItem);
        			});
        		});

            vm.validMac = function(mac) {
                var regex = /^([0-9a-fA-F]{2}[:-]?){5}([0-9a-fA-F]{2})$/;
                    return regex.test(mac);
            }

            vm.checkMacExisted = function(maclist, index) {

                //="'{{'MACADRESS_CORRECT_FIELD' | translate}}'"
                //ng-pattern = "/^([0-9a-fA-F]{2}[:-]?){5}([0-9a-fA-F]{2})$/"
                if (index === undefined || !maclist) {
                    return true;
                }

                var macForTest = angular.element('#MacAddress' + index)[0];

                if (!macForTest || !macForTest.value) {
                    return true;
                }

                if (testPattern(macForTest.value.substring(0,17)) === null) {
        				return cmsBase.translateSvc.getTranslate(AppDefine.Resx.MAC_ADDRESS_INVALID_MSG);
                }                

                if (checkDuplicateMac(maclist, index) === true) {
        				return cmsBase.translateSvc.getTranslate(AppDefine.Resx.MAC_DUPLICATE_MSG);
                } else {
                    return true;
                }
            }

            function testPattern(mac) {
                
                    var re = /^([0-9a-fA-F]{2}[:-]?){5}([0-9a-fA-F]{2})$/;
                    return re.exec(mac);
                
            }

            function checkDuplicateMac(maclist, index) {
                if (!maclist || index === undefined) return false;
                var result = false;
                var mac = maclist[index];
                var macLen = maclist.length;
                for (var i = 0; i < macLen; i++) {
                    var macitem = maclist[i];
                    if (i != index && mac && isDuplicateMac(mac, macitem)) {
                        result = true;
                        break;
                    }
                }
                return result;
            }

            function isDuplicateMac(mac1, mac2) {
                var regex = new RegExp('-', 'g');
                return (mac1.MacAddress
                        && mac2
                        && mac2.MacAddress
                        && mac2.MacAddress.replace(regex, '').toLowerCase().indexOf(mac1.MacAddress.toLowerCase()) !== -1);
            }

            //active();            
            function active(node, add) {
                //var objTranfer = cmsBase.sharingData.getObject();
                addNew = add; // objTranfer.addNew;
                items = node; // objTranfer.items;
        			
					//Load resources referrences
        			cmsBase.translateSvc.partLoad('Sites');
        			cmsBase.translateSvc.partLoad('Configuration/Users');
        			cmsBase.translateSvc.partLoad('Configuration/GoalType');

                dataContext.injectRepos(['configuration.siteadmin', 'configuration.commoninfo', 'configuration.user', 'configuration.calendar']).then(function() {
                    $timeout(function() {
        					getActiveData(); //.finally(getExtensionData);
                    }, 0, false);
                });
            }

            vm.searchUses = function (user) {
                var query = vm.userquery.replace(' ', '').toLowerCase();

                var firstname = user.FName?  user.FName.toLowerCase(): "";
                var lastname = user.LName ? user.LName.toLowerCase() : "";
                var fullname = firstname + lastname;
                if (firstname.indexOf(query) != -1
                    || lastname.indexOf(query) != -1
                    || fullname.indexOf(query) != -1
                    || user.PosName && user.PosName.toLowerCase().indexOf(query) != -1) {
                    return true;
                }
                return false;
            };

            function getActiveData() {
                if (addNew) {
                    vm.isActived = true;
        				getExtensionData();
        				return;
                }
        			else {
        				getSites();
        			}
            }

            vm.convertColor = function (data) {
                return colorSvc.numtoRGB(data);
            };

            vm.checkUser = function (user) {
                user.Checked = !user.Checked;
            }

            vm.checkCalendarEvent = function(ca) {
                ca.Checked = !ca.Checked;
                setCaEventCommbox();
            }

            function setCaEventCommbox() {
                vm.caCheckedList = '';
        			var caleng = vm.calenderEventsDB.length;
                for (var i = 0; i < caleng; i++) {
        				if (vm.calenderEventsDB[i].Checked === true) {
        					vm.caCheckedList += vm.calenderEventsDB[i].Name + '; ';
                    }
                }
            }

        		//vm.addMacMore = function () {
        		//	if (!vm.site.Macs) {
        		//		vm.site.Macs = [];
        		//	}

        		//	vm.site.Macs.push({ Id: 0, MacAddress: '', DvrName: '', Image: '', Files: [] });
        		//}

        		//vm.removeMacFn = function (mac) {
        		//	if (vm.site.Macs.length <= 1) {
        		//		return;
        		//	}
        		//	vm.site.Macs.splice(vm.site.Macs.indexOf(mac), 1);
        		//}

        		vm.removeFileFn = function (name) {
        			if (!name) { return; }

        			switch (name) {
        				case vm.defField.ActualBudget:
        					vm.siteFieldAs.ActualBudgetAs = '';
        					vm.siteFieldAs.ActualBudgetUrl = '';
        					//reset field model
        					vm.site.ActualBudget = vm.siteFieldAs.ActualBudgetAs;
        					vm.site.ActualBudgetBytes = null;
        					break;
        				case vm.defField.FixturePlan:
        					vm.siteFieldAs.FixturePlanAs = '';
        					vm.siteFieldAs.FixturePlanUrl = '';
        					//reset field model.
        					vm.site.FixtureBytes = null;
        					vm.site.FixturePlan = vm.siteFieldAs.FixturePlanAs;
        					break;
        				case vm.defField.ImageSite:
        					vm.siteFieldAs.ImageSiteAs = '';
        					vm.siteFieldAs.ImageSiteUrl = '';
        					//reset field model.
        					vm.site.ImageSiteBytes = null;
        					vm.site.ImageSite = vm.siteFieldAs.ImageSiteAs;
        					break;
        				default:
        					return;
        			}
        		}

        		function GetShortName(filename) {
        			//ex: development...angularjs.pdf
        			if (!filename) return;
        			var lenstr = filename.length;
        			if (lenstr > 30) {
        				var firtPart = filename.substr(0, 15);
        				var secondPart = filename.substr(lenstr - 10, lenstr);
        				return firtPart + '...' + secondPart;
        			}
        			return filename;
        		}

            function getSites() {
        			dataContext.siteadmin.getSite({ siteKey: items.ID }).then(
						function (data) {
                    if (data.Macs.length === 0) {
                        data.Macs.push({ Id: 0, MacAddress: '', DvrName: '', Image: '', Files: [] });
                    }
                    vm.site = data;
							//format Working Hours to local time
        					vm.site.WorkingHours = UpdateWorkingHourToLocalTime(vm.site.WorkingHours);
        				
                        vm.siteFieldAs.ImageSiteUrl = (!vm.site.ImageSite || vm.site.SiteKey === 0) ? '' : vm.urlForDownloadSite + '?skey=' + vm.site.SiteKey + '&fname=' + vm.site.ImageSite + '&fdname=' + AppDefine.SiteUploadField.imageSiteField;
        				vm.siteFieldAs.ImageSiteAs = GetShortName(vm.site.ImageSite);
        				vm.siteFieldAs.FixturePlanUrl = (!vm.site.FixturePlan || vm.site.SiteKey === 0) ? '' : vm.urlForDownloadSite + '?skey=' + vm.site.SiteKey + '&fname=' + vm.site.FixturePlan + '&fdname=' + AppDefine.SiteUploadField.fixturePlanField;
        				vm.siteFieldAs.FixturePlanAs = GetShortName(vm.site.FixturePlan);
        				vm.siteFieldAs.ActualBudgetUrl = (!vm.site.ActualBudget || vm.site.SiteKey === 0) ? '' : vm.urlForDownloadSite + '?skey=' + vm.site.SiteKey + '&fname=' + vm.site.ActualBudget + '&fdname=' + AppDefine.SiteUploadField.actualBudgetField;
        				vm.siteFieldAs.ActualBudgetAs = GetShortName(vm.site.ActualBudget);
						}
						, function (error) {
                    siteadminService.ShowError(error);
                    vm.isActived = true;
						}).then(function () {
							vm.isActived = true;
							getExtensionData();
							GetImageSite(vm.site.SiteKey);
                });
            }

            function getExtensionData() {
                vm.isLoadData = false;
                try {
                    cmsBase.$q.all([
                        getGoals(),
                        getCalendarList(),
                        getTreeMetrics(),
                        getCountries()
                    ]).then(function() {
                        vm.isLoadData = true;
                    });
                    getUserList();
	vm.getAllHaspLicense(vm.site.SiteKey);

                } catch (eror) {
                    vm.isLoadData = false;
                    vm.isActived = true;
                }
            }

            function getGoals() {
                var def = cmsBase.$q.defer();
                goalSvc.GetSimpleGoals( function ( data ) {
                    //April-06-2016 Tri Add Item Clear Goal
                    vm.goals = [];                    
                    vm.goals.push({
                        GoalID: null,
                        GoalName: cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT)
                    });
                    var goalsleng = data.length;
                    for (var i = 0; i < goalsleng; i++) {
                        if (vm.site.GoalId === data[i].GoalID) {
                            vm.goalSelected = data[i];
                        }
                        vm.goals.push(data[i]);
                    }
                    if (vm.site.GoalId === null || vm.site.GoalId === undefined) {
                        vm.goalSelected = vm.goals[0];
                    }
                    
                    def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
                    def.reject();
                });
                return def.promise;
            }

            function getCalendarList() {
        		var def = cmsBase.$q.defer();
                dataContext.calendar.GetCalendarList(function (data) {
                    $timeout(function() {
        					vm.calenderEventsDB = data;
        					var caleng = vm.calenderEventsDB.length;
                        for (var i = 0; i < caleng; i++) {
        						if (findCalendarCheck(vm.calenderEventsDB[i])) {
        							vm.calenderEventsDB[i].Checked = true;
        							vm.CalEventSelected.push(vm.calenderEventsDB[i]);
                            } else {
        							vm.calenderEventsDB[i].Checked = false;
                            }
                        }
                        setCaEventCommbox();
                    }, 10, false);
        				def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
        				def.reject();
                });
        			return def.promise;
            }

            function findCalendarCheck(ca) {
                var result = false;

                if (!vm.site || !vm.site.CalendarEvent) {
                    return;
                }

                var casize = vm.site.CalendarEvent.length;
                for (var i = 0; i < casize; i++) {
                    if (vm.site.CalendarEvent[i] === ca.ID) {
                        result = true;
                        return result;
                    }
                }
                return result;
            }

            function getCountries() {
        			var def = cmsBase.$q.defer();
                dataContext.commoninfo.getCountries(function (data) {
                    $timeout(function() {
                        vm.countries = data;
                        var coutriesleng = vm.countries.length;
                        for (var i = 0; i < coutriesleng; i++) {
                            if (vm.site.Country === vm.countries[i].Id) {
                                vm.countrySelected = vm.countries[i];
                            }
                        }
                    }, 10, false);
                    //if (vm.countrySelected && vm.countrySelected.Code) {
                    //    loadStates(vm.countrySelected.Code.trim());
                    //}
        				def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
        				def.reject();
                });
        			return def.promise;
            }

            function getTreeMetrics() {
        			var def = cmsBase.$q.defer();
                dataContext.siteadmin.getTreeMetrics(function (data) {
                    //$timeout(function() {
                        rebuildtreemetric(data);
                        vm.treeMectric = { Id: -1, Name: '', MetricType: 0, Childs: data }
                   // }, 10, false);
        				def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
        				def.reject();
                });
        			return def.promise;
            }

            function getUserList() {
                var def = cmsBase.$q.defer();
                dataContext.user.getUsers(function (data) {

                    $timeout(function() {
                    vm.user = data;
                    if ( vm.site && vm.site.DvrUsers && vm.site.DvrUsers.length > 0 ) {
                        CheckedUsers( vm.user, vm.site.DvrUsers );
                    }

                    }, 10, false);

                    def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
                    def.reject();
                });
                return def.promise;
            }

        		vm.getAllHaspLicense = function (siteKey) {
        			dataContext.siteadmin.GetAllHaspLicense({ siteKey: siteKey }).then(
						function (response) {
							if (response) {
								var resdata = Enumerable.From(response);
								var localdata = Enumerable.From(vm.site.HaspLicense);
								vm.HaspLicenseData = resdata.Except(localdata, "$.KDVR").ToArray();
								vm.serialListToRender = vm.site.HaspLicense;
							}
						});
        		}

        		function GetImageSite(siteKey) {
        			if (!siteKey) { return; }
        			dataContext.siteadmin.GetImageSite({ skey: siteKey }).then(
						function (data) {
							if (data.Data) {
								vm.site.Files = data.Data;
							}
						});
        		}

            function CheckedUsers( users, dvrusers ) {
                var aUser = Enumerable.From( users );
                var aDvrs = Enumerable.From( dvrusers );
                var matchs = aUser.Join( aDvrs, "$.UID", "$", function ( user, dvr ) { return user } );
                matchs.ForEach( function ( item ) { item.Checked = true; } );

            }

            function findUser(user) {
                var result = false;

                if (!vm.site || !vm.site.DvrUsers) {
                    return;
                }

                var usersize = vm.site.DvrUsers.length;
                for (var i = 0; i < usersize; i++) {
                    if (vm.site.DvrUsers[i] === user.UID) {
                        result = true;
                        return result;
                    }
                }
                return result;
            }

            function loadStates(countryCode) {
        			//var def = cmsBase.$q.defer();
                dataContext.commoninfo.getStates({ countryCode: countryCode }, function (data) {
                    vm.states = data;
                    var stateleng = vm.states.length;
        				vm.stateSelected = {};
                    for (var i = 0; i < stateleng; i++) {
                        if (vm.site.StateProvince === vm.states[i].Id) {
                            vm.stateSelected = vm.states[i];
                        }
                    }
        				//def.resolve();
                }, function (error) {
                    siteadminService.ShowError(error);
        				//def.reject();
                });
        			//return def.promise;
            }

            function rebuildtreemetric(treeMectric) {
                angular.forEach(treeMectric, function (nodes) {
                    refreshcheckmetric(nodes);
                    checkinglist(nodes);
                });
            }

            function refreshcheckmetric(node) {
                if (node.Childs && node.Childs.length > 0) {
                    angular.forEach(node.Childs, function (n) {
                        if (n.Childs && n.Childs.length > 0) {
                            refreshcheckmetric(n);
                        } else {
                            n.Checked = hasChecked(n);
                        }
                    });
                }
            }

            function hasChecked(metric) {
                var result = false;

                if (!vm.site || !vm.site.DvrMetrics) {
                    return result;
                }

                var metricsize = vm.site.DvrMetrics.length;
                for (var i = 0; i < metricsize; i++) {
                    //vm.site.DvrMetrics.forEach(function(m) {
                    if (vm.site.DvrMetrics[i] === metric.Id) {
                        result = true;
                        return result;
                    }
                }
                return result;
            }

            function checkinglist(node) {

        			var result = AppDefine.treeStatus.Uncheck;
                var intermid = false;
        			var interval = AppDefine.treeStatus.Uncheck;
                var i = 0;
                if (node.Childs && node.Childs.length > 0) {
                    angular.forEach(node.Childs, function (n) {
                        var ch = n.Checked;
                        if (n.Childs && n.Childs.length > 0) {
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
                } else {
                    result = node.Checked;
                }
                node.Checked = result;
                return result;
            }

            $scope.checklist = function (node) {

                if (node.Childs.length > 0)
                    return;

                var valCheck = nodechecked(node.Checked);
                node.Checked = valCheck;

                // checknode(node, valCheck);
                checknode(vm.treeMectric, node);

                angular.forEach(vm.treeMectric, function (nodes) {
                    checkinglist(nodes);
                });

            }

            function checknode(nodes, currentnode) {
                var metricsize = nodes.length;
                for (var i = 0; i < metricsize; i++) {
                    //angular.forEach(nodes, function (n) {
                    if (nodes[i].Id === currentnode.ParentId) {
                        if (nodes[i].Childs.length > 0) {
                            checkitems(nodes[i].Childs, currentnode);
                        }
                    }
                }
            }

            function checkitems(nodelist, currentnode) {
                var metricsize = nodelist.length;
                for (var i = 0; i < metricsize; i++) {
                    if (nodelist[i].Id !== currentnode.Id) {
        					nodelist[i].Checked = AppDefine.treeStatus.Uncheck;;
                    }
                }
            }

            function nodechecked(status) {
        			var result = AppDefine.treeStatus.Uncheck;
        			if (status == AppDefine.treeStatus.Uncheck || status == AppDefine.treeStatus.Indeterm) {
        				result = AppDefine.treeStatus.Checked;
                } else {
        				result = AppDefine.treeStatus.Uncheck;
                }
                return result;
            }

            vm.pickupdateFn = function ($event, itemNo) {
                $event.preventDefault();
                $event.stopPropagation();

                vm.datepickerStatus.opencloseStart.value = false;
                vm.datepickerStatus.openClaseEnd.value = false;
                vm.datepickerStatus.remodelStart.value = false;
                vm.datepickerStatus.remodelEnd.value = false;
                vm.datepickerStatus.leaseStart.value = false;
                vm.datepickerStatus.leaseEnd.value = false;
                vm.datepickerStatus.serviceStart.value = false;
                vm.datepickerStatus.serviceEnd.value = false;
                vm.datepickerStatus.install.value = false;

                switch (itemNo) {
                    case vm.datepickerStatus.opencloseStart.index:
                        vm.datepickerStatus.opencloseStart.value = true;
                        break;
                    case vm.datepickerStatus.openClaseEnd.index:
                        vm.datepickerStatus.openClaseEnd.value = true;
                        break;
                    case vm.datepickerStatus.remodelStart.index:
                        vm.datepickerStatus.remodelStart.value = true;
                        break;
                    case vm.datepickerStatus.remodelEnd.index:
                        vm.datepickerStatus.remodelEnd.value = true;
                        break;
                    case vm.datepickerStatus.leaseStart.index:
                        vm.datepickerStatus.leaseStart.value = true;
                        break;
                    case vm.datepickerStatus.leaseEnd.index:
                        vm.datepickerStatus.leaseEnd.value = true;
                        break;
                    case vm.datepickerStatus.serviceStart.index:
                        vm.datepickerStatus.serviceStart.value = true;
                        break;
                    case vm.datepickerStatus.serviceEnd.index:
                        vm.datepickerStatus.serviceEnd.value = true;
                        break;
                    case vm.datepickerStatus.install.index:
                        vm.datepickerStatus.install.value = true;
                        break;
                }
            }

            vm.dateOptions = {
                formatYear: 'yy',
                startingDay: 1,
                showWeeks: false
            };

            $scope.changestate = function () {
                $scope.status = {
                    isFirstOpen: true,
                    isSecondtOpen: true
                };

                angular.element($('input:text.ng-invalid'))[0].focus();
            }

            $scope.closestate = function () {
                cmsBase.$state.go('^', { tree: vm.tree });
            }

            $scope.modalShown = false;
            vm.addworkinghoursFn = function (wk) {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var wkInstance = $modal.open({
                        templateUrl: 'configuration/sites/workinghours.html',
                        controller: 'workinghoursCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return wk;
                            },
                            siteId: function () {
                                return vm.site.SiteId;
                            }
                        }
                    });

                    wkInstance.result.then(function (data) {
                        if (data) {
                            vm.site.WorkingHours = data;
                        }
                        $scope.modalShown = false;
                    });
                }
            }

        		function UpdateWorkingHourToUTC(workingHoursData) {
        			var WorkingHours = [];
        			angular.forEach(workingHoursData, function (workHourItem) {
        				var workHours = {
        					ScheduleId: workHourItem.ScheduleId,
        					OpenTime: $filter('date')(workHourItem.OpenTime, AppDefine.DateTimeUTC),
        					CloseTime: $filter('date')(workHourItem.CloseTime, AppDefine.DateTimeUTC),
        					SiteId: workHourItem.SiteId
        				};
        				WorkingHours.push(workHours);
        			});
        			return WorkingHours;
            }

        		function UpdateWorkingHourToLocalTime(workingHoursData) {
        			var WorkingHours = [];
        			angular.forEach(workingHoursData, function (workHourItem) {
        				var time = new Date(workHourItem.OpenTime);
        				var openTimeUTC = new Date(time.getUTCFullYear(), time.getUTCMonth(), time.getUTCDate(), time.getUTCHours(), time.getUTCMinutes());
        				time = new Date(workHourItem.CloseTime);
        				var closeTimeUTC = new Date(time.getUTCFullYear(), time.getUTCMonth(), time.getUTCDate(), time.getUTCHours(), time.getUTCMinutes());
        				var workHours = {
        					ScheduleId: workHourItem.ScheduleId,
        					OpenTime: openTimeUTC,
        					CloseTime: closeTimeUTC,
        					SiteId: workHourItem.SiteId
        				};
        				WorkingHours.push(workHours);
        			});
        			return WorkingHours;
                }

            function getMetricToSave(metric) {
                var result = [];

                if (!vm.treeMectric) {
                    return metric;
                }

                angular.forEach(vm.treeMectric.Childs, function (nodes) {
                    getChildMetricToSave(nodes, result);
                });

                return result;
            }

            function getChildMetricToSave(node, metricSave) {
                if (node.Childs && node.Childs.length > 0) {
                    angular.forEach(node.Childs, function (n) {
                        if (n.Childs && n.Childs.length > 0) {
                            getChildMetricToSave(n);
                        } else {
                            if (n.Checked === true) {
                                metricSave.push(n.Id);
                            }
                        }
                    });
                }
            }

            function getUserToSave(user) {
                var result = [];

                if (!vm.user) {
                    return user;
                }

                var userLeng = vm.user.length;
                for (var i = 0; i < userLeng; i++) {
                    if (vm.user[i].Checked === true) {
                        result.push(vm.user[i].UID);
                    }
                }

                return result;
            }

            function getLengthFileDifine(data) {
                var i = 0;
                while (data[i] !== ',') {
                    i++;
                }
                return i;
            }

        		function UpdateHaspLicenseData() {
        			var ret = [];
        			angular.forEach(vm.serialListToRender, function (item) {
        				ret.push(item);
        			});
        			return ret;
        		}

        		$scope.$on(AppDefine.Events.FILESELECTEDCHANGE, function (event, args) {
                var file = args.file;
        			var fieldName = args.modelName;
        			var fieldData = args.modelData;

        			angular.forEach(file, function (f, index) {
                    $timeout(function () {
                        var fileReader = new FileReader();
        					fileReader.readAsDataURL(f);
                        fileReader.onload = function (image) {
                            var data = image.target.result;
                            var source = data.substring(getLengthFileDifine(data));
                            if (source.indexOf(",") > -1) {
                                source = source.substring(1);
                            }

                            $scope.$apply(function() {
        							vm.site[fieldName] = f.name;
        							vm.site[fieldData] = source;
        							switch (fieldName) {
        								case vm.defField.ActualBudget:
        									vm.siteFieldAs.ActualBudgetAs = GetShortName(vm.site.ActualBudget);
        									break;
        								case vm.defField.FixturePlan:
        									vm.siteFieldAs.FixturePlanAs = GetShortName(vm.site.FixturePlan);
        									break;
        								case vm.defField.ImageSite:
        									vm.siteFieldAs.ImageSiteAs = GetShortName(vm.site.ImageSite);
        									break;
        							}
                            });
                        }
                    }, 0, false);
        			});
            });

        		vm.saveSite = function () {

        		    vm.site.HaspLicense = UpdateHaspLicenseData();
        		    if (vm.site.HaspLicense.length <= 0) {
        		    	vm.isWrongHaspKey = true;
        		    	var msg=cmsBase.translateSvc.getTranslate(AppDefine.Resx.SERIAL_REQUIRE);
        		    	if (msg != "")
        		    		cmsBase.cmsLog.error(msg);
		                return;
		            }

		            vm.site.DvrMetrics = getMetricToSave(vm.site.DvrMetrics);
                vm.site.DvrUsers = getUserToSave(vm.site.DvrUsers);
        			vm.site.WorkingHours = UpdateWorkingHourToUTC(vm.site.WorkingHours);

        			if (addNew) {
        				if (items.Type === AppDefine.NodeType.Region) {
        					vm.site.RegionKey = items.ID;
        				}
        				else {
        					vm.site.RegionKey = items.ParentKey;
        				}
        				dataContext.siteadmin.addSite(vm.site).then(
							function (data) {
        						if (data.ReturnStatus) {
        					var msg = "";
        							if (callfn) callfn.callback(data.Data);
        					callfn = null;
        							msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
        					cmsBase.cmsLog.success(msg);
        						}
        						else {
        							angular.forEach(data.ReturnMessage, function (msgKey) {
        								if (msgKey == AppDefine.Resx.SITE_NAME_EXIST) {
        									msg = formatString(cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]), vm.site.ServerId);
        									cmsBase.cmsLog.warning(msg);
        								}
										else if (msgKey == AppDefine.Resx.HASP_LICENSE_EXIST_MSG) {
											var hasps = [];
											angular.forEach(data.Data.HaspLicense, function (hasp) { hasps.push(hasp.SerialNumber); });
											msg = formatString(cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]), hasps.toString());
        									cmsBase.cmsLog.warning(msg);
        								}
        								else {
        									msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
        									cmsBase.cmsLog.warning(msg);
        								}
        							});
        						}
        				},
						function (error) {
								msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.ADD_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
                    });
        			}
        			else {
        				dataContext.siteadmin.editSite(vm.site).then(
							function (data) {
        						if (data.ReturnStatus) {
        					var msg = "";
        					if (callfn) callfn.callback(vm.site);
        					callfn = null;
        							msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
        					cmsBase.cmsLog.success(msg);
        						}
        						else {
        							angular.forEach(data.ReturnMessage, function (msgKey) {
        								if (msgKey == AppDefine.Resx.SITE_NAME_EXIST) {
        									msg = formatString(cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]), vm.site.ServerId);
        									cmsBase.cmsLog.warning(msg);
        								}
										else if (msgKey == AppDefine.Resx.HASP_LICENSE_EXIST_MSG) {
											var hasps = [];
											angular.forEach(data.Data.HaspLicense, function (hasp) { hasps.push(hasp.SerialNumber); });
											msg = formatString(cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]), hasps.toString());
        									cmsBase.cmsLog.warning(msg);
        								}
        								else {
        									msg = cmsBase.translateSvc.getTranslate(data.ReturnMessage[0]);
        									cmsBase.cmsLog.warning(msg);
        								}
        							});
        						}
        				},
						function (error) {
								msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.EDIT_FAIL_MSG);
								cmsBase.cmsLog.error(msg);
        				});
        			}
            }

            vm.validationDatePickerMsg = function(sDate, eDate) {
                if (!sDate) {
        				return cmsBase.translateSvc.getTranslate(AppDefine.Resx.END_DATE_GREATER_THAN_START_DATE);
                }

                if (eDate <= sDate) {
        				return cmsBase.translateSvc.getTranslate(AppDefine.Resx.END_DATE_GREATER_THAN_START_DATE);
                }
        			return cmsBase.translateSvc.getTranslate(AppDefine.Resx.END_DATE_GREATER_THAN_START_DATE);
            }

            vm.validationDatePicker = function (sDateSite, eDateSite) {
                var result = false;
                var sDate = sDateSite === null ? 0 : Date.parse(sDateSite);
                var eDate = eDateSite === null ? 0 : Date.parse(eDateSite);

                if (!sDate && !eDate) {
                    return true;
                }

                if (!sDate || !eDate) {
                    return false;
                }

                if (eDate > sDate) {
                    return true;
                }
                return result;
            }

            vm.refeshCalendarEvent = function () {
                getCalendarList();
            }

            vm.refeshGoads = function () {
                getGoals();
            }

            vm.refeshCountries = function () {
                getCountries();
            }

            vm.refeshMetric = function (e) {
                e.stopPropagation();
                //e.preventDefault();
                getTreeMetrics();
            }

            vm.refeshUsers = function (e) {
                e.stopPropagation();
                getUserList();
            }

            vm.refeshStates = function () {
                if (vm.countrySelected) {
                    loadStates(vm.countrySelected.Code);
                }
            }

            vm.dropdownOpen = false;
            vm.caCheckedList = '';

            vm.selecttoggle = function(elem) {
                if (vm.dropdownOpen === true) {
                    vm.dropdownOpen = !vm.dropdownOpen;
                }
            }

            vm.openUsers = function(user) {
                showUserDialog(user);
            }

            function showUserDialog(user) {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'configuration/users/edit.html',
                        controller: 'editadduserCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return user;
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                        $scope.modalShown = false;
        					if (data != AppDefine.ModalConfirmResponse.CLOSE) {
                            getUserList();
                        }
                    });
                }
            }

            vm.addGoad = function() {
                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    try {
                        var jobInstance = $modal.open({
                            templateUrl: 'Configuration/GoalType/edit.html',
                            controller: 'editaddgoaltypeCtrl as vm',
                            size: 'lg',
                            backdrop: 'static',
                            backdropClass: 'modal-backdrop',
                            keyboard: false,
                            resolve: {
                                items: function () {
                                    return undefined;
                                }
                            }
                        });

                        jobInstance.result.then(function (data) {
                            $scope.modalShown = false;
        						if (data != AppDefine.ModalConfirmResponse.CLOSE) {
                                vm.site.GoalId = data.GoalID;
                                getGoals();
                            }
                        });
                    } finally {
                        $scope.modalShown = false;
                    }
                }
            }

            vm.addCalendarEvent = function () {
                var msg = "Unsupport this function now.";
                cmsBase.cmsLog.warning(msg);
                return;

        			//if (vm.dropdownOpen === true) {
        			//	vm.dropdownOpen = !vm.dropdownOpen;
        			//}

        			//if (!$scope.modalShown) {
        			//	$scope.modalShown = true;
        			//	try {

        			//		cmsBase.translateSvc.partLoad('configuration/calendar');

        			//		var calInstance = $modal.open({
        			//			templateUrl: 'configuration/Calendar/edit.html',
        			//			controller: 'CalendarEditAddCtrl as vm',
        			//			size: 'lg',
        			//			backdrop: 'static',
        			//			backdropClass: 'modal-backdrop',
        			//			keyboard: false,
        			//			resolve: {
        			//				items: function () {
        			//					return null;
        			//				}
        			//			}
        			//		});

        			//		calInstance.result.then(function (data) {
        			//			if (!data) {
        			//				getCalendarList();
        			//			}
        			//			$scope.modalShown = false;
        			//		});
        			//	} finally {
        			//		$scope.modalShown = false;
        			//	}
        			//}
            }

        		vm.openUrl = function (url) {
        			if (!url) { return; }
        			window.open(url, "_blank");
        		}

        		vm.selectMetricFn = function (node, scope) {
        			scope.checkFn(node, scope.props.parentNode, scope);
        		}

        		//Thang Pham, function combobbox Goals changed, Aug 07 2015
        		vm.ddlGoalsChanged = function () {
        			if (!$.isEmptyObject(vm.goalSelected)) {
        				vm.site.GoalId = vm.goalSelected.GoalID;
        			}
        		}

        		//Thang Pham, function combobbox Calendar Events changed, Aug 07 2015
        		vm.ddlCalEventChanged = function () {
        			if (!$.isEmptyObject(vm.CalEventSelected)) {
        			    vm.site.CalendarEvent = [];
        			    angular.forEach(vm.CalEventSelected, function (value, key) {
        			        vm.site.CalendarEvent.push(value.ID);
        			    });
        			}
        		}

        		//Thang Pham, function combobbox Countries changed, Aug 07 2015
        		vm.ddlCountriesChanged = function () {
        			if (!$.isEmptyObject(vm.countrySelected)) {
        			vm.site.Country = vm.countrySelected.Id;
        			if (vm.countrySelected.Code) {
        				loadStates(vm.countrySelected.Code);
        			}
        		}
        		}

        		//Thang Pham, function combobbox States changed, Aug 07 2015
        		vm.ddlStatesChanged = function () {
        			if (!$.isEmptyObject(vm.stateSelected)) {
        			vm.site.StateProvince = vm.stateSelected.Id;
        		}
        }

        		//vm.GenerateMACAddressLabel = function (index) {
        		//	var infixString = "th";
        		//	if (index == 1 || index == 21 || index == 31) {
        		//		infixString = "st";
        		//	}
        		//	else if (index == 2 || index == 22) {
        		//		infixString = "nd";
        		//	}
        		//	else if (index == 3 || index == 23) {
        		//		infixString = "rd";
        		//	}
        		//	else {
        		//		infixString = "th";
        		//	}

        		//	return formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.MAC_ADDRESS), index, infixString);
        		//}

        		vm.removeSerial = function (data) {
        			if (data) {
        				vm.serialListToRender.splice(vm.serialListToRender.indexOf(data), 1);
        				var serial = angular.copy(data);
        				//serial.checked = false;
        				$scope.$broadcast(AppDefine.Events.REMOVESERIALNUMBER, serial);
        			}
        		}

        		function formatString(format) {
        			var args = Array.prototype.slice.call(arguments, 1);
        			return format.replace(/{(\d+)}/g, function (match, number) {
        				return typeof args[number] != 'undefined' ? args[number] : match;
        			});
        		};
        	}
    });
})();