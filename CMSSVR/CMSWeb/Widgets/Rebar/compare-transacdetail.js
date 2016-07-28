(function () {
	'use strict';

	define(['cms'], function (cms) {
	    cms.register.controller('comparetransacdetailCtrl', comparetransacdetailCtrl);

	    comparetransacdetailCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', '$modalInstance', 'items', 'AppDefine', 'rebarDataSvc'];

	    function comparetransacdetailCtrl($scope, dataContext, cmsBase, $modalInstance, items, AppDefine, rebarDataSvc) {

	        $scope.TaxtList = [];
	        $scope.PaymentList = [];
	        $scope.isMaxCom = false;
	        $scope.cancel = function () {
	            $modalInstance.close();
	        }

	        $scope.fullSize = function () {
	            $scope.isMaxCom = !$scope.isMaxCom;
	            cmsBase.modalfullscreen($scope.isMaxCom, 'modal-custom-for-compare');
	        }

	        active();
	        $scope.expand = false;
	        function active() {
	            $scope.TaxtList = [];
	            $scope.PaymentList = [];
	            var param = { tranId: items.TranId };
	            $scope.data = items.data;
	            $scope.picSource = null;
	            if ($scope.data.CompanyLogo) {
	                $scope.picSource = AppDefine.ImageOptions.PrefixImageEmbedding + $scope.data.CompanyLogo;
	            }

	            rebarDataSvc.getTransactionInfo(param, function (data) {
	                $scope.datacompare = data;
	                listTax();
	                listPayment();
	            }, function (error) {

	            });
	        }

	        function listTax() {

	            if ($scope.data.Taxs) {
	                $scope.data.Taxs.forEach(function (t) {
	                    var tax = $scope.datacompare.Taxs && $scope.datacompare.Taxs.length> 0 ? Enumerable.From($scope.datacompare.Taxs).Where(function(x) { return x.Id === t.Id; }).FirstOrDefault() : null;
	                    if (tax) {
	                        tax.Checked = true;
	                        $scope.TaxtList.push({ Id: t.Id, Name: t.Name, Ammount: t.Ammount, AmmountCom: tax.Ammount });
	                    } else {
	                        $scope.TaxtList.push({ Id: t.Id, Name: t.Name, Ammount: t.Ammount, AmmountCom: 0 });
	                    }
	                });
	            }


	            if ($scope.datacompare.Taxs) {
	                $scope.datacompare.Taxs.forEach(function(t) {
	                    //if (!t.Checked) {
	                        $scope.TaxtList.push({ Id: t.Id, Name: t.Name, Ammount: 0, AmmountCom: t.Ammount });
	                    //}
	                });
	            }
	        }

	        function listPayment() {
	            if ($scope.data.Payments) {
	                $scope.data.Payments.forEach(function (t) {
	                    var tax = $scope.datacompare.Payments && $scope.datacompare.Payments.length > 0 ? Enumerable.From($scope.datacompare.Payments).Where(function (x) { return x.Id === t.Id; }).FirstOrDefault() : null;
	                    if (tax) {
	                        tax.Checked = true;
	                        $scope.PaymentList.push({ Id: t.Id, Name: t.Name, Ammount: t.Ammount, AmmountCom: tax.Ammount });
	                    } else {
	                        $scope.PaymentList.push({ Id: t.Id, Name: t.Name, Ammount: t.Ammount, AmmountCom: 0 });
	                    }
	                });
	            }


	            if ($scope.datacompare.Payments) {
	                $scope.datacompare.Payments.forEach(function (t) {
	                    if (!t.Checked) {
	                        $scope.PaymentList.push({ Id: t.Id, Name: t.Name, Ammount: 0, AmmountCom: t.Ammount });
	                    }
	                });
	            }
	        }
	    }
	});
})();