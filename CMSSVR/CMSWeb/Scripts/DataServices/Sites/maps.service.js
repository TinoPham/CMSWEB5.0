(function () {
	'use strict';
	define(['cms'], mapsservice);

	function mapsservice(cms) {
		cms.register.service('maps.service', mapsSVc);
		mapsSVc.$inject = ['$resource', 'cmsBase', 'AppDefine','$log','$filter'];

		function mapsSVc($resource, cmsBase, AppDefine, $log, $filter) {
		    var url = AppDefine.Api.Maps;
			var mapsRe = $resource(url, {}, {
			    SaveMaps: { method: 'POST', url: url + '/Maps' },
			    InsertModelFromDialog: { method: 'POST', url: url + '/InsertModelFromDialog' },
			    DeleteModelFromButtonX: { method: 'POST', url: url + '/DeleteModelFromButtonX' },
				GetMaps: { method: 'GET', url: url + '/Gets', headers: cms.EncryptHeader() },
				GetImages: { method: 'GET', url: url + '/Images', headers: cms.EncryptHeader() },
				UploadService: { mothod: 'POST', url: url + '/upload', headers: cms.EncryptHeader(), }
                
			});
			var MapsModel, MapsImage, ChannelsPosition;
			InitModel();
			function InitModel() {

				

				ChannelsPosition = {
					ChannelID: 0,
					Leftpoint: 0,
					Toppoint: 0
				}
				MapsImage = {
					ImageID: 0,
					ImageURL: "",
					ImageByte: [],
					Channels: [ChannelsPosition]
				}

				

				MapsModel = {
					siteKey: 0,
					mapImage: [MapsImage]
				};
			}



			return {
				create: function () {
					var service = {
						SaveMaps: SaveMaps,
						GetMaps: getMaps,
						GetImages:GetImages,
						MapsModel: function () { return MapsModel },
						MapsImage: function () { return MapsImage },
						ChannelsPosition: function () { return ChannelsPosition; },
						UploadService: Upload,
						InsertModelFromDialog: InsertModelFromDialog,
						DeleteModelFromButtonX: DeleteModelFromButtonX

					};
					return service;
				}
			};

			function Upload(formdata, successFn, errorFn)
			{
			    var xhr = new XMLHttpRequest();
                
			    mapsRe.UploadService(formdata, function (result) {
			                                     var data = cms.GetResponseData(result)
			                                        }
                                             , function (error) { return });
			}

			function InsertModelFromDialog(model, successFn, current) {
			    mapsRe.InsertModelFromDialog(model, function (result) {
			        var data = cms.GetResponseData(result);
			        successFn(data, current);
			      //  cmsBase.cmsLog.success($filter('translate')(AppDefine.Resx.SAVE_SUCCESS));
			    }, function (error) {
			        //errorFn(error);
			        // cmsBase.cmsLog.error(MSG_FAIL);
			        // cmsBase.cmsLog.warning(MSG_FAIL);
			    });
			}

			function DeleteModelFromButtonX(model) {
			    mapsRe.DeleteModelFromButtonX(model, function (result) {
			        //var data = cms.GetResponseData(result);
			        //successFn(data);
			        //cmsBase.cmsLog.success($filter('translate')(AppDefine.Resx.SAVE_SUCCESS));
			    }, function (error) {
			        //errorFn(error);
			        // cmsBase.cmsLog.error(MSG_FAIL);
			        //  cmsBase.cmsLog.warning(MSG_FAIL);
			    });
			}

			function SaveMaps(model, successFn, errorFn) {
				mapsRe.SaveMaps(model, function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
					//cmsBase.cmsLog.success($filter('translate')(AppDefine.Resx.SAVE_SUCCESS));
				}, function (error) {
					errorFn(error);
				   // cmsBase.cmsLog.error(MSG_FAIL);
				  //  cmsBase.cmsLog.warning(MSG_FAIL);
				});
			}




			function getMaps(sitekey,successFn, errorFn) {
				mapsRe.GetMaps({'sitekey':sitekey}, function (result) {
					var data = cms.GetResponseData(result);
					successFn(data);
				}, function (error) {
					errorFn(error);
				});
			}

			function GetImages(sitekey,filename,rootdata,thumbnail, successFn, errorFn) {
			    mapsRe.GetImages({ 'sitekey': sitekey, 'filename': filename,'thumbnail':thumbnail }, function (result) {
			        var data = cms.GetResponseData(result);
			        successFn(data, rootdata, thumbnail);
			    }, function (error) {
			        errorFn(error);
			    });
			}

		}
	}
})();