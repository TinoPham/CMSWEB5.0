( function () {
	'use strict';
	//connection state value:
							//connecting: 0,
							//connected: 1,
							//reconnecting: 2,
							//disconnected: 4
	define( ['cms'], function ( cms ) {

		cms.factory( 'SignalSvc', SignalSvc );
		SignalSvc.$inject = ['$rootScope'];
		function SignalSvc( $rootScope ) {
			function HubProxy( serverUrl, hubName, startOptions, stateChanged ) {
				var connection = $.hubConnection( serverUrl );
				//$.connection.hub.qs = { 'version': '2.2' };
				var proxy = connection.createHubProxy( hubName );
				var startoptions = startOptions;
				var tryingToReconnect = false;
				var disconnected_callback, reconnecting_callback, reconnected_callback;

				connection.reconnecting( _reconnecting );
				connection.reconnected( _reconnected );
				connection.disconnected( _disconnected );
				
				function _connect( donecallback, failedcallback ) {

					connection.start( startoptions ).done( function () {
						console.log( 'Now connected, connection ID =' + connection.id );
						if ( donecallback )
							donecallback();
					} ).fail( function ( error ) {
						console.log(error);
							if ( failedcallback ) failedcallback();
					} );
				}

				function _reconnecting( data ) {
					tryingToReconnect = true;
					if ( reconnecting_callback )
						reconnecting_callback( data );
				}

				function _reconnected( data ) {
					tryingToReconnect = false;
					if ( reconnected_callback )
						reconnected_callback( data );
				}
				function _disconnected( data ) {

					if( tryingToReconnect)
						setTimeout( _connect, 5000 ); // Restart connection after 5 seconds.

					if ( disconnected_callback )
						disconnected_callback(data);
				}

				return {

					error: function(callback){
						if ( callback )
							connection.error = callback;
					},

					stateChanged: function ( callback ) {
						if(callback)
							connection.stateChanged(callback);
					},

					connectionSlow: function ( callback ) {
						if ( callback )
							connection.connectionSlow( callback );
					},

					reconnecting: function ( callback ) {
						reconnecting_callback = callback;
					},

					reconnected: function ( callback ) {
						reconnected_callback = callback;
					},

					disconnected: function ( callback ) {
						disconnected_callback = callback;
					},

					///connect to server
					connect: function ( donecallback, failedcallback ) { _connect( donecallback, failedcallback ); },
					//register event from server
					on: function ( eventName, callback ) {
						proxy.on( eventName, function ( result ) {
							$rootScope.$apply( function () {
								if ( callback ) {
									callback( result );
								}
							} );
						} );
					},
					//turn off event
					off: function ( eventName, callback ) {
						proxy.off( eventName, function ( result ) {
							$rootScope.$apply( function () {
								if ( callback ) {
									callback( result );
								}
							} );
						} );
					},
					//call server function
					invoke: function ( methodName, data, callback ) {
						proxy.invoke( methodName, data )
							.done( function ( result ) {
								$rootScope.$apply( function () {
									if ( callback ) {
										callback( result );
									}
								} );
							} );
					},
					connection: connection
				};
			};

			return HubProxy;
		}
		

	});
} )();