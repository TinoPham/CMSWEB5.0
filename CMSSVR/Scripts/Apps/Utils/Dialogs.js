//var AppUtils = angular.module('AppUtils');
define(['require', 'jquery'], function (require, jquery) {
    var dlg = function ($modal)
    {
        function errorDialogCtrl ($scope, $modalInstance, msg,data) {
            $scope.msg = angular.isDefined(msg) ? msg : "An unknown error has occurred.", $scope.close = function () {
                data = 100;
                $modalInstance.close();
                }
        }

        function waitDialogCtrl($scope, $modalInstance, $timeout, msg, progress)
        {
                var o = $scope, a =$modalInstance, l =$timeout, n= msg, e = progress;
            o.msg = angular.isDefined(n) ? n : "Waiting on operation to complete.", o.progress = angular.isDefined(e) ? e : 100, o.$on("dialogs.wait.complete", function () {
                l(function () {
                    a.close();
                })
            }), o.$on("dialogs.wait.message", function (a, l) {
                o.msg = angular.isDefined(l.msg) ? l.msg : o.msg
            }), o.$on("dialogs.wait.progress", function (a, l) {
                o.msg = angular.isDefined(l.msg) ? l.msg : o.msg, o.progress = angular.isDefined(l.progress) ? l.progress : o.progress
            }), o.getProgress = function () {
                return {
                    width: o.progress + "%"
                }
            }
        }

        function notifyDialogCtrl($scope, $modalInstance, header, msg) {
            var o = $scope, a = $modalInstance, l = header, n = msg;
                o.header = angular.isDefined(l) ? l : "Notification", o.msg = angular.isDefined(n) ? n : "Unknown application notification.", o.close = function () {
                    a.close();
                }
            }
        
        function confirmDialogCtrl($scope, $modalInstance, header, msg){
        var o = $scope, a = $modalInstance, l = header, n = msg;
                o.header = angular.isDefined(l) ? l : "Confirmation", o.msg = angular.isDefined(n) ? n : "Confirmation required.", o.no = function () {
                    a.dismiss("no");
                }, o.yes = function () {
                    a.close("yes");
                }
            }


        return {
            
            error: function (a,b) {
                return $modal.open({
                    templateUrl: "/api/shared/error.html",
                    controller: errorDialogCtrl,
                    windowClass: "metro window-overlay",
                    resolve: {
                        msg: function() {
                            return angular.copy(a);
                        },
                        data: function() {
                            return b;
                        }
                    }
                });
            },
            wait: function (a, l) {
                return $modal.open({
                    templateUrl: "/api/shared/wait.html",
                    controller: waitDialogCtrl,
                    windowTemplateUrl: "/api/shared/wait.html",
                    resolve: {
                        msg: function () {
                            return angular.copy(a);
                        },
                        progress: function () {
                            return angular.copy(l);
                        }
                    }
                });
            },
            notify: function (a, l) {
                return $modal.open({
                    templateUrl: "/api/shared/notify.html",
                    controller: notifyDialogCtrl,
                    windowTemplateUrl: "/api/shared/notify.html",
                    resolve: {
                        header: function () {
                            return angular.copy(a);
                        },
                        msg: function () {
                            return angular.copy(l);
                        }
                    }
                });
            },
            confirm: function (a, l) {
                return $modal.open({
                    templateUrl: "/api/shared/confirm.html",
                    controller: confirmDialogCtrl,
                    windowTemplateUrl: "/api/shared/confirm.html",
                    resolve: {
                        header: function () {
                            return angular.copy(a);
                        },
                        msg: function () {
                            return angular.copy(l);
                        }
                    }
                });
            },
            create: function (a, l, n, e) {
                var i = angular.isDefined(e.key) ? e.key : !0,
                    s = angular.isDefined(e.back) ? e.back : !0;
                return $modal.open({
                    templateUrl: a,
                    controller: l,
                    keyboard: i,
                    backdrop: s,
                    resolve: {
                        data: function () {
                            return angular.copy(n);
                        }
                    }
                });
            }
        };

    }
    return dlg;

//    utils.factory('Dialogs', ['$modal', function ($modal) {

//        return {
//            error: function (a) {
//                return $modal.open({
//                    templateUrl: "/views/api/shared/error.html",
//                    controller: "errorDialogCtrl",
//                    windowClass: "metro window-overlay",
//                    resolve: {
//                        msg: function () {
//                            return angular.copy(a)
//                        }
//                    }
//                })
//            },
//            wait: function (a, l) {
//                return $modal.open({
//                    templateUrl: "/shared/wait.html",
//                    controller: "waitDialogCtrl",
//                    windowTemplateUrl: "/shared/wait.html",
//                    resolve: {
//                        msg: function () {
//                            return angular.copy(a)
//                        },
//                        progress: function () {
//                            return angular.copy(l)
//                        }
//                    }
//                })
//            },
//            notify: function (a, l) {
//                return $modal.open({
//                    templateUrl: "/shared/notify.html",
//                    controller: "notifyDialogCtrl",
//                    windowTemplateUrl: "/shared/notify.html",
//                    resolve: {
//                        header: function () {
//                            return angular.copy(a)
//                        },
//                        msg: function () {
//                            return angular.copy(l)
//                        }
//                    }
//                })
//            },
//            confirm: function (a, l) {
//                return $modal.open({
//                    templateUrl: "/shared/confirm.html",
//                    controller: "confirmDialogCtrl",
//                    windowTemplateUrl: "/shared/confirm.html",
//                    resolve: {
//                        header: function () {
//                            return angular.copy(a)
//                        },
//                        msg: function () {
//                            return angular.copy(l)
//                        }
//                    }
//                })
//            },
//            create: function (a, l, n, e) {
//                var i = angular.isDefined(e.key) ? e.key : !0,
//                    s = angular.isDefined(e.back) ? e.back : !0;
//                return $modal.open({
//                    templateUrl: a,
//                    controller: l,
//                    keyboard: i,
//                    backdrop: s,
//                    resolve: {
//                        data: function () {
//                            return angular.copy(n)
//                        }
//                    }
//                })
//            }
//        }
//    }
//    ]);
}
    );
