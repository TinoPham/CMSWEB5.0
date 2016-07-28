(function() {
    'use strict';

    define(['cms'], function(cms) {
        cms.factory('base.service', baseSvc);
        baseSvc.$inject = ['cmsBase'];
        function baseSvc(cmsBase) {

            var $q = cmsBase.$q;

            function ConstructorBase(entityName) {
                this.entityName = entityName;
                this.log = cmsBase.cmsLog;
                this.callFailed = callFailed.bind(this);
                this.select = select;
                this.queryData = queryData;
            }

            var odata = {
                
            }

            odata.where().top.skip();

            var queryData = {
                select : select,
                where: where,
                and: and,
                or: or,
                equal: equal,
                notEqual: notEqual,
                differentFrom: differentFrom,
                greaterThan: greaterThan,
                greaterThanOrEqual: greaterThanOrEqual,
                lessThanOrEqual: lessThanOrEqual,
                lessThan: lessThan,
                startWith: startWith,
                contain: contain,
                endswith: endswith,
                orderByascending: orderByascending,
                orderdescending: orderdescending,
                skip: skip,
                take: take
            }

            function select() {
                var result = Array.prototype.slice.call(arguments, 0);
                return "&$select=" + result.join(", ");
            }

            function and(aSelector, bSelector) {
                return "(" + aSelector + " and " + bSelector + ")";
            }

            function or(fieldName, value) {
                return "(" + fieldName + " or " + value + ")";
            }

            function equal(fieldName, value) {
                return "(" + fieldName + " eq " + value + ")";
            }

            function notEqual(fieldName, value) {
                return "(" + fieldName + " ne " + value + ")";
            }

            function differentFrom(fieldName, value) {
                return "(" + fieldName + " not " + value + ")";
            }

            function greaterThan(fieldName, value) {
                return "(" + fieldName + " gt " + value + ")";
            }

            function greaterThanOrEqual(fieldName, value) {
                return "(" + fieldName + " ge " + value + ")";
            }

            function lessThanOrEqual(fieldName, value) {
                return "(" + fieldName + " le " + value + ")";
            }

            function lessThan(fieldName, value) {
                return "(" + fieldName + " lt " + value + ")";
            }

            function where(selector) {
                return '&$filter=' + selector;
            }

            function orderByascending() {
                var result = Array.prototype.slice.call(arguments, 0);
                return "&$orderby=" + result.join(", ") + " asc";
            }

            function orderdescending() {
                var result = Array.prototype.slice.call(arguments, 0);
                return "&$orderby=" + result.join(", ") + " desc";
            }

            function skip(value) {
                return "&$skip=" + value;
            }

            function startWith(fieldName, value) {
                return "startswith(" + fieldName + "," + value + ")";
            }

            function contain(fieldName, value) {
                return "substringof(" + value + "," + fieldName + ")";
            }

            function endswith(fieldName, value) {
                return "endswith(" + fieldName + "," + value + ")";
            }

            function take(value) {
                return "&$top=" + value;
            }

            ConstructorBase.prototype = {
                Constructor: ConstructorBase
            };

            return ConstructorBase;

            function callFailed(error) {
                var msg = (error.message || '');
                this.log.error(msg, error);
                return $q.reject(new Error(msg));
            }
        }
    });

})();