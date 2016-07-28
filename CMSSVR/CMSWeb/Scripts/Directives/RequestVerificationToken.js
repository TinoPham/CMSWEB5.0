(function(){
    define(['cms'], function(cms) {
        cms.directive( 'ncgRequestVerificationToken', ['$http', function ( $http ) {
            return function ( scope, element, attrs ) {
                $http.defaults.headers.common['RequestVerificationToken'] = attrs.ncgRequestVerificationToken || "";
            };
        } ])});
    }

    )();
