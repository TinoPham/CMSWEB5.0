(function () {
	define(['cms'], function (cms) {
	    cms.register.service('LdapSvc', LdapSvc);
	    LdapSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q'];

	    function LdapSvc(AppDefine, Cookies, $resource, $http, $q) {

	        var apibase = AppDefine.Api.LDAP + '/:dest';
	        var LDAP = $resource(apibase, { dest: "@dest" }, {
	            'GetAllSynUser': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetAllSynUser" }, interceptor: { response: function ( response ) { return response; } } }
	            , 'GetAllSynUserType': { method: 'GET', headers: cms.EncryptHeader(), isArray: false, params: { dest: "GetAllSynUserType" }, interceptor: { response: function ( response ) { return response; } } }
                , 'AddSynUser': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "AddSynUser" }, interceptor: { response: function ( response ) { return response; } } }
                , 'DeleteSynUser': { method: 'POST', headers: cms.EncryptHeader(), params: { dest: "DeleteSynUser" }, interceptor: { response: function ( response ) { return response; } } }
			});
			var LDAPModel = InitLDAPModel();
			function InitLDAPModel() {
			    return {
			        SynID: 0
                    , ServerIP: null
                    , UserID: null
                    , PassWord: null
                    , isSSL: true
                    , Interval: 1
                    , Time: null
                    , LastSyn: Date.now()
                    , isEnable: true
                    , isForceUpdate: true
                    , SynType: 0
                    , CreateBy: 0
                    , LastSynresult: null
                    , SynName: null
                    , UUsername: null
				};
			}

			var GetAllSynUser = function () {
			    var def = $q.defer();

			    LDAP.GetAllSynUser().$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
			    return def.promise;
			};

			var GetAllSynUserType = function () {
			    var def = $q.defer();

			    LDAP.GetAllSynUserType().$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
			    return def.promise;
			};

			var AddSynUser = function (synUserModel) {
			    var def = $q.defer();

			    LDAP.AddSynUser(synUserModel).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
			    return def.promise;
			};

			var DeleteSynUser = function (SynID) {
			    var def = $q.defer();

			    LDAP.DeleteSynUser(SynID).$promise.then(
                    function ( response ) {
                        var data = cms.GetResponseData( response );
                        def.resolve(data);
                    },
                    function (error) {
                        console.log(error);
                        def.reject(error);
                    }
                );
			    return def.promise;
			};

			return {
			    GetAllSynUser: GetAllSynUser,
			    AddSynUser: AddSynUser,
			    GetAllSynUserType: GetAllSynUserType,
			    DeleteSynUser: DeleteSynUser,
			    LDAPModel: function () { return LDAPModel; }
			};

		}

	});

}
)();
