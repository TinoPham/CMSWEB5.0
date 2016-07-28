/// <reference path="mapupload.html" />
(function () {
    'use strict';

    define(['cms',
        'DataServices/Sites/maps.service',
        'Widgets/Sites/mapupload',
		'Services/dialogService'
       ],
        function (cms) {
            cms.register.controller('mapsCtrl', mapsCtrl);
        	mapsCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', '$timeout', 'maps.service', '$modal', '$upload', '$filter', 'dialogSvc','$document'];
       
        	function mapsCtrl($scope, dataContext, cmsBase, AppDefine, $timeout, mapsservice, $modal, $upload, $filter, dialogSvc, $document) {
            var apiURL = "../api/cmsweb/maps/images?sitekey=";
            var sitedata = dataContext.sitedata;
            var isActive = false;
            $scope.AllSites = AllSites();
        
           //  $scope.$watch('mapmodels.mapImage', function (newvalue, oldvalue) {
        			// if (newvalue == undefined) { return; }
        			// else {
        			// 	if (newvalue.length > 0) {
        			// 		if (oldvalue == undefined || oldvalue == null) {
           //                  $timeout(function () {
           //                      setImagesize();
           //                  }, 300);
           //              }
        			// 		else {
        			// 			if (oldvalue.length == 0) {
           //                      $timeout(function () {
           //                          setImagesize();
           //                      }, 300);
           //                  }
           //              }
           //          }
           //      }
           //  });

        
            $scope.viewDef = {
                ROOT: 0,
                REGION: 1,
                DISTRICT: 2,
                SITE: 3
        		};
            $scope.chanels = [];
            $scope.open = (function () {
        		});

            $scope.iscleared = false;
            $scope.clearlabel = function () {
        			if ($scope.iscleared == false) { return AppDefine.Resx.BTN_CLEAR; }
        			else { return AppDefine.Resx.BTN_UNDO; }
        		};

            $scope.clear = (function () {
                if ($scope.iscleared == false) {
                    $scope.imagemodels.Channels = [];
                    $scope.iscleared = true;
                    $scope.$applyAsync();
                }
                else {
                    for (var i = 0; i < $scope.mapmodels.mapImage.length; i++) {
                        if ($scope.mapmodels.mapImage[i].ImageID == $scope.imagemodels.ImageID) {
                            $scope.$emit(AppDefine.Events.SITE_TAPS.CHANNEL_LOAD, i);
                            $scope.iscleared = false;
                            $scope.$applyAsync();
                            return;
                        }
                    }
                }
            });

            $scope.mapmodels = {};
            $scope.init = function (model) {
                $scope.provider = model;
                $scope.viewS = 1;
                $scope.GetMaps();
                if ($scope.activealert == false) {
                    isActive = true;
                }
           //         $timeout(function () {
           //           setImagesize();
        			// }, 300);
            };

            $scope.$on(AppDefine.Events.SITE_TAPS.SELECT_MAPS, function (event,model) {
                isActive = true;
                $scope.selectMaps(model);
            });

            $scope.$on(AppDefine.Events.SITE_TAPS.DES_SELECT_MAPS, function () {
                isActive = false;
            });

            $scope.selectMaps = function (model) {
                if ($scope.oldID != model.ID) {
                $scope.GetMaps();
                    $scope.oldID = model.ID;
                }
        		};

            $scope.$on(AppDefine.Events.SITE_TAPS.SHOW_HIDE_TREE, function (e) {
                $scope.onresize(e);
            });

            $scope.$on(AppDefine.Events.SITE_TAPS.CHANGE_NODE_TREE, function (e, agr) {
                if (agr.Type == 1) {
                    $scope.imagemodels = {};
                    $scope.viewS = 1;
                    if ($scope.oldID != agr.ID) {
                        $scope.provider = agr;
                    }

        				if (isActive) {
                        $scope.GetMaps();
                        $scope.oldID = agr.ID;
                    }
                }
            });

            $scope.$on(AppDefine.Events.SITE_TAPS.UPLOAD_COMPLETE, function (e, agr) {
                $scope.mapmodels.siteKey = $scope.provider.ID;
                angular.forEach($scope.mapmodels.mapImage, function (value, key) {
                    if (value.ImageID == $scope.imagemodels.ImageID) {
                        //angular.copy($scope.imagemodels.Channels, $scope.mapmodels.mapImage[key].Channels);
        					$scope.mapmodels.mapImage[key].Channels = $scope.imagemodels.Channels;
                        $scope.mapmodels.mapImage[key].ImageURL = $scope.imagemodels.ImageURL;
                    }
                });
                var cloneobject = {};
                angular.copy($scope.mapmodels, cloneobject);
                angular.forEach(cloneobject.mapImage, function (value, key) {
        				if (cloneobject.mapImage[key].ImageID <= 0) {
                        //cloneobject.mapImage[key].Caption = new Date().getTime() + "_" + cloneobject.mapImage[key].Caption;
                    }
                    cloneobject.mapImage[key].ImageURL = "";
                    cloneobject.mapImage[key].ImageByte = [];
                });
                mapsservice.create().SaveMaps(cloneobject, $scope.SaveMapsComplete, $scope.SaveMapsError);
            });

            $scope.iconClass = function (type) {
                var icon = AppDefine.SITE_TAPS.iconList[type].icon;
                return icon;
            };
            $scope.MapsModel = {
                siteKey: 0,
                mapImage: []
            };
            $scope.MapsImage = {
                ImageID: "",
                ImageURL: "",
                ImageByte: [],
                Channels: []
        		};
            $scope.ChannelsPosition = {
                ChannelID: "",
                Leftpoint: "",
                Toppoint: ""
        		};
        		$scope.removeChannel = function (event) { };
            var per = 0;
        		$scope.resultupload = 0;
            var objectupload = {
                id:0,
                complete: 0,
                prevloaded:0
            };
            var uploadArray = [];
            var _files = [];
            //function generateUUID() {
            //    var d = new Date().getTime();
            //    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            //        var r = (d + Math.random()*16)%16 | 0;
            //        d = Math.floor(d/16);
            //        return (c=='x' ? r : (r&0x3|0x8)).toString(16);
            //    });
            //    return uuid;
            //};
            var popup_flag = false;
            function upload() {
                for (var key = 0; key < $scope.mapmodels.mapImage.length; key++) {
                    if ($scope.mapmodels.mapImage[key].ImageByte == undefined || $scope.mapmodels.mapImage[key].ImageByte == null || $scope.mapmodels.mapImage[key].ImageByte == "") continue;
                    var config = {
                        url: AppDefine.Api.Maps + '/upload?sitekey=' + $scope.provider.ID + '&id=' + $scope.mapmodels.mapImage[key].ImageID,
                       headers: { 'Content-Type': 'multipart/form-data' },
                       data: $scope.mapmodels.mapImage[key].ImageID,
                       file: $scope.mapmodels.mapImage[key].ImageByte
        				};

                    $upload.upload(config)
                    .progress((function (i) {
                        return function (e) {
                                if (uploadArray.length == 0) {
                                    objectupload = {};
                                    objectupload.complete = e.loaded;
                                    objectupload.id = i;
                                    objectupload.prevloaded = e.loaded;
									uploadArray.push(objectupload);
                                }
                                else {
                                    var flag = false;
                                    for (var j = 0; j < uploadArray.length; j++) {
                                        var value = uploadArray[j];
                                        if (i == value.id) {
                                            uploadArray[j].complete = objectupload.complete + e.loaded - objectupload.prevloaded;
                                            uploadArray[j].id = i;
                                            uploadArray[j].prevloaded = e.loaded;
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (flag == false) {
                                        objectupload = {};
                                        objectupload.complete = e.loaded;
                                        objectupload.id = i;
                                        objectupload.prevloaded = e.loaded;
										uploadArray.push(objectupload);
                                    }
                                }
                                var summarize = 0;
                                for (var j = 0; j < uploadArray.length; j++) {
                                    var value = uploadArray[j];
                                    summarize = summarize + value.complete;
                                }
                            var per = summarize / $scope.AllFileSize;
                                $scope.progressbar = per * 100;
							};
                    })(key)).success( function (data, status, headers, config) {
                        $scope.$emit(AppDefine.Events.SITE_TAPS.UPDATE_MODEL, data);
                    });
                            }
        			//var ep = document.getElementById('fUpload');
        			//ep.value = '';
                        }

            $scope.$on(AppDefine.Events.SITE_TAPS.UPDATE_MODEL, function (evt, data) {
                for (var key = 0 ; key < $scope.mapmodels.mapImage.length; key++) {
                    if ($scope.mapmodels.mapImage[key].ImageID == data.Data.ImageID) {
                        $scope.mapmodels.mapImage[key].Caption = data.Data.ImageURL;
                        $scope.mapmodels.mapImage[key].ImageByte = undefined;
                        $scope.mapmodels.mapImage[key].Flag = 1;
                    }
                }
                var iscomplete = Enumerable.From($scope.mapmodels.mapImage).Where(function (x) { return (x.ImageID < 0 && (x.Flag == 0 || x.Flag == undefined)) }).ToArray().length > 0;
        			if (iscomplete == false) {
        			    $scope.$emit(AppDefine.Events.SITE_TAPS.UPLOAD_COMPLETE);
            }
            });

            $scope.caculatepercent = function (number) {
        			return parseInt(number);
        		};
           
            $scope.AllFileSize = 0;
            $scope.submit = function () {
                $scope.AllFileSize = 0;
                per = 0;
                uploadArray = [];
                popup_flag = false;
                angular.forEach($scope.mapmodels.mapImage, function (value, key) {
                    if (value.ImageByte != undefined && value.ImageByte != null && value.ImageByte != "") 
                        $scope.AllFileSize = $scope.AllFileSize + value.ImageByte.size;
                });

                if ($scope.AllFileSize == 0) { $scope.$emit(AppDefine.Events.SITE_TAPS.UPLOAD_COMPLETE); return; }
                upload();
            };

            $scope.SaveMapsComplete = function (event) {
                $scope.GetMapSuccess(event);
                $scope.viewS = 1;
        			cmsBase.cmsLog.success($filter('translate')(AppDefine.Resx.SAVE_MAP_IMAGE_SUCCESS_MSG));
        		};

            $scope.SaveMapsError = function (event) {
              //  alert('Saving Maps Error!')
        		};

            var startX = 0,
				startY = 0,
				x = 0,
				y = 0;

            function percentPosition(parentEle, obj) {
                var _obj = {};
                _obj.Toppoint = (obj.top / parentEle.clientHeight) * 100;
                _obj.Leftpoint = (obj.left / parentEle.clientWidth) * 100;
                return _obj;
            }

            $scope.CheckExistsChannel = function (channelid) {
                var check = Enumerable.From($scope.imagemodels.Channels)
					.Where(function (ch) {
							return ch.ChannelID == channelid;
					}).ToArray();

                if (check.length > 0) return false;
                check = Enumerable.From($scope.mapmodels.mapImage).Where(function (imag) {
                    return (Enumerable.From(imag.Channels).Where(function (ch) {
                        return ch.ChannelID == channelid && $scope.imagemodels.ImageID != imag.ImageID;
                    }).ToArray().length) > 0;
                }).ToArray();
                if (check.length > 0) return false;
                return true;
            };

            $scope.xChannel = function (channelID, $event) {
                angular.forEach($scope.imagemodels.Channels, function (velue, key) {
                    if (velue.ChannelID == channelID) $scope.imagemodels.Channels.splice(key, 1);
                    return;
                });
            };
               


            $scope.textWidth =  function getTextWidth(text, font) {
                // re-use canvas object for better performance
                var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
                var context = canvas.getContext("2d");
                context.font = font;
                var metrics = context.measureText(text);
                return metrics.width;
            };

        		$scope.handleDrop = function (e) {
                if ($scope.viewS == 3) {
                    var data = {};
                    var format = "text/html";
                    try {
                        data = e.dataTransfer.getData(format);
        				}
        				catch (ex) {
                        data = e.dataTransfer.getData("text");
                    }

                    var obj = JSON.parse(data);
                    if (obj.Type == 3) {
                        if ($scope.CheckExistsChannel(obj.ID) == false) {
                            cmsBase.cmsLog.warning($filter('translate')(AppDefine.Resx.CHANNEL_HAS_BEEN_USED));
                            return;
                        }
                        var siteID = getSiteID(obj.ParentKey);
                        if (siteID != $scope.provider.ID) { cmsBase.cmsLog.warning($filter('translate')(AppDefine.Resx.CANNOT_DROP)); return };
                        var parent = document.getElementById("c_maps");
                        obj.top = e.offsetY - parent.clientHeight + 40;
                        obj.left = e.offsetX - 40;
                        obj.parentWidth = parent.clientWidth;
                        obj.parentHeight = parent.clientHeight;
                        var _obj = percentPosition(parent, obj);
                        obj.Leftpoint = _obj.Leftpoint;
                        obj.Toppoint = _obj.Toppoint;
                        obj.Name = obj.Name;
                      
                        // x = 0; y = 0;
                        var img_maps = {
                            ChannelID: obj.ID,
                            Leftpoint: obj.Leftpoint,
                            Toppoint: obj.Toppoint,
                            ChannelName: obj.Name,
                            Status: obj.Status,

                        };
                        $scope.iscleared = false;
                        if ($scope.imagemodels.Channels == undefined || $scope.imagemodels.Channels == null) $scope.imagemodels.Channels = [];
                        $scope.imagemodels.Channels.push(img_maps);
                    }
                }
                $scope.$applyAsync();
        		};

            $scope.iconChannelStatusClass = function (node) {
                if (node.Status != null) {
                    var stt = AppDefine.CHANNEL_STATUS[node.Status].icon;
                    return stt == undefined || stt == null || stt == "" ? AppDefine.CHANNEL_STATUS[5].icon : stt;
                }
                return AppDefine.CHANNEL_STATUS[5].icon;
        		};

            $scope.handleDragOver = function (e) {
                e.preventDefault();
        		};

            $scope.onmove = function () {
                $scope.$applyAsync();
            };

            $scope.onUp = function (event, obj) {
                $scope.showtrashbox = false;
                var parent = document.getElementById("c_maps");
                for (var i = 0; i < $scope.imagemodels.Channels.length; i++) {
                    if ($scope.imagemodels.Channels[i].ChannelID == obj.id) {
                        var _obj = percentPosition(parent, obj);
                        $scope.imagemodels.Channels[i].Leftpoint = _obj.Leftpoint;
        					$scope.imagemodels.Channels[i].Toppoint = _obj.Toppoint;
                        break;
                    }
                }
                $scope.$applyAsync();
              //  console.log("ID:" + obj.id + ", X:" + obj.left + ", Y:" + obj.top)
        		};

            $scope.onresize = (function (e) {
                if ($scope.viewS == 3) {
                    $timeout(function () {
                        var clone = {};
                        angular.copy($scope.imagemodels, clone);
        					$scope.imagemodels = {};
                        $scope.$applyAsync();
                        $scope.imagemodels = clone;
                        $scope.$applyAsync();
                    }, 1010);
                }
            });

            var id = 0;
            function getLengthFileDifine(data) {
                var i = 0;
                while (data[i] !== ',') {
                    i++;
                }
                return i;
            }

            $scope.Cancel = function (view, action) {
                $scope.iscleared = false;
                switch (view) {
                    case 3:
                        $scope.viewS = 1;
                        //$scope.images = [];
                        $scope.chanels = [];
                        break;
                    default:
                        $scope.viewS = 3;
                }
                angular.forEach($scope.mapmodels.mapImage, function (value, key) {
                    if (value.ImageID == $scope.imagemodels.ImageID) {
                        if (action == AppDefine.Events.SITE_TAPS.BACK_ACTION) {
                         //   angular.copy($scope.imagemodels, $scope.mapmodels.mapImage[key]);
                         
                            $scope.mapmodels.mapImage[key].ImageURL = $scope.imagemodels.ImageURL;
                            $scope.mapmodels.mapImage[key].ImageByte = $scope.imagemodels.ImageByte;
                            angular.copy({}, $scope.imagemodels);
                            return;
                        }
                        $scope.imagemodels = {};
                    }
                });
        		};

            $scope._tempid = -1;
            $scope.header = function () {
                if ($scope.imagemodels.Title) {
                    return $scope.provider.Name + " / " + $scope.imagemodels.Title;
                }
                else {
                    if($scope.provider)
        					return $scope.provider.Name;
                    else {
        					return "";
                    } 
                };
        		};

            $scope.$on(AppDefine.Events.SITE_TAPS.FILESELECTEDCHANGE, function (event, agr) {
                var file = agr.files;
                if (file) {
                    for (var i = 0; i < file.length; i++) {
                        var reader = new FileReader();
                        reader.onload = (function (f) {
                            return (function (e) {
                                var data = e.target.result;
                                if ($scope.mapmodels.mapImage == undefined) $scope.mapmodels.mapImage = [];
                                $scope.mapmodels.mapImage.push({
                                    ImageID: $scope._tempid--,
                                    ImageURL: e.currentTarget.result,
                                    ImageByte: f,
                                    Channels: [],
                                    Caption: f.name,
                                    Title: f.name
                                });
                                $scope.$applyAsync();

                                //upload image
                                uploadFromDialog($scope);
                            })
                        })(file[i]);

                        reader.readAsDataURL(file[i]);
                    }
                } else {
                    //do somthing
                }
                var invalid_files = agr.invalid_files;
        			if (invalid_files) {
                    if (invalid_files.length > 0) {
                        var str_valid = Enumerable.From(invalid_files).Select(function (in_f) { return in_f.name }).ToArray();
                        invalid_files = null;
        					cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.OVER_SIZE_FILE), str_valid));
                    }
                }

            });

        	// function upload image when choose from dialog
            function uploadFromDialog(scope) {
                
                var key = $scope.mapmodels.mapImage.length - 1;
                if ($scope.mapmodels.mapImage[key].ImageByte == undefined || $scope.mapmodels.mapImage[key].ImageByte == null || $scope.mapmodels.mapImage[key].ImageByte == "") return;
                var config = {
                    url: AppDefine.Api.Maps + '/uploadfromdialog?sitekey=' + $scope.provider.ID + '&id=' + $scope.mapmodels.mapImage[key].ImageID,
                    headers: { 'Content-Type': 'multipart/form-data' },
                    data: $scope.mapmodels.mapImage[key].ImageID,
                    file: $scope.mapmodels.mapImage[key].ImageByte
                };

                uploadArray = [];

                $upload.upload(config)
                .progress((function (i) {
                    return function (e) {
                        if (uploadArray.length == 0) {
                            objectupload = {};
                            objectupload.complete = e.loaded;
                            objectupload.id = i;
                            objectupload.prevloaded = e.loaded;
                            uploadArray.push(objectupload);
                        }
                        else {
                            var flag = false;
                            for (var j = 0; j < uploadArray.length; j++) {
                                var value = uploadArray[j];
                                if (i == value.id) {
                                    uploadArray[j].complete = objectupload.complete + e.loaded - objectupload.prevloaded;
                                    uploadArray[j].id = i;
                                    uploadArray[j].prevloaded = e.loaded;
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false) {
                                objectupload = {};
                                objectupload.complete = e.loaded;
                                objectupload.id = i;
                                objectupload.prevloaded = e.loaded;
                                uploadArray.push(objectupload);
                            }
                        }
                        var summarize = 0;
                        for (var j = 0; j < uploadArray.length; j++) {
                            var value = uploadArray[j];
                            summarize = summarize + value.complete;
                        }
                        var per = summarize / $scope.AllFileSize;
                        $scope.progressbar = per * 100;
                    };
                })(key)).success(function (data, status, headers, config) {
                    $scope.$emit(AppDefine.Events.SITE_TAPS.UPDATEFROMDIALOG_MODEL, data);
                });
            }

            $scope.$on(AppDefine.Events.SITE_TAPS.UPDATEFROMDIALOG_MODEL, function (evt, data) {
               
                var cloneobject = {};
                var current = undefined;
                angular.copy($scope.mapmodels, cloneobject);
                cloneobject.mapImage = [];
                
                for (var key = 0 ; key < $scope.mapmodels.mapImage.length; key++) {
                    if ($scope.mapmodels.mapImage[key].ImageID == data.Data.ImageID) {
                        $scope.mapmodels.mapImage[key].Caption = data.Data.ImageURL;
                        $scope.mapmodels.mapImage[key].ImageByte = undefined;
                        $scope.mapmodels.mapImage[key].Flag = 1;
                        cloneobject.mapImage.push($scope.mapmodels.mapImage[key]);
                        cloneobject.mapImage[0].ImageByte = [];
                        current = key;
                    }
                }
                $scope.$emit(AppDefine.Events.SITE_TAPS.UPLOADFROMDIALOG_COMPLETE, cloneobject, current);
            });

            $scope.$on(AppDefine.Events.SITE_TAPS.UPLOADFROMDIALOG_COMPLETE, function (e, cloneobject, current) {
                mapsservice.create().InsertModelFromDialog(cloneobject, $scope.UploadImageSuccess, current);
            });

            $scope.$on(AppDefine.Events.SITE_TAPS.DELETEFROMBUTONX_COMPLETE, function (e, cloneobject) {
                mapsservice.create().DeleteModelFromButtonX(cloneobject);
            });
                
            $scope.mapmodels = {};
            $scope.imagemodels = {};
            $scope.ChannelsTreeList = [];
            $scope.imageURL = (function (filename, thumbnail) {
                return apiURL + $scope.provider.ID + AppDefine.SITE_TAPS.filename_p + filename + AppDefine.SITE_TAPS.thumbnail_p + thumbnail;
            });

            $scope.UploadImageSuccess = function (data, current) {
                if (data) {
                    if (data.mapImage.length == 0) { return; }
                    if (current === undefined) { return; }
                    $scope.mapmodels.mapImage[current].ImageID = data.mapImage[data.mapImage.length - 1].ImageID;
                }
            };

            $scope.GetMapSuccess = function (data) {
        		$scope.mapmodels = data;
                $scope.$applyAsync();
                angular.forEach($scope.provider.Sites, function (value, i) {
                    $scope.ChannelsTreeList = $scope.ChannelsTreeList.concat($scope.provider.Sites[i].Sites);
                });
                if (data) {
        				if (data.mapImage.length == 0) { return; }
                    for (var i = 0; i < $scope.mapmodels.mapImage.length; i++) {
                        //  $scope.GetImages(data.mapImage[i], true);
                        $scope.mapmodels.mapImage[i].ImageURL = $scope.imageURL($scope.mapmodels.mapImage[i].Caption, true);
                    }
                }
            };

            $scope.GetMapFail = function (data) {
              
                console.log('Get Map Fail');
        		};

            $scope.GetMaps = function () {
                if ($scope.provider) {
                var sitekey = $scope.provider.ID;
                mapsservice.create().GetMaps(sitekey, $scope.GetMapSuccess, $scope.GetMapFail);
            }
        		};

            $scope.GetImages = function (data, thumnail) {
                var sitekey = $scope.provider.ID;
                mapsservice.create().GetImages(sitekey, data.Caption, data, thumnail, $scope.GetImagesSuccess, $scope.GetMapFail);    
        		};

            $scope.Globaldata = {};
            $scope.GetImagesSuccess = function (data, rootdata, thumbnail) {        
                if (thumbnail == true) {
                  
                    var obj = {};
                    for (var i = 0; i < $scope.mapmodels.mapImage.length; i++) {
                        if ($scope.mapmodels.mapImage[i].Caption == data.Name) {
        						$scope.mapmodels.mapImage[i].ImageURL = ''; //'data:image/jpeg;base64,' + data.Data;
                            $scope.$applyAsync();
                            break;
                        }
                    }
        			}
        			else {
                    for (var i = 0; i < $scope.mapmodels.mapImage.length; i++) {
                        if ($scope.mapmodels.mapImage[i].Caption == data.Name) {
                            $scope.mapmodels.mapImage[i].Flag = 1;
        						$scope.mapmodels.mapImage[i].ImageURL = ''; //'data:image//jpeg;base64,' + data.Data;
                            angular.copy($scope.mapmodels.mapImage[i], $scope.imagemodels);
                            $scope.$applyAsync();
                            break;
                        }
                    }
                }
        		};

        		$scope.xClick = function (model, e) {
        			RemoveMapImageConfirm(model);
        		};

        		function RemoveMapImageConfirm(model) {
        			var modalOptions = {
        				headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
        				bodyText: AppDefine.Resx.REMOVE_MAP_IMAGE_CONFIRM
        			};
        			var modalDefaults = {
        				backdrop: true,
        				keyboard: true,
        				modalFade: true,
        				templateUrl: 'Widgets/DeleteDialog.html',
        				size: 'sm'
                }

        			dialogSvc.showModal(modalDefaults, modalOptions).then(function (result) {
        				if (result === AppDefine.ModalConfirmResponse.OK) {
        					RemoveMapImage(model);
        				}
        			});
        		}

        		function RemoveMapImage(model) {
                angular.forEach($scope.mapmodels.mapImage, function (value, key) {
                    if (model.ImageID == value.ImageID) {
                        $scope.mapmodels.mapImage.splice(key, 1);
                       // return;
                    }
                });
                angular.forEach(_files, function (value, key) {
                    if (model.Caption == value.name) {
                        _files.splice(key, 1);
                        return;
                    }
                });

        		    // Add model delete
                var cloneobject = {};
                angular.copy($scope.mapmodels, cloneobject);
                cloneobject.mapImage = [];
                cloneobject.mapImage.push(model);
                $scope.$emit(AppDefine.Events.SITE_TAPS.DELETEFROMBUTONX_COMPLETE, cloneobject);
                
                //  return model;
            }

            $scope.images = [];
            $scope.provider = null;
            $scope.viewS = 0;
            $scope.dropover = function (e) {
                console.log(e);
        		};

            $scope.Oclick = function (elem, event) {
                if (elem.Type == 2) {
                    $scope.viewS = 1;
                } else {
        				$scope.viewS = 0;
                }

                $scope.parentNode = elem.Name;
                $scope.provider = elem;
                $scope.$applyAsync();
            };

            $scope.selectedImgURL = "";
            $scope.$on(AppDefine.Events.SITE_TAPS.CHANNEL_LOAD, (function (event, ele) {
                $timeout(function () {
                    angular.copy($scope.mapmodels.mapImage[ele].Channels, $scope.imagemodels.Channels);
                    $scope.$applyAsync();
        			}, 1000);
            }));

            function getSiteID(id) {
                var result = Enumerable.From($scope.AllSites)
                             .Where(function (x) { return (Enumerable.From(x.Sites).Where(function (y) { return y.ID == id }).ToArray().length > 0) }).ToArray();
        			if (result) { return result[0].ID; }
                return 0;
            }

            function AllSites() {
                var site_lv1 = [] , site_lv2 = [],district = [];
                district = Enumerable.From(sitedata.Sites).Select(function (s) { return s.Sites }).ToArray();
        			for (var i = 0; i < district.length; i++) {
                    site_lv1 = district[i].concat(site_lv1);
                }

                district = Enumerable.From(site_lv1).Select(function (s) { return s.Sites }).ToArray();
                site_lv1 = Enumerable.From(site_lv1).Where(function (s) { return s.Type == 1 }).ToArray();

                for (var i = 0; i < district.length; i++) {
                    site_lv2 = district[i].concat(site_lv2);
                }

        			site_lv2 = Enumerable.From(site_lv2).Where(function (s) { return s.Type == 1 }).ToArray();
                var result = site_lv1.concat(site_lv2);
                return result;
            }

            $scope.imgClick = function (elem, event) {
                $scope.viewS = 3;
                //if (elem.image.ImageID > 0) {
                    for (var i = 0; i < $scope.mapmodels.mapImage.length; i++) {
                        if ($scope.mapmodels.mapImage[i].Caption == elem.image.Caption) {
                            $scope.imagemodels.ImageID = elem.image.ImageID;
                            $scope.imagemodels.ImageURL = elem.image.ImageID > 0 ? elem.image.ImageURL.replace(AppDefine.SITE_TAPS.thumbnail_true, AppDefine.SITE_TAPS.thumbnail_false) : elem.image.ImageURL;
                            $scope.imagemodels.Flag = elem.image.Flag;
                            $scope.imagemodels.Caption = elem.image.Caption;
                            $scope.imagemodels.Title = elem.image.Title;
                            $scope.imagemodels.ImageByte = elem.image.ImageByte;
                            $scope.imagemodels.Channels = [];
                            $scope.$emit(AppDefine.Events.SITE_TAPS.CHANNEL_LOAD, i);
                            return;
                        }
                    }

                    angular.copy(elem.image, $scope.imagemodels);
                    $scope.$applyAsync();
              //}  
            };

    		function formatString(format) {
    			var args = Array.prototype.slice.call(arguments, 1);
    			return format.replace(/{(\d+)}/g, function (match, number) {
    				return typeof args[number] != 'undefined' ? args[number] : match;
    			});
    		};

        		function setImagesize() {
                var width_site_map_box = $('.site-map-box').width();
                
                var map_detail_height = $(window)['0'].innerHeight;
                $('.map-no-data').height(map_detail_height -291);  
               
            }


            $scope.Sitemaps_Recycle_bin_js = false;    	
            $scope.ShowTrash = function () {
               $scope.Sitemaps_Recycle_bin_js = true;           
            };
              $scope.HideTrash = function () {
               $scope.Sitemaps_Recycle_bin_js = false;
            };
                      
              $scope.ngDrop = function (event) {
                  $scope.drag('Drop');
              };
              $scope.ngDragenter = function (event) {
                  $scope.xChannel($scope.CurrentDragedChannelID, null);
                  $scope.showtrashbox = false;
              };
              $scope.ngDragover = function (event) {
              };
                  
              $scope.ngDragend = function (event) {
              };

              $scope.CurrentDragedChannelID = 0;
              $scope.showtrashbox = false;
           
              $scope.ChannelDrag = function (event)
              {
                  $scope.showtrashbox = true;
                  $scope.CurrentDragedChannelID = event.currentTarget.id;
                  $scope.$applyAsync();
           
              };
              var isChangInput = false;
              $scope.isChangInput = function ($event) {
                  isChangInput = true;
              };
          
              $scope.KeyEnter = function ($event) {
                  if (($event.keyCode == 13 || $event.type == 'blur') && isChangInput == true)
                  {
                      $scope.submit();
                      isChangInput = false;
                  }
                  return;
              }

              

        }
    });
})();

