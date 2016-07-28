(function () {
    'use strict';
    define(["cms"], function (cms) {
        cms.register.controller('ConversionCompCtrl', ConversionCompCtrl);
        ConversionCompCtrl.$inject = ['$rootScope', '$scope', '$modal', 'cmsBase', 'dataContext', 'AppDefine', '$timeout', 'siteadminService', 'AccountSvc', 'DashboardReportsSvc', '$filter', 'chartSvc', 'fiscalyearservice', 'bamhelperSvc', 'Utils', 'ReportService'];

        function ConversionCompCtrl($rootScope, $scope, $modal, cmsBase, dataContext, AppDefine, $timeout, siteadminService, AccountSvc, DashboardReportsSvc, $filter, chartSvc, fiscalyearservice, bamhelperSvc, utils, rptService) {
            $scope.vm = {
                currentImagesIndex: 0,
                currentImgSrc: '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-001.jpg',
                images: [
                    '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-001.jpg',
                    '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-002.jpg',
                    '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-003.jpg',
                    '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-004.jpg',
                   '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-005.jpg',
                   '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-006.jpg',
                   '../CMSWeb/BAM/Asset/Conversion/Conversion_Comparision_01082016-page-007.jpg',
                ],
                isCurrentImgVisible: true,
                loadImage: function () {
                    var that = this;
                    $timeout(function () {
                        that.isCurrentImgVisible = true;
                        that.currentImgSrc = that.images[that.currentImagesIndex];
                    }, 750);
                },
                navigateNext: function () {
                    this.isCurrentImgVisible = false;
                    this.currentImagesIndex++;
                    if (this.currentImagesIndex >= this.images.length) {
                        this.currentImagesIndex = 0;
                    }
                    this.loadImage();
                },
                navigatePrevious: function () {
                    this.isCurrentImgVisible = false;
                    this.currentImagesIndex--;
                    if (this.currentImagesIndex < 0) {
                        this.currentImagesIndex = this.images.length - 1;
                    }
                    this.loadImage();
                }
            };
            $scope.vm.loadImage();
        }
    });
})();