(function () {
    'use strict';

    define(['cms'], function (cms) {
        cms.register.controller('editaddldapCtrl', ['$scope', '$modalInstance', 'items', 'LdapSvc', 'AccountSvc', 'AppDefine', 'cmsBase',
                                function ($scope, $modalInstance, items, LdapSvc, AccountSvc, AppDefine, cmsBase) {

            $scope.days = ['1', '2', '3', '4', '5', '6', '7'];
            $scope.message = null;
            $scope.ldapSelected = {};
            $scope.btn_Type = AppDefine.Resx.BTN_NEW;
            $scope.mytime = new Date();
            $scope.synUserType = [];
            $scope.typeSelected = [];
            $scope.Resx = AppDefine.Resx;

            //timepicker
            $scope.pop = {
                mytime: new Date(),
                hstep: 1,
                mstep: 1,
                ismeridian: false
            };           

            if (items != null) {
                $scope.ldapSelected = angular.copy(items);
                $scope.btn_Type = AppDefine.Resx.BTN_EDIT;
            } else {
                $scope.ldapSelected.Interval = $scope.days[0];
                $scope.ldapSelected.isEnable = true;
                $scope.ldapSelected.isForceUpdate = true;
            }

            AddValueToCombobox();

            function AddValueToCombobox() {
                if ($scope.ldapSelected.Time != null) {
                    var timeValue = $scope.ldapSelected.Time.split(":");
                    $scope.pop.mytime.setHours(timeValue[0], timeValue[1]);
                }
            }

			LdapSvc.GetAllSynUserType().then(function (data) {
                $scope.synUserType = data;
				$scope.typeSelected= $scope.synUserType[0];
				
				for(var i=0; i< $scope.synUserType.length; i++){
					if($scope.ldapSelected.SynType == $scope.synUserType[i].SynID ){
						$scope.typeSelected= $scope.synUserType[i];
					} 
				}				
			});

			$scope.ChangeValue = function () {
			    $scope.myForm.serverip.$setValidity('unique', true);
			}

			$scope.CloseLDAP = function () {
				$modalInstance.close(AppDefine.ModalConfirmResponse.CLOSE);
			}

			function addZero(i) {
			    if (i < 10) {
			        i = "0" + i;
			    }
			    return i;
			}

            $scope.SaveLDAP = function () {
                if ($scope.ldapSelected.SynID == null) {
                    $scope.ldapSelected.SynID = 0;
                }
                
                $scope.ldapSelected.SynType = $scope.typeSelected.SynID;
                $scope.ldapSelected.SynName = $scope.typeSelected.SynName;
                var user = AccountSvc.UserModel();
                $scope.ldapSelected.CreateBy = user.UserID;
                $scope.ldapSelected.LastSyn = new Date();

                var time = $scope.pop.mytime;
                $scope.ldapSelected.Time = addZero(time.getHours()) + ":" + addZero(time.getMinutes()); 

                LdapSvc.AddSynUser($scope.ldapSelected)
                    .then(
                        function (data) {
                            if (data.ReturnStatus == false) {
                                $scope.message = data.ReturnMessage[0];
                                $scope.myForm.serverip.$setValidity('unique', false);
                                var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_FAIL);
                                cmsBase.cmsLog.error(msg);
                            }
                            else {
                                console.log(data);
                                $modalInstance.close();
                                var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_SUCCESS);
                                cmsBase.cmsLog.info(msg);
                            }
                        }
                        , function (data, error) {
                            console.log(data + error);
                            alert(data + error);
                            var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SAVE_FAIL);
                            cmsBase.cmsLog.error(msg);
                        }
                    );
                
            }
        }]);
        
    });
})();