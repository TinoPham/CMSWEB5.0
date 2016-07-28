( function () {
    define( ['cms'], function ( cms ) {

        cms.register.service( 'SiteSvc', SiteSvc );

        SiteSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http'];

        function SiteSvc( AppDefine, Cookies, $resource, $http ) {

            var apibase = AppDefine.Api.Site;
            var Sites = $resource( apibase, null, {
                'get': { url: apibase + '/sites', headers: cms.EncryptHeader(), isArray: false, method: 'GET', interceptor: { response: function ( response ) { return response; } } }
                  , GetDVRInfoRebarTransact: { url: apibase + '/GetDVRInfoRebarTransact', headers: cms.EncryptHeader(), isArray: false, method: 'GET', interceptor: { response: function (response) { return response; } } }
                
                //, 'post': { method: 'POST', interceptor: { response: function ( response ) { return response; } } }
                //, 'edit': { method: 'PUT', interceptor: { response: function ( response ) { return response; } } }
                //, 'delete': { method: 'DELETE', interceptor: { response: function ( response ) { return response; } } }
            } );
            

            var GetSites = function ( successfunction, ErrorFunction ) {

                Sites.get( null,
                    function ( response ) {
                        if ( successfunction != null ){
                            var data = cms.GetResponseData( response );
                            successfunction( data);
                        }
                    },
                     function ( response ) {
                         ErrorFunction( response );
                     }
                    );

            };
            
            var GetDVRInfoRebarTransact = function (timeline, DVRInfo,succfnc,errfunc) {
                Sites.GetDVRInfoRebarTransact({ 'kdvr': DVRInfo.PacId }, function (result) {
                    var data = cms.GetResponseData(result);
                    succfnc(data, DVRInfo, timeline);

                }, function (err) {
                   // errfunc(err);
                });
            };
            
            return {
                GetSites: GetSites
                ,GetDVRInfoRebarTransact:GetDVRInfoRebarTransact
                //,Login: Login,
                //SID: function () { return SID },
                //UserModel: function () { return UserModel; }

            };
            
        }

    } );

}
)();
