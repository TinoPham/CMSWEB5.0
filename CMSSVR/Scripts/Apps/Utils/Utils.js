define(function () {
	var utils = function () {
		if ( typeof String.prototype.startsWith != 'function' ) {
			
			// see below for better implementation!
			
			String.prototype.startsWith = function ( str ) {
				
				return this.indexOf( str ) == 0;
				
			};
		}
		if (typeof String.prototype.endsWith != 'function') {

		    // see below for better implementation!

		    String.prototype.endsWith = function (str) {

		        return this.indexOf(str) == 0;

		    };
		}
		if ( typeof String.prototype.Contains != 'function' ) {

			// see below for better implementation!

			String.prototype.Contains = function ( str ) {

				var n = this.match( new RegExp( str, "i" ) );
				if ( n == null )
					return false;
				return true;
			};
		}

		if ( typeof Date.prototype.toUTCDateParam != 'function' ) {

		    // see below for better implementation!

		    Date.prototype.toUTCDateParam = function () {
		        var date = this;
		        var year = date.getUTCFullYear();
		        var month = date.getUTCMonth() + 1;
		        var day = date.getUTCDate();
		        var ret = year + "";
		        if (month < 10)
		            month = "0" + month;
		        if (day < 10)
		            day = "0" + day;
		        return ret + month + day;

		    };

		}

		if ( typeof Date.prototype.toDateParam != 'function' ) {

			// see below for better implementation!

			Date.prototype.toDateParam = function () {
				var date = this;
				var year = date.getFullYear();
				var month = date.getMonth() + 1;
				var day = date.getDate();
				var ret = year + "";
				if ( month < 10 )
					month = "0" + month;
				if ( day < 10 )
					day = "0" + day;
				return ret + month + day;

			};
		}

        return {
            toUTCDate: function (local_date) {
                var _date = _ISODateString(local_date);
                var dd = new Date(_date);
                return dd;
            },
            toLocalDate: function (utcDate) {
                var tzo = ((new Date()).getTimezoneOffset() / 60) * (-1);
                var userTime = new Date(utcDate.setHours(utcDate.getHours() + tzo).toLocaleString());
                return userTime;
            },
            isString: function(val){
                return _isString( val );
            },
            isObject : function(val){
                return _isObject( val );
            },
            isArray: function(val){
                return _isArray( val );
            },
            Rand : function(min, max){
                return _Rand(min, max);
            },
            EncryptString: function ( data, token, b64encoder ) {
                return _EncryptString( data, token, b64encoder, 32 );
            },
            DecryptString: function ( data, token, b64encoder ) {
                return _DecryptString( data, token, b64encoder, 32 );
            },
            Object2QueryString: function ( obj, prefix ) {
                return _Object2QueryString( obj, prefix);
            },
            AddProperty: function ( obj, pname,value) {
                _AddProperty( obj, pname, value );
            },
            RemoveProperty: function ( obj, pname ) {
                _RemoveProperty( obj, pname);
            },
            Equal: function(a, b){
              return  _isEquivalent(a,b);
			},
			isNullOrEmpty: function (val) {
				return _isNullOrEmpty(val);
			},
			Root: function () { return _Root(); },
			CookiePath: function () { return _CookiePath(); },
			Resolve: function ( path ) { return _resolve( path ); },
			ValidIpaddress: function (param) { return _ValidateIpaddress(param); },
			compareDate: function (date1, date2) { return _compareDate(date1, date2); },
			compareDateWithoutTime: function (date1, date2) { return _compareDateWithoutTime(date1, date2); },
			beginOfDate: function(dtval) {
				return new Date(dtval.getFullYear(), dtval.getMonth(), dtval.getDate(), 0, 0, 0, 0);
			},
			ChannelImageURL: function (channel, apiURL) {
				if (channel) {
					return apiURL + "name=C_" + $scope.pad(channel.ChannelNo + 1) + ".jpg&kdvr=" + channel.KDVR.toString();
				}
				else {
					return null;
				}
			},
			newGUID: function() {
			    function s4() {
			        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
			    }

			    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
			}
        }
        var DateDefine = ["year", "y", "lastyear", "ly", "month", "m", "lastmonth", "lm", "week", "w", "lastweek", "lw", "day", "d", "yesterday", "today", "hour", "h", "minute", "min", "second", "s", "now", "n"]

        function _DecryptString( enctext, token, b64encoder, key_len ) {
            if ( !enctext )
                return enctext;
            var hindex = enctext.indexOf( ':' );
            if ( hindex <= 0 )
                return enctext;
            var header = enctext.substring( 0, hindex );
            var data = enctext.substring(hindex + 1 );
            header = b64encoder.decode( header );
            hindex = header.indexOf( '\n' );
            if ( hindex < 0 )
                return enctext;
            //remove date time
            header = header.substring( hindex + 1 );
            hindex = 0;
            hindex = header.indexOf( '\n', hindex + 1 );
            if ( hindex < 0 )
                return enctext;

            var ikey = header.substring( 0, hindex );
            //console.log("ikey: " +  ikey );
            var iiv = header.substring( hindex + 1 );
            //console.log( "iiv: " + iiv );
            var key = _GetString( token, parseInt( ikey ), key_len, true );
            //console.log( "key : " + key );
            var iv = _GetString( token, parseInt( iiv ), 16, true );
        	//console.log( "iv : " + iv );
            if ( key == null || iv == null )
            	return null;
            var ctext = CryptoJS.AES.decrypt( data, key, { iv: iv } );
            //console.log( "ctext : " + ctext );
            return ctext.toString( CryptoJS.enc.Utf8 );
        }

        function _EncryptString( data, token, b64encoder, key_len ) {
            //var klen = 32;
            var pidx = _Rand( 0, token.length - key_len );
            var ividx = _Rand( 0, token.length - key_len );
            var pkey = _GetString( token, pidx, key_len, true );
            var ivkey = _GetString( token, ividx, key_len, true );
            var ctext = CryptoJS.AES.encrypt( data, pkey, { iv: ivkey } );

            var header = _EcrpytHeader( pidx, ividx );
            return b64encoder.encode( header ) + ':' + ctext.toString();

        }

        function _EcrpytHeader( ikey, iiv ) {
            var date = new Date();
            var ret = date.getUTCFullYear().toString() + _pad( date.getUTCMonth() + 1 ) + _pad( date.getUTCDate() ) + _pad( date.getUTCHours()) + _pad( date.getUTCMinutes() ) + _pad( date.getUTCSeconds() );
            ret += '\n';
            ret += ikey.toString() + '\n' + iiv.toString() + '\n';
            return ret;
        }

        function _isString( val ) {
            return typeof val == 'string';
        }

        function _isObject( val ) {
            return Object.prototype.toString.call( val ) === '[object Object]';
        }

        function _isArray( myArray ) {
            if (!myArray || !myArray.constructor )
                return false;

            return myArray.constructor.toString().indexOf( "Array" ) > -1;
        }

        function _GetString( data, index, len, parser ) {

            if (len <= 0 || index < 0)
                return null;

			if (!data) { return null; }

			var dlen = data.length;
			if (index + len > dlen){
                return null;
			}

            var idex = data.substring( index, index + len );
            if ( parser )
                return CryptoJS.enc.Utf8.parse( idex );
            return idex;
        }

        function _Rand( min, max ) {

            var x = Math.floor(( Math.random() * max ) + min );
            return x;
        }

        function _pad(number) {
            if (number < 10) {
                return '0' + number;
            }
            return number;
        }

        function _ISODateString(date) {

            return date.getFullYear() +
                  '-' + _pad( date.getMonth() + 1 ) +
                  '-' + _pad( date.getDate() ) +
                  'T' + _pad( date.getHours() ) +
                  ':' + _pad( date.getMinutes() ) +
                  ':' + _pad( date.getSeconds() ) +
                  '.' + (date.getMilliseconds() / 1000).toFixed(3).slice(2, 5) +
                  'Z';

        }

        function _Object2QueryString( obj, prefix ) {
            if ( !obj )
                return "";

            var str = [];
            for ( var p in obj ) {
                var k = prefix ? prefix + "[" + p + "]" : p,
                    v = obj[k];
				if (_isArray(v)) {
                    str.push(( k ) + "=" + encodeURIComponent(v) )
                }
                else
                 str.push( angular.isObject( v ) ? _Object2QueryString( v, k ) : ( k ) + "=" + encodeURIComponent( v ) );
            }
            return str.join( "&" );
        }

        function _AddProperty( object, propertyname, value ) {
            if ( !object || !propertyname )
                return;

            object[propertyname] = value;
        }

        function _RemoveProperty( object, propertyname ) {
            if ( !object || !propertyname )
                return;
            if ( !object.hasOwnProperty( propertyname ) )
                return;
            delete object[propertyname];
        }

        function _isEquivalent( a, b ) {
            // Create arrays of property names
            var aProps = Object.getOwnPropertyNames( a );
            var bProps = Object.getOwnPropertyNames( b );

            // If number of properties is different,
            // objects are not equivalent
            if ( aProps.length != bProps.length ) {
                return false;
            }

            for ( var i = 0; i < aProps.length; i++ ) {
                var propName = aProps[i];

                // If values of same property are not equal,
                // objects are not equivalent
                if ( a[propName] !== b[propName] ) {
                    return false;
                }
            }

            // If we made it this far, objects
            // are considered equivalent
            return true;
        }

		function _DateDefine(key) {
			switch (key) {
                case "year":
                case "y":
                    break;
                case "lastyear":
                case "ly":
                    break;
                case "month":
                case "m":
                    break;
                case "lastmonth":
                case "lm":
                    break;
                case "week":
                case "w":
                case "lastweek":
                case  "lw":
                    break;
                case "day":
                case "d":
                    break;
                case "yesterday":
                    break;
                case "today":
                    break;
                case "hour":
                case "h":
                    break;
                case "minute":
                case "min":
                    break;
                case "second":
                case "s":
                    break;
                case "now", "n":
                    break;
            }

        }

		function _isNullOrEmpty(val) {
			if (val === "" || val === null || isNaN(val) || val === undefined)
				return true;

			return false;
		}

		function _Root() {
			var z = window.location.pathname.split( '/' );
			if ( z.length == 0 )
				return window.location;
			return window.location.origin + "/" + z[1] + "/";
		}

		function _CookiePath() {
			var cmsweb = "/cmsweb";
			var path = window.location.pathname.toLowerCase();
			var pos = path.lastIndexOf(cmsweb);
			var res = path.substring( 0, pos );
			if ( res.length == 0 )
				return "/";
			return res;

		}

		function _resolve( relative ) {
			var resolved = relative;
			if ( relative.charAt( 0 ) == '~' )
				resolved = this._relativeRoot() + relative.substring( 2 );
			return resolved;
		}

		function _relativeRoot() { return '<%= ResolveUrl("~/") %>'; }

		function _ValidateIpaddress(param) {
			var pattern = /^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$/g;
			var bValidIP = pattern.test( param );
			return bValidIP;
		}

		function _compareDateWithoutTime(date1, date2) {
			//We don't need compare time. Because when user chosen date, maybe 2 date different about hour, minute, second.
			/*
			 * Return 0, if date1 === date
			 * Return 1, if date1 > date2
			 * Return -1, if date1 < date2
			 */
			var d1 = new Date(date1).setHours(0, 0, 0, 0);
			var d2 = new Date(date2).setHours(0, 0, 0, 0);
			if (d1 === d2) {
				return 0;
			}
			else if (d1 > d2) {
				return 1;
			}
			else {
				return -1;
			}
		}

		function _compareDate(date1, date2) {
			var d1 = date1.getTime();
			var d2 = date2.getTime();
			if (d1 === d2) {
				return 0;
			}
			else if (d1 > d2) {
				return 1;
			}
			else {
				return -1;
			}
		}

    };
    return utils;
});

