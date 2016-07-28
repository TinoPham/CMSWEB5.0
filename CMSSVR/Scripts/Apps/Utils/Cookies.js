//$.cookie is a object supported by Jquery cookies
    
define(function () {
    var cookies = function () {
        return {
            get: function (key) {
                return $.cookie(key);
            },
            set: function (key, value, domain, path, expired) {
                expired = expired || 7;
                if (path)
                	$.cookie( key, value, { domain: domain , path: path, expires: expired } );
                else
                	$.cookie(key, value, {  expires: expired });
            },
            remove: function (key) {
                $.removeCookie(key);
            }

        };
    };
    return cookies;
}
    );
