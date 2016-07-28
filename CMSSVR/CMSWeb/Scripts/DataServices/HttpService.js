( function () {
    define( ['cms'], function ( cms ) {

        var injectParams = ['$http', '$rootScope', '$q'];
        var factsvr = function ( $http, $rootScope, $q ) {

            this.post = function ( url, data ) {
                var deferred = $q.defer();

                $http.post( url, data )
                .success( function ( response, status, headers, config ) {
                    deferred.resolve( response, status, headers, config );
                } )
                .error( function ( response, status, headers, config ) {
                    deferred.reject( response, status, headers, config );
                } );
                return deferred.promise;
            }

            this.get = function( url ) {
                var deferred = $q.defer();

                $http.get( url )
                .success( function ( response, status, headers, config ) {
                    deferred.resolve( response, status, headers, config );
                } )
                .error( function ( response, status, headers, config ) {
                    deferred.reject( response, status, headers, config );
                } );
                return deferred.promise;

            }

            this.get = function( url, pram ) {
                var deferred = $q.defer();

                $http( {
                    url: url,
                    method: "GET",
                    params: pram
                } )
                .success( function ( response, status, headers, config ) {
                    deferred.resolve( { data: response, status: status, headers: headers(), config: config } );
                } )
                .error( function ( response, status, headers, config ) {
                    deferred.reject( {data: response, status: status, headers: headers(), config : config});
                } );
                return deferred.promise;

            }
        };
        factsvr.$inject = injectParams;
        cms.service( 'HttpSvr', factsvr );
    }

    );
}
)();
