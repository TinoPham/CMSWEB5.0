(function () {
	'use strict';
	define([], function () {
		var module = angular.module('cms.directives', []);

		module.directive('toggleNavCollapsedMin', ['$rootScope', function ($rootScope) {
			return {
				restrict: 'A',
				link: function (scope, ele, attrs) {
					var app = $('#app');
					return ele.on('click', function (e) {
						if (app.hasClass('nav-collapsed-min')) {
							app.removeClass('nav-collapsed-min');
						} else {
							app.addClass('nav-collapsed-min');
							$rootScope.$broadcast('nav:reset');
						}
						return e.preventDefault();
					});
				}
			};//end return
		} //end function
		]);//end directive

		module.directive("scroll", ['$window', function ($window) {
			return {
				restrict: 'AE',
				link: function ($scope, element, attrs) {
					angular.element($window).bind("scroll", function () {
						if (this.pageYOffset >= 100) {
							$scope.boolChangeClass = true;
							// console.log('Scrolled below header.');
						} else {
							$scope.boolChangeClass = false;
							// console.log('Header is in view.');
						}
						$scope.$applyAsync();
					});
				}
			};
		}]);

        module.directive('inputRestrictor', [function() {
            return{
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element, attr, ngModelCtrl) {
                    var pattern = new RegExp(attr.inputRestrictor, "g"); // /[^0-9a-zA-Z !\\#$%&+,\-.\/:;<=>?@\[\]^_{|}~]*/g;

                    function inputstr(text) {
                        if (!text)
                            return text;
                        var transformedInput = text.replace(pattern, '');
                        if (transformedInput !== text) {
                            ngModelCtrl.$setViewValue(transformedInput);
                            ngModelCtrl.$render();
                        }
                        return transformedInput;
                    }
                    ngModelCtrl.$parsers.push(inputstr);
                }
            }
        }]);

        module.directive('modaldraggable', function ($document) {

            return {
                restrict: 'A',
                replace: false,
                link: link
            };
            function link(scope, element) {
                var startX = 0,
                  startY = 0,
                  x = 0,
                  y = 0;
             

                element.on('mousedown', function (event) {
                    element = angular.element(document.getElementsByClassName("modal-dialog"));
                    console.log("added directive");

                    event.preventDefault();
                    startX = event.screenX - x;
                    startY = event.screenY - y;
                    $document.on('mousemove', mousemove);
                    $document.on('mouseup', mouseup);
                });

                function mousemove(event) {
                    y = event.screenY - startY;
                    x = event.screenX - startX;
                    element.css({
                        top: y + 'px',
                        left: x + 'px'
                    });
                }

                function mouseup() {
                    $document.unbind('mousemove', mousemove);
                    $document.unbind('mouseup', mouseup);
                }
            };
        });

		module.directive('clickElsewhere', function ($parse, $rootScope) {
			return {
				restrict: 'A',
				compile: function ($element, attr) {
					var fn;
					fn = $parse(attr['clickElsewhere']);
					return function (scope, element) {
						var offEvent;
						offEvent = $rootScope.$on('click', function (event, target) {

							// console.log( typeof( target) );
							console.log(target);

							if (element.find($(target)).length || element.is($(target))) {
								console.log(target);
								return;
							}
							return scope.$apply(function () {
								return fn(scope);
							});
						});
						return scope.$on('$destroy', offEvent);
					};
				}
			};
		});

		module.directive('menuDropdown', function () {
			return {
				restrict: 'A',
				link: function (scope, elem, attrs) {

					elem.bind('click', handleLeftBar);
                    
                    if (document.body.clientWidth > 1024) {
					elem.bind('mouseover', handleLeftBarOver);
                    }

					function handleLeftBarOver(e) {
						if ($(this).parent().hasClass("has_sub")) {
							e.preventDefault();
						}
						if ($("#wrapper").hasClass("enlarged")) {

							if (!$(this).parent().hasClass("parent")) {
								return;
							}

							if ($(this).parent().hasClass("subdrop")) {
								return;
							} else {
								$(this).parent().addClass("subdrop");
								$(this).parent().next("ul").slideUp(350);
								//elem.off('click', handleLeftBar);
							}
						}
					}

					function handleLeftBar(e) {
						if ($(this).parent().hasClass("has_sub")) {
							e.preventDefault();
						}

						if (!$("#wrapper").hasClass("enlarged")) {
							//Left bar Large mode
							if ($(this).hasClass("subdrop")) {
								$(this).removeClass("subdrop");
								$(this).next("ul").slideUp(350);
								$(".pull-right i", $(this).parent()).removeClass("icon-up-open-mini").addClass("icon-down-open-mini");
							}
							else {
								// hide any open menus and remove all other classes
								$("ul", $(this).parents("ul:first")).slideUp(350);
								$("a", $(this).parents("ul:first")).removeClass("subdrop");
								$("#sidebar-menu .pull-right i").removeClass("icon-up-open-mini").addClass("icon-down-open-mini");

								//// open our new menu and add the open class
								$(this).next("ul").slideDown(350);
								$(this).addClass("subdrop");
								$(".pull-right i", $(this).parents(".has_sub:last")).removeClass("icon-down-open-mini").addClass("icon-up-open-mini");
								$(".pull-right i", $(this).siblings("ul")).removeClass("icon-up-open-mini").addClass("icon-down-open-mini");

								//ThangPha, Trigger close left bar, July 08 2015
								if ($('body').hasClass("mobile") && !$(this).parent().hasClass("has_sub")) {
									$("#btn-open-left").trigger("click");
								}
							}
						}
						else {
							//if ($(this).parent().hasClass("parent")) {

							//    if ($(this).parent().hasClass("subdrop")) {
							//        return;
							//    }
							//}
							//Left bar Small mode
							if ($(this).parent().hasClass("has_sub")) {
								if ($(this).parent().hasClass("subdrop")) {
									$(this).parent().removeClass("subdrop");
									$(this).next("ul").slideUp(350);
									if (!$(this).parent().hasClass("parent")) {
										$(this).find("i").removeClass("icon-up-open-mini").addClass("icon-down-open-mini");
									}
								}
								else {
									$("ul", $(this).parents("ul:first")).slideUp(350);
									$("li", $(this).parents("ul:first")).removeClass("subdrop");

									$(this).next("ul").slideDown(350);
									$(this).parent().addClass("subdrop");
									if (!$(this).parent().hasClass("parent")) {
										$(this).find("i").removeClass("icon-down-open-mini").addClass("icon-up-open-mini");
									}
								}
							}
							else {
								//ThangPham, Trigger close left bar, July 08 2015
								$(this).closest('.parent').addClass("clicked").children("a:first").trigger("click");
							}
						}
					}

					scope.$on('$destroy', function () {
						elem.off('click', handleLeftBar);
						elem.off('mouseover', handleLeftBarOver);
					});

				}
			};
		});

		module.directive('btnLeftBar', function () {
			return {
				restrict: 'A',
				controller: ['$scope', '$timeout', function ($scope, $timeout) {
					$timeout(function () {
						setSideLeftForMobile();
					}, 30);
				}],
				link: function (scope, ele, attrs) {
					ele.bind('click', function (e) {
						// e.stopPropagation();
						$("#wrapper").toggleClass("enlarged");
						$("#wrapper").addClass("forced");

						toggle_click();
						$(window).resize(); //Dispatched event to resize.nggrid
						//toggle_slimscroll(".slimscrollleft");
						setSideLeftForMobile();
					});

					//Handler rezie event
					$(window).on('resize', setSideLeftForMobile);
				}
			};

			function toggle_click() {
				if ($("#wrapper").hasClass("enlarged")) {
					if ($("body").hasClass("fixed-left")) {
						$("body").removeClass("fixed-left").addClass("fixed-left-void");
						$(".topbar-profile-bam").css("margin-left", 0);
						$(".topbar-compare").css("margin-left", 0);
					} else {
						$(".topbar-profile-bam").css("margin-left", 50);
						$(".topbar-compare").css("margin-left", 50);
					}
					$(".left ul").removeAttr("style");



					if ($("body").hasClass("mobile")) {
						// Set header in BAM
						$(".topbar-profile-bam").css("margin-left", 0);
						$(".topbar-compare").css("margin-left", 0);
					} else {
						// Set header in BAM
						$(".topbar-profile-bam").css("margin-left", 50);
						$(".topbar-compare").css("margin-left", 50);
					}
				}
				else {
					if ($("body").hasClass("fixed-left-void")) {
						$("body").removeClass("fixed-left-void").addClass("fixed-left");

						//// Set header in BAM
						//$(".topbar-profile-bam").css("margin-left", 240);
						//$(".topbar-compare").css("margin-left", 240);
					}

					if ($("body").hasClass("mobile")) {
						// Set header in BAM
						$(".topbar-profile-bam").css("margin-left", 0);
						$(".topbar-compare").css("margin-left", 0);
					} else {
						// Set header in BAM
						$(".topbar-profile-bam").css("margin-left", 240);
						$(".topbar-compare").css("margin-left", 240);
					}
					$(".subdrop").siblings("ul:first").show();  //Auto expand the first element with tag 'ul' that has '.subdrop' class.


				}


			}

			function toggle_slimscroll(item) {
				if ($("#wrapper").hasClass("enlarged")) {
					$(item).css("overflow", "inherit").parent().css("overflow", "inherit");
					$(item).siblings(".slimScrollBar").css("visibility", "hidden");
				} else {
					$(item).css("overflow", "hidden").parent().css("overflow", "hidden");
					$(item).siblings(".slimScrollBar").css("visibility", "visible");
				}
			}

			function setSideLeftForMobile() {
				var w = $(window).width();
				var h = $(window).height();
				var dw = $(document).width();
				var dh = $(document).height();

				$('.slimscrollleft').slimScroll({ height: 'auto', position: 'left', size: '3px' });

				if (!$("#wrapper").hasClass("forced")) {
					if (w > 1024) {
						$("#wrapper").removeClass("enlarged");
					}
					else {
						$("#wrapper").addClass("enlarged");
						$(".left ul").removeAttr("style");
					}

					toggle_click();
				}

				// Set footer for width brower < 767
				//if (w <= 767) {
				//    if ($("#wrapper").hasClass("enlarged")) {
				//        $(".footer").css('position', 'absolute');
				//    } else {
				//        $(".footer").css('position', 'fixed');
				//    }
				//} else {
				//    $(".footer").css('position', 'fixed');
				//}


				toggle_slimscroll(".slimscrollleft");

				var strDevice = navigator.userAgent.toLowerCase();
				var isWindowsPhone = false;
				if (strDevice.search("tablet") >= 0 & strDevice.search("trident") >= 0) {
					isWindowsPhone = true;
				}

				if (isWindowsPhone) {
					if (!$("#wrapper").hasClass("enlarged")) {
						$(".slimscrollleft").css("overflow-y", "auto").parent().css("overflow", "hidden");
						$(".slimscrollleft").siblings(".slimScrollBar").css("visibility", "hidden");
					}
				}
			}
		});

		module.directive('cmsDraggable', [function () {
			return {
				restrict: 'A',
				scope: {
					onDrop: '=',
					onOver: '=',
					onEnter: '=',
					onLeave: '=',
					onDrag: '=',
					onDragStart: '='
				},
				link: function (scope, elm) {

					function drop(e) {
						var node = getnode(e);
						scope.onDrop(e, node);
					}

					function dragover(e) {
						var node = getnode(e);
						scope.onOver(e, node);
					}

					function dragenter(e) {
						var node = getnode(e);
						scope.onEnter(e, node);
					}

					function dragleave(e) {
						var node = getnode(e);
						scope.onLeave(e, node);
					}

					function getnode(e) {
						var data = e.dataTransfer ? e.dataTransfer.getData("text/plain") : undefined;
						if (!data) return {};
						return data;
					}

					function drag(e) {
						var node = getnode(e);
						scope.onDrag(e, node);
					}

					function dragstart(e) {
						var node = getnode(e);
						scope.onDragStart(e, node);
					}

					if (scope.onDrop && typeof scope.onDrop === 'function') elm.bind('drop', drop);

					if (scope.onDrag && typeof scope.onDrag === 'function') elm.bind('drag', drag);

					if (scope.onDragStart && typeof scope.onDragStart === 'function') elm.bind('dragstart', dragstart);

					if (scope.onOver && typeof scope.onOver === 'function') elm.bind('dragover', dragover);

					if (scope.onEnter && typeof scope.onEnter === 'function') elm.bind("dragenter", dragenter);

					if (scope.onLeave && typeof scope.onLeave === 'function') elm.bind("dragleave", dragleave);

					scope.$on('$destroy', function () {
						elm.off('drop', drop);
						elm.off("drag", drag);
						elm.off("dragstart", dragstart);
						elm.off('dragover', dragover);
						elm.off("dragenter", dragenter);
						elm.off("dragleave", dragleave);
					});
				}
			}
		}]);

		module.directive('cmsEnter', ['AppDefine', function (AppDefine) {
			return {
				restrict: 'A',
				scope: {
					vailddata: '=',
					callFn: '&'
				},
				link: function (scope, element, attrs) {
					element.bind("keydown keypress", function (event) {
						if (event.which === AppDefine.keyCodes.enter) {
							var vaildata = scope.vailddata;
							if (!vaildata) {
								scope.callFn();
								event.preventDefault();
							}
						}
					});
				}
			};
		}//end function
		]);//end directive

		module.directive('loadingIndicator', function () {
			return {
				restrict: 'E',
				template: "<div class='col-lg-12' ng-show=\"!vm.ready\"><h1>Loading <i class='icon-cog fa-spin'></i></h1></div>",
				link: function (scope, elem, attrs) {

				}
			};
		});

		module.directive('tooltipHtml', [
            '$document', '$compile', '$q', 'cmscompile', '$timeout', function ($document, $compile, $q, cmscompile, $timeout) {
            	var wraptooltip = '<div class="tooltip-wrap-content"></div>';
            	var tooltipcontent = angular.element(wraptooltip);
            	return {
            		restrict: 'A',
            		scope: {
            			model: '=',
            			parentModel: '='
            		},
            		link: function (scope, elem, attrs) {

            			var delay = attrs.delay ? attrs.delay : 3000;
            			if (!attrs.js || !attrs.url || !attrs.tooltipId) return;
            			var contentbuilded;

            			var hide = {
            				instance: undefined,
            				show: function (e) {
            					if (contentbuilded) {
            						var getcurrenttooltip = angular.element('#' + attrs.tooltipId);
            						if (getcurrenttooltip && getcurrenttooltip.length > 0) {
            							var offest = elem.offset();
            							tooltipcontent.css({ left: e.pageX, top: e.pageY, position: 'absolute', display: 'block', 'z-index': '100' });
            						} else {
            							bindtoBody(contentbuilded, e);
            						}
            					} else {
            						compileUrl(scope, attrs.js, attrs.url).then(function (content) {
            							content.attr("id", attrs.tooltipId);
            							contentbuilded = content;
            							bindtoBody(content, e);
            						});
            					}
            				},
            				hide: function (e) {
            					$timeout.cancel(hide.instance);

            					hide.instance = $timeout(function () {
            						tooltipcontent.css('display', 'none');
            					}, delay);
            				},
            				cancel: function () {
            					$timeout.cancel(hide.instance);
            				}
            			}

            			function bindtoBody(content, e) {
            				tooltipcontent.empty();
            				tooltipcontent.append(content);
            				tooltipcontent.css({ left: e.pageX, top: e.pageY, position: 'absolute', display: 'block', 'z-index': '100' });

            			}

            			var isadded = angular.element('.tooltip-wrap-content');
            			if (isadded.length <= 0) {
            				angular.element(document.body).append(tooltipcontent);
            			}

            			elem.on('mouseover touchstart', elemOverFn).on('mouseout touchmove', elemOutFn);
            			tooltipcontent.on('mouseover touchstart', tooltipcontentOverFn).on('mouseout touchmove', tooltipOutFn);

            			function compileUrl(scope, js, url) {
            				var resolve = [];
            				var defer = $q.defer();
            				try {
            					resolve.push(cmscompile.requireLoad([js]));
            					$q.all(resolve).then(function (result) {
            						cmscompile.getTemplate(url).then(function (r) {
            							var template = angular.element(r);
            							var compiledcontent = $compile(template)(scope);
            							defer.resolve(compiledcontent);
            						});

            					}, function () {
            						defer.reject();
            					});
            				} catch (e) {
            					$q.reject();
            					defer.reject();
            				}
            				return defer.promise;
            			}

            			function elemOverFn(e) {
            				hide.show(e);
            				hide.cancel();
            			}

            			function elemOutFn(e) {
            				hide.hide();
            			}

            			function tooltipcontentOverFn(e) {
            				hide.cancel();
            			}

            			function tooltipOutFn(e) {
            				hide.hide();
            			}

            			scope.$on('$destroy', function () {
            				elem.off('mouseover touchstart', elemOverFn);
            				elem.off('mouseout touchmove', elemOutFn);
            				tooltipcontent.off('mouseover touchstart', tooltipcontentOverFn);
            				tooltipcontent.off('mouseout touchmove', tooltipOutFn);
            				angular.element(tooltipcontent).remove();
            			});
            		}
            	}
            }
		]);

		module.directive('popoverHtml', ['$document', '$compile', '$q', 'cmscompile', '$timeout', function ($document, $compile, $q, cmscompile, $timeout) {
			var template = "<div class='popover-html'><div>Loading...</div><div>";
			return {
				restrict: 'EA',
				scope: {
					htmlUrl: '@',
					jsUrl: '@',
					placement: '@',
					model: '=',
					isValid: '=',
					mouseover: '=',
					parentModel: '='
				},
				link: function (scope, elem, attr) {
					var hide = {
						instance: undefined,
						show: function () {
							elmcontent.css('display', 'block');
						},
						hide: function () {
							$timeout.cancel(hide.instance);

							hide.instance = $timeout(function () {
								elmcontent.css('display', 'none');
							}, 3000, false);
						},
						cancel: function () {
							$timeout.cancel(hide.instance);
						}
					};

					scope.isLoaded = false;
					var popover = angular.element(template);

					var initPosition = '<div class=" popover-html-content popover ' + scope.placement + '"></div>';

					var elmcontent = angular.element(initPosition);
					elmcontent.css('position', 'absolute').css('display', 'none');

					elem.append(elmcontent);

					if (scope.mouseover && scope.mouseover == true) {
						elem.on('mouseover', function () {
							scope.showPopup();
							hide.cancel();
						}).on('mouseout', function () {
							hide.hide();
						});

						elmcontent.on('mouseout', function () {
							hide.hide();
						}).on('mouseover', function () {
							hide.cancel();
						});
					} else {
						elem.on('click', insideClickHandler);
						$document.on('click', outsideClickHandler);
					}

					scope.showPopup = function () {

						if (!scope.isValid) return;

						if (scope.isLoaded === false) {
							compileTemplate(scope, elem, attr, popover).then(function () {
								elmcontent.css('position', 'absolute').css('display', 'block');
								scope.isLoaded = true;
							});
						} else {
							var elmpopover = elem.find('.popover-html-content');
							elmcontent.css('position', 'absolute').css('display', 'block');
							positionPlacehold(elmpopover, scope.placement, elem, scope);

						}
					}

					scope.selecttoggle = function (e) {
						outsideClickHandler(e);
					}

					function insideClickHandler(e) {
						scope.showPopup();
					}

					function outsideClickHandler(e) {

						var isChild = elem.find(e.target).length > 0;

						if (!isChild) {
							elmcontent.css('position', 'absolute').css('display', 'none');
							//console.log(e);
						}
					}

					scope.$on('$destroy', function () {

						if (scope.mouseover && scope.mouseover == true) {
							elem.off('mouseover', insideClickHandler);
							elem.off('mouseout', insideClickHandler);
							elmcontent.off('mouseover', insideClickHandler);
							elmcontent.off('mouseout', insideClickHandler);
						} else {
							elem.off('click', insideClickHandler);
							$document.off('click', outsideClickHandler);
						}
						console.log('dispaly destry');
						popover.remove();

					});

				}
			}

			function positionPlacehold(popover, placement, elem, scope) {
				var align = 'right';

				if (!placement) {
					placement = 'right';
				}

				var popoverRect = getBoundingClientRect(popover[0]);
				var rect = getBoundingClientRect(elem[0]);
				var top, left;

				var doc = getDocumentSize();

				var positionX = function () {
					if (align === 'center') {
						return Math.round(rect.width / 2 - popoverRect.width / 2);
					} else if (align === 'right') {
						return -popoverRect.width;
					}
					return 0;
				};

				var positionY = function () {
					if (align === 'center') {
						return Math.round(rect.height / 2 - popoverRect.height / 2);
					} else if (align === 'bottom') {
						return -popoverRect.height;
					}
					return 0;
				};

				if (placement === 'top') {
					if (rect.top - popoverRect.height > 0) {
						top = -popoverRect.height;
						left = positionX();
					} else {
						top = 0;
						left = positionX();
					}
				} else if (placement === 'right') {
					if (rect.right + popoverRect.width > doc.right) {
						top = positionY();
						left = 0 - popoverRect.width;
						popover.removeClass('right');
						popover.addClass('left');
					} else {
						top = positionY();
						left = rect.width;
						popover.removeClass('left');
						popover.addClass('right');
					}
				} else if (placement === 'bottom') {
					if (rect.bottom + popoverRect.height < doc.height) {
						top = 0;
						left = positionX();
					} else {
						top = -popoverRect.height;
						left = positionX();
					}
				} else if (placement === 'left') {
					if (rect.left - popoverRect.width > 0) {
						top = positionY();
						left = 0 - popoverRect.width;
						popover.removeClass('right');
						scope.placement = 'left';
						popover.addClass('left');
					} else {
						top = positionY();
						left = rect.width;
						popover.removeClass('left');
						scope.placement = 'right';
						popover.addClass('right');
					}
				}

				popover
                    .css('z-index', '1051')
                  .css('top', top + 'px')
                  .css('left', left + 'px');


				var elmpopover = popover.find('.popoverhtml-arrow');
				if (elmpopover.length > 0) {
					return;
				}

				var toptri = rect.height / 2 + 10;
				var elemTriangle = angular.element('<div class="arrow popoverhtml-arrow"></div>');
				elemTriangle.css('top', 15 + 'px');
				//  .css('left', 0 + 'px');
				popover.append(elemTriangle);


			}

			function getDocumentSize() {
				var doc = document.documentElement || document.body.parentNode || document.body;

				//var x = document.getElementsByTagName("body");
				var modal = angular.element(document).find('.modal-dialog')[0];

				if (modal) {
					return {
						height: modal.clientHeight,
						width: modal.clientWidth,
						right: modal.clientWidth + modal.offsetLeft,
						offsetWidth: modal.offsetWidth,
						offsetHeight: modal.offsetHeight,
						scrollHeight: modal.scrollHeight,
						scrollWidth: modal.scrollWidth
					}
				}

				return {
					height: doc.clientHeight,
					width: doc.clientWidth,
					offsetWidth: doc.offsetWidth,
					offsetHeight: doc.offsetHeight,
					scrollHeight: doc.scrollHeight,
					scrollWidth: doc.scrollWidth
				}
			}

			function getBoundingClientRect(elm) {
				var w = window;
				var doc = document.documentElement || document.body.parentNode || document.body;
				var x = w.pageXOffset ? w.pageXOffset : doc.scrollLeft;
				var y = w.pageYOffset ? w.pageYOffset : doc.scrollTop;
				var rect = elm.getBoundingClientRect();

				// ClientRect class is immutable, so we need to return a modified copy
				// of it when the window has been scrolled.
				if (x || y) {
					return {
						bottom: rect.bottom + y,
						left: rect.left + x,
						right: rect.right + x,
						top: rect.top + y,
						height: rect.height,
						width: rect.width
					};
				}
				return rect;
			}

			function compileTemplate(scope, elem, attr, templatebuild) {
				var resolve = [];
				var defer = $q.defer();
				try {
					if (scope.jsUrl) {

						resolve.push(cmscompile.requireLoad([scope.jsUrl]));
					}

					$q.all(resolve).then(function (result) {

						cmscompile.getTemplate(scope.htmlUrl).then(function (r) {
							var $popover = templatebuild;
							$popover.html(r);

							$compile($popover)(scope, function (data) {
								//elem.empty();
								var elmcontent = elem.find('.popover-html-content');
								elmcontent.css('position', 'absolute').css('display', 'block');

								//var test = elem.find('cms-select-container');
								elmcontent.empty();
								elmcontent.append(data);

								positionPlacehold(elmcontent, scope.placement, elem, scope);
								//elem.replaceWith(data);
								defer.resolve();
							});

						});

					}, function () {
						defer.reject();

					});
				} catch (e) {
					$q.reject();
					defer.reject();
				}
				return defer.promise;
			}
		}]);

		module.directive('resizer', ['$document', '$timeout', function ($document, $timeout) {

			return function ($scope, elem, attrs) {
				var elemresize = angular.element('<div class="sidebar-resizer"></div>');
				var resizerscope, offsetLeftScope, resizerScopeWidth, resizerRight, resizerLeft, resizeTime;
				var percentRight, percentLeft;
				elem.on('touchstart mousedown', mousestart);

				function mousestart(event) {
					event.preventDefault();

					resizerscope = angular.element(attrs.resizerScope);
					resizerscope.append(elemresize);
					offsetLeftScope = findOffsetLeft(resizerscope[0]);
					resizerScopeWidth = parseInt(resizerscope.css('width'));
					resizerRight = angular.element(attrs.resizerRight);
					resizerLeft = angular.element(attrs.resizerLeft);
					$document.on('touchmove mousemove', mousemove);
					$document.on('touchend touchcancel mouseup', mouseup);
				}

				function mousemove(event) {

					if (resizeTime) {
						$timeout.cancel(resizeTime);

					}

					resizeTime = $timeout(function () {

						if (attrs.resizer === 'vertical') {
							var clientWidth = document.body.clientWidth;
							var x = event.pageX || event.originalEvent.touches[0].pageX;

							var hideobj = angular.element(attrs.resizerHide);
							hideobj.css('display', 'none');


							var realx = x - offsetLeftScope;
							var rightsize = clientWidth - x;
							var leftsize = realx;

							var percentR = rightsize;// ((resizerScopeWidth - realx) / resizerScopeWidth) * 100;
							var percentL = leftsize;//100 - percentR;

							if (attrs.resizerMin) {

								var resizerMin = parseInt(attrs.resizerMin);

								if (percentR <= resizerMin) return;
							}

							if (attrs.resizerMax) {

								var resizerMax = parseInt(attrs.resizerMax);

								if (percentR > resizerMax) return;
							}

							percentRight = percentR;
							percentLeft = percentL;
							elemresize.css({ display: 'block', left: realx + 'px' });
							//resizerRight.css({ width: percentRight + '%' });
							//resizerLeft.css({ width: percentLeft + '%' });

						} else if (attrs.resizer === 'horizontal') { //horizontal
							throw Error('Unsupport horizontal resize!');
						}
					}, 3, false);
				}

				function findOffsetLeft(elm) {
					var result = 0;
					if (elm.offsetParent) {
						do {
							result += elm.offsetLeft;
						} while (elm = elm.offsetParent);
					}
					return result;
				}

				function mouseup(event) {
					event.preventDefault();
					resizerRight.css({ width: percentRight + 'px' });
					resizerLeft.css({ width: percentLeft + 'px' });
					$timeout(function () {
						elemresize.css({ display: 'none' });
					}, 3, false);
					var hideobj = angular.element(attrs.resizerHide);
					hideobj.css('display', 'block');

					$document.unbind('touchmove mousemove', mousemove);
					$document.unbind('touchend touchcancel mouseup', mouseup);
				}

				$scope.$on('$destroy', function () {
					elem.unbind('touchstart mousedown', mousestart);
				});
			}
		}]);

		module.directive('fullScreen', ['$document', function (documents) {
			return {
				restrict: 'EA',
				link: function (scope, element, attrs) {


					function toggleFullScreen(e) {
						var document = documents[0];
						//var fullscreenEnabled = document.fullscreenEnabled || document.mozFullScreenEnabled || document.webkitFullscreenEnabled;
						//if (fullscreenEnabled) {
						if (!document.fullscreenElement && !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {
							launchIntoFullscreen(document.documentElement);
						} else {
							exitFullscreen();
						}
						// }
					}

					function launchIntoFullscreen(element) {
						if (element.requestFullscreen) {
							element.requestFullscreen();
						} else if (element.mozRequestFullScreen) {
							element.mozRequestFullScreen();
						} else if (element.webkitRequestFullscreen) {
							element.webkitRequestFullscreen();
						} else if (element.msRequestFullscreen) {
							element.msRequestFullscreen();
						}
					}

					function exitFullscreen() {
						if (document.exitFullscreen) {
							document.exitFullscreen();
						} else if (document.msExitFullscreen) {
							document.msExitFullscreen();
						}
						else if (document.mozCancelFullScreen) {
							document.mozCancelFullScreen();
						} else if (document.webkitExitFullscreen) {
							document.webkitExitFullscreen();
						}
					}

					element.on('click', toggleFullScreen);

					scope.$on('$destroy', function (e) {
						element.off('click', toggleFullScreen);
					});
				}

			};


		}]);

		module.directive('noCloseOnClick', function () {
			return {
				restrict: 'A',
				compile: function (ele, attrs) {
					return ele.on('click', function (event) {
						return event.stopPropagation();
					});
				}
			}
		});

		module.directive('slimscroll', ['$window', '$timeout', function ($window, $timeout) {
			'use strict';

			return {
				restrict: 'A',
				scope: {
					scrollToEnd: '=',
					heightAuto: '='
				},
				link: function ($scope, $elem, $attr) {
					var sheight = 'scrollHeight';
					var sto = 'scrollToEnd';
					var off = [];
					var option = {};

					if (isWindowPhone()) { return; }

					$scope.$watch(sto, function (newVal, oldVal) {

						if (newVal === false) return;

						//if ($attr.slimscroll) {
						//	option = $scope.$eval($attr.slimscroll);
						//	var scrollToValue = $elem.prop(sheight) + 'px';
						//	if (option.height) {
						//		option.scrollTo = scrollToValue.toString();
						//	} else {
						//		angular.extend(option, { scrollTo: scrollToValue });
						//	}
						//}

						if ($attr.slimscroll) {
							option = $scope.$eval($attr.slimscroll);
							if (option.height == "auto") {
								angular.extend(option, { height: ($elem[0].clientHeight) + "px" });
							}
						}

						$($elem).slimScroll(option);
						//$($elem).slimScroll({ destroy: true });
					});

					var refresh = function () {
						$timeout(function () {
							if ($attr.slimscroll) {
								option = $scope.$eval($attr.slimscroll);
							} else if ($attr.slimscrollOption) {
								option = $scope.$eval($attr.slimscrollOption);
							}

							//if ($attr.heightAuto && $attr.heightAuto == 'true') {
							//	var oftop = findOffsetTop($elem[0]);
							//	var height = $window.innerHeight - oftop - 100 + 'px';
							//	if (option.height) {
							//		option.height = height.toString();
							//	} else {
							//		angular.extend(option, { height: height.toString() });
							//	}
							//}

							if (option.height == "auto") {
								angular.extend(option, { height: ($elem[0].clientHeight) + "px" });
							}

							$($elem).slimScroll(option);
						}, 50);
						//$($elem).slimScroll({ destroy: true });
					};

					function findOffsetTop(elm) {
						var result = 0;
						if (elm.offsetParent) {
							do {
								result += elm.offsetTop;
							} while (elm = elm.offsetParent);
						}
						return result;
					}

					angular.element($window).on("resize", refresh);

					var init = function () {

						refresh();

						if ($attr.slimscroll && !option.noWatch) {
							off.push($scope.$watchCollection($attr.slimscroll, refresh));
						}

						if ($attr.slimscrollWatch) {
							off.push($scope.$watchCollection($attr.slimscrollWatch, refresh));
						}

						if ($attr.slimscrolllistento) {
							off.push($scope.$on($attr.slimscrolllistento, refresh));
						}
					};

					var destructor = function () {
						off.forEach(function (unbind) {
							unbind();
						});
						angular.element($window).off("resize", refresh);
						off = null;
					};

					off.push($scope.$on('$destroy', destructor));
					init();

					function isWindowPhone() {
						return /IEMobile|windows phone|tablet pc/i.test(navigator.userAgent.toLowerCase());
					}
				}
			};
		}]);

		module.directive("outsideClick", ['$document', '$parse', function ($document, $parse) {
			return {
				link: function ($scope, $element, $attributes) {
					var scopeExpression = $attributes.outsideClick,
                        onDocumentClick = function (event) {
                        	var isChild = $element.find(event.target).length > 0;

                        	if (!isChild) {
                        		$scope.$applyAsync(scopeExpression);
                        	}
                        };

					$document.on("click", onDocumentClick);

					$element.on('$destroy', function () {
						$document.off("click", onDocumentClick);
					});
				}
			}
		}]);

		module.directive('cmsPopover', ['$compile', '$timeout', '$http', '$templateCache', function ($compile, $timeout, $http, $templateCache) {
			var getTemplate = function (templatelink) {
				return $http.get(templatelink, { cache: $templateCache });
			}

			return {
				restrict: 'EA',

				scope: {
					templatelink: '=',
					placement: '@',
					trigger: '@',
					popData: '='
				},

				link: function (scope, elem, attrs) {
					var cmsPopContent;
					if (scope.templatelink) {
						var loadhtml = getTemplate(scope.templatelink);
						var promise = loadhtml.success(function (html) {
							cmsPopContent = $compile(html)(scope);
							var options = {
								content: cmsPopContent,
								placement: scope.placement,
								trigger: scope.trigger,
								html: true,
								template: '<div class="popover cmspopover" role="tooltip"><div class="arrow"></div><div class="popover-content"></div></div>'
							}

							$(elem).popover(options);
						});
					}
				}
				//,template: '<div ng-include="getContentUrl()"></div>'
			}
		}]);

		module.directive('inputUpload', function () {
			return {
				//scope: {
				//	model: '@',
				//	modelBytes: '@'
				//}, 
				//  scope:{model:'@',maxsize:'@'},
				link: function (scope, el, attrs) {
					el.bind('change', function (event) {
						var files = event.target.files;
						var unlimitsize = (attrs.maxSize == 0 || attrs.maxSize == undefined || attrs.maxSize == null)
						var valid_list_files = unlimitsize ? files : Enumerable.From(files).Where(function (_files) { return _files.size <= (parseInt(attrs.maxSize) * 1000000) }).Select(function (_files) { return _files }).ToArray();
						var invalid_list_files = unlimitsize ? null : Enumerable.From(files).Where(function (_files) { return _files.size > (parseInt(attrs.maxSize) * 1000000) }).Select(function (_files) { return _files }).ToArray();
						if (attrs.multiple && attrs.multiple === true) {
							scope.$emit("filesSelected", { files: valid_list_files, model: scope.model, modelBytes: scope.modelBytes, invalid_files: invalid_list_files });
						} else {
							//iterate files since 'multiple' may be specified on the element
							for (var i = 0; i < files.length; i++) {
								//emit event upward
								scope.$emit("fileSelected", { file: files[i], model: scope.model, modelBytes: scope.modelBytes });
							}
						}
					});
				}
			}
		});

		module.directive('fileUpload', ['cmsBase', 'AppDefine', function (cmsBase, AppDefine) {
			/*
				// FILE INPUT UPLOAD DIRECTIVE
				// [file-upload] attribute: allow input file or files.
				// [options] = {
						accept: allow type files upload - default is empty
						,multiple: allow upload multiple files - default is false
						,maxSize: number file size maximum - default is null
						,maxFile: number file upload maximum - default is null
						,maxLength: maxlength of file name - default is null
						,modelName: the field name that contain name of files on Model - default is empty
						,modelData: the field name that contain data of files on Model - default is empty
					}
			*/
			var MB_UNIT_NUMBER = 1048576;
			var MB_UNIT_STRING = "MB";
			var _default = {
				accept: ''
				, multiple: false
				, maxSize: null
				, maxFile: null
				, maxLength: null
				, modelName: ''
				, modelData: ''
			};

			return {
				link: function (scope, el, attrs) {
					var options = _default;
					el.bind('change', function (event) {
						options = angular.copy(_default);
						angular.extend(options, scope.$eval(attrs.fileUpload));
						options.accept = attrs.accept || _default.accept;
						options.multiple = attrs.multiple || _default.multiple;
						if (options.maxSize) {
							options.maxSize = options.maxSize * MB_UNIT_NUMBER;
						}

						var fileInputs = event.target.files;
						var fileOutputs = [];

						fileOutputs = filterFiles(fileInputs, options);

						scope.$emit(AppDefine.Events.FILESELECTEDCHANGE, { file: fileOutputs, modelName: options.modelName, modelData: options.modelData });
					});

					//ThangPham, fix issue select file again on Chrome, IE ---https://code.google.com/p/closure-library/issues/detail?id=421
					el.bind('click', function (event) {
						this.value = '';
					});
				}
			};

			function filterFiles(fileInputs, options) { //accept, maxSize
				var files = [];
				if (!fileInputs) { return files; }
				if (!options) { return fileInputs; }

				var totalSize = 0;
				var invalid = false;
				angular.forEach(fileInputs, function (file) {
					totalSize += file.size;
					var extension = file.name.substr((file.name.lastIndexOf('.') + 1)).toLowerCase();
					if (invalid === false && options.accept && options.accept.indexOf(extension) === -1) {
						invalid = true;
						return cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FILE_TYPE_UPLOAD_REQUIRED), options.accept));

					}
					else if (invalid === false && options.maxSize != null && options.maxSize <= totalSize) {
						invalid = true;
						return cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.FILE_SIZE_UPLOAD_REQUIRED), (options.maxSize / MB_UNIT_NUMBER) + MB_UNIT_STRING));
					}
					else if (invalid === false && options.maxLength != null && file.name.length > options.maxLength) {
						invalid = true;
						return cmsBase.cmsLog.warning(formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.MAX_LENGTH_FILE_NAME_UPLOAD), file.name, options.maxLength));
					}
					else {
						files.push(file);
					}
				});

				if (invalid) {
					return files = [];
				}

				return files;
			}

			function formatString(format) {
				var args = Array.prototype.slice.call(arguments, 1);
				return format.replace(/{(\d+)}/g, function (match, number) {
					return typeof args[number] != 'undefined' ? args[number] : match;
				});
			}
		}]);

		module.directive('funcHeader', ['AppDefine', function (AppDefine) {
			return {
				restrict: 'EA',
				scope: {
					listview: '=',
					query: '=',
					title: '=',
					titlecount: '=',
					addFn: '&',
					editFn: '&',
					delFn: '&',
					refreshFn: '&',
					importFn: '&',
					showViewMode: '='

				},
				link: function (scope, elem, attrs) {
					scope.btnGrid = true;
					scope.btnList = true;
					if ($('body').hasClass("mobile")) {
						scope.btnGrid = false;
					}

					scope.showAdd = scope.$eval(attrs.addFn) === undefined ? true : scope.$eval(attrs.addFn);
					scope.showEdit = scope.$eval(attrs.editFn) === undefined ? true : scope.$eval(attrs.editFn);
					scope.showDel = scope.$eval(attrs.delFn) === undefined ? true : scope.$eval(attrs.delFn);
					scope.showRefresh = scope.$eval(attrs.refreshFn) === undefined ? true : scope.$eval(attrs.refreshFn);
					scope.showImport = scope.$eval(attrs.importFn) === undefined ? true : scope.$eval(attrs.importFn);
					scope.showViewMode = scope.$eval(attrs.showViewMode) === undefined ? true : scope.$eval(attrs.showViewMode);
					scope.showFilter = scope.$eval(attrs.query) === undefined ? true : scope.$eval(attrs.query);
					scope.SetModeView = function (value) {
						scope.listview = value;
						//$(window).resize();
					};

					//Handler grid selected row event
					scope.editDisabled = true;
					scope.deleteDisabled = true;
					scope.$on(AppDefine.Events.ROWSELECTEDCHANGE, function (event, arg) {
						if (arg) {
							scope.editDisabled = (arg.length == 0 || arg.length > 1);
							scope.deleteDisabled = (arg.length == 0);
						}
					});
				},
				templateUrl: 'widgets/function-header.html'
			}
		}]);

		module.directive('treecheckbox', [function () {
			return {
				require: '?ngModel',
				link: function (scope, el, attrs, ctrl) {
					var truthy = true;
					var falsy = false;
					var nully = null;
					ctrl.$formatters = [];
					ctrl.$parsers = [];

					var threestate = attrs.indeterminate;
					if (threestate === undefined || threestate === null)
						threestate = true;
					else
						threestate = threestate == 'true' ? true : false;

					ctrl.$render = function () {
						var d = ctrl.$viewValue;
						switch (d) {
							case truthy:
								el.prop('indeterminate', false);
								el.prop('checked', true);
								break;
							case falsy:
								el.prop('indeterminate', false);
								el.prop('checked', false);
								break;
							default:
								el.prop('indeterminate', true);
						}
					};
					/*
                    el.bind('change', function(){
                        console.log(el.data('checked'));
                    });*/
					/*
                    el.bind('click', function() {
                      var d;
                      switch(el.data('checked')){
                      case falsy:
                        d = truthy;
                        break;
                      case truthy:
                        if( threestate === true)
                          d = nully;
                        else
                          d = falsy;
                        break;
                      default:
                        d = falsy;
                      }
                      ctrl.$setViewValue(d);
                      scope.$apply(ctrl.$render);
                    });
                    */
				}
			};
		}]);

		module.directive('chartHeader', function () {
			return {
				restrict: 'EA',
				scope: {
					period: '=',
					severity: '=',
					last24hFn: '&',
					last3dFn: '&',
					lastweekFn: '&'

				},
				link: function (scope, elem, attrs) {
					scope.period = scope.$eval(attrs.period) == undefined ? false : scope.$eval(attrs.period);
					scope.last24h = scope.$eval(attrs.last24h) == undefined ? true : scope.$eval(attrs.last24h);
					scope.last3d = scope.$eval(attrs.last3d) == undefined ? true : scope.$eval(attrs.last3d);
					scope.lastweek = scope.$eval(attrs.lastweek) == undefined ? true : scope.$eval(attrs.lastweek);
				},
				templateUrl: 'widgets/chart-header.html'

			}
		});

		module.directive('autofocus', ['$timeout', function ($timeout) { //ThangPham, Cover 'autofucus' of html5 support for IOS safari.
			return {
				restrict: 'A',
				link: function ($scope, $element) {
					$timeout(function () {
						$element[0].focus();
					}, 500);
					$scope.$applyAsync();
				}
			}
		}]);

		module.directive('cmsBody', ['$rootScope', function ($rootScope) {
			//ThangPham, set mode mobile on cmsweb page, July 08 2015
			//Mobile mode = browser width < 768px.
			var PHONE_WIDTH = 767;
			return {
				restrict: 'A',
				link: function (scope, elem, attrs) {
					setMobileMode();
					//Handler rezie event
					$(window).on('resize', setMobileMode);
					detectBrowser();

				}
			}

			function setMobileMode() {
				var w = $(window).width();
				if (w > PHONE_WIDTH) {
					$('body').removeClass("mobile");
				} else {
					$('body').addClass("mobile");
				}
			}

			function detectBrowser() {

				if ((navigator.userAgent.indexOf("Opera") || navigator.userAgent.indexOf('OPR')) != -1) {
					$rootScope.cmsBrowser = 'forOpera';
				}
				else if (navigator.userAgent.indexOf("Chrome") != -1) {
					$rootScope.cmsBrowser = 'forChrome';
				}
				else if (navigator.userAgent.indexOf("Safari") != -1) {
					$rootScope.cmsBrowser = 'forSafari';
				}
				else if (navigator.userAgent.indexOf("Firefox") != -1) {
					$rootScope.cmsBrowser = 'forFirefox';
				}
				else if ((navigator.userAgent.indexOf("MSIE") != -1) || (!!document.documentMode == true)) //IF IE > 10
				{
					$rootScope.cmsBrowser = 'forIE';
				}
			}

		}]);

		module.directive('popMenu', function () {
			return {
				restrict: 'A',
				link: function ($scope, $element) {
					$element.on('click', function (event) {
						$(this).parent().toggleClass("open");
						$(this).prop("aria-expanded", true);
					});

					//$("#close").on('click', function (e) {
					//	if ($("#btn-popMenu").parent().hasClass("open")) {
					//		$("#btn-popMenu").parent().removeClass("open");
					//		$("#btn-popMenu").prop("aria-expanded", false);
					//	}
					//});
				}
			}
		});

		module.directive('metricChildInput', ['$timeout', function ($timeout) {//This Directive used for Site Metric only.
			return {
				restrict: 'A',
				link: function ($scope, $element) {
					if ($scope.item != null) {
						if ($scope.item.hasOwnProperty("MListID")) {
							if ($scope.item.MListID > 0) {
								$element[0].focus();
							}
						}
					}
				}
			}
		}]);

		module.directive('ngResize', ['$window', function ($window) {
			return {
				restrict: 'A',
				scope: { onResize: '=' },
				link: function (scope, element) {
					//var w = angular.element($window);
					//scope.getWindowDimensions = function () {
					//    return {
					//        'h': w.height(),
					//        'w': w.width()
					//    };
					//};
					//scope.$watch(scope.getWindowDimensions, function (newValue, oldValue) {
					//scope.windowHeight = newValue.h;
					//scope.windowWidth = newValue.w;

					//scope.style = function () {

					//};

					//}, true);

					angular.element($window).on("resize", scope.onResize);
					//w.bind('resize', function () {

					//    if (scope.onResize && typeof scope.onResize === 'function') {
					//        scope.onResize(element);
					//    }
					//    scope.$apply();
					//});
				}
			}
		}]);

		module.directive('imageonload', ['$timeout', function ($timeout) {
			return {
				restrict: 'A',
				link: function (scope, element, attrs) {
					element.bind('load', function () {

					});
				}
			};
		}]);

		module.directive('droppable', function ($document, $window) {
			return {
				restrict: 'A',
				link: function (scope, element, attrs) {
					element[0].addEventListener('drop', scope.handleDrop, false);
					element[0].addEventListener('dragover', scope.handleDragOver, false);
				}
			}
		});

		module.directive('ngDraggable', function ($document, $window) {
			return {
				restrict: 'A',
				link: function (scope, $element, attrs) {
					$element.bind('drop', scope.ngDrop);
					$element.bind('dragover', scope.ngDragover);
					$element.bind('dragenter', scope.ngDragenter);
					$element.bind('dragend', scope.ngDragend);
				}
			}
		});

		module.directive('ddraggable', ['$document', '$timeout', function ($document, $timeout) {
			return {
				restrict: 'A',
				$scope: {
					onUp: '=',
					cmodel: [],
					onTop: 0,
					onLeft: 0,
					cmap: "",
					onmove: '='

				},
				link: (function ($scope, element, attr) {
					var startX = 0, startY = 0, x = 0, y = 0, top = 0, left = 0, sx = 0, sy = 0;
					var _position = {};
					var p_witdh = 0;
					var parent = document.getElementById(attr.parentid);

					x = parent.clientWidth * $scope.cn.Leftpoint / 100;
					y = parent.clientHeight * $scope.cn.Toppoint / 100;

					parent = document.getElementById(attr.parentid);
					var marrginTop = 0 - parent.clientHeight;
					var marginButtom = -28;
					var marginLeft = 0;
					var e_width = getTextWidth($scope.cn.ChannelName, '600 12px Open Sans');;
					e_width = e_width == undefined || e_width == null ? 0 : e_width;
					var marginRight = parent.clientWidth - e_width;


					if (x > marginRight) x = marginRight
					if (x < marginLeft) x = marginLeft;
					if (y < marrginTop) y = marrginTop;
					if (y > marginButtom) y = marginButtom;




					function getTextWidth(text, font) {
						// re-use canvas object for better performance
						var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
						var context = canvas.getContext("2d");
						context.font = font;
						var metrics = context.measureText(text);
						return metrics.width;
					};

					function dragstart(event) {
						$scope.ChannelDrag(event);
						event.preventDefault();
						var top = parseFloat(element.css('top').replace('px', ''));
						var left = parseFloat(element.css('left').replace('px', ''));
						x = left;
						y = top;
						sx = x;
						sy = y;
						startX = event.originalEvent.screenX - x;
						startY = event.originalEvent.screenY - y;
						$document.on('mousemove', mousemove);
						$document.on('mouseup', dragend);
						var p = event.currentTarget.getElementsByTagName("p")[0];
						p_witdh = p.offsetWidth;

					};
					element.on('mousedown', dragstart);
					function mousemove(event) {
						event.preventDefault();
						event.stopPropagation();
						y = event.screenY - startY;
						x = event.screenX - startX;

						var parent = document.getElementById(attr.parentid);
						var marrginTop = 0 - parent.clientHeight;
						var marginButtom = -28;
						var marginLeft = 0;
						var marginRight = parent.clientWidth - element[0].clientWidth;
						if (x > (marginRight)) x = (marginRight);
						if (x < marginLeft) x = marginLeft;

						if (y > marginButtom) y = marginButtom;
						element.css({
							top: y + 'px',
							left: x + 'px'

						});
					}
					function dragend(event) {

						var TrashBox = { x: 0, y: 0, w: 0, h: 0 };
						var elTrashBox = undefined;
						var elMaintab = undefined;
						if (elTrashBox == undefined) {
							elTrashBox = document.getElementById("trashbox");
							elMaintab = document.getElementById("maintab");
						}
						TrashBox.h = elTrashBox.clientHeight;
						TrashBox.w = elTrashBox.clientWidth;
						TrashBox.x = elMaintab.clientWidth - (elTrashBox.clientWidth + 30);
						TrashBox.y = -(elMaintab.clientHeight - 15);
						var inTrashZone = false;
						inTrashZone = x >= TrashBox.x && x <= TrashBox.x + TrashBox.w && y >= TrashBox.y && y <= TrashBox.y + TrashBox.h;

						if (inTrashZone == true) {
							$scope.ngDragenter(event);

						}
						if ((event.target.id != "trashbox" && y < marrginTop)) {
							y = sy;
							x = sx;
						}
						var obj = {};
						obj.top = y;
						obj.left = x;
						obj.id = element[0].id;

						$document.off('mousedown', dragstart);
						$document.off('mousemove', mousemove);
						$document.off('mouseup', dragend);

						if (inTrashZone == false) {
							element.css({
								top: y + 'px',
								left: x + 'px'

							});
						}

						event.stopPropagation();
						$scope.onUp(event, obj);
					}
					function extract_percentPosition(parentEle, obj) {
						var _obj = {};
						_obj.top = (parentEle.clientHeight * obj.Toppoint) / 100;
						_obj.left = (parentEle.clientWidth * obj.Leftpoint) / 100;
						return _obj;
					}
					element[0].setAttribute('style', ' float:left; background-color:transparent; border:none; padding-top:0px; height:0px; padding-bottom:0px; margin-bottom:0px;padding-left:0px;padding-right:0px;position: relative; cursor: pointer;top:' + y + 'px;left:' + x + 'px');
				})
			}
		}]);

		module.directive('reportFrame', ['$compile', '$timeout', function ($compile, $timeout) {
			return {
				restrict: 'EA',
				replace: false,
				scope: {
					reportId: '=',
					type: '=',
					requestObj: '=',
					style: '='
				},
				link: function (scope, elem, attrs) {

					var type = scope.type !== undefined ? scope.type : "rdlc";
					scope.style = scope.style ? scope.style : {};

					var loading = '<div class="iframe-loading-report"><div class="loading">{{"LOADING"| translate}}</div></div>';
					var framecontent = angular.element(loading);
					var compileHtml = $compile(framecontent)(scope);

					elem.append(compileHtml);
					var loadingreport = elem.find('.iframe-loading-report');
					loadingreport.css('display', 'block');

					var reportframe = elem.find('.report-frame');
					reportframe.css('min-height', '0px');
					reportframe.css('height', '0px');

					//$timeout(function () {
					//    loadingreport.css('display', 'none');

					//    reportframe.css('display', 'none');
					//}, 1000, false);

					var optionParams = "";
					if (scope.requestObj) {
						optionParams = "&" + parseParam(scope.requestObj);
					}

					scope.reportSource = "../Report/reportviewer.aspx?reportId=" + scope.reportId + "&engineId=" + type + optionParams;

					elem.find('iframe').bind('load', function (event) {
						loadingreport.css('display', 'none');
						reportframe.css('min-height', '300px');
						reportframe.css('height', '100%');
						ZoomInit();


					});


					//$(element).load(function () {
					//    // cached body element overrides when src loads
					//    // hence have to cache it again            
					//    loadingreport.css('display', 'none');
					//    reportframe.css('display', 'block');
					//});


					function parseParam(obj) {

						var str = "";

						if (!obj) {
							return str;
						}

						for (var key in obj) {
							if (str != "") {
								str += "&";
							}
							str += key + "=" + obj[key];
						}
						return str;
					}

					function ZoomInit() {
						$timeout(function () {
							//alert("Hello");
							var content = angular.element(".panelContent");
							var body = window.innerWidth;
							//var parent = angular.element("#ReportViewerManager_ctl09");
							//var parent = document.getElementById("ReportViewerManager_ctl09");
							//var x = angular.element("#VisibleReportContentReportViewerManager_ctl09");
							var iframe = angular.element(".report-frame");
							if (iframe === undefined || iframe.length === 0) {
								return;
							}
							var document = iframe.contents();
							var parent = document[0].getElementById("ReportViewerManager_ctl09");
							var x = document[0].getElementById("VisibleReportContentReportViewerManager_ctl09");

							var reportDiv = $("div[id*='_oReportCell']").css("overflow-x");

							var form = document[0].getElementById("form1");
							//var reportDiv = $("div[id$='P4d4efb60980a401f873815114950fa2b_1_oReportCell']").css("overflow-x");
							CreateControlRP(document[0].getElementById("ReportViewerManager_ctl05"));


							//var namecss = $(".report-frame").find("#form1").css("overflow-x");
							if (content === undefined || x === undefined || content === null || x === null) {
								return;
							}
							if (x.offsetWidth > 0 && x.offsetWidth !== undefined) {
								//var div = x.firstChild;
								//var table = div.firstChild;
								var table = x.firstChild;
								do {
									if (table === undefined || table === null) {
										return;
									}
									if (table.localName === "table") {
										break;
									}
									table = table.firstChild;
								}
								while (1);
								if (table !== undefined && table !== null) {
									//var btnNextPage = document[0].getElementById("ReportViewerManager_ctl05_ctl00_Next_ctl00_ctl00");
									//btnNextPage.addEventListener("click", function () {
									//    $timeout(function () {
									//        ZoomInit();

									//    }, 1000);
									//});

									if (table.offsetWidth > 0 && (content[0].offsetWidth - table.offsetWidth > 10)) {
										for (var i = 0; i <= 10; i++) {
											table.style.zoom = (100 + (i * 5)) / 100;
											table.style.MozTransform = 'scale(' + (100 + ((i - 1) * 5)) / 100 + ')';
											table.style.MozTransformOrigin = 'top';
											var widthparent = $(".report-frame").width() - 20;
											//var widthchild = form.scrollWidth;
											var widthchild = table.scrollWidth * ((100 + (i * 5)) / 100);
											if (table.offsetWidth > 0 && widthparent <= widthchild) {
												if (((100 + ((i - 1) * 5)) / 100) < 1) {
													table.style.zoom = 1;
													table.style.MozTransform = 'scale(1)';
													table.style.MozTransformOrigin = 'top';
												} else {
													table.style.zoom = (100 + ((i - 1) * 5)) / 100;
													table.style.MozTransform = 'scale(' + (100 + ((i - 1) * 5)) / 100 + ')';
													table.style.MozTransformOrigin = 'top';
												}
												return;
											}
										}
										//table.style.zoom = temp.style.zoom;
									} else if (table.offsetWidth > 0 && content[0].offsetWidth - table.offsetWidth <= 10) {
										table.style.zoom = 1;
										table.style.MozTransform = 'scale(1)';
										table.style.MozTransformOrigin = 'top';
									}
								}
							}
						}, 200);
					}


					function CreateControlRP(root) {
						if (root === undefined || root === null) {
							return;
						}

						var controls = $(root).find(":input");

						angular.forEach(controls, function (index, value) {

						});

						var contentcontrols = $(root).find("> div:first").clone()[0];

						var html = '';
						html += '<div style="display:inline-block;font-family:Verdana;font-size:8pt;vertical-align:top;">';
						html += '<table cellpadding="0" cellspacing="0" style="display:inline;">';
						html += '   <tbody>';
						html += '      <tr>';
						html += '         <td height="28px">';
						// Custom Control
						html += '            <div id="ReportViewerManager_ctl05_ctl00_ZoomOut" class="ReportViewerManagerZoomOut" onclick="ClickZoomOut()">';
						html += '                                   <i class="icon-zoom-out-custom icon-zoom-out-custom_enable" id="controlZoomOut" >';
						html += '                                   </i>';
						html += '            </div>';
						// End Custom Control
						html += '         </td>';
						html += '         <td width="4px"></td>';
						html += '         <td height="28px">';
						// Custom Control
						html += '            <div id="ReportViewerManager_ctl05_ctl00_ZoomIn" onclick="ClickZoomIn()">';
						html += '                                   <i class="icon-zoom-in-custom icon-zoom-in-custom_enable" id="controlZoomIn">';
						html += '                                   </i>';
						html += '            </div>';
						// End Custom Control
						html += '         </td>';
						html += '      </tr>';
						html += '   </tbody>';
						html += '</table>';
						html += '</div>';

						$(root).find("> div:first").append(html);

					}


				},
				template: '<iframe class="report-frame" ng-style="style" ng-src="{{reportSource}}"></iframe>'
			}
		}]);

		module.directive('rbSearch', ['$timeout', '$rootScope', '$filter', '$state', 'cmsBase', 'AppDefine', 'rebarDataSvc', 'Utils', 'POSCommonSvc',
        function ($timeout, $rootScope, $filter, $state, cmsBase, AppDefine, rebarDataSvc, Utils, POSCommonSvc) {
        	return {
        		restrict: 'E',
        		//replace: true,
        		//transclude: true,
        		scope: {
        			searchConfig: '=?',
        			searchModel: '=',
        			searchData: '=?',
        			singleDate: '=?',
        			advanceSearch: '=?',
        			modeFor: '=?',
        			searchSubmitEvent: '&'
        		},
        		templateUrl: 'Scripts/Directives/TemplateDirective/rb-search.html',
        		link: function (scope, elem, attrs, ctrl) {
        			var AND_VALUE = 1;
        			var OR_VALUE = 0;
        			scope.conditionPrefixConst = { And: 'AND_STRING', Or: 'OR_STRING' };
        			scope.conditionTypeConst = { Any: "Any", Lesster: "<", Equal: "=", Greater: ">" };
        			scope.TenderTypeSelected = [];
        			scope.RegisterSelected = [];
        			scope.EmployeeSelected = [];
        			scope.DescriptionSelected = [];
        			scope.itemNames = {
        				filterTypeItem: 'filterTypeItem',
        				dateItem: 'dateItem',
        				tenderTypeItem: 'tenderTypeItem',
        				registerItem: 'registerItem',
        				transactionItem: 'transactionItem',
        				employeeItem: 'employeeItem',
        				totalItem: 'totalItem',
        				descriptionItem: 'descriptionItem'
        			};
        			scope.ddlNames = {
        				Payment: 'ddlPaymentList',
        				Register: 'ddlRegister',
        				Employee: 'ddlemployee',
        				Description: 'ddlDescription'
        			};
        			scope.filterTypeConst = { Custom: 1, Yesterday: 2, Last7days: 3, Last30days: 4, CurrentDay: 5 };
        			scope.filterType = {
        				//Custom: { key: scope.filterTypeConst.Custom, name: cmsBase.translateSvc.getTranslate('CUSTOM_STRING'), isShow: true },
        				//Yesterday: { key: scope.filterTypeConst.Yesterday, name: cmsBase.translateSvc.getTranslate('YESTERDAY_STRING'), isShow: true },
        				//Last7days: { key: scope.filterTypeConst.Last7days, name: cmsBase.translateSvc.getTranslate('LAST7DAY_STRING'), isShow: true },
        				//Last30days: { key: scope.filterTypeConst.Last30days, name: cmsBase.translateSvc.getTranslate('LAST30DAY_STRING'), isShow: true },
        				//CurrentDay: { key: scope.filterTypeConst.CurrentDay, name: cmsBase.translateSvc.getTranslate('CURRENT_DAY_STRING'), isShow: false }
        				Custom: { key: scope.filterTypeConst.Custom, name: 'CUSTOM_STRING', isShow: true },
        				Yesterday: { key: scope.filterTypeConst.Yesterday, name: 'YESTERDAY_STRING', isShow: true },
        				Last7days: { key: scope.filterTypeConst.Last7days, name: 'LAST7DAY_STRING', isShow: true },
        				Last30days: { key: scope.filterTypeConst.Last30days, name: 'LAST30DAY_STRING', isShow: true },
        				CurrentDay: { key: scope.filterTypeConst.CurrentDay, name: 'CURRENT_DAY_STRING', isShow: false }
        			};
        			var configDefault = {
        				filterTypeItem: { name: 'filterTypeItem', visable: true, isReadOnly: false },
        				dateFrom: { name: 'dateFrom', visable: true, isReadOnly: false, options: { format: 'L', ignoreReadonly: false, maxDate: getMaxDate() } },
        				dateTo: { name: 'dateTo', visable: true, isReadOnly: false, options: { format: 'L', ignoreReadonly: false, maxDate: getMaxDate() } },
        				tenderTypeItem: { name: 'tenderTypeItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And },
        				registerItem: { name: 'registerItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And },
        				transactionItem: { name: 'transactionItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And, conditionType: scope.conditionTypeConst.Any },
        				employeeItem: { name: 'employeeItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And },
        				totalItem: { name: 'totalItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And, conditionType: scope.conditionTypeConst.Any },
        				descriptionItem: { isUsed: false, name: 'descriptionItem', visable: true, conditionPrefix: scope.conditionPrefixConst.And }
        			};

        			var dtNow = new Date();
        			var modelDefault = {
        				ReportID: 1,
        				DateFrom: Utils.beginOfDate(dtNow), //new Date(),
        				DateTo: dtNow, //new Date(),
        				SiteKeys: '',
        				PaymentIDs: [],
        				PaymentIDs_AND: AND_VALUE,
        				RegIDs: [],
        				RegIDs_AND: AND_VALUE,
        				EmpIDs: [],
        				EmpIDs_AND: AND_VALUE,
        				TransNB: 1,
        				TransNB_OP: configDefault.transactionItem.conditionType,
        				TransNB_AND: AND_VALUE,
        				TransAmount: 1,
        				TransAmount_OP: configDefault.totalItem.conditionType,
        				TransAmount_AND: AND_VALUE,
        				DescIDs: [],
        				DescIDs_AND: AND_VALUE,
        				GroupByField: 1, //default group by Site name
        				PACIDs: [],
        				MaxRows: 100000
        			};
        			var searchDataDefault = {
        				TenderTypeList: [],
        				RegisterList: [],
        				EmployeeList: [],
        				DescriptionList: []
        			};

        			scope.paymentCurrentPage = 0;
        			scope.paymentTotalPage = 0;

        			scope.registerCurrentPage = 0;
        			scope.registerTotalPage = 0;

        			scope.employeeCurrentPage = 0;
        			scope.employeeTotalPage = 0;

        			scope.descriptionCurrentPage = 0;
        			scope.descriptionTotalPage = 0;

        			scope.$watch('modeFor', function (newVal, oldVal) {
        				if (newVal !== oldVal) {
        					if (newVal === AppDefine.State.REBAR || newVal === AppDefine.State.REBAR_DASHBOARD) {
        						changeFilterTypeFn(scope.filterType.Last7days);
        					}
        					else if (newVal === AppDefine.State.REBAR_WEEKATGLANCE) {
        						changeFilterTypeFn(scope.filterType.Last30days);
        					}
        					else if (newVal === AppDefine.State.REBAR_ADHOC || newVal === AppDefine.State.REBAR_ADHOCDETAILS) {
        						changeFilterTypeFn(scope.filterType.Custom);
        					}
        					else {
        						//Anh, Keep search date when change between Canned Report, #3568, 2016-05-24
        						if (!isCannedRpt(newVal) || !isCannedRpt(oldVal)) {
        							changeFilterTypeFn(scope.filterType.CurrentDay); //set default search 1 days for Quick search, Canned reports
        						}
        					}
        				}
        			});

        			scope.$on("loadmoredata", function (e, arg) {
        				switch (arg) {
        					case scope.ddlNames.Payment: //"ddlPaymentList":
        						getTenderTypeList(scope.paymentCurrentPage + 1);
        						if (scope.paymentCurrentPage > 1)
        							scope.$applyAsync();
        						break;
        					case scope.ddlNames.Register: //"ddlRegister":
        						getRegisterList(scope.registerCurrentPage + 1);
        						if (scope.registerCurrentPage > 1)
        							scope.$applyAsync();
        						break;
        					case scope.ddlNames.Employee: //"ddlemployee":
        						getEmployeeList(scope.employeeCurrentPage + 1);
        						if (scope.employeeCurrentPage > 1)
        							scope.$applyAsync();
        						break;
        					case scope.ddlNames.Description: //"ddlDescription":
        						getDescriptionList(scope.descriptionCurrentPage + 1);
        						if (scope.descriptionCurrentPage > 1)
        							scope.$applyAsync();
        						break;
        				}
        			});

        			scope.$on("filterdata", function (e, arg) {
        				switch (arg.name) {
        					case scope.ddlNames.Payment: //"ddlPaymentList":
        						//Anh, Keep checked list while filter
        						//scope.TenderTypeSelected = [];
        						//scope.searchModel.PaymentIDs = [];
        						if (arg.filter === "") {
        							scope.searchData.TenderTypeList = [];
        							scope.paymentCurrentPage = 0;
        							scope.paymentTotalPage = 0;
        							getTenderTypeList(scope.paymentCurrentPage + 1);
        						}
        						else {
        							filterTenderList(arg.filter);
        						}
        						break;
        					case scope.ddlNames.Register: //"ddlRegister":
        						//scope.RegisterSelected = [];
        						//scope.searchModel.RegIDs = [];
        						if (arg.filter === "") {
        							scope.searchData.RegisterList = [];
        							scope.registerCurrentPage = 0;
        							scope.registerTotalPage = 0;
        							getRegisterList(scope.registerCurrentPage + 1);
        						}
        						else {
        							filterRegisterList(arg.filter);
        						}
        						break;
        					case scope.ddlNames.Employee: //"ddlemployee":
        						//scope.EmployeeSelected = [];
        						//scope.searchModel.EmpIDs = [];
        						if (arg.filter === "") {
        							scope.searchData.EmployeeList = [];
        							scope.employeeCurrentPage = 0;
        							scope.employeeTotalPage = 0;
        							getEmployeeList(scope.employeeCurrentPage + 1);
        						}
        						else {
        							filterEmployeeList(arg.filter);
        						}
        						break;
        					case scope.ddlNames.Description: //"ddlDescription":
        						//scope.DescriptionSelected = [];
        						//scope.searchModel.DescIDs = [];
        						if (arg.filter === "") {
        							scope.searchData.DescriptionList = [];
        							scope.descriptionCurrentPage = 0;
        							scope.descriptionTotalPage = 0;
        							getDescriptionList(scope.descriptionCurrentPage + 1);
        						}
        						else {
        							filterDescriptionList(arg.filter);
        						}
        						break;
        				}
        			});
        			scope.$on("checkedchange", function (e, arg) {
        				//Anh, Keep checked list while filter
        				switch (arg.name) {
        					case scope.ddlNames.Payment:
        						//scope.TenderTypeSelected = scope.$$childTail.TenderTypeSelected;
        						var modelIt = arg.item.model;
        						var changed = false;
        						if (arg.item.checked) {
        							if (isExistItem(scope.TenderTypeSelected, modelIt.ID) == false) {
        								scope.TenderTypeSelected.push(modelIt);
        								changed = true;
        							}
        						}
        						else {
        							if (isExistItem(scope.TenderTypeSelected, modelIt.ID) == true) {
        								changed = true;
        								for (var i = scope.TenderTypeSelected.length - 1; i >= 0; i--) {
        									if (scope.TenderTypeSelected[i].ID == modelIt.ID) {
        										scope.TenderTypeSelected.splice(i, 1);
        										break;
        									}
        								} //for
        							} //if exist
        						}
        						if (changed) {
        							if (scope.TenderTypeSelected.length > 0) {
        								scope.searchModel.PaymentIDs = Enumerable.From(scope.TenderTypeSelected).Select(function (x) { return x.ID; }).ToArray();
        							}
        							else {
        								scope.searchModel.PaymentIDs = [];
        							}
        						}
        						break;
        					case scope.ddlNames.Register:
        						var modelIt = arg.item.model;
        						var changed = false;
        						if (arg.item.checked) {
        							if (isExistItem(scope.RegisterSelected, modelIt.ID) == false) {
        								scope.RegisterSelected.push(modelIt);
        								changed = true;
        							}
        						}
        						else {
        							if (isExistItem(scope.RegisterSelected, modelIt.ID) == true) {
        								changed = true;
        								for (var i = scope.RegisterSelected.length - 1; i >= 0; i--) {
        									if (scope.RegisterSelected[i].ID == modelIt.ID) {
        										scope.RegisterSelected.splice(i, 1);
        										break;
        									}
        								} //for
        							} //if exist
        						}
        						if (changed) {
        							if (scope.RegisterSelected.length > 0) {
        								scope.searchModel.RegIDs = Enumerable.From(scope.RegisterSelected).Select(function (x) { return x.ID; }).ToArray();
        							}
        							else {
        								scope.searchModel.RegIDs = [];
        							}
        						}
        						break;
        					case scope.ddlNames.Employee:
        						var modelIt = arg.item.model;
        						var changed = false;
        						if (arg.item.checked) {
        							if (isExistItem(scope.EmployeeSelected, modelIt.ID) == false) {
        								scope.EmployeeSelected.push(modelIt);
        								changed = true;
        							}
        						}
        						else {
        							if (isExistItem(scope.EmployeeSelected, modelIt.ID) == true) {
        								changed = true;
        								for (var i = scope.EmployeeSelected.length - 1; i >= 0; i--) {
        									if (scope.EmployeeSelected[i].ID == modelIt.ID) {
        										scope.EmployeeSelected.splice(i, 1);
        										break;
        									}
        								} //for
        							} //if exist
        						}
        						if (changed) {
        							if (scope.EmployeeSelected.length > 0) {
        								scope.searchModel.EmpIDs = Enumerable.From(scope.EmployeeSelected).Select(function (x) { return x.ID; }).ToArray();
        							}
        							else {
        								scope.searchModel.EmpIDs = [];
        							}
        						}
        						break;
        					case scope.ddlNames.Description:
        						var modelIt = arg.item.model;
        						var changed = false;
        						if (arg.item.checked) {
        							if (isExistItem(scope.DescriptionSelected, modelIt.ID) == false) {
        								scope.DescriptionSelected.push(modelIt);
        								changed = true;
        							}
        						}
        						else {
        							if (isExistItem(scope.DescriptionSelected, modelIt.ID) == true) {
        								changed = true;
        								for (var i = scope.DescriptionSelected.length - 1; i >= 0; i--) {
        									if (scope.DescriptionSelected[i].ID == modelIt.ID) {
        										scope.DescriptionSelected.splice(i, 1);
        										break;
        									}
        								} //for
        							} //if exist
        						}
        						if (changed) {
        							if (scope.DescriptionSelected.length > 0) {
        								scope.searchModel.DescIDs = Enumerable.From(scope.DescriptionSelected).Select(function (x) { return x.ID; }).ToArray();
        							}
        							else {
        								scope.searchModel.DescIDs = [];
        							}
        						}
        						break;
        				}
        			});
        			scope.$on("uncheckall", function (e, arg) {
        				switch (arg) {
        					case scope.ddlNames.Payment:
        						scope.TenderTypeSelected = [];
        						scope.searchModel.PaymentIDs = [];
        						break;
        					case scope.ddlNames.Register:
        						scope.RegisterSelected = [];
        						scope.searchModel.RegIDs = [];
        						break;
        					case scope.ddlNames.Employee:
        						scope.EmployeeSelected = [];
        						scope.searchModel.EmpIDs = [];
        						break;
        					case scope.ddlNames.Description:
        						scope.DescriptionSelected = [];
        						scope.searchModel.DescIDs = [];
        						break;
        				}
        			});
        			scope.$on("checkall", function (e, arg) {
        				switch (arg) {
        					case scope.ddlNames.Payment:
        						scope.TenderTypeSelected = scope.$$childTail.TenderTypeSelected;
        						if (scope.TenderTypeSelected.length > 0) {
        							scope.searchModel.PaymentIDs = Enumerable.From(scope.TenderTypeSelected).Select(function (x) { return x.ID; }).ToArray();
        						}
        						else {
        							scope.searchModel.PaymentIDs = [];
        						}
        						break;
        					case scope.ddlNames.Register:
        						scope.RegisterSelected = scope.$$childTail.RegisterSelected;
        						if (scope.RegisterSelected.length > 0) {
        							scope.searchModel.RegIDs = Enumerable.From(scope.RegisterSelected).Select(function (x) { return x.ID; }).ToArray();
        						}
        						else {
        							scope.searchModel.RegIDs = [];
        						}
        						break;
        					case scope.ddlNames.Employee:
        						scope.EmployeeSelected = scope.$$childTail.EmployeeSelected;
        						if (scope.EmployeeSelected.length > 0) {
        							scope.searchModel.EmpIDs = Enumerable.From(scope.EmployeeSelected).Select(function (x) { return x.ID; }).ToArray();
        						}
        						else {
        							scope.searchModel.EmpIDs = [];
        						}
        						break;
        					case scope.ddlNames.Description:
        						scope.DescriptionSelected = scope.$$childTail.DescriptionSelected;
        						if (scope.DescriptionSelected.length > 0) {
        							scope.searchModel.DescIDs = Enumerable.From(scope.DescriptionSelected).Select(function (x) { return x.ID; }).ToArray();
        						}
        						else {
        							scope.searchModel.DescIDs = [];
        						}
        						break;
        				}
        			});

        			scope.searchConfig = scope.searchConfig !== undefined ? scope.searchConfig : angular.copy(configDefault);

        			if ($state.current.name === AppDefine.State.REBAR || $state.current.name === AppDefine.State.REBAR_DASHBOARD) {
        				changeFilterTypeFn(scope.filterType.Last7days); //set default search 7 days for Rebar - Dashboard
        			}
        			else if ($state.current.name === AppDefine.State.REBAR_WEEKATGLANCE) {
        				changeFilterTypeFn(scope.filterType.Last30days);
        			}
        			else if ($state.current.name === AppDefine.State.REBAR_ADHOC || $state.current.name === AppDefine.State.REBAR_ADHOCDETAILS) {
        				changeFilterTypeFn(scope.filterType.Custom);
        			}
        			else {
        				changeFilterTypeFn(scope.filterType.CurrentDay); //set default search 1 days for Quick search, Canned reports
        			}

        			//scope.searchConfig = scope.searchConfig !== undefined ? scope.searchConfig : angular.copy(configDefault);
        			//console.log(scope.searchConfig);

        			//Anh, set default value before get startTime, endTime
        			if (!scope.searchModel) {
        				scope.searchModel = angular.copy(modelDefault);
        			}
        			if (scope.searchModel !== undefined) {
        				scope.startTime = angular.copy(scope.searchModel.DateFrom);
        				scope.endTime = angular.copy(scope.searchModel.DateTo);
        			}
        			//else {
        			//	scope.searchModel = angular.copy(modelDefault);
        			//}

        			if (scope.searchData) {
        				scope.searchData.TenderTypeList = scope.searchData.TenderTypeList !== undefined ? scope.searchData.TenderTypeList : angular.copy(searchDataDefault.TenderTypeList);
        				scope.searchData.RegisterList = scope.searchData.RegisterList !== undefined ? scope.searchData.RegisterList : angular.copy(searchDataDefault.RegisterList);
        				scope.searchData.EmployeeList = scope.searchData.EmployeeList !== undefined ? scope.searchData.EmployeeList : angular.copy(searchDataDefault.EmployeeList);
        				scope.searchData.DescriptionList = scope.searchData.DescriptionList !== undefined ? scope.searchData.DescriptionList : angular.copy(searchDataDefault.DescriptionList);
        			}
        			else {
        				scope.searchData = angular.copy(searchDataDefault);
        			}

        			scope.isView = true; //used for advance search only.

        			scope.toggleMode = function () {

        				if (!scope.searchConfig.tenderTypeItem.visable) {
        					scope.searchModel.PaymentIDs = scope.TenderTypeSelected = [];
        					scope.searchModel.PaymentIDs_AND = AND_VALUE;
        				}

        				if (!scope.searchConfig.registerItem.visable) {
        					scope.searchModel.RegIDs = scope.RegisterSelected = [];
        					scope.searchModel.RegIDs_AND = OR_VALUE;
        				}

        				if (!scope.searchConfig.transactionItem.visable) {
        					scope.searchModel.TransNB = 1;
        					scope.searchModel.TransNB_OP = scope.conditionTypeConst.Any;
        					scope.searchModel.TransNB_AND = AND_VALUE;
        				}

        				if (!scope.searchConfig.employeeItem.visable) {
        					scope.searchModel.EmpIDs = scope.EmployeeSelected = [];
        					scope.searchModel.EmpIDs_AND = OR_VALUE;
        				}

        				if (!scope.searchConfig.totalItem.visable) {
        					scope.searchModel.TransAmount = 0;
        					scope.searchModel.TransAmount_OP = scope.conditionTypeConst.Any;
        					scope.searchModel.TransAmount_AND = AND_VALUE;
        				}

        				scope.isView = !scope.isView;
        			};

        			scope.toggleConditionPrefix = function (fieldItem) {
        				switch (fieldItem) {
        					case scope.itemNames.tenderTypeItem:
        						if (scope.searchConfig.tenderTypeItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.tenderTypeItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.PaymentIDs_AND = 1;
        						}
        						else {
        							scope.searchConfig.tenderTypeItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.PaymentIDs_AND = 0;
        						}
        						break;
        					case scope.itemNames.registerItem:
        						if (scope.searchConfig.registerItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.registerItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.RegIDs_AND = 1;
        						}
        						else {
        							scope.searchConfig.registerItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.RegIDs_AND = 0;
        						}
        						break;
        					case scope.itemNames.transactionItem:
        						if (scope.searchConfig.transactionItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.transactionItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.TransNB_AND = 1;
        						}
        						else {
        							scope.searchConfig.transactionItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.TransNB_AND = 0;
        						}
        						break;
        					case scope.itemNames.employeeItem:
        						if (scope.searchConfig.employeeItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.employeeItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.EmpIDs_AND = 1;
        						}
        						else {
        							scope.searchConfig.employeeItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.EmpIDs_AND = 0;
        						}
        						break;
        					case scope.itemNames.totalItem:
        						if (scope.searchConfig.totalItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.totalItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.TransAmount_AND = 1;
        						}
        						else {
        							scope.searchConfig.totalItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.TransAmount_AND = 0;
        						}
        						break;
        					case scope.itemNames.descriptionItem:
        						if (scope.searchConfig.descriptionItem.conditionPrefix === scope.conditionPrefixConst.Or) {
        							scope.searchConfig.descriptionItem.conditionPrefix = scope.conditionPrefixConst.And;
        							scope.searchModel.DescIDs_AND = 1;
        						}
        						else {
        							scope.searchConfig.descriptionItem.conditionPrefix = scope.conditionPrefixConst.Or;
        							scope.searchModel.DescIDs_AND = 0;
        						}
        						break;
        				}
        			}

        			scope.toggleConditionType = function (fieldItem) {
        				switch (fieldItem) {
        					case scope.itemNames.transactionItem:
        						if (scope.searchConfig.transactionItem.conditionType === scope.conditionTypeConst.Greater) {
        							scope.searchConfig.transactionItem.conditionType = scope.conditionTypeConst.Lesster;
        							scope.searchModel.TransNB_OP = scope.conditionTypeConst.Lesster;
        						}
        						else if (scope.searchConfig.transactionItem.conditionType === scope.conditionTypeConst.Lesster) {
        							scope.searchConfig.transactionItem.conditionType = scope.conditionTypeConst.Equal;
        							scope.searchModel.TransNB_OP = scope.conditionTypeConst.Equal;
        						}
        						else if (scope.searchConfig.transactionItem.conditionType === scope.conditionTypeConst.Equal) {
        							scope.searchConfig.transactionItem.conditionType = scope.conditionTypeConst.Any;
        							scope.searchModel.TransNB_OP = scope.conditionTypeConst.Any;
        						}
        						else if (scope.searchConfig.transactionItem.conditionType === scope.conditionTypeConst.Any) {
        							scope.searchConfig.transactionItem.conditionType = scope.conditionTypeConst.Greater;
        							scope.searchModel.TransNB_OP = scope.conditionTypeConst.Greater;
        						}
        						break;
        					case scope.itemNames.totalItem:
        						if (scope.searchConfig.totalItem.conditionType === scope.conditionTypeConst.Greater) {
        							scope.searchConfig.totalItem.conditionType = scope.conditionTypeConst.Lesster;
        							scope.searchModel.TransAmount_OP = scope.conditionTypeConst.Lesster;
        						}
        						else if (scope.searchConfig.totalItem.conditionType === scope.conditionTypeConst.Lesster) {
        							scope.searchConfig.totalItem.conditionType = scope.conditionTypeConst.Equal;
        							scope.searchModel.TransAmount_OP = scope.conditionTypeConst.Equal;
        						}
        						else if (scope.searchConfig.totalItem.conditionType === scope.conditionTypeConst.Equal) {
        							scope.searchConfig.totalItem.conditionType = scope.conditionTypeConst.Any;
        							scope.searchModel.TransAmount_OP = scope.conditionTypeConst.Any;
        						}
        						else if (scope.searchConfig.totalItem.conditionType === scope.conditionTypeConst.Any) {
        							scope.searchConfig.totalItem.conditionType = scope.conditionTypeConst.Greater;
        							scope.searchModel.TransAmount_OP = scope.conditionTypeConst.Greater;
        						}
        						break;
        				}
        			}

        			scope.ddlChanged = function (fieldItem) {
        				//Anh, Keep checked list while filter
        				return; //Anh, don't use this function anymore - support 
        				//switch (fieldItem) {
        				//	case scope.itemNames.tenderTypeItem:
        				//		break;
        				//		scope.TenderTypeSelected = scope.$$childTail.TenderTypeSelected;
        				//		if (scope.TenderTypeSelected.length > 0) {
        				//			scope.searchModel.PaymentIDs = Enumerable.From(scope.TenderTypeSelected).Select(function (x) { return x.ID; }).ToArray();
        				//		}
        				//		else {
        				//			scope.searchModel.PaymentIDs = [];
        				//		}
        				//		break;
        				//	case scope.itemNames.registerItem:
        				//		scope.RegisterSelected = scope.$$childTail.RegisterSelected;
        				//		if (scope.RegisterSelected.length > 0) {
        				//			scope.searchModel.RegIDs = Enumerable.From(scope.RegisterSelected).Select(function (x) { return x.ID; }).ToArray();
        				//		}
        				//		else {
        				//			scope.searchModel.RegIDs = [];
        				//		}
        				//		break;
        				//	case scope.itemNames.employeeItem:
        				//		scope.EmployeeSelected = scope.$$childTail.EmployeeSelected;
        				//		if (scope.EmployeeSelected.length > 0) {
        				//			scope.searchModel.EmpIDs = Enumerable.From(scope.EmployeeSelected).Select(function (x) { return x.ID; }).ToArray();
        				//		}
        				//		else {
        				//			scope.searchModel.EmpIDs = [];
        				//		}
        				//		break;
        				//	case scope.itemNames.descriptionItem:
        				//		scope.DescriptionSelected = scope.$$childTail.DescriptionSelected;
        				//		if (scope.DescriptionSelected.length > 0) {
        				//			scope.searchModel.DescIDs = Enumerable.From(scope.DescriptionSelected).Select(function (x) { return x.ID; }).ToArray();
        				//		}
        				//		else {
        				//			scope.searchModel.DescIDs = [];
        				//		}
        				//		break;
        				//}
        			}

        			scope.clickOutside = function (event, element) {
        				if (angular.element(element).hasClass('open')) {
        					angular.element(element).removeClass('open');
        				}
        			};

        			scope.changeFilterType = function (value) {
        				changeFilterTypeFn(value);
        			};

        			function getMaxDate() {
        				return moment().format('L');
        			}

        			scope.searchSubmitEventHandler = function () {
        				scope.clickOutside(null, '.advanceSearch');
        				if (scope.searchModel.DateTo.getTime() - scope.searchModel.DateFrom.getTime() >= 0) {
        					$rootScope.$broadcast("RebarDynamicFilterRender", { config: scope.searchConfig, data: angular.copy(scope.searchModel), itemNames: scope.itemNames });
        					scope.searchSubmitEvent();
        				}
        				else
        					$rootScope.$broadcast("INVALID_DATE", {});
        			};

        			//broadcast event to render the breadcrumbs.
        			$rootScope.$broadcast("RebarDynamicFilterRender", { config: scope.searchConfig, data: angular.copy(scope.searchModel), itemNames: scope.itemNames });

        			function isExistItem(list, iid) {
        				var item = Enumerable.From(list).Where(function (x) { return x.ID === iid; })
								.Select(function (x) { return x; }).FirstOrDefault();
        				if (item) {
        					return true;
        				}
        				else {
        					return false;
        				}
        			}
        			function appendItems(list, newdata) {
        				var lsItems = list;
        				if (!lsItems) {
        					lsItems = [];
        				}
        				if (angular.isArray(newdata)) {
        					var newItems = Enumerable.From(newdata).Where(function (x) { return isExistItem(list, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
        					lsItems = lsItems.concat(newItems);
        				}
        				else {
        					if (isExistItem(lsItems, newdata.ID) == false) {
        						lsItems.push(newdata);
        					}
        				}
        				if (lsItems) {
        					lsItems.sort(function (a, b) {
        						return a.Name.localeCompare(b.Name);
        					});
        				}
        				return lsItems;
        			}

        			function FilterCache(name, value) {
        				var item = POSCommonSvc.GetCache(name, true);
        				if (item == null)
        					return null;

        				var IE_Items = Enumerable.From(item);
        				var result = IE_Items.Where(function (x) { return x.Name.Contains(value) == true });
        				return result.ToArray();
        			}
        			function GetCacheData(name, parampage) {
        				var item = POSCommonSvc.GetCache(name, true);
        				if (item != null) {
        					var IE_Items = Enumerable.From(item);
        					var response = { data: {}, CurrentPage: parampage.PageNumber, totalPages: 0 };
        					var totalCount = IE_Items.Count();
        					response.totalPages = parseInt(totalCount / parampage.PageSize);
        					if (totalCount % parampage.PageSize > 0)
        						response.totalPages = response.totalPages + 1;

        					var array = IE_Items.Skip(parampage.PageSize * (parampage.PageNumber - 1)).Take(parampage.PageSize).ToArray();
        					response.data = array;
        					response.CurrentPage = parampage.PageNumber;
        					return response;
        				}
        				return null;
        			}
        			//Get data
        			function getTenderTypeList(pageNumber) {
        				if (scope.paymentTotalPage < scope.paymentCurrentPage) { return; }

        				var params = {
        					PageSize: 10,
        					PageNumber: pageNumber
        				};
        				var items = GetCacheData(AppDefine.POSItemKeys.Payments, params);
        				if (items) {
        					scope.searchData.TenderTypeList = appendItems(scope.searchData.TenderTypeList, items.data);

        					scope.paymentCurrentPage = items.CurrentPage;
        					scope.paymentTotalPage = items.totalPages;
        				}

        				//rebarDataSvc.GetPaymentList(params, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//var newItems = Enumerable.From(response.Data).Where(function (x) { return isExistItem(scope.searchData.TenderTypeList, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
        				//	//scope.searchData.TenderTypeList = scope.searchData.TenderTypeList.concat(newItems);//(response.Data);
        				//	//if (scope.searchData.TenderTypeList) {
        				//	//	scope.searchData.TenderTypeList.sort(function (a, b) {
        				//	//		return a.Name.localeCompare(b.Name);
        				//	//	});
        				//	//}
        				//	scope.searchData.TenderTypeList = appendItems(scope.searchData.TenderTypeList, response.Data);

        				//	scope.paymentCurrentPage = response.CurrentPage;
        				//	scope.paymentTotalPage = response.TotalPage;
        				//	localStorage.setItem("TenderTypeList", JSON.stringify(response.Data));
        				//},
        				//function (error) {
        				//	console.log(error);
        				//});
        			}

        			function getRegisterList(pageNumber) {
        				if (scope.registerTotalPage < scope.registerCurrentPage) { return; }

        				var params = {
        					PageSize: 10,
        					PageNumber: pageNumber
        				};


        				var items = GetCacheData(AppDefine.POSItemKeys.Registers, params);
        				if (items) {
        					scope.searchData.RegisterList = appendItems(scope.searchData.RegisterList, items.data);

        					scope.registerCurrentPage = items.CurrentPage;
        					scope.registerTotalPage = items.totalPages;
        				}

        				//rebarDataSvc.GetRegisterList(params, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//var newItems = Enumerable.From(response.Data).Where(function (x) { return isExistItem(scope.searchData.RegisterList, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
        				//	//scope.searchData.RegisterList = scope.searchData.RegisterList.concat(newItems);//(response.Data);
        				//	//if (scope.searchData.RegisterList) {
        				//	//	scope.searchData.RegisterList.sort(function (a, b) {
        				//	//		return a.Name.localeCompare(b.Name);
        				//	//	});
        				//	//}
        				//	scope.searchData.RegisterList = appendItems(scope.searchData.RegisterList, response.Data);

        				//	scope.registerCurrentPage = response.CurrentPage;
        				//	scope.registerTotalPage = response.TotalPage;
        				//	localStorage.setItem("RegisterList", JSON.stringify(response.Data));
        				//},
        				//function (error) {
        				//	console.log(error);
        				//});
        			}

        			function getEmployeeList(pageNumber) {
        				if (scope.employeeTotalPage < scope.employeeCurrentPage) { return; }

        				var params = {
        					PageSize: 10,
        					PageNumber: pageNumber
        				};

        				var items = GetCacheData(AppDefine.POSItemKeys.Operators, params);
        				if (items) {
        					scope.searchData.EmployeeList = appendItems(scope.searchData.EmployeeList, items.data);

        					scope.employeeCurrentPage = items.CurrentPage;
        					scope.employeeTotalPage = items.totalPages;
        				}

        				//rebarDataSvc.GetOperatorList(params, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//var newItems = Enumerable.From(response.Data).Where(function (x) { return isExistItem(scope.searchData.EmployeeList, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
        				//	//scope.searchData.EmployeeList = scope.searchData.EmployeeList.concat(newItems);//(response.Data);
        				//	//if (scope.searchData.EmployeeList) {
        				//	//	scope.searchData.EmployeeList.sort(function (a, b) {
        				//	//		return a.Name.localeCompare(b.Name);
        				//	//	});
        				//	//}
        				//	scope.searchData.EmployeeList = appendItems(scope.searchData.EmployeeList, response.Data);

        				//	scope.employeeCurrentPage = response.CurrentPage;
        				//	scope.employeeTotalPage = response.TotalPage;
        				//	localStorage.setItem("EmployeeList", JSON.stringify(response.Data));
        				//},
        				//function (error) {
        				//	console.log(error);
        				//});
        			}

        			function getDescriptionList(pageNumber) {
        				if (scope.descriptionTotalPage < scope.descriptionCurrentPage) { return; }

        				var params = {
        					PageSize: 10,
        					PageNumber: pageNumber
        				};

        				var items = GetCacheData(AppDefine.POSItemKeys.Descriptions, params);
        				if (items) {
        					scope.searchData.DescriptionList = appendItems(scope.searchData.DescriptionList, items.data);

        					scope.descriptionCurrentPage = items.CurrentPage;
        					scope.descriptionTotalPage = items.totalPages;
        				}

        				//rebarDataSvc.GetDescriptionList(params, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//var newItems = Enumerable.From(response.Data).Where(function (x) { return isExistItem(scope.searchData.DescriptionList, x.ID) == false; }).Select(function (x) { return x; }).ToArray();
        				//	//scope.searchData.DescriptionList = scope.searchData.DescriptionList.concat(newItems);//(response.Data);
        				//	//if (scope.searchData.DescriptionList) {
        				//	//	scope.searchData.DescriptionList.sort(function (a, b) {
        				//	//		return a.Name.localeCompare(b.Name);
        				//	//	});
        				//	//}
        				//	scope.searchData.DescriptionList = appendItems(scope.searchData.DescriptionList, response.Data);

        				//	scope.descriptionCurrentPage = response.CurrentPage;
        				//	scope.descriptionTotalPage = response.TotalPage;
        				//	localStorage.setItem("DescriptionList", JSON.stringify(response.Data));
        				//},
        				//function (error) {
        				//	console.log(error);
        				//});
        			}

        			function filterTenderList(value) {
        				if (!value) { return; }

        				var result = FilterCache(AppDefine.POSItemKeys.Payments, value);
        				scope.searchData.TenderTypeList = appendItems(result, scope.TenderTypeSelected);

        				//rebarDataSvc.filterPayment({ filter: value }, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//response.sort(function (a, b) {
        				//	//		return a.Name.localeCompare(b.Name);
        				//	//	});
        				//	//scope.searchData.TenderTypeList = response;
        				//	scope.searchData.TenderTypeList = appendItems(response, scope.TenderTypeSelected);
        				//},function (error) {
        				//	console.log(error);
        				//});
        			}

        			function filterRegisterList(value) {
        				if (!value) { return; }

        				var result = FilterCache(AppDefine.POSItemKeys.Registers, value);
        				scope.searchData.RegisterList = appendItems(result, scope.RegisterSelected);

        				//rebarDataSvc.filterRegister({ filter: value }, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//response.sort(function (a, b) {
        				//	//	return a.Name.localeCompare(b.Name);
        				//	//});
        				//	//scope.searchData.RegisterList = response;
        				//	scope.searchData.RegisterList = appendItems(response, scope.RegisterSelected);
        				//}, function (error) {
        				//	console.log(error);
        				//});
        			}

        			function filterEmployeeList(value) {
        				if (!value) { return; }

        				var result = FilterCache(AppDefine.POSItemKeys.Operators, value);
        				scope.searchData.EmployeeList = appendItems(result, scope.EmployeeSelected);

        				//rebarDataSvc.filterOperator({ filter: value }, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//response.sort(function (a, b) {
        				//	//	return a.Name.localeCompare(b.Name);
        				//	//});
        				//	//scope.searchData.EmployeeList = response;
        				//	scope.searchData.EmployeeList = appendItems(response, scope.EmployeeSelected);
        				//}, function (error) {
        				//	console.log(error);
        				//});
        			}

        			function filterDescriptionList(value) {
        				if (!value) { return; }

        				var result = FilterCache(AppDefine.POSItemKeys.Descriptions, value);
        				scope.searchData.DescriptionList = appendItems(result, scope.DescriptionSelected);

        				//rebarDataSvc.filterDescription({ filter: value }, function (response) {
        				//	//Anh, Keep checked list while filter
        				//	//response.sort(function (a, b) {
        				//	//	return a.Name.localeCompare(b.Name);
        				//	//});
        				//	//scope.searchData.DescriptionList = response;
        				//	scope.searchData.DescriptionList = appendItems(response, scope.DescriptionSelected);
        				//}, function (error) {
        				//	console.log(error);
        				//});
        			}

        			function changeFilterTypeFn(value) {
        				scope.filterTypeSelected = value;
        				if (scope.singleDate || scope.advanceSearch) {
        					scope.searchConfig.dateFrom.isReadOnly = scope.searchConfig.dateTo.isReadOnly = false;
        				}
        				else {
        					scope.searchConfig.dateFrom.isReadOnly = scope.searchConfig.dateTo.isReadOnly = value.key !== scope.filterTypeConst.Custom;
        				}

        				switch (scope.filterTypeSelected.key) {
        					case scope.filterTypeConst.Yesterday:
        						// Tri Add: Fix bug #3628
        						scope.searchModel.DateTo = new Date();
        						var yesterday = new Date(scope.searchModel.DateTo);
        						yesterday.setDate(yesterday.getDate() - 1);
        						scope.searchModel.DateFrom = scope.searchModel.DateTo = yesterday;
        						scope.searchConfig.dateFrom.options.ignoreReadonly = scope.searchConfig.dateTo.options.ignoreReadonly = false;
        						break;
        					case scope.filterTypeConst.Last7days:
        						// Tri Add: Fix bug #3628
        						scope.searchModel.DateTo = new Date();
        						var last7days = new Date(scope.searchModel.DateTo);
        						last7days.setDate(last7days.getDate() - 6);
        						scope.searchModel.DateFrom = last7days;
        						//scope.searchModel.DateTo = new Date();
        						scope.searchConfig.dateFrom.options.ignoreReadonly = scope.searchConfig.dateTo.options.ignoreReadonly = false;
        						break;
        					case scope.filterTypeConst.Last30days:
        						// Tri Add: Fix bug #3628
        						scope.searchModel.DateTo = new Date();
        						var last30days = new Date(scope.searchModel.DateTo);
        						last30days.setDate(last30days.getDate() - 29);
        						scope.searchModel.DateFrom = last30days;
        						//scope.searchModel.DateTo = new Date();
        						scope.searchConfig.dateFrom.options.ignoreReadonly = scope.searchConfig.dateTo.options.ignoreReadonly = false;
        						break;
        					case scope.filterTypeConst.CurrentDay:
        						scope.searchModel.DateTo = new Date();
        						scope.searchModel.DateFrom = Utils.beginOfDate(scope.searchModel.DateTo);
        						scope.searchConfig.dateFrom.options.ignoreReadonly = scope.searchConfig.dateTo.options.ignoreReadonly = true;
        						break;
        					default:
        						scope.searchConfig.dateFrom.options.ignoreReadonly = scope.searchConfig.dateTo.options.ignoreReadonly = true;
        						break;
        				}
        			}

        			function isCannedRpt(stName) {
        				var ret = false;
        				switch (stName) {
        					case AppDefine.State.REBAR_REFUNDS:
        					case AppDefine.State.REBAR_VOIDS:
        					case AppDefine.State.REBAR_CANCELS:
        					case AppDefine.State.REBAR_NOSALES:
        					case AppDefine.State.REBAR_DISCOUNTS:
        					case AppDefine.State.REBAR_QUICKSEARCH: //Quick search will not change date like canned report
        						ret = true;
        						break;
        					default:
        						break;
        				}
        				return ret;
        			}
        		}
        	};
        }]);

		module.directive('cmsSwitchButton', ['$rootScope', function ($rootScope) {
			return {
				restrict: 'EA',
				replace: false,
				templateUrl: 'Scripts/Directives/TemplateDirective/switch-button.html',
				scope: {
					label: '@',
					model: '=',
					isDisable: '='
				},
				link: function (scope, elem, attrs) {

				}
			};
		}]);

		return module;
	});

    
})
();