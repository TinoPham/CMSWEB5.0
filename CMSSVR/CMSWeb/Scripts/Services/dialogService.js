(function () {
    define(['cms'], function (cms) {
        cms.register.service('dialogSvc', dialogSvc);
		dialogSvc.$inject = ['$modal', 'AppDefine'];

		function dialogSvc($modal, AppDefine) {
            var modalDefaults = {
                backdrop: true,
                keyboard: true,
                modalFade: true,
                templateUrl: 'Widgets/DeleteDialog.html'
            };

            var modalOptions = {
				closeButtonText: AppDefine.Resx.BTN_CLOSE,
				actionButtonText: AppDefine.Resx.BTN_OK,
				headerText: AppDefine.Resx.HEADER_CONFIRM_DEFAULT,
				bodyText: AppDefine.Resx.BODY_CONFIR_DEFAULT
            };

            this.showModal = function (customModalDefaults, customModalOptions) {
                if (!customModalDefaults) customModalDefaults = {};
                customModalDefaults.backdrop = 'static';
                return this.show(customModalDefaults, customModalOptions);
            };

            this.show = function (customModalDefaults, customModalOptions) {
                //Create temp objects to work with since we're in a singleton service
                var tempModalDefaults = {};
                var tempModalOptions = {};

                //Map angular-ui modal custom defaults to modal defaults defined in this service
                angular.extend(tempModalDefaults, modalDefaults, customModalDefaults);

                //Map modal.html $scope custom properties to defaults defined in this service
                angular.extend(tempModalOptions, modalOptions, customModalOptions);

                if (!tempModalDefaults.controller) {
                    tempModalDefaults.controller = function ($scope, $modalInstance) {
                        $scope.modalOptions = tempModalOptions;
                        $scope.modalOptions.ok = function (result) {
							$modalInstance.close(AppDefine.ModalConfirmResponse.OK);
                        };
                        $scope.modalOptions.close = function (result) {
							$modalInstance.close(AppDefine.ModalConfirmResponse.CANCEL);
                        };
                    };

                    tempModalDefaults.controller.$inject = ['$scope', '$modalInstance'];
                }

                return $modal.open(tempModalDefaults).result;
            };
        }      

    });
}
)();
