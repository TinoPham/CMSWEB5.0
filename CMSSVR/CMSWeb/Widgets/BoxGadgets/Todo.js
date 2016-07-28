(function() {
    'use strict';

	define(['cms', 'Scripts/Directives/contextMenu', 'Services/dialogService'], function (cms) {
        cms.register.controller('todoCtrl', todoCtrl);

		todoCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'AppDefine', 'dialogSvc', '$timeout'];
		function todoCtrl($scope, dataContext, cmsBase, AppDefine, dialogSvc, $timeout) {
            var vm = this;
			var newContent = 'TODO_DEFAULT_VALUE';
            var todoStatusClass = ['high', 'medium', 'low', 'none'];
            var model = {
            	Id: 0,
            	UserId: 0,
            	CreatedOn: null,
            	Content: "",
                Color: 0,
                Icon: "",
                Status: 0,
            	Font: 0,
            	Urgency: 0,
            	Recurrence: 0
            }

            //vm.statusTodo = false;
            vm.DivHeight = AppDefine.ChartHeight + 27;
            vm.todoSelected = {};
            vm.processTodo = {};
			//vm.crolltoend = false;
            angular.copy(model, vm.todoSelected);

            active().finally(function() {
                $scope.$parent.isLoading = false;
            });
            
            function active() {
                var def = cmsBase.$q.defer();

                dataContext.injectRepos(['todo']).then(function () {
                    dataContext.todo.getTodo(function (data) {
                        vm.todos = data;
                        refreshStatus();
                        def.resolve();
                    }, function (error) {
                        cmsBase.cmsLog.error(error.data.Data);
                        def.reject();
                    });
                });

                return def.promise;
            }

            vm.dragstart = function(e, node) {
                e.stopPropagation();
                e.preventDefault();
            }

            vm.editTodo = function (todo, isNew) {
            	angular.copy(todo, vm.todoSelected);
            	if (isNew) {
            		vm.todoSelected.Content = '';
            	}
            }

            vm.checkedTodo = function (todo) {
				var todoData = {};
				angular.copy(todo, todoData);
				todoData.Status = todoData.Status === false ? 0 : 1;
				dataContext.todo.editTodo(todoData, function (data) {
                    data.Status = data.Status === 0 ? false : true;
                    angular.copy(data, todo);
                }, function(error) {
                    cmsBase.cmsLog.error(error.data.Data);
                });
            }

            vm.addNote = function () {
				//vm.crolltoend = true;
            	model.Content = cmsBase.translateSvc.getTranslate(newContent);

                dataContext.todo.insertTodo(model, function (data) {
                    //$scope.$appl
                    data.Status = data.Status === 0 ? false : true;
                    vm.todos.unshift(data);
					vm.editTodo(data, true);
					//vm.crolltoend = false;
                }, function (error) {
                    cmsBase.cmsLog.error(error.data.Data);
                });
            }

            vm.onRightClick = function(todo) {
                vm.processTodo = todo;
            }

            vm.changeTodo = function (todo) {
            	vm.todoSelected.Status = vm.todoSelected.Status === false ? 0 : 1;
            	if (vm.todoSelected.Content == null || vm.todoSelected.Content.length == 0) {
            		cmsBase.cmsLog.error(cmsBase.translateSvc.getTranslate(AppDefine.Resx.CONTENT_IS_EMPTY));
            		return;
            	}
                dataContext.todo.editTodo(vm.todoSelected, function (data) {
                    data.Status = data.Status === 0 ? false : true;
                    angular.copy(data, todo);
                    angular.copy(model, vm.todoSelected);
                }, function (error) {
                    cmsBase.cmsLog.error(error.data.Data);
                });
            }

            vm.cancelEditTodo = function () {
                angular.copy(model, vm.todoSelected);
            }

            vm.deleteTodo = function(todo) {
				var modalData = {
                    closeButtonText: AppDefine.Resx.BTN_CANCEL,
                    actionButtonText: AppDefine.Resx.BTN_DELETE,
					headerText: AppDefine.Resx.TODO_HEADER,
                    bodyText: AppDefine.Resx.DELETE_TODO_CONFIRM
                };
				var modalOptions = {
					size: 'sm'
				};

				dialogSvc.showModal(modalOptions, modalData).then(function (result) {
					if (result === AppDefine.ModalConfirmResponse.OK) {
                        dataContext.todo.deleteTodo(todo.Id, function () {
                            vm.todos.splice(vm.todos.indexOf(todo), 1);
                        }, function (error) {
                            cmsBase.cmsLog.error(error.data.Data);
                        });
                    }
                });
            }

            vm.todoStatusChange = function(todo) {
                var classstr = '';
                if (todo.Status === 1) classstr += 'todo-done ';
                classstr += todoStatusClass[todo.Color];
                return classstr;
            }

            vm.menuSelectHigh = function () {
                vm.processTodo.Urgency = 0;
                vm.processTodo.Color = 0;
                updateTodoColor(vm.processTodo);
            }
            vm.menuSelectMedium = function() {
                vm.processTodo.Urgency = 1;
                vm.processTodo.Color = 1;
                updateTodoColor(vm.processTodo);
            }
            vm.menuSelectLow = function() {
                vm.processTodo.Urgency = 2;
                vm.processTodo.Color = 2;
                updateTodoColor(vm.processTodo);
            }
            vm.menuSelectNone = function() {
                vm.processTodo.Urgency = 3;
                vm.processTodo.Color = 3;
                updateTodoColor(vm.processTodo);
            }

            function updateTodoColor(processTodo) {
                dataContext.todo.editTodo(processTodo, function (data) {
                   // data.Status = data.Status === 0 ? false : true;
                	angular.copy(data, processTodo);
                	vm.todoSelected.Urgency = processTodo.Urgency; //Update for current selected item
                	vm.todoSelected.Color = processTodo.Color;
                }, function (error) {
                    cmsBase.cmsLog.error(error.data.Data);
                });
            }

            function refreshStatus() {
                angular.forEach(vm.todos, function (todo) {
                    todo.Status = todo.Status === 0 ? false : true;
                });
            }
        }
    });
})();