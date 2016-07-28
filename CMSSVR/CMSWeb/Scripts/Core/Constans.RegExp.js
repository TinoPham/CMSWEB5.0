(function () {
	'use strict';

	define(function () {
		var regExp = {
			InputRestriction: /^[A-Za-z0-9%@!()\-_$#&:+\s\/]*$/,
			LoginRestriction: /^[A-Za-z0-9@_\s]*$/,
			//NameRestriction: /^[^~`\^\*=|\\;\'\"\.<>,\?\{\}\[\]]*$/,
			EmailRestriction: /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})$/,
			NumberRestriction: /^[0-9]*$/,
			DecimalRestriction: /^[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*$/,
			PhoneRestriction: /^\(?(\d{3})\)?[ .-]?(\d{3})[ .-]?(\d{4})$/,
			GoalValueRestriction: /^\d+(\.\d{1,4})?$/,
			ZipCodeRestriction: /^[A-Za-z0-9\s]{0,10}$/,
			HaspLicenseRestriction: /^[0-9]{2}[a-zA-Z0-9]{4}$/,
			PasteExp: /^\d{1,10}/
		};
		return regExp;
	});
})();