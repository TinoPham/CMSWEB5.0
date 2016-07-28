(function() {
    'use strict';

   var app =  angular.module('cmsResourceMock', ['ngMockE2E']);

   app.run(function ($httpBackend, $resource) {
       var routes = [
           {
               "name": "sites",
               "state": "sites",
               "url": "sites",
               "translate": "LAYOUT_SITES",
               "abstract": true,
               "child": []
           },
           {
               "name": "bam",
               "state": "bam",
               "url": "bam",
               "translate": "LAYOUT_BAM",
               "abstract": true,
               "child": []
           },
           {
               "name": "rebar",
               "state": "rebar",
               "url": "rebar",
               "translate": "LAYOUT_REBAR",
               "abstract": true,
               "child": []
           },
           {
               "name": "configuration",
               "state": "configuration",
               "url": "configuration",
               "isResource": true,
               "translate": "LAYOUT_CONFIGURATION",
               "abstract": true,
               "child": [
                   {
                       "name": "site",
                       "state": "configuration.site",
                       "url": "configuration/site",
                       "translate": "LAYOUT_CONFIG_SITE",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "account",
                       "state": "configuration.account",
                       "url": "configuration/account",
                       "isResource": true,
                       "translate": "LAYOUT_CONFIG_USER",
                       "dependens": ['configuration/account/accountDetails.js'],
                       "abstract": false,
                       "child": []
                   },
                      {
                          "name": "usergroups",
                          "state": "configuration.usergroups",
                          "url": "configuration/usergroups",
                          "isResource": true,
                          "translate": "Account",
                          "dependens": ['dataservices/configuration/usergroups.service.js'],
                          "abstract": false,
                          "child": [{
                              "name": "usergroupsdetail",
                              "state": "configuration.usergroupsdetail",
                              "url": "configuration/usergroups",
                              "params":":id",
                              "translate": "LAYOUT_CONFIG_USER",
                              "dependens": ['dataservices/configuration/usergroups.service.js'],
                              "abstract": false,
                              "child": []
                          }]
                      },
                   {
                       "name": "calendar",
                       "state": "configuration.calender",
                       "url": "configuration/calender",
                       "translate": "LAYOUT_CONFIG_CALENDAR",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "jobtitle",
                       "state": "configuration.jobtitle",
                       "url": "configuration/jobtitle",
                       "isResource": true,
                       "translate": "LAYOUT_CONFIG_JOBTILE",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "sitemetric",
                       "state": "configuration.sitemetric",
                       "url": "configuration/sitemetric",
                       "translate": "LAYOUT_CONFIG_SITEMETRIC",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "goaltype",
                       "state": "configuration.goaltype",
                       "url": "configuration/goaltype",
                       "translate": "LAYOUT_CONFIG_GOALTYPE",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "companyinformation",
                       "state": "configuration.companyinformation",
                       "url": "configuration/companyinformation",
                       "translate": "LAYOUT_CONFIG_COMPANYINFOR",
                       "abstract": false,
                       "child": []
                   },
                   {
                       "name": "recipient",
                       "state": "configuration.recipient",
                       "url": "configuration/recipient",
                       "translate": "LAYOUT_CONFIG_RECIPIENT",
                       "abstract": false,
                       "child": []
                   }
               ]

           }
       ];

        var usersJson = {
            "users": {
                "admin": {
                    "id": "1",
                    "username": "admin",
                    "password": "admin",
                    "userRole": "admin"
                },
                "editor": {
                    "id": "2",
                    "username": "editor",
                    "password": "editor",
                    "userRole": "editor"
                },
                "guest": {
                    "id": "3",
                    "username": "guest",
                    "password": "guest",
                    "userRole": "guest"
                }
            }
        };

        var userUrl = "/api/users";

        function loginMethod(method, url, data, header) {
            var head = header;
            var user = angular.fromJson(data);
            var token = {};
            if (user.userName == user.password) {
                token = { access_token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEyMzQ1Njc4OTAsIm5hbWUiOiJhZG1pbiIsInJvbGVzIjoiYWRtaW4ifQ.jIauvty8BNN_DAfuZDC6OQs2Aj1gM-hmQC1HPrICi1c', refresh_token: '' };
                
            }
            return [200, token, {}];
            
        }

        function getUsers(method, url, data, header) {
            if (!ValidateToken(header)) {
                return [401];
            }
            var userlist = usersJson;
            return [200, userlist, {}];
        }

        function ValidateToken(header) {
            if (header.Authorization == 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEyMzQ1Njc4OTAsIm5hbWUiOiJhZG1pbiIsInJvbGVzIjoiYWRtaW4ifQ.jIauvty8BNN_DAfuZDC6OQs2Aj1gM-hmQC1HPrICi1c') {
                return true;
            }
            else {
                return false;
            }
        }

        function getRoutes(method, url, data, header) {
            if (!ValidateToken(header)) {
                return [401];
            }
            var routeList = routes;
            return [200, routeList, {}];
        }


       var usergroups = [
           { id: '1', groupName: 'User', description: 'normal user', roles: 'user' },
           { id: '2', groupName: 'Admin', description: 'administrator', roles: 'admin' },
           { id: '3', groupName: 'Manager', description: 'Manager', roles: 'manager' }
       ];

       function getusergroup(method, url, data, header) {
           if (!ValidateToken(header)) {
               return [401];
           }
            var usergroup = { 'id': 0 };
            var params = url.split('/');
            var length = params.length;
            var id = params[length - 1];

          

            if (id > 0) {
                for (var i = 0; i < usergroups.length; i++) {
                    if (usergroups[i].id == id) {
                        usergroup = usergroups[i];
                        break;
                    }
                }
            }

            return [200, usergroup, {}];
        }

       function usergroupGetAll(method, url, data, header) {
            if (!ValidateToken(header)) {
                return [401];
            }
           return [200, usergroups, {}];
       }

       function userRegister(method, url, data, header) {
           return [405, {message:'error'}, {}];
       }

       var edittingRegex = new RegExp('/api/usergroup'+"/[0-9][0-9]*",'');
       
       $httpBackend.whenPOST('api/account/register').respond(userRegister);
       $httpBackend.whenGET('/api/userGroups/getAll').respond(usergroupGetAll);
       $httpBackend.whenGET('/api/getRoutes').respond(getRoutes);
       $httpBackend.whenGET(edittingRegex).respond(getusergroup);
        $httpBackend.whenGET('/api/getUsers').respond(getUsers);
        $httpBackend.whenPOST('/api/login').respond(loginMethod);
        $httpBackend.whenGET(userUrl).respond(usersJson);
        $httpBackend.whenGET(/\.html$/).passThrough();
        $httpBackend.whenGET(/\.json$/).passThrough();
       $httpBackend.whenPOST(/.*/).passThrough();
       $httpBackend.whenGET(/.*/).passThrough();


//$httpBackend.whenGET(/views\/.*/).passThrough();
       //$httpBackend.whenPOST(/.*/).passThrough()
       //$httpBackend.whenGET(/.*/).passThrough()
   });
})();