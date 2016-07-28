(function() {
    'use strict';
    
    var app = angular.module( 'cms.auth', [] ).factory( 'InterceptorSvc', InterceptorSvc);
   
    //app.factory( 'RequestsMonitor', function ( $rootScope ) {
    //    $rootScope.startedRequests = [];
    //    $rootScope.finishedRequests = [];
    //    $rootScope.pendingRequests = [];

    //    return {
    //        _nextId: 1,
    //        recordStart: function ( config ) {
    //            if ( !config._id ) {
    //                config._id = this._nextId++;
    //            }

    //            $rootScope.startedRequests.push( config );
    //            $rootScope.pendingRequests.push( config );
    //        },
    //        recordEnd: function ( config ) {
    //            var index = $rootScope.finishedRequests.indexOf( config );
    //            if ( index != -1 ) {
    //                $rootScope.finishedRequests.push( {
    //                    url: config.url,
    //                    _id: 'duplicate of ' + config._id
    //                } );
    //            } else {
    //                $rootScope.finishedRequests.push( config );
    //            }

    //            index = $rootScope.pendingRequests.indexOf( config );
    //            if ( index != -1 ) {
    //                $rootScope.pendingRequests.splice( index, 1 );
    //            }
    //        }
    //    };
    //} );

    //app.config( ['$provide', function ( $provide ) {
    //    $provide.decorator( '$templateRequest', function ( $delegate ) {
    //        var mySilentProvider = function ( tpl, ignoreRequestError ) {
    //            return $delegate( tpl, true );
    //        }
    //        return mySilentProvider;
    //    } );
    //}] );
    
    
    InterceptorSvc.$inject = ['$q', '$injector', '$location', 'AppDefine', 'Utils', 'Base64', 'Cookies'];

    function InterceptorSvc( $q, $injector, $location, AppDefine, Utils, Base64, Cookies) {
        var authInterceptorFactory = {
            request: _request,
            responseError: _responseError,
            response:_response
        };
        var url_v = require.toUrl( '' );
        return authInterceptorFactory;

        function _response( response ) {
            
            if ( response.data && isEnCryptHeader( response.config, AppDefine.Encrypt_Header ) ) {
                var data = response.data;
                var token = GetTokenID( response, AppDefine, Cookies );
                var jsonString = Utils.DecryptString( response.data, token, Base64 );//CryptoJS.AES.decrypt( data, id ).toString( CryptoJS.enc.Utf8 );
                response.data = JSON.parse( jsonString );
            }
            
            return response || $q.when( response );
            //return response;
        }

        function _request( config ) {
            
            if ( config.data && isEnCryptHeader( config, AppDefine.Encrypt_Header ) ) {
                var token = GetTokenID( config, AppDefine, Cookies );
                if ( token ) {
                    var str_data = JSON.stringify( config.data );
                    var enc = Utils.EncryptString( str_data, token, Base64 );
                    config.data = enc;
                }
                
                
            }

        	//config.url = getVersionByUrl(config.url);
            if ( config.url.indexOf( '.tpl.html' ) < 0 && config.url.indexOf( 'template/' ) < 0 && ( config.url.endsWith( '.html' ) > 0 || config.url.endsWith( '.json' ) > 0 || config.url.endsWith( '.css' ) > 0 ) ) {
            	config.url += url_v;
            }
            
            return config || $q.when( config );
        }


        function getVersionByUrl(url) {
            if (
                url &&
                (
                (url.indexOf('template/') < 0)
                && (url.indexOf('.html') !== -1)
                || url.indexOf('.json') !== -1
                || url.indexOf('.css') !== -1)
                ) {
                return url + '?' + Version;
            }
            return url;
        }

        function isEnCryptHeader( config, encypt_header ) {
            if ( config && encypt_header ) {
                var header = config.method == 'GET' ? config.headers['Accept'] : config.headers['Content-Type'];
                if (!header) return false;
                var enc_header = header.length < encypt_header.length ? '' : header.substring( 0, encypt_header.length );
                return enc_header == encypt_header;
            }
            return false;

        }

        function GetTokenID(config, appdefine, cookies ) {
            var token = cookies.get( appdefine.XSRF_TOKEN_KEY );
            if ( !token || token.length == 0 ) {
            	if (!config.hasOwnProperty('config'))
            		token = config.headers[appdefine.SID];
            	else
            		token = config.config.headers[appdefine.SID];
            }
            return token;
        }
        
        
        function _responseError(rejection) {
			//if (rejection.status === 401) {
			//	$location.path('/login');
			//}
			//return $q.reject(rejection);

            if (rejection.status === 401) { //Unauthorized: 401
                var accSvc = $injector.get('AccountSvc');
                accSvc.ClearUser();
                $location.path('/login');
            }
			else if (rejection.status === 400 && isEnCryptHeader(rejection.config, AppDefine.Encrypt_Header)) {//BadRequest: 400
				var data = rejection.data;
				var token = GetTokenID(rejection, AppDefine, Cookies);
				var jsonString = Utils.DecryptString(rejection.data, token, Base64);//CryptoJS.AES.decrypt( data, id ).toString( CryptoJS.enc.Utf8 );
				rejection.data = JSON.parse(jsonString);
			}
			
            return $q.reject(rejection);
        }
    }
})();