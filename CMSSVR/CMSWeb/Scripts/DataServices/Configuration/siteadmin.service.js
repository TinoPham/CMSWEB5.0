(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.register.service('siteadmin.service', siteadminSvc);

		siteadminSvc.$inject = ['$resource', 'cmsBase', 'AppDefine', '$http', 'Utils', '$q'];
		function siteadminSvc($resource, cmsBase, AppDefine, $http, Utils, $q) {
            var url = AppDefine.Api.Sites;
            var goaldtypeurl = AppDefine.Api.GoalType;

            var siteadminResource = $resource(url, { userId: '@userId' }, {
                getDVRInfo: { method: 'GET', url: url + '/MacAddress', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                getTreeSite: { method: 'GET', url: url + '/TreeSites', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                getSites: { method: 'GET', url: url + '/Sites', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                sitesByPACID: { method: 'GET', url: url + '/SitesByPACID', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                siteById: { method: 'GET', url: url + '/SiteById', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                getSite: { method: 'GET', url: url + '/GetSite', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                gettreemetrics: { method: 'GET', url: url + '/GetTreeMetrics', isArray: false, headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                getMacFiles: { method: 'GET', url: url + '/GetMacFiles', isArray: false, interceptor: { response: function (response) { return response; } } },
				getAllMacFiles: { method: 'GET', url: url + '/GetAllMacFiles?', isArray: false, interceptor: { response: function (response) { return response; } } },
                //getFileSite: { method: 'GET', url: url + '/GetFileSite', interceptor: { response: function (response) { return response; } } },
                getRegion: { method: 'GET', url: url + '/GetRegion', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                editRegion: { method: 'POST', url: url + '/EditRegion', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                addRegion: { method: 'POST', url: url + '/AddRegion', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                saveTreeNode: { method: 'POST', url: url + '/SaveTreeNode', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                deleteRegion: { method: 'POST', url: url + '/DeleteRegion', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                deleteMultiRegion: { method: 'POST', url: url + '/DeleteMultiRegion', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
                editSite: { method: 'POST', url: url + '/EditSite', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                addSite: { method: 'POST', url: url + '/AddSite', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
                deleteSite: { method: 'POST', url: url + '/DeleteSite', headers: cms.EncryptHeader(), interceptor: { response: function ( response ) { return response; } } },
				getZipCode: { method: 'GET', url: url + '/ZipCode', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetImageSite: { method: 'GET', url: url + '/GetImageSite', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetAllHaspLicense: { method: 'GET', url: url + '/GetAllHaspLicense', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetPacInfoBySitesAPI: { method: 'GET', url: url + '/GetPacInfoBySites', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				sitesByPACIDHO: { hideOverlay: true, method: 'GET', url: url + '/SitesByPACID', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } },
				GetPacInfoBySitesHO: { hideOverlay: true, method: 'GET', url: url + '/GetPacInfoBySites', headers: cms.EncryptHeader(), interceptor: { response: function (response) { return response; } } }
            });

            var goaltypeResource = $resource(goaldtypeurl, { userId: '@userId' }, {
                getGoals: { method: 'GET', url: goaldtypeurl + '/GetGoals', isArray: true }
            });

            var treesite = null;
            var cacheTreeSite = null;

            return {
                create: createRepo
            };

            function createRepo() {

                var userService = {
                    getDVRInfo:getDVRInfo,
                    getRegion: GetRegion,
                    editRegion: EditRegion,
                    addRegion: AddRegion,
                    getSites: GetSites,
                    sitesByPACID: SitesByPACID,
                    siteById:SiteById,
					GetSiteByUserId: GetSiteByUserId,
                    getSite: GetSite,
                    getTreeMetrics: GetTreeMetrics,
                    saveTreeNode: SaveTreeNode,
                    getGoals: GetGoals,
                    deleteRegion: DeleteRegion,
                    deleteMultiRegion: DeleteMultiRegion,
                    editSite: EditSite,
                    addSite: AddSite,
                    deleteSite: DeleteSite,
                    refeshTreeCache: refeshTreeCache,
					getMacFiles: GetMacFiles,
					getSitesWithChannels: GetSitesWithChannels,
					getAllUserSites:GetAllUserSites,
					//getFileSite: GetFileSite,
					ChartGetSite: ChartGetSite,
					getAllMacFiles: getAllMacFiles,
					getZipCode: getZipCode,
					GetImageSite: GetImageSite,
					GetAllHaspLicense: GetAllHaspLicense,
					GetPacInfoBySites: getPacInfoBySites,
					sitesByPACIDHO: SitesByPACIDHO,
					GetPacInfoBySitesHO: getPacInfoBySitesHO
                };
                return userService;
            }

            //function GetFileSite(data, successFn, errorFn) {
            //    $http.get(url + '/GetFileSite', {
            //        responseType: 'arraybuffer',
            //        params: data,
            //        headers: {
            //            "Content-Type": "application/x-download;"
            //        }
            //    }).success(function(data) {
            //        //saveAs(new Blob([data], { type: "application/octet-stream'" }), 'testfile.zip');
            //    }).error(errorFn);
            //}

			function getAllMacFiles(data) {
				//var strpram = Utils.Object2QueryString(data, null);
				var def = $q.defer();
				siteadminResource.getAllMacFiles(data).$promise.then(
					function (result) {
                    var data = cms.GetResponseData(result);
						def.resolve(data);
					}
					, function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function getDVRInfo(data, successFn, errorFn) {
                siteadminResource.getDVRInfo({'sitekey':data}, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function SiteById(data, successFn, errorFn) {
                siteadminResource.siteById(data, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function GetMacFiles(data, successFn, errorFn) {
                siteadminResource.getMacFiles(data, function (result) {
                    var data = cms.GetResponseData(result);
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            function SaveTreeNode(data, successFn, errorFn) {
                siteadminResource.saveTreeNode( data, function ( result ) {
                    var data = cms.GetResponseData( result );
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

			function AddRegion(data) {
				var def = $q.defer();
				siteadminResource.addRegion(data).$promise.then(
					function (response) {
						def.resolve(response.data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

            function refeshTreeCache() {
            	treesite = null;
            	cacheTreeSite = null;
            }

            function GetSites(successFn, errorFn) {
                if (treesite) {
                    successFn(treesite);
                    return;
                }
                siteadminResource.getSites().$promise.then(
                    function(result) {
                        var data = cms.GetResponseData(result);
                        if (data) {
                            // treesite = data;
                            successFn(data);
                        } else {
                            // treesite = result;
                            successFn(result);
                        }

                    }, function(error) {
                        errorFn(error);
                    });
            }

            function SitesByPACID(successFn, errorFn) {
                if (treesite) {
                    successFn(treesite);
                    return;
                }
                siteadminResource.sitesByPACID().$promise.then(
                    function(result) {
                        var data = cms.GetResponseData(result);
                        if (data) {
                            // treesite = data;
                            successFn(data);
                        } else {
                            // treesite = result;
                            successFn(result);
                        }

                    }, function(error) {
                        errorFn(error);
                    });
            }
            function SitesByPACIDHO(successFn, errorFn) {
            	if (treesite) {
            		successFn(treesite);
            		return;
            	}
            	siteadminResource.sitesByPACIDHO().$promise.then(
                    function (result) {
                    	var data = cms.GetResponseData(result);
                    	if (data) {
                    		// treesite = data;
                    		successFn(data);
                    	} else {
                    		// treesite = result;
                    		successFn(result);
                    	}

                    }, function (error) {
                    	errorFn(error);
                    });
            }

            function GetSitesWithChannels(param, successFn, errorFn) {
			    if (treesite) {
			        successFn(treesite);
			        return;
			    }
			    siteadminResource.getSites(param).$promise.then(
                     function (result) {
                         var data = cms.GetResponseData(result);
                         if (data) {
                             // treesite = data;
                             successFn(data);
                         }
                         else {
                             // treesite = result;
                             successFn(result);
                         }

                     }
                     , function (error) {
                         errorFn(error);
                     });
            }

            function GetAllUserSites(successFn, errorFn) {
                if (treesite) {
                    successFn(treesite);
                    return;
                }
                siteadminResource.getSites({ allUsers: true }).$promise.then(
                     function (result) {
                         var data = cms.GetResponseData(result);
                         if (data) {
                             // treesite = data;
                             successFn(data);
                         }
                         else {
                             // treesite = result;
                             successFn(result);
                         }

                     }
                     , function (error) {
                         errorFn(error);
                     });
            }

			function GetSiteByUserId(data, successFn, errorFn) {

                if (treesite) {
                    successFn(treesite);
                    return;
                }
                siteadminResource.getTreeSite().$promise.then(
                     function ( result ) {
                         var data = cms.GetResponseData( result );
                         if ( data ){
                            // treesite = data;
                             successFn( data );
                         }
                         else {
                            // treesite = result;
                             successFn( result);
                         }

                     }
                     , function (error) {
                         errorFn(error);
                     });
			}

			function ChartGetSite(param, successFn, errorFn) {
				if (param == 1 && cacheTreeSite) {
					successFn(cacheTreeSite);
					return;
				}
				siteadminResource.getTreeSite().$promise.then(
					function ( result ) {
						var data = cms.GetResponseData( result );
						if ( data ){
							cacheTreeSite = angular.copy(data);
							successFn( data );
						}
						else {
							cacheTreeSite = angular.copy(result);
							successFn( result);
						}
					}
					, function (error) {
						errorFn(error);
					});
			}

			function GetTreeMetrics( successFn, errorFn ) {
			    siteadminResource.gettreemetrics().$promise.then(
                    function ( success ) {
                        var data = cms.GetResponseData( success );
                        if ( data ) {
                            successFn(  data );
                        }
                    }
                    , function ( error ) {
                        if ( errorFn )
                            errorFn( error );
                    }
                    );
            }

			function GetSite(data) {
				var def = cmsBase.$q.defer();
				siteadminResource.getSite(data).$promise.then(
					function (result) {
                    var data = cms.GetResponseData( result );
						def.resolve(data);
					}
					, function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function EditRegion(data) {
				var def = $q.defer();
				siteadminResource.editRegion(data).$promise.then(
					function (response) {
						def.resolve(response.data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function DeleteRegion(data) {
				var def = $q.defer();
				siteadminResource.deleteRegion(data).$promise.then(
					function (result) {
                    var data = cms.GetResponseData( result );
						def.resolve(data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function DeleteMultiRegion(data) {
				var def = $q.defer();
				siteadminResource.deleteMultiRegion(data).$promise.then(
					function (result) {
                    var data = cms.GetResponseData(result);
						def.resolve(data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function AddSite(data) {
				var def = $q.defer();
				siteadminResource.addSite(data).$promise.then(
					function (response) {
						def.resolve(response.data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function EditSite(data) {
				var def = $q.defer();
				siteadminResource.editSite(data).$promise.then(
					function (response) {
						def.resolve(response.data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

			function DeleteSite(data) {
				var def = $q.defer();
				siteadminResource.deleteSite(data).$promise.then(
					function (result) {
                    var data = cms.GetResponseData( result );
						def.resolve(data);
					},
					function (error) {
						def.reject(error);
                });
				return def.promise;
            }

            function GetRegion(data, successFn, errorFn) {
                siteadminResource.getRegion( data, function ( result ) {
                    var data = cms.GetResponseData( result );
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

            //function GetTreeSite(successFn, errorFn) {
            //    siteadminResource.getTreeSite( function ( result ) {
            //        var data = cms.GetResponseData( result );
            //        successFn(data);
            //    }, function(error) {
            //        errorFn(error);
            //    });
            //}

            function GetGoals(successFn, errorFn) {
                goaltypeResource.getGoals( function ( result ) {
                    var data = cms.GetResponseData( result );
                    successFn(data);
                }, function (error) {
                    errorFn(error);
                });
            }

			function getZipCode(data) {
				var def = $q.defer();
				siteadminResource.getZipCode(data).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}

			function GetImageSite(data) {
				var def = $q.defer();
				siteadminResource.GetImageSite(data).$promise.then(
					function (result) {
						//var data = cms.GetResponseData(result);
						def.resolve(result.data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}

			function GetAllHaspLicense(data) {
				var def = $q.defer();
				siteadminResource.GetAllHaspLicense(data).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}

			function getPacInfoBySites(data) {
				var def = $q.defer();
				siteadminResource.GetPacInfoBySitesAPI(data).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}

			function getPacInfoBySitesHO(data) {
				var def = $q.defer();
				siteadminResource.GetPacInfoBySitesHO(data).$promise.then(
					function (result) {
						var data = cms.GetResponseData(result);
						def.resolve(data);
					}, function (error) {
						def.reject(error);
					});
				return def.promise;
			}
        }
    });
})();