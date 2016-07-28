// Source: https://github.com/amitava82/angular-multiselect

angular.module('ui.multiselect', [
  'multiselect.tpl.html'
])

  //from bootstrap-ui typeahead parser
  .factory('optionParser', ['$parse', function ($parse) {

  	//                      00000111000000000000022200000000000000003333333333333330000000000044000
  	var TYPEAHEAD_REGEXP = /^\s*(.*?)(?:\s+as\s+(.*?))?\s+for\s+(?:([\$\w][\$\w\d]*))\s+in\s+(.*)$/;

  	return {
  		parse: function (input) {

  			var match = input.match(TYPEAHEAD_REGEXP), modelMapper, viewMapper, source;
  			if (!match) {
  				throw new Error(
				  "Expected typeahead specification in form of '_modelValue_ (as _label_)? for _item_ in _collection_'" +
					" but got '" + input + "'.");
  			}

  			return {
  				itemName: match[3],
  				source: $parse(match[4]),
  				viewMapper: $parse(match[2] || match[1]),
  				modelMapper: $parse(match[1])
  			};
  		}
		, numtoRGB: function (num) {
			num >>>= 0;
			var b = num & 0xFF,
			 g = (num & 0xFF00) >>> 8,
			 r = (num & 0xFF0000) >>> 16;
			//a = ( (num & 0xFF000000) >>> 24 ) / 255 ;
			return "rgb(" + [r, g, b].join(",") + ")";
		}
  	};
  }])

  .directive('multiselect', ['$parse', '$document', '$compile', '$interpolate', '$timeout', 'optionParser', 'cmsBase', 'AppDefine',

    function ($parse, $document, $compile, $interpolate, $timeout, optionParser, cmsBase, AppDefine) {
    	return {
    		restrict: 'E',
    		require: 'ngModel',
    		link: function (originalScope, element, attrs, modelCtrl) {
    			var exp = attrs.options,
				parsedResult = optionParser.parse(exp),
				isMultiple = attrs.multiple ? true : false,
				required = false,
				scope = originalScope.$new(),
				changeHandler = attrs.change || angular.noop,
				hideIcon = attrs.hideIcon ? true : false;
    		    //2016-06-28 Tri Add flag dropdown of Alert Types.
    			scope.isAlertTypes = attrs.isalerttypes ? true : false;

    			scope.items = [];
    			//scope.header = 'SELECT';
    			scope.header = cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT);
    			scope.multiple = isMultiple;
    			scope.disabled = false;
    			scope.hideIcon = hideIcon;
    			scope.filterText = "";
    			scope.autocomplete = attrs.autocomplete !== undefined ? true : false;
    			var timer;

    			originalScope.$on('$destroy', function () {
    				scope.$destroy();
    				$timeout.cancel(timer);
    			});

    			var popUpEl = angular.element('<multiselect-popup></multiselect-popup>');

    			//required validator
    			if (attrs.required || attrs.ngRequired) {
    				required = true;
    			}
    			attrs.$observe('required', function (newVal) {
    				required = newVal;
    			});

    			//watch disabled state
    			scope.$watch(function () {
    				return $parse(attrs.disabled)(originalScope);
    			}, function (newVal) {
    				scope.disabled = newVal;
    			});

    			//watch single/multiple state for dynamically change single to multiple
    			scope.$watch(function () {
    				return $parse(attrs.multiple)(originalScope);
    			}, function (newVal) {
    				isMultiple = newVal || false;
    			});

    			//watch option changes for options that are populated dynamically
    			scope.$watch(function () {
    				return parsedResult.source(originalScope);
    			}, function (newVal) {
    				if (angular.isDefined(newVal))
    					parseModel();
    			}, true);

    			//watch model change
    			scope.$watch(function () {
    				return modelCtrl.$modelValue;
    			}, function (newVal, oldVal) {
    				//when directive initialize, newVal usually undefined. Also, if model value already set in the controller
    				//for preselected list then we need to mark checked in our scope item. But we don't want to do this every time
    				//model changes. We need to do this only if it is done outside directive scope, from controller, for example.
    				if (angular.isDefined(newVal)) {
    					markChecked(newVal);
    					scope.$eval(changeHandler);
    				}
    				getHeaderText();
    				modelCtrl.$setValidity('required', scope.valid());
    			}, true);

    			//watch searchText.label changed
    			scope.$watch("filterText", function (newVal, oldVal) {
    				if (newVal !== oldVal && scope.autocomplete) {
    					$timeout.cancel(timer);
    					timer = $timeout(function () {
    						//scope.uncheckAll(); //Anh, Keep checked while filter
    						scope.$emit("filterdata", { filter: newVal, name: attrs.name });
    					}, 700);
    				}
    			});

    			function parseModel() {
    				scope.items.length = 0;
    				var model = parsedResult.source(originalScope);
    				if (!angular.isDefined(model) || model === null) { return; }
    				for (var i = 0; i < model.length; i++) {
    					var local = {};
    					local[parsedResult.itemName] = model[i];
    					scope.items.push({
    						label: parsedResult.viewMapper(local),
    						model: model[i],
    						checked: false
    					});
    				}

    				//ThangPham, keep list selected last
    				if (angular.isDefined(modelCtrl.$modelValue)) {
    					//console.log(modelCtrl.$modelValue);
    					markChecked(modelCtrl.$modelValue);
    					getHeaderText();
    				}
    			}

    			parseModel();

    			element.append($compile(popUpEl)(scope));

    			function getHeaderText() {
    				if (is_empty(modelCtrl.$modelValue)) return scope.header = attrs.msHeader || cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT);

    				if (isMultiple) {
    					if (attrs.msSelected) {
    						scope.header = $interpolate(attrs.msSelected)(scope);
    					}
    					else {
    						scope.header = modelCtrl.$modelValue.length + ' ' + cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECTED);
    					}

    				}
    				else {
    					var local = {};
    					local[parsedResult.itemName] = modelCtrl.$modelValue;
    					scope.header = parsedResult.viewMapper(local);
    				}
    			}

    			function is_empty(obj) {
    				if (!obj) return true;
    				if (obj.length && obj.length > 0) return false;
    				for (var prop in obj) if (obj[prop]) return false;
    				return true;
    			};

    			scope.valid = function validModel() {
    				if (!required) return true;
    				var value = modelCtrl.$modelValue;
    				return (angular.isArray(value) && value.length > 0) || (!angular.isArray(value) && value != null);
    			};

    			function selectSingle(item) {
    				if (item.checked) {
    					//scope.uncheckAll(); //ThangPham, Always keep last selected item for single select, Sept 28 2015.
    				} else {
    					scope.uncheckAll();
    					item.checked = !item.checked;
    				}
    				setModelValue(false);
    			}

				function selectMultiple(item) {
					item.checked = !item.checked;
					setModelValue(true);
					scope.$emit("checkedchange", { item: item, name: attrs.name });
				}

    			function setModelValue(isMultiple) {
    				var value;

    				if (isMultiple) {
    					value = [];
    					angular.forEach(scope.items, function (item) {
    						if (item.checked) value.push(item.model);
    					})
    				} else {
    					angular.forEach(scope.items, function (item) {
    						if (item.checked) {
    							value = item.model;
    							return false;
    						}
    					})
    				}
    				modelCtrl.$setViewValue(value);
    			}

    			function markChecked(newVal) {
    				if (!angular.isArray(newVal)) {
    					angular.forEach(scope.items, function (item) {
    						if (angular.equals(item.model, newVal)) {
    							item.checked = true;
    							return false;
    						}
    					});
    				} else {
    					angular.forEach(newVal, function (i) {
    						angular.forEach(scope.items, function (item) {
    							//item.checked = false;
    							if (angular.equals(item.model, i)) {
    								item.checked = true;
    							}
    						});
    					});
    				}
    			}

    			scope.checkAll = function () {
    				if (!isMultiple) return;
    				angular.forEach(scope.items, function (item) {
    					item.checked = true;
    				});
    				setModelValue(true);
    				scope.$emit("checkall", attrs.name);
    			};

    			scope.uncheckAll = function () {
    				angular.forEach(scope.items, function (item) {
    					item.checked = false;
    				});
    				setModelValue(true);
    				scope.$emit("uncheckall", attrs.name);
    			};

    		    //2016-06-29 Tri add function save show hide Alert
    			scope.Save_ShowHide_Alert = function () {
    			    var value = [];
    			    angular.forEach(scope.items, function (item) {
    			        if (item.checked) value.push(item.model);
    			    })
    			    scope.$emit("SaveShowHideAlert", value);
    			}

    			scope.select = function (item) {
    				if (isMultiple === false) {
    					selectSingle(item);
    					scope.toggleSelect();
    				} else {
    					selectMultiple(item);
    				}
    			};

    			scope.jqueryScrollbarOptions = {
    				"onScroll": function (y, x) {
    					//console.log(modelCtrl);
    					if (y.scroll <= 0) { return; }

    					if (Math.ceil(y.scroll) >= Math.ceil(y.maxScroll)) {
    						scope.$emit("loadmoredata", attrs.name);
    					}
    				}
    			};

    			scope.toggleLoadData = function (e) {
    				//if (scope.items && scope.items.length > 0) { return; }
    				scope.$emit("loadmoredata", attrs.name);
    			};
    		}
    	};
    }])

  .directive('multiselectPopup', ['$document', 'optionParser', function ($document, optionParser) {
  	return {
  		restrict: 'E',
  		scope: false,
  		replace: true,
  		templateUrl: 'multiselect.tpl.html',
  		link: function (scope, element, attrs) {

  			scope.isVisible = false;

  			scope.toggleSelect = function () {
  				if (element.hasClass('open')) {
  					element.removeClass('open');
  					$document.unbind('click', clickHandler);
  				} else {
  					element.addClass('open');
  					$document.bind('click', clickHandler);
  					scope.focus();
  				}
  			};

  			function clickHandler(event) {
  				if (elementMatchesAnyInArray(event.target, element.find(event.target.tagName)))
  					return;
  				element.removeClass('open');
  				$document.unbind('click', clickHandler);
  				scope.$apply();
  			}

  			scope.focus = function focus() {
  				var searchBox = element.find('input')[0];
  				searchBox.focus();
  			}

  			var elementMatchesAnyInArray = function (element, elementArray) {
  				for (var i = 0; i < elementArray.length; i++)
  					if (element == elementArray[i])
  						return true;
  				return false;
  			}

  			scope.numtoRGB = function (num) {
  				return optionParser.numtoRGB(num);
  			}
  		}
  	}
  }]);

angular.module('multiselect.tpl.html', [])

  .run(['$templateCache', function ($templateCache) {
  	$templateCache.put('multiselect.tpl.html',

      "<div class=\"btn-group\" ng-click=\"toggleLoadData()\">\n" +
      "  <span class=\"form-control dropdown-toggle\" ng-click=\"toggleSelect()\" ng-disabled=\"disabled\" ng-class=\"{'error': !valid()}\">\n" +
      "    <div class=\"select-header\">{{header | translate}}</div> <span class=\"caret pull-right\"></span>\n" +
      "  </span>\n" +
      "  <ul class=\"dropdown-menu\">\n" +
      "    <li class=\"input-search\">\n" +
      "      <input ng-show=\"!autocomplete\" class=\"form-control input-sm\" type=\"text\" ng-model=\"searchText.label\" autofocus=\"autofocus\" placeholder=\"Filter\" />\n" +
	  "      <input ng-show=\"autocomplete\" class=\"form-control input-sm\" type=\"text\" ng-model=\"filterText\" autofocus=\"autofocus\" placeholder=\"Filter\" />\n" +
      "    </li>\n" +
      "    <li ng-show=\"multiple\" role=\"presentation\" class=\"multiple-controls\">\n" +
      "      <button class=\"btn btn-link btn-xs\" ng-click=\"checkAll()\" type=\"button\"><i class=\"glyphicon glyphicon-ok\"></i>{{'CHECK_ALL' | translate}}</button>\n" +
      "      <button class=\"btn btn-link btn-xs\" ng-click=\"uncheckAll()\" type=\"button\"><i class=\"glyphicon glyphicon-remove\"></i>{{'UNCHECK_ALL' | translate}}</button>\n" +
      //2016-06-28 Tri add button OK of dropdown Alert Types
      "      <button ng-if=\"isAlertTypes\" class=\"btn btn-link btn-xs\" ng-click=\"Save_ShowHide_Alert()\" type=\"button\"><i class=\"glyphicon glyphicon-save\"></i>{{'BTN_DONE' | translate}}</button>\n" +
      "    </li>\n" +
      "    <li>\n" +
	  "			<ul class=\"dropdown-menu-list scroll-body scrollbar-dynamic\" data-jquery-scrollbar=\"jqueryScrollbarOptions\"> \n" + //slimscroll=\"{height: '250px'}\"
	  "				<li ng-repeat=\"i in items | filter:searchText\" title=\"\">\n" +
	  "					<div ng-class=\"{'checkbox checkbox-default': !hideIcon}\" ng-if=\"multiple\" ng-click=\"select(i); focus()\"> \n" +
		"					<input type=\"checkbox\" ng-checked=\"i.checked\" ng-hide=\"hideIcon\"> \n" +
		"					<label class=\"i-name\">{{i.label | translate}}</label> \n" +
		"				</div>" +
		"				<div ng-class=\"{'radio radio-default': !hideIcon}\" ng-if=\"!multiple\" ng-click=\"select(i); focus()\"> \n" +
		"					<input type=\"radio\" ng-checked=\"i.checked\" ng-hide=\"hideIcon\"> \n" +
		"					<label class=\"i-name\"> \n" +
		"						<span ng-if=\"i.model.Color !== undefined\" class=\"i-color\" ng-style=\"{'border-color': numtoRGB(i.model.Color) } \"></span> \n" + //Set for Job Title Select {{}};
  		"						{{i.label | translate}} \n" +
		"					</label> \n" +
		"				</div>" +
		"				<div style=\"clear: both;\"></div>\n" +
		"			</li> \n" +
		"		</ul> \n" +
       "    </li>\n" +
       // 2015-06-11 Tri add button close.
       "			<li class=\"btn-close-select-dropdown\"> \n" +
        "               <div ng-click='toggleSelect()'>{{'BTN_CLOSE' | translate}}</div>" +
        "			</li> \n" +
      "  </ul>\n" +
      "</div>");
  }]);
