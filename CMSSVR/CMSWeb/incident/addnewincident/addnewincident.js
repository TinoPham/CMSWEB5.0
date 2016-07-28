(function () {
    define(['cms'], function (cms) {
        cms.register.controller('addnewincidentCtrl', addnewincidentCtrl);
        addnewincidentCtrl.$inject = ['$scope', '$rootScope', '$timeout', '$stateParams', 'cmsBase', 'AppDefine','$modal',];
        function addnewincidentCtrl($scope, $rootScope, $timeout, $stateParams, cmsBase, AppDefine, $modal) {

            var vm = this;
            vm.optionsDate = { format: 'L', maxDate: $scope.maxdate, ignoreReadonly: true };
            vm.optionsDatetime = { ignoreReadonly: true };
			vm.dateFrom = new Date();
			vm.optionsTime = { format: 'LT'};

			vm.demoMenu = 1;

            detechScroll();


            // vm.demoMenu = function (){
            // 	return 1;
            // }

            vm.showVideoDialog = function () {

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'incident/addnewincident/modal/video.html',
                        controller: 'addnewincidentCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                          windowClass: 'incident-video-modal',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                      
                    });
                }
			} /* END :: function showNarrativeDialog	 */

            vm.showNarrativeDialog = function () {

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'incident/addnewincident/modal/narrative.html',
                        controller: 'addnewincidentCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                          windowClass: 'incident-narrative-modal',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                      
                    });
                }
			} /* END :: function showNarrativeDialog	 */


            vm.showNoteDialog = function () {

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'incident/addnewincident/modal/note.html',
                        controller: 'addnewincidentCtrl as vm',
                        size: 'sm',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                          windowClass: 'incident-note-modal',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                      
                    });
                }
			} /* END :: function showNoteDialog	 */



        	vm.showLinkDialog = function () {

                if (!$scope.modalShown) {
                    $scope.modalShown = true;
                    var userInstance = $modal.open({
                        templateUrl: 'incident/addnewincident/modal/link.html',
                        controller: 'addnewincidentCtrl as vm',
                        size: 'md',
                        backdrop: 'static',
                        backdropClass: 'modal-backdrop',
                          windowClass: 'incident-link-modal',
                        keyboard: false,
                        resolve: {
                            items: function () {
                                return { };
                            }
                        }
                    });

                    userInstance.result.then(function (data) {
                      
                    });
                }
			} /* END :: function showLinkDialog	 */

			vm.cancel = function () {
	            if ($scope.hasChanged === true) {
	                $modalInstance.close($scope.hasChanged);
	            }
	            $modalInstance.close();
	        }


            function detechScroll(){            	
            	 $(document).on("scroll", onScroll);
    
				    //smoothscroll
				    $('body').on('click', '#nav-right a[href^="#"]', function(e) {				   
				        e.preventDefault();
				        $(document).off("scroll");

				        $('a').each(function () {
				            $(this).removeClass('active');
				        })
				        $(this).addClass('active');
				      
				        var target = this.hash,
				            menu = target;

				        $target = $(target);
				        $('html, body').stop().animate({
				            'scrollTop': ( $target.offset().top -100 )			           
				        }, 500, 'swing', function () {
				            // window.location.hash = target;
				            $(document).on("scroll", onScroll);				            
				        });
				    });


        	} /* END :: detechScroll */

			function onScroll (event){
			  
				var scrollPos = $(document).scrollTop();			    
				

			    $('#nav-right > li > a').each(function () {	
			        var currLink = $(this);
			        var refElement = $(currLink.attr("href"));	
			        var elementChildName = refElement.selector+"-nav-child";

			        if (refElement.position().top <= scrollPos && refElement.position().top + refElement.height() > scrollPos) {
			            $('#nav-right > li > a ').removeClass("active");
			            currLink.addClass("active");

			            /*For child*/			            
			             $( elementChildName+' li  a').each(function () {	 
					        var currLink = $(this);
					        var refElement = $(currLink.attr("href"));						      
					        if (refElement.offset().top-120 <= scrollPos && refElement.offset().top-120 + refElement.height() > scrollPos) {
					            $(elementChildName+' > li > a ').removeClass("active");
					            currLink.addClass("active");					           
					        }
					        else{
					            currLink.removeClass("active");
					        }
					    });
			            /*END :: For child*/
			        }
			        else{
			            currLink.removeClass("active");
			             $(elementChildName+' > li > a ').removeClass("active");

			        }
			    });
            }/* END :: onScroll */




        } /* END :: addnewincidentCtrl */
    });    
})();