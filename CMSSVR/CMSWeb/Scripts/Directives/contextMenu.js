//github.com/ianwalter/ng-context-menu/blob/development/src/ng-context-menu.js
(function() {
    'use strict';

    define(['cms'], contextMenuDirective);

    function contextMenuDirective(cms) {
        cms.register.directive('contextMenu', contextMenu);

        contextMenu.$inject = ['$document'];

        function contextMenu($document) {
            return {
                restrict: 'A',
                scope: {
                    callback: '&contextMenu',
                    disabled: '&contextMenuDisabled',
                    closeCallback: '&contextCallbackClose',
                    marginBottom: '&contextMenuMarginBottom'
                },
                link: function($scope, $element, $attrs) {
                    var opened = false;
                    var menuEle = null;
                    var ele = null;

                    function open(event, menuElement) {
                        menuElement.addClass('open');

                        var doc = $document[0].documentElement;
                        var docLeft = (window.pageXOffset || doc.scrollLeft) - (doc.scrollLeft || 0);
                        var docTop = (window.pageYOffset || doc.scrollTop) - (doc.clientTop || 0);
                        var elementWidth = menuElement[0].scrollWidth;
                        var elementHeight = menuElement[0].scrollHeight;
                        var docWidth = doc.clientWidth + docLeft;
                        var docHeight = doc.clientHeight + docTop;
                        var totalWidth = elementWidth + event.pageX;
                        var totalHeight = elementHeight + event.pageY;
                        var left = Math.max(event.pageX - docLeft, 0);
                        var top = Math.max(event.pageY - docTop, 0);

                        if (totalWidth > docWidth) {
                            left = left - (totalWidth - docWidth);
                        }

                        if (totalHeight > docHeight) {
                            var marginBottom = $scope.marginBottom || 0;
                            top = top - (totalHeight - docHeight) - marginBottom;
                        }

                        menuElement.css('top', top + 'px');
                        menuElement.css('left', left + 'px');
                        opened = true;
                    }

                    function close(menuElement) {
                        menuElement.removeClass('open');
                        if (opened) {
                            $scope.closeCallback();
                        }

                        opened = false;
                    }

                    $element.bind('contextmenu', function(event) {
                        if (!$scope.disabled()) {
                            if (menuEle !== null) {
                                close(menuEle);
                            }

                            menuEle = angular.element(document.getElementById($attrs.target));

                            ele = event.target;

                            event.preventDefault();
                            event.stopPropagation();
                            $scope.$apply(function() {
                                $scope.callback({ $event: event });
                            });

                            $scope.$apply(function() {
                                open(event, menuEle);
                            });
                        }
                    });

                    function handleKeyUpEvent(event) {
                        if (!$scope.disabled() && opened && event.keyCode === 27) {
                            $scope.$apply(function() {
                                close(menuEle);
                            });
                        }
                    }

                    function handleClickEvent(event) {
                        if (!$scope.disabled() && opened && (event.button !== 2 || event.target !== ele)) {
                            $scope.$apply(function() {
                                close(menuEle);
                            });
                        }
                    }

                    $document.bind('keyup', handleKeyUpEvent);
                    $document.bind('click', handleClickEvent);
                    $document.bind('contextmenu', handleClickEvent);

                    $scope.$on('$destroy', function() {
                        $document.unbind('keyup', handleKeyUpEvent);
                        $document.unbind('click', handleClickEvent);
                        $document.unbind('contextmenu', handleClickEvent);
                    });
                }
            };
        }
    }
})();