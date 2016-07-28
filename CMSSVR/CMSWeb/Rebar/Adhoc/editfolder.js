(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('editfolderadhocCtrl', editfolderadhocCtrl);

	    editfolderadhocCtrl.$inject = ['$scope', '$modalInstance', '$filter', 'items', 'AccountSvc', 'AppDefine', 'cmsBase', 'adhocDataSvc'];

	    function editfolderadhocCtrl($scope, $modalInstance, $filter, items, AccountSvc, AppDefine, cmsBase, adhocDataSvc) {
			var vm = this;
			active();

			vm.folder = {
			    FolderID:0,
	            FolderName: "",
	            folderNameDes:""
	        };

	        function active() {
	            if (items && items.isNew === false) {
	                adhocDataSvc.getAdhocFoldersById({ folderId: items.report.Id }, function (result) {
	                    vm.folder = result;
	                }, function(err) {
	                    
	                });
	            }

	        }	

			vm.Cancel = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			vm.Save = function () {
			    adhocDataSvc.addAdhocReportFolder(vm.folder, function (result) {
			        vm.folder = result;
			        $modalInstance.close(vm.folder);
			    }, function (err) {
			        var msg = cmsBase.translateSvc.getTranslate(err.data.Data.ReturnMessage[0]);
			        cmsBase.cmsLog.error(msg);
			    });
			  
			}

			
		}
	});
})();

