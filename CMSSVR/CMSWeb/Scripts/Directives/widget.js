(function() {
    'use strict';

    define(['cms', '../../Content/libs/jquery-sortable/js/Sortable.js', '../../Widgets/Dashboard/selectWidget', '../../Widgets/Dashboard/addDashboard'], function (cms, dySortable) {
        cms.register.service('widgetsvc', widgetSvc);
        cms.register.directive('widget', cmswidget);
        cmswidget.$inject = ['$compile', '$injector', '$http', '$q', '$timeout', '$templateCache', 'widgetsvc'];
        cms.register.directive('cmsDashboard', cmsDashboardDt);
		cmsDashboardDt.$inject = ['$modal', '$localStorage', 'widgetsvc', 'AppDefine', '$timeout'];
        cms.register.directive('dashboardRow', dashboardRowDt);
        dashboardRowDt.$inject = ['$compile', 'widgetsvc'];
        cms.register.directive('dashboardColumn', dashboardColumnDt);
        dashboardColumnDt.$inject = ['$compile', '$modal', 'widgetsvc', 'cmsBase', 'AppDefine', 'dialogSvc', 'Utils'];
        cms.register.directive('widgetPanel', widgetPanelDt);
        widgetPanelDt.$inject = ['widgetsvc'];

        function widgetSvc() {

            var widgetResource = {
                Event: {
                    destroy: '$destroy',
                    modelChanged: 'modelChanged',
                    widgetChange: 'widgetChange',
                    widgetReresh: 'widgetReresh',
                    widgetRemove: 'widgetRemove'
                },
                Attributes: {
                    widgets: 'widgets',
                    group: 'group',
                    widgetProgress: 'widget-progress',
                    cmsWidgetId: 'cms-widget-id',
                    handle: '.widget-drag-handle',
                    ghostClass: 'placeholder',
                    widgetsortable: '.widgetsortable',
                    widgetExisted: 'Widget_Existed'
                },
                Template: {
                    addDashboard: 'Widgets/Dashboard/addDashboard.html',
                    dashboard: 'Widgets/Dashboard/dashboard.html',
                    widgetPanel: 'Widgets/Dashboard/widget.html',
                    row: 'Widgets/Dashboard/dashboard-row.html',
                    column: 'Widgets/Dashboard/dashboard-column.html',
                    selectWidget: 'Widgets/Dashboard/selectWidget.html',
                    columnTempate: '<dashboard-column column="column" model="row" collapse="collapse" edit-mode="editMode" ng-repeat="column in row.Columns track by $index" />',
                    rowTemplate: '<dashboard-row row="row" model="column" collapse="collapse" edit-Mode="editMode" ng-repeat = "row in column.Rows  track by $index" />'
                }

            };

            var svc = {
                DragCurrentColumn: {},
                Resources: widgetResource
            };

            return svc;
        }

		//dashboard.html
		function cmsDashboardDt($modal, $localStorage, widgetsvc, AppDefine, $timeout) {
            return {
                replace: true,
                restrict: 'EA',
                transclude: false,
                scope: {
                    name: '@',
                    editMode: '=',
                    editable: '@',
                    model: '=',
                    saveDashboard: '&',
                    levelMode: '=',
					loadDefinedDashboard: '&'
                },
                link: function($scope, $element, $attr) {

                    var modelIsDirty = false;
                    $scope.isCreate = false;

                    $scope.editModeFn = function() {
						$scope.isCreate = false;

						if (!$scope.editMode) {
							$scope.modelBackup = angular.copy($scope.model, {});
						} else {
							$localStorage.dashboard = $scope.model;
						}

						if (modelIsDirty === true) {
							$scope.saveDashboard({ editMode: $scope.editMode }).then(function() {
								$scope.editMode = !$scope.editMode;
								modelIsDirty = false;
							}, function ErrorFn(error){
								$scope.cancelEditMode();
							}).catch(function ExceptFn(error){
								modelIsDirty = false;
							});
							//modelIsDirty = false;
							return;
						}

						$scope.editMode = !$scope.editMode;
						modelIsDirty = false;
                    }

					//$scope.setNewestStatus = function (item) {
					//	$timeout(function () {
					//		$("div[cms-widget-id=" + item.Id + "]").children(".widget-content").addClass("animated infinite shake");
					//	}, 100);
					//}

                    if ($scope.editMode && $scope.editMode === true) {
                        $scope.isCreate = false;
                       $scope.modelBackup = angular.copy($scope.model, {});
                        modelIsDirty = false;
                    }

					$scope.addWidgetDialog = function() {
						showWd($scope.model, true);
					}

					$scope.cancelEditMode = function() {
						$scope.isCreate = false;
						$scope.editMode = false;
						if (modelIsDirty === true) {
							angular.copy($scope.modelBackup, $scope.model);
							//$scope.modelBackup = angular.copy($scope.modelBackup, $scope.model);
							modelIsDirty = false;
						}
					}

					$scope.loadDefinedDshBoard = function (level) {
						$scope.isCreate = false;
						$scope.levelMode = level;
						modelIsDirty = true;
						$scope.loadDefinedDashboard({ levelMode: level }).then(function (retData) {
							$scope.editMode = true;
							$scope.model = retData;
							$scope.$emit(widgetsvc.Resources.Event.modelChanged);
						});
						$scope.editMode = true;
					}

                    $scope.$on(widgetsvc.Resources.Event.destroy, function() {
                        $scope.fullmax = true;
                    });

					$scope.$on(widgetsvc.Resources.Event.modelChanged, function (e, arg) {
                        modelIsDirty = true;
						//if (arg) {
						//	$scope.setNewestStatus(arg);
						//}
                    });

                    $scope.$on(AppDefine.Events.DASHBOARDSETTING, function (event, args) {
					    $scope.editModeFn();
					});

                    function showWd(w, addnew) {
                        if (!$scope.modalShown) {
                            $scope.modalShown = true;
							var dashboardInstance = $modal.open({
                                templateUrl: widgetsvc.Resources.Template.addDashboard,
                                controller: 'addDashboardtCtrl as vm',
								size: 'dashboard', //ThangPham, Customize modal size, June 17 2015
                                backdrop: 'static',
                                keyboard: false,
                                resolve: {
                                    items: function() {
                                        return w;
                                    },
                                    addNew: function() {
                                        return addnew;
                                    }
                                }
                            });

							dashboardInstance.result.then(function (returndata) {
                                $scope.modalShown = false;

                                if (!returndata) {
                                    return;
                                }
                                $scope.isCreate = true;
                                returndata.StyleId = w.StyleId;
                                returndata.UserId = w.UserId;
                                $scope.model = returndata;
                                $scope.$emit(widgetsvc.Resources.Event.modelChanged);
                            });
                        }
                    }
                },
                templateUrl: widgetsvc.Resources.Template.dashboard
            };
        }

		//Widget.html
        function widgetPanelDt(widgetsvc) {
            return {
                restrict: 'EA',
                replace: true,
                transclude: false,
                templateUrl: widgetsvc.Resources.Template.widgetPanel,
                scope: {
                    column: '=',
                    currentColumn: '=',
                    editMode: '=',
                    collapse: '='
                },
                link: function($scope, $element, $attr) {
                    $scope.isLoading = true;
                    $scope.isMax = false;
                    $element.addClass(widgetsvc.Resources.Attributes.widgetProgress);

                    $scope.getWidgetClass = function () {
						var cssClass = '';
						if (!$scope.column.Group.IsHeader) {
							cssClass = '';
						}
						else {
						    cssClass += 'panel dashboard-panel portlets ';
							var pram = JSON.parse("{" + $scope.column.TemplateParams + "}");
							if (pram && pram.hasOwnProperty("Gui")){
								$scope.param = pram.Gui;
							}
							else {
								$scope.param = pram;
							}
							cssClass += $scope.param.WidgetSelectClass? $scope.param.WidgetSelectClass: '';
						}
						return cssClass;
                    }

					//$scope.stopAnimation = function (Id) {
					//	$("div[cms-widget-id=" + Id + "]").children(".widget-content").removeClass("animated infinite shake");
					//}

                    $scope.close = function() {
                        $scope.currentColumn.Widgets.splice($scope.currentColumn.Widgets.indexOf($scope.column), 1);
                        synWidgetOrder($scope.currentColumn.Widgets);
                        $scope.$emit(widgetsvc.Resources.Event.widgetRemove, $scope.currentColumn);
                        $scope.$emit(widgetsvc.Resources.Event.modelChanged);
                    }

                    $scope.refresh = function () {
                        $scope.$broadcast(widgetsvc.Resources.Event.widgetReresh, $scope.column);
                    }

                    $scope.edit = function() {
                        $scope.$emit(widgetsvc.Resources.Event.widgetChange, $scope.column);
                    }

                    $scope.fullSize = function () {
                    	if (!$scope.isMax)
                    		$scope.isCollapsed = false;
                        $scope.isMax = !$scope.isMax;
                    }

					$scope.$on(widgetsvc.Resources.Event.widgetReresh, function (e, data) {
						$scope.getWidgetClass();
					});
                }
            };
        }

        function synWidgetOrder(widgets) {
            var lengwidget = widgets.length;
            for (var i = 0; i < lengwidget; i++) {
                widgets[i].Order = i;
            }
        }

		//dashboard-column.html
        function dashboardColumnDt($compile, $modal, widgetsvc, cmsBase, AppDefine, dialogSvc, Utils) {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    column: '=',
                    editMode: '=',
                    model: '=',
                    collapse: '='
                },
                templateUrl: widgetsvc.Resources.Template.column,
                link: function($scope, $element, $attr) {
                    var sortable = {};
                	$scope.$root.dashboard = $scope.$parent.$parent.model;
                    $scope.addWidgetDialog = function() {
                        if ($scope.column.Widgets.length >= $scope.column.Group.MaxWidgets) {
                            return;
                        }
                        showWd($scope.column, true, sortable);
                    }

                    $scope.$on(widgetsvc.Resources.Event.widgetChange, function(e, data) {
                        showWd($scope.column, false, sortable, data);
                    });

                    $scope.$on(widgetsvc.Resources.Event.widgetRemove, function(e, data) {
                        if (data.Widgets) {
                            setSorableOption(sortable, $scope.column.Widgets.length, $scope.column.Group.MaxWidgets); //, $scope.column.Group.Id);
                        }
                    });

                    function isExistedWidget(columns, returndata) {
                        var rw = false;
                        angular.forEach(columns, function(c) {
                            angular.forEach(c.Widgets, function(w) {
                                if (returndata.Id === w.Id) {
                                    rw = true;
                                    return;
                                }
                            });
                        });
                        return rw;
                    }

                    function showWd(w, addnew, sort, widget) {
                        if (!$scope.modalShown) {
                            $scope.modalShown = true;
							var widgetInstance = $modal.open({
                                templateUrl: widgetsvc.Resources.Template.selectWidget,
                                controller: 'swidgetCtrl as vm',
                                size: 'md',
                                backdrop: 'static',
                                keyboard: false,
                                resolve: {
                                    items: function() {
                                        return w;
                                    },
                                    addNew: function() {
                                        return addnew;
                                    }
                                }
                            });

							widgetInstance.result.then(function (returndata) {
                                $scope.modalShown = false;

                                if (!returndata) {
                                    return;
                                }

								if (isExistedWidget($scope.model.Columns,returndata)) {
									var msg = cmsBase.translateSvc.getTranslate(widgetsvc.Resources.Attributes.widgetExisted);
									cmsBase.cmsLog.warning(msg);
									return;
								}

								if (addnew) {
									var widgetleng = $scope.column.Widgets.length;
									returndata.PositionId = $scope.column.PositionId;
									returndata.TemplateDisable = false;
									returndata.Order = widgetleng;
									returndata.StyleId = 1;

									$scope.column.Widgets.push(returndata);

									setSorableOption(sortable, widgetleng, $scope.column.Group.MaxWidgets);//, $scope.column.Group.Id);

								}
								else {
									widget.Id = returndata.Id;
									widget.Name = returndata.Name;
									widget.Description = returndata.Description;
									widget.Group = returndata.Group;
									widget.GroupSizeId = returndata.GroupSizeId;
									widget.TemplateJs = returndata.TemplateJs;
									widget.TemplateUrl = returndata.TemplateUrl;
									widget.TemplateParams = returndata.TemplateParams

									if (returndata.hasOwnProperty('Param')) {
										widget.Param = returndata.Param;
									}
									else {
										Utils.RemoveProperty(widget, "Param");
									}
									if (returndata.hasOwnProperty('Title')) {
										widget.Title = returndata.Title;
									}
									else {
										Utils.RemoveProperty(widget, "Title");
									}
									if (returndata.hasOwnProperty('TypeSize')) {
										widget.TypeSize = returndata.TypeSize;
									}
									else {
										Utils.RemoveProperty(widget, "TypeSize");
									}

									if (returndata.hasOwnProperty('TemplateDisable')) {
										widget.TemplateDisable = returndata.TemplateDisable;
									}
									else {
										widget.TemplateDisable = false;
										//Utils.RemoveProperty(widget, "TemplateDisable");
									}
									//if (returndata.hasOwnProperty('Order')) {
									//	widget.Order = returndata.Order;
									//}
									//else {
									//	Utils.RemoveProperty(widget, "Order");
									//}
									//if (returndata.hasOwnProperty('PositionId')) {
									//	widget.PositionId = returndata.PositionId;
									//}
									//else {
									//	Utils.RemoveProperty(widget, "PositionId");
									//}
									if (returndata.hasOwnProperty('Style')) {
										widget.Style = returndata.Style;
									}
									//else {
									//	Utils.RemoveProperty(widget, "Style");
									//}
									if (returndata.hasOwnProperty('StyleId')) {
										widget.StyleId = returndata.StyleId;
									}
									//else {
									//	Utils.RemoveProperty(widget, "StyleId");
									//}
									//widget = angular.copy(returndata);
									$scope.$broadcast(widgetsvc.Resources.Event.widgetReresh, widget);
								}

								$scope.$emit(widgetsvc.Resources.Event.modelChanged, returndata);
							});
						}
					}

                    if (angular.isDefined($scope.column.Rows) && angular.isArray($scope.column.Rows)) {
                        var rowTemplate = widgetsvc.Resources.Template.rowTemplate;
                        $compile(rowTemplate)($scope, function(data) {
                            $element.append(data);
                        });
                    } else {
                        sortable = sortableColumn($scope, $element, $attr, $scope.column, $scope.model);
                    }
                }
            }

            function setSorableOption(sortab, numberWidget, maxWidget) {
                var isput = numberWidget < maxWidget;
                sortab.option(widgetsvc.Resources.Attributes.group, {
                    name: widgetsvc.Resources.Attributes.widgets,// + groupId,
                    pull: true,
                    put: isput
                });
            }

            function findWidget(column, index) {
                var widget = null;
                for (var i = 0; i < column.Widgets.length; i++) {
                    var w = column.Widgets[i];
                    if (w.Id === index) {
                        widget = w;
                        break;
                    }
                }
                return widget;
            }

            function addWidgetElement($scope, model, column, e, sortable) {
                var idstr = e.item.getAttribute(widgetsvc.Resources.Attributes.cmsWidgetId);
                var id = idstr ? parseInt(idstr) : -1;
                var widclumn = widgetsvc.DragCurrentColumn;
                var wd = findWidget(widclumn, id);
                if (wd) {
                    wd.PositionId = $scope.column.PositionId;
                    $scope.$apply(function () {
                        $scope.column.Widgets.splice(e.newIndex, 0, wd);
                        synWidgetOrder($scope.column.Widgets);
                        //column.Widgets.push(wd);
                    });
                   // e.from.insertBefore(e.item, nextSibling);
                }

                setSorableOption(sortable, $scope.column.Widgets.length, $scope.column.Group.MaxWidgets);//, $scope.column.Group.Id);
            }

            function removeWidgetElement($scope, model, col, e, sortable) {
                var idstr = e.item.getAttribute(widgetsvc.Resources.Attributes.cmsWidgetId);
                var idw = idstr ? parseInt(idstr) : -1;
                var wdleng = $scope.column.Widgets.length;
                for (var i = 0; i < wdleng; i++) {
                    if ($scope.column.Widgets[i].Id === idw) {
                        $scope.$apply(function() {
                            $scope.column.Widgets.splice($scope.column.Widgets.indexOf($scope.column.Widgets[i]), 1);
                            synWidgetOrder($scope.column.Widgets);
                        });
                        break;
                    }
                }
                setSorableOption(sortable, $scope.column.Widgets.length, $scope.column.Group.MaxWidgets);//, $scope.column.Group.Id);
            }

            function sortableColumn($scope, $element, $attr, column, model) {
                var el = $element.find(widgetsvc.Resources.Attributes.widgetsortable)[0];
                var isput = column.Widgets.length < $scope.column.Group.MaxWidgets;
                var sortable = dySortable.create(el, {
                    group: {
                        name: widgetsvc.Resources.Attributes.widgets, // + $scope.column.Group.Id,
                        pull: true,
                        put: isput
                    },
                    handle: widgetsvc.Resources.Attributes.handle,
					filter: ".js-remove, .js-edit",
					onFilter: function (evt) { //ThangPham, fix issue: ng-click not work with sortable on mobile, June 18 2015
						var item = evt.item,
							ctrl = evt.target;
						var currentWidget;

						if (item.attributes.hasOwnProperty(widgetsvc.Resources.Attributes.cmsWidgetId)) {
							var widgetId = item.attributes[widgetsvc.Resources.Attributes.cmsWidgetId].value;
							angular.forEach($scope.column.Widgets, function(widget, i){
								if (widgetId == widget.Id)
									currentWidget = widget;
							});
						}

						if (ctrl.className.indexOf("js-remove") != -1) {  // Click on remove button
							console.log('remove'); // remove sortable item
							var modalData = {
								headerText: AppDefine.Resx.HEADER_CONFIRM_DELETE,
								bodyText: AppDefine.Resx.REMOVE_WIDGET_CONFIRM
							}
							var modalOptions = {
								backdrop: true,
								keyboard: true,
								modalFade: true,
								templateUrl: 'Widgets/DeleteDialog.html',
								size: 'sm'
							}
							dialogSvc.showModal(modalOptions, modalData).then(function (result) {
								if (result === AppDefine.ModalConfirmResponse.OK) {
							$scope.column.Widgets.splice($scope.column.Widgets.indexOf(currentWidget), 1);
							synWidgetOrder($scope.column.Widgets);
									//$scope.$apply();
							$scope.$emit(widgetsvc.Resources.Event.widgetRemove, $scope.column);
							$scope.$emit(widgetsvc.Resources.Event.modelChanged);
						}
							});
						}
						else if (ctrl.className.indexOf("js-edit") != -1) {  // Click on edit link
							console.log('edit');
							$scope.$emit(widgetsvc.Resources.Event.widgetChange, currentWidget);
						}
					},
                    ghostClass: widgetsvc.Resources.Attributes.ghostClass,
                    animation: 150,
                    onStart: function(evt) {
						console.log('sortable - start event');
                        widgetsvc.DragCurrentColumn = $scope.column;
                    },
                    onAdd: function(evt) {
						console.log('sortable - add event');
                        addWidgetElement($scope, model, column, evt, sortable);
						$scope.$emit(widgetsvc.Resources.Event.modelChanged, column);
                    },
                    onRemove: function(evt) {
						console.log('sortable - remove event');
                        removeWidgetElement($scope, model, column, evt, sortable);
                        $scope.$emit(widgetsvc.Resources.Event.modelChanged);
                    },
                    onUpdate: function(evt) {
						console.log('sortable - update event');
                        $scope.$apply(function() {
                            $scope.column.Widgets.splice(evt.newIndex, 0, $scope.column.Widgets.splice(evt.oldIndex, 1)[0]);
                            synWidgetOrder($scope.column.Widgets);
                        });
                        $scope.$emit(widgetsvc.Resources.Event.modelChanged);
                    }
                });
                $element.on(widgetsvc.Resources.Event.destroy, function() {
                    sortable.destroy();
                });

                return sortable;
            }
        }

        function dashboardRowDt($compile, widgetsvc) {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    row: '=',
                    editMode: '=',
                    model: '='
                },
                templateUrl: widgetsvc.Resources.Template.row,
                link: function($scope, $element, $attrs) {
                    if (angular.isDefined($scope.row.Columns) && angular.isArray($scope.row.Columns)) {
                        var columnTempate = widgetsvc.Resources.Template.columnTempate;
                        $compile(columnTempate)($scope, function(data) {
                            $element.append(data);
                        });
                    }
                }
            }
        }

        function cmswidget($compile, $injector, $http, $q, $timeout, $templateCache, widgetsvc) {
            return {
                restrict: 'EA',
                replace: true,
                scope: {
                    isLoading: '=',
                    widgetModel: '='
                },
                require: '?ngModel',
                link: function(scope, elem, attr, ngModel) {
                    var widget = scope.widgetModel;

                    compileWidget(scope, elem, attr, widget).finally(function () {
                        scope.isLoading = false;
                    });

                    scope.$on(widgetsvc.Resources.Event.widgetReresh, function (e, data) {
                        scope.isLoading = true;
                        compileWidget(scope, elem, attr, scope.widgetModel).finally(function () {
                            scope.isLoading = false;
                        });
                    });
                }
            };

            function requireLoad(file) {
                var defer = $q.defer();
                require(file, function() {
                    defer.resolve();
                }, function() {
                    defer.reject();
                });

                return defer.promise;
            }

            function getTemplate(widget) {
                var deferred = $q.defer();

                if (widget.TemplateUrl) {
                    var tpl = $templateCache.get(widget.TemplateUrl);
                    if (tpl) {
                        deferred.resolve(tpl);
                    } else {
                        $http.get(widget.TemplateUrl)
                            .success(function(response) {
                                $templateCache.put(widget.TemplateUrl, response);
                                deferred.resolve(response);
                            })
                            .error(function() {
                                deferred.reject();
                            });
                    }
                }

                return deferred.promise;
            }

            function compileWidget($scope, $element, $attrs, widget) {

                var resolve = [];
                var defer = $q.defer();
                try {
                    if (widget.TemplateJs) {

                        resolve.push(requireLoad([widget.TemplateJs]));
                    }

                    $q.all(resolve).then(function(result) {
                        $scope.widget = widget;
                        getTemplate(widget).then(function(r) {

                            if ($scope.$$childTail) {
                                $scope.$$childTail.$destroy();
                            }

                            $compile(r)($scope, function(data) {
                                $element.empty();
                                $element.append(data);
                                defer.resolve();
                                $timeout(angular.noop, 500);
                            });
                        });
                    }, function() {
                        defer.reject();
                        $scope.isLoading = false;
                    });
                } catch (e) {
                    $q.reject();
                    defer.reject();
                    $scope.isLoading = false;
                }
                return defer.promise;
            }
        }
    });
})();