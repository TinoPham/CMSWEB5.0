( function () {

	define( ['cms'], function ( cms ) {

		cms.register.service( 'POSCommonSvc', POSCommonSvc );

		POSCommonSvc.$inject = ['AppDefine', '$resource', '$http', 'Utils', '$q', '$cacheFactory'];
		function POSCommonSvc( AppDefine, $resource, $http, Utils, $q, $cacheFactory ) {

			var POSItemCache = $cacheFactory( 'POSItemCache' );
			var apibase = AppDefine.Api.POSItems + '/:name';
			var api_dataservice = _InitResource();
			function _InitResource() {
					var header = cms.EncryptHeader ? cms.EncryptHeader() : null;
					return $resource( apibase, { name: "@name" }, {
					'get': { method: 'GET', interceptor: { response: function ( response ) { return response; } } }
				} );
			}

			function getURL( url ) {
				var jqxhr = $.ajax( {
					type: "GET",
					url: url,
					contentType: "application/json; charset=utf-8",
					cache: false,
					async: false
				} );

				var response = { valid: jqxhr.statusText, data: jqxhr.responseJSON };

				return response;
			}

			function _LoadDataKey( name, synchronous )
			{
				if ( synchronous ) {
					var valitems = null;
					var reply = getURL( AppDefine.Api.POSItems + '/' + name );
					if ( reply.valid == 'OK' && reply.data != null ) {
						valitems = cms.GetResponseData( reply.data );
						POSItemCache.put( name, valitems )
					}
					return valitems;
				}
				else

				return api_dataservice.get({ name : name},
											function ( response ) {
												var value = cms.GetResponseData( response );
												if ( value )
													POSItemCache.put( name, value )
											},
											function ( response ) {
												var headers = response.headers();
											} );

			}

			function _GetCache( name, load )
			{
				var items = POSItemCache.get( name );
				if ( items )
					return items;

				return _LoadDataKey( name, true );
			}
			function _RemoveCache( name ) {
				return POSItemCache.remove( name );
			}
			function _ClearCache( ) {
				return POSItemCache.removeAll( );
			}

			return {
				LoadDataCache: _LoadDataKey,
				GetCache: _GetCache,
				RemoveCache: _RemoveCache,
				ClearCache:_ClearCache
			}


		}
	} );
}
)();