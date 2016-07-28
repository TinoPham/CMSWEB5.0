( function () {
    define( ['cms'], function ( cms ) {

        cms.register.service( 'ReportService', ReportService );

        ReportService.$inject = ['AppDefine', '$resource', '$http', 'Utils'];

        function ReportService( AppDefine, $resource, $http, Utils ) {

            var apibase = AppDefine.Api.Report;
            var Alerts = $resource( apibase, null, {
              
                'DshAlertCMP': { url: apibase + '/dshalertcmp?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
                , 'DshAlerts': { url: apibase + '/dshalerts?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
                , 'DshConversion': { url: apibase + '/dshconversion?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
                , 'DshTraffic': { url: apibase + '/dshtraffic?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
                , 'DshConversionMap': { url: apibase + '/dshconversionmap?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
				, 'DshConversionSites': { url: apibase + '/dshconversionsites?:p', params: {}, headers: cms.EncryptHeader(), isArray: false, method: 'get' }
            } );

            var _AlertCompare = function ( param, Success, Error ) {

                var strpram = Utils.Object2QueryString( param, null);
                Alerts.DshAlertCMP( { p: strpram } ).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        if ( Success )
                            Success( data );
                    },
                    function ( error ) {
                        if ( Error )
                            Error( error );
                    }
                    );
            }

            var _DshAlerts = function ( param, Success, Error ) {

                var strpram = Utils.Object2QueryString( param, null );
                Alerts.DshAlerts( { p: strpram } ).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        if ( Success )
                            Success( data );
                    },
                    function ( error ) {
                        if ( Error )
                            Error( error );
                    }
                    );
            }
            var GetData = function ( name, param, Success, Error ) {
                var strpram = Utils.Object2QueryString( param, null );
                Alerts[name]( { p: strpram } ).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        if ( Success )
                            Success( data );
                    },
                    function ( error ) {
                        if ( Error )
                            Error( error );
                    }
                    );
            }
            return {
                AlertCompare: function ( param, Success, Error ) { return GetData( "DshAlertCMP", param, Success, Error ); }
                , DshAlerts: function ( param, Success, Error ) { return GetData( "DshAlerts", param, Success, Error ); }
                , DshConversion: function ( param, Success, Error ) { return GetData( "DshConversion", param, Success, Error ); }
                , DshTraffic: function ( param, Success, Error ) { return GetData( "DshTraffic", param, Success, Error ); }
                , DshConversionMap: function ( param, Success, Error ) { return GetData( "DshConversionMap", param, Success, Error ); }
				, DshConversionSites: function ( param, Success, Error ) { return GetData( "DshConversionSites", param, Success, Error ); }
            };
        }
    } );

}
)();