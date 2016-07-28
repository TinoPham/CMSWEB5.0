// Source: https://github.com/amitava82/angular-multiselect

angular.module('ngSelectSerial', [
  'multiselectSerial.tpl.html'
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

  .directive('multiselectSerial', ['$parse', '$document', '$compile', '$interpolate', 'optionParser', 'AppDefine', 'cmsBase', 'AppDefine',

    function ($parse, $document, $compile, $interpolate, optionParser, AppDefine, cmsBase, AppDefine) {
    	return {
    		restrict: 'E',
    		require: 'ngModel',
    		link: function (originalScope, element, attrs, modelCtrl) {

    			var exp = attrs.options,
				parsedResult = optionParser.parse(exp),
				isMultiple = attrs.multiple ? true : false,
				hideSelectAllBtn = attrs.HideSelectAllBtn ? true : false,
				required = false,
				scope = originalScope.$new(),
				changeHandler = attrs.change || angular.noop,
				hideIcon = attrs.hideIcon ? true: false;

    			scope.items = [];
    			scope.incurredItems = [];
    		    //scope.header = 'Select';
    			scope.header = cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT);
    			scope.multiple = isMultiple;
    			scope.hideSelectAllBtn = hideSelectAllBtn;
    			scope.disabled = false;
    			scope.hideIcon = hideIcon;
    			scope.itemSelected = [];

    			originalScope.$on('$destroy', function () {
    				scope.$destroy();
    			});

    			originalScope.$on(AppDefine.Events.REMOVESERIALNUMBER, function (event, arg) {
    				if (angular.isArray(arg)) {
    					angular.forEach(arg, function (value) {
    						scope.items.push({
    							label: value.SerialNumber,
    							model: value,
    							checked: false
    						});
    					});
    				}
    				else {
    					scope.items.push({
    						label: arg.SerialNumber,
    						model: arg,
    						checked: false
    					});
    				}
    			});

    			var popUpEl = angular.element('<multiselect-serial-popup></multiselect-serial-popup>');

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

    			function parseModel() {
    				scope.items.length = 0;
    				var model = parsedResult.source(originalScope);
    				if (!angular.isDefined(model)) return;
    				for (var i = 0; i < model.length; i++) {
    					var local = {};
    					local[parsedResult.itemName] = model[i];
    					scope.items.push({
    						label: parsedResult.viewMapper(local),
    						model: model[i],
    						checked: false
    					});
    				}
    				scope.incurredItems = angular.copy(scope.items);
    				setIncurredItemsData();
    			}

    			parseModel();

    			element.append($compile(popUpEl)(scope));

    			function getHeaderText() {
    			    if (is_empty(modelCtrl.$modelValue)) return scope.header = attrs.msHeader || cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECT);

    				if (isMultiple) {
    					if (attrs.msSelected) {
    						scope.header = $interpolate(attrs.msSelected)(scope);
    					} else {
    					    scope.header = modelCtrl.$modelValue.length + ' ' + cmsBase.translateSvc.getTranslate(AppDefine.Resx.HEADER_SELECTED);
    					}

    				} else {
    					var local = {};
    					local[parsedResult.itemName] = modelCtrl.$modelValue;
    					scope.header = parsedResult.viewMapper(local);
    				}

    				setIncurredItemsData();
    			}

    			function setIncurredItemsData() {
    				angular.forEach(modelCtrl.$viewValue, function (item) {
    					if(!isInArray(scope.incurredItems, item.SerialNumber)){
							scope.incurredItems.push({
    						label: item.SerialNumber,
    						model: item,
    						checked: false
    					});
    					}
    				});
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
    					angular.forEach(scope.items, function (item) {
    						item.checked = false;
    						angular.forEach(newVal, function (i) {
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
    			};

    			scope.uncheckAll = function () {
    				angular.forEach(scope.items, function (item) {
    					item.checked = false;
    				});
    				setModelValue(true);
    			};

    			scope.select = function (item) {
    				if (isMultiple === false) {
    					selectSingle(item);
    					scope.toggleSelect();
    				} else {
    					selectMultiple(item);
    				}

    				scope.itemSelected = [];
    				angular.forEach(modelCtrl.$viewValue, function (value) {
    					scope.itemSelected.push({
    						label: value.SerialNumber,
    						model: value,
    						checked: true
    					});
    				});
    			}

    			scope.addItem = function (value) {
    				if(!value){
    					//var msg = cmsBase.translateSvc.getTranslate(AppDefine.Resx.SERIAL_NUMBER_REQUIRED_MSG);
    					//cmsBase.cmsLog.warning(msg);
    					return;
    				}

    				if (isInArray(scope.incurredItems, value)) {
    					var msg = formatString(cmsBase.translateSvc.getTranslate(AppDefine.Resx.SERIAL_NUMBER_EXIST_MSG), value);
    					cmsBase.cmsLog.warning(msg);
    					return;
    				}

    				var item = {
    					label: value,
    					checked: false,
    					model: {
    						KDVR: 0,
    						SerialNumber: value,
							ServerID: 'Virtual DVR'
    					}
    				};
    				scope.items.push(item);
    				scope.incurredItems.push(item);
    				scope.$applyAsync();
    			}

    			scope.removeItem = function (item) {
    				if (item.model.KDVR == 0) {//only allow remove Serial number that added by user on GUI.
    					scope.items.splice(scope.items.indexOf(item), 1);
    					scope.itemSelected.splice(scope.itemSelected.indexOf(item), 1);
    					scope.incurredItems.splice(scope.incurredItems.indexOf(item), 1);
    				}
    			}

    			scope.GenerateSerial = function () {
    				if (scope.itemSelected.length > 0) {
    					var data = [];
    					angular.forEach(scope.itemSelected, function (item) {
    						var dataItems = angular.copy(scope.items);
    						for (var i = 0; i < dataItems.length; i++) {
    							if (dataItems[i].model.SerialNumber === item.model.SerialNumber) {
    								scope.items.splice(i, 1);
    							}
    						}
    						data.push(item.model);
    					});
    					scope.$emit(AppDefine.Events.GENERATESERIALNUMBER, data);
    					scope.itemSelected = [];
    					scope.$applyAsync();
    				}
    			}

    			function isInArray(arr, item){
    				var ret = $.grep(arr, function(orginalItem){
    					return orginalItem.model.SerialNumber == item;
    				});
    				return ret.length > 0;
    			}

    			function indexOfArray(arr, item) {
    				var ret = $.grep(arr, function (orginalItem) {
    					return orginalItem.model.SerialNumber == item;
    				});
    			}

    			function formatString(format) {
    				var args = Array.prototype.slice.call(arguments, 1);
    				return format.replace(/{(\d+)}/g, function (match, number) {
    					return typeof args[number] != 'undefined' ? args[number] : match;
    				});
    			};
    		}
    	};
    }])

  .directive('multiselectSerialPopup', ['$document', 'optionParser', function ($document, optionParser) {
  	return {
  		restrict: 'E',
  		scope: false,
  		replace: true,
  		templateUrl: 'multiselectSerial.tpl.html',
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

  			scope.showDropdown = function () {
  			    if (element.hasClass('open')) {
			          return;
			      }
			      element.addClass('open');
		          $document.bind('click', clickHandler);
		          scope.focus();
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

angular.module('multiselectSerial.tpl.html', [])

  .run(['$templateCache', function ($templateCache) {
  	$templateCache.put('multiselectSerial.tpl.html',

		"<div class=\"input-group\">\n" +
			"<div class=\"dropdown-toggle\" ng-click=\"toggleSelect()\" ng-disabled=\"disabled\" ng-class=\"{'error': !valid()}\">\n" +
				"<input class=\"form-control input-sm\" type=\"text\" ng-model=\"searchText.label\" placeholder=\"{{'ADD_NEW_DVR' | translate}}\" maxlength=\"10\" />\n" + // autofocus=\"autofocus\"
				"<span class=\"caret pull-right\"></span>\n" +
			" </div>\n" +
			"<ul class=\"dropdown-menu\">\n" +
				"<li ng-show=\"hideSelectAllBtn\" role=\"presentation\" class=\"multiple-controls\">\n" +
					"<button class=\"btn btn-link btn-xs\" ng-click=\"checkAll()\" type=\"button\"><i class=\"glyphicon glyphicon-ok\"></i> Check all</button>\n" +
					"<button class=\"btn btn-link btn-xs\" ng-click=\"uncheckAll()\" type=\"button\"><i class=\"glyphicon glyphicon-remove\"></i> Uncheck all</button>\n" +
				"</li>\n" +
				"<li>\n" +
					"<ul class=\"dropdown-menu-list scroll-body scrollbar-dynamic\" data-jquery-scrollbar> \n" +
						"<li ng-repeat=\"i in items | filter:searchText\" title=\"\">\n" +
							"<div ng-class=\"{'checkbox checkbox-default': !hideIcon}\" ng-if=\"multiple\" ng-click=\"select(i); focus()\"> \n" +
								"<input type=\"checkbox\" ng-checked=\"i.checked\" ng-hide=\"hideIcon\"> \n" +
								"<label class=\"i-name\">{{i.label}} (Server ID: {{i.model.ServerID}})</label> \n" +
								"<span ng-if=\"i.model.KDVR ==0\" class=\"icon-trash remove-item\" ng-click=\"removeItem(i)\"></span>" +
							"</div>" +
							"<div ng-class=\"{'radio radio-default': !hideIcon}\" ng-if=\"!multiple\" ng-click=\"select(i); focus()\"> \n" +
								"<input type=\"radio\" ng-checked=\"i.checked\" ng-hide=\"hideIcon\"> \n" +
								"<label class=\"i-name\"> \n" +
									"<span ng-if=\"i.model.Color !== undefined\" class=\"i-color\" ng-style=\"{'border-color': numtoRGB(i.model.Color) } \"></span> \n" + //Set for Job Title Select {{}};
										"{{i.label}} (Server ID: {{i.model.ServerID}}) \n" +
								"</label> \n" +
								"<span ng-if=\"i.model.KDVR==0\" class=\"icon-trash remove-item\" ng-click=\"removeItem(i)\"></span>" +
							"</div>" +
							"<div style=\"clear: both;\"></div>\n" +
						"</li> \n" +
					"</ul> \n" +
				"</li>\n" +
				"<li class=\"footer-dropdown\">\n" +
					"<div><span class=\"btn btn-success btn-block\" ng-click=\"GenerateSerial(); toggleSelect();\">{{'BTN_ADD' | translate}}</span></div>\n" +
				"</li>\n" +
			"</ul>\n" +
			"<span class=\"input-group-addon\" ng-click=\"addItem(searchText.label); showDropdown();\"><span class=\"icon-plus-circled-1\"></span></span>\n" +
		"</div>");
  }]);
