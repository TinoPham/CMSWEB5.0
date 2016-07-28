(function(){
    define( ['cms'], function ( cms ) {

        cms.service( 'AccountSvc', AccountSvc );

        AccountSvc.$inject = ['AppDefine', 'Cookies', '$resource', '$http', '$q', 'Utils'];

        function AccountSvc( AppDefine, Cookies, $resource, $http, $q, Utils ) {

            var apibase = AppDefine.Api.Account + '/:dest';
            var Account = _InitResource();
            var UserModel = InitUserModel();
            var Sites = [];
            var SID = "";

            function InitUserModel() {
                return {
                  UserID: 0
                , FName: null
                , LName: null
                , GroupID: 0
                , PositionID: 0
                , CompanyID: 0
                , Accepted: false
                , Menus: null
                , Sites: null
                , Functions: null
                , Levels: null
				, UPhoto: null
				, Email: null
                };
            }

            function UpdateSID( headers ) {
                if ( headers != null ) {
                    SID = headers.sid;
                    $http.defaults.headers.common[AppDefine.SID] = headers.sid;
                    //$http.defaults.headers.post['Content-Type'] = 'application/encrypt';
                    //$http.defaults.headers.put['Content-Type'] = 'application/encrypt';
                    //$http.defaults.headers.delete['Content-Type'] = 'application/encrypt';
                }

            };

            function _InitResource() {

                var header = cms.EncryptHeader ? cms.EncryptHeader() : null;
                return $resource( apibase, { dest: "@dest" }, {
                    'get': { method: 'GET', interceptor: { response: function ( response ) { return response; } } }
                    , 'post': { method: 'POST', headers: header, interceptor: { response: function (response) { return response; } } }
                    , 'edit': { method: 'PUT', headers: header, interceptor: { response: function ( response ) { return response; } } }
                    , 'delete': { method: 'DELETE', headers: header, interceptor: { response: function (response) { return response; } } }
                    , 'logout': { method: 'GET', headers: header, params: { dest: "LogOut" }, interceptor: { response: function (response) { return response; } } }
                    , 'changePassword': { method: 'POST', headers: header, params: { dest: "ChangePassword" }, interceptor: { response: function (response) { return response; } } }
                    , 'resetPassword': { method: 'GET', headers: header, params: { dest: "ResetPassword" }, interceptor: { response: function (response) { return response; } } }
                } );

            }

            function Login_Error( ErrorFunction , error) {
            	if ( error == null )
            		error = new Object();
				error.Status = AppDefine.Resx.SET_COOKIE_FAIL_MSG;
            	ErrorFunction( error );
            }

            function Login_OK( OkFunction, data) {
				UserModel = data;
            	if ( OkFunction != null )
					OkFunction(data);
            }

            var Initialize = function ( successfunction, ErrorFunction ) {
                Account.get( null,
                    function ( response ) {
                        UpdateSID( response.headers() );
                        var data = cms.GetResponseData( response );
                        if ( data ){
                        	Login_OK( successfunction, data );
                        }
                    },
                     function ( response ) {
                         var headers = response.headers();
                         UpdateSID( headers );
                         if ( ErrorFunction != null )
                             ErrorFunction( response );
                     }
                    );

            };

            var Login = function ( model, successfunction, ErrorFunction ) {
                var newmodel = angular.copy( model );
                newmodel.UserName = CryptoJS.AES.encrypt( model.UserName, SID ).toString();
                newmodel.Password = CryptoJS.AES.encrypt( model.Password, SID ).toString();
                newmodel.SID = SID;
                Account.post( newmodel ).$promise.then(
                    function ( response ) {
                    	var data = cms.GetResponseData( response );
                    	if ( data ) {
                    		Login_OK( successfunction, data );
                    	}
                    	else {
                    		if ( ErrorFunction != null )
                    			Login_Error( ErrorFunction, response );
                    	}
                    },
                    function ( response ) {
                    	if ( ErrorFunction != null )
                    		ErrorFunction(response );
                    }

                    );

            };

            var ChangePassword = function (model, successfunction, ErrorFunction) {
                var newmodel = angular.copy(model);
                newmodel.CurrentPassword = CryptoJS.AES.encrypt(model.CurrentPassword, SID).toString();
                newmodel.NewPassword = CryptoJS.AES.encrypt(model.NewPassword, SID).toString();
                newmodel.SID = SID;
                Account.changePassword(newmodel).$promise.then(
                    function (response) {
                        var data = cms.GetResponseData(response);
                        if (data) {
                            Login_OK(successfunction, data);
                        }
                        else {
                            if (ErrorFunction != null)
                                Login_Error(ErrorFunction, response);
                        }
                    },
                    function (response) {
                        if (ErrorFunction != null)
                            ErrorFunction(response);
                    }

                    );

            };

            function resetPassword(data) {
                var def = $q.defer();
                Account.resetPassword(data).$promise.then(
					function (result) {
					    def.resolve(result);
					}
					, function (error) {
					    def.reject(error);
					}
				);
                return def.promise;
            }

            function WriteLogout() {
                var def = $q.defer();
                Account.logout().$promise.then(
					function (result) {
					    def.resolve(result);
					}
					, function (error) {
					    def.reject(error);
					}
				);
                return def.promise;
            }
            
            var Logout = function () {
                var def = $q.defer();
                WriteLogout().then( function() {
                    def.resolve();
                    },
                    function(){
                    	//var aliasPath = [];
                    	var aliasPath = Utils.CookiePath(); //$Url.cookiepath(); //GetAliasPath( location.pathname );
                    	var getc = Cookies.get( AppDefine.XSRF_TOKEN_KEY );
                    	if ( getc )
                    	{
                    		var domain;
                    		if ( Utils.ValidIpaddress( window.location.hostname ) )
                    			domain = window.location.hostname;
                    		else
                    			domain = '.' + window.location.hostname;

                    		Cookies.set( AppDefine.XSRF_TOKEN_KEY, '', domain, aliasPath, -365 ); //"/"
                    	}
                    	
                        UserModel = InitUserModel();
                        Sites = [];
                        def.reject();
                    }                    
                );
                return def.promise;
			};

			function GetAliasPath(path)
			{
				var ret = "/";
				if (!path) { return ret; }

				var aliasPath = path.split('/');
				if(aliasPath.length > 1)
				{
					if (aliasPath[1] == "api" || aliasPath[1] == "cmsweb") { return ret;}
					ret = ret + aliasPath[1];
				}

				return ret;
			}

			var isAuthenticated = function () {
				var isAuth = UserModel == null ? false : UserModel.UserID > 0;
				return isAuth;
			};

			function ClearUser() {
			    UserModel = InitUserModel();
            }

			var isDashboard = false;

            return {
                Initialize: Initialize
                , InitResource: function () { Account = _InitResource(); }
				, Login: Login
                , ChangePassword: ChangePassword
                , resetPassword: resetPassword
				,LogOut: Logout
				, SID: function () { return SID }
                , ClearUser: ClearUser
				,UserModel: function () { return UserModel; }
                , Sites: Sites
				, isAuthenticated: isAuthenticated
                , isDashboard: isDashboard
				, ProfileChange: function ( lname, fname, uphoto, email ) { UserModel.LName = lname; UserModel.UPhoto = uphoto; UserModel.FName = fname; UserModel.Email = email; }
            };
        }
    } );
    }
)();
