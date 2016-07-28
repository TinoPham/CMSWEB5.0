(function () {
	define(['cms'], function (cms) {
		cms.service('colorSvc', colorSvc);
		colorSvc.$inject = [];

		function colorSvc() {

			var JOBTITLE_DEFAULT_COLOR = 16777215; //fff - white

			function getDefaultColor() {
				return JOBTITLE_DEFAULT_COLOR;
			}
			function rgbtoHex(rgb) {
				rgb = rgb.match(/^rgba?[\s+]?\([\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?,[\s+]?(\d+)[\s+]?/i);
				return (rgb && rgb.length === 4) ? "#" +
				 ("0" + parseInt(rgb[1], 10).toString(16)).slice(-2) +
				 ("0" + parseInt(rgb[2], 10).toString(16)).slice(-2) +
				 ("0" + parseInt(rgb[3], 10).toString(16)).slice(-2) : '';
			}
			function rgbtoNum(rgb) {
				var hex = rgbtoHex(rgb);
				var num = hextoNum(hex);
				return num;
			}
			function hextoNum(hex) {
				while (hex.charAt(0) == "#")
					hex = hex.substr(1);
				return parseInt(hex, 16);
			}
			function numtoRGB(num) {
				num >>>= 0;
				var b = num & 0xFF,
				 g = (num & 0xFF00) >>> 8,
				 r = (num & 0xFF0000) >>> 16;
				//a = ( (num & 0xFF000000) >>> 24 ) / 255 ;
				return "rgb(" + [r, g, b].join(",") + ")";
			}

			return {
				getdefaultColor: getDefaultColor,
				rgbtoHex: rgbtoHex,
				rgbtoNum: rgbtoNum,
				hextoNum: hextoNum,
				numtoRGB: numtoRGB
			}
		}
	});

})();
