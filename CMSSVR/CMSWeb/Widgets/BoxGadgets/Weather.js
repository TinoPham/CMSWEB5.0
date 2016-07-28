(function() {
    'use strict';

    define(['cms', 'Services/dialogService'], function (cms) {
        cms.register.controller('weatherCtrl', weatherCtrl);

        weatherCtrl.$inject = ['$scope', 'dataContext', 'cmsBase', 'dialogSvc', 'AppDefine', '$timeout', '$window', '$filter'];

        function weatherCtrl($scope, dataContext, cmsBase, dialogSvc, AppDefine, $timeout, $window, $filter) {
            var vm = this;
            //vm.url = "http://www.accuweather.com/en/vn/hanoi/353412/current-weather/353412";
            //var location = '10.8071409,106.67040779999999';

            

            var latitude = '';
            var longitude = '';
            var now = new Date();
            vm.isErr = true;
            vm.msgError = 'WT_MSGERR_CF';
            $scope.units = undefined;

            function compareCode(value) {
                var result = 0;
                var number = parseInt(value);

                if (number === 800) {    //Sunny
                    result = 34;
                } else if (number === 801 || number === 802) {
                    result = 28;
                } else if (number === 803 || number === 804) {//cloudy
                    result = 26;
                } else if (number === 900 || number === 781 || number === 901 || number === 902) {//tonado
                    result = 0;
                } else if (number === 500 || number === 501 || number === 502 || number === 503 || number === 504 || number === 511 || number === 520 || number === 521 || number === 522 || number === 531) {//rain
                    result = 40;
                } else if (number === 600 || number === 601 || number === 602 || number === 611 || number === 612 || number === 615 || number === 616 || number === 620 || number === 621 || number === 622) {//snow
                    result = 42;
                } else if (number === 300 || number === 301 || number === 302 || number === 310 || number === 311 || number === 312 || number === 313 || number === 314 || number === 321) {//Drizzle
                    result = 40;
                } else if (number === 200 || number === 201 || number === 202 || number === 210 || number === 211 || number === 212 || number === 221 || number === 230 || number === 231 || number === 232) {//Thunderstorm
                    result = 2;
                } else if (number === 905) {//windy
                    result = 24;
                } else {
                    result = 54;
                }
                return result;
            }

            function errorHandler(err) {
                //if (err.code == 1) {
                //    console.log("Error: Access is denied!");
                //}

                //else if (err.code == 2) {
                //    console.log("Error: Position is unavailable!");
                //}
                //else {
                //    console.log("Error: Access is denied!");
                //}
                vm.isErr = true;
                vm.msgError = "WT_MSGERR_CF";
            }

            function getAltTemp(unit, temp) {
                if (unit === 'f') {
                    return Math.round((5.0 / 9.0) * (temp - 32.0));
                } else {
                    return Math.round((9.0 / 5.0) * temp + 32.0);
                }
            }

            function convertDate(value) {
                var str = value.split(" ");
                //return str[0] + 'T' + str[1];
                return str[0];
            }

            function customgetHour(value) {
                var str = value.split(" ");
                return str[1].substring(0, 2);
            }
            
            function loadWeatherOpenWeather(location, units)
            {
                var options = {
                    location: location,
                    unit: units,
                };

                if (units == 'c') {
                    options.unit = 'metric';
                } else {
                    options.unit = 'imperial';
                }

                var urlOpenWeather = 'http://api.openweathermap.org/data/2.5/forecast?lat=' + latitude + '&lon=' + longitude + '&units=' + options.unit + '&appid=c64fd270e81c6ecb0cafc9870abefdbf';

                jQuery.getJSON(
                    encodeURI(urlOpenWeather),
                    function (data) {
                        if (data !== null && data != undefined) {
                            var weather = {};
                            var days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
                            var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'June', 'July', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec'];

                            weather.city = data.city.name;
                            weather.date = new Date(convertDate(data.list[0].dt_txt));

                            weather.day = 'WT_' + days[weather.date.getDay()];

                            //weather.temp = Math.round(data.list[0].main.temp);

                            weather.temp_imperial = Math.round(data.list[0].main.temp);
                            weather.temp_metric = getAltTemp($scope.units, Math.round(data.list[0].main.temp));

                            //weather.high = Math.round(data.list[0].main.temp_max);
                            //weather.low = Math.round(data.list[0].main.temp_min);

                            weather.high_imperial = Math.round(data.list[0].main.temp_max);
                            weather.high_metric = getAltTemp($scope.units, Math.round(data.list[0].main.temp_max));

                            weather.low_imperial = Math.round(data.list[0].main.temp_min);
                            weather.low_metric = getAltTemp($scope.units, Math.round(data.list[0].main.temp_min));
                        
                            weather.code = compareCode(data.list[0].weather[0].id);
                            weather.currently = 'WT_CURRENTLY' + weather.code;
                            //weather.currently = data.list[0].weather[0].description;
                            
                            
                            
                            var day = days[now.getDay()];
                            weather.forecast = [];
                            weather.forecast.push(weather);
                            for (var i = 1; i < data.list.length; i++) {
                                var forecasttemp = {};

                                forecasttemp.date = new Date(convertDate(data.list[i].dt_txt));
                                var currentid = weather.forecast.length - 1;
                                if (forecasttemp.date.getDate() != weather.forecast[currentid].date.getDate()) {
                                    forecasttemp = data.list[i];

                                    forecasttemp.code = compareCode(data.list[i].weather[0].id);

                                    //forecasttemp.high = Math.round(data.list[i].main.temp_max);
                                    //forecasttemp.low = Math.round(data.list[i].main.temp_min);

                                    forecasttemp.high_imperial = Math.round(data.list[i].main.temp_max);
                                    forecasttemp.high_metric = getAltTemp($scope.units, Math.round(data.list[i].main.temp_max));

                                    forecasttemp.low_imperial = Math.round(data.list[i].main.temp_min);
                                    forecasttemp.low_metric = getAltTemp($scope.units, Math.round(data.list[i].main.temp_min));
                                    
                                    

                                    forecasttemp.date = new Date(convertDate(data.list[i].dt_txt));

                                    //Set translate 
                                    forecasttemp.day = 'WT_' + days[forecasttemp.date.getDay()];


                                    weather.forecast.push(forecasttemp);
                                } else if (forecasttemp.date.getDate() == weather.forecast[currentid].date.getDate()) {
                                    if (weather.forecast[currentid].high_imperial < Math.round(data.list[i].main.temp_max)) {
                                        weather.forecast[currentid].high_imperial = Math.round(data.list[i].main.temp_max);
                                        weather.forecast[currentid].high_metric = getAltTemp($scope.units, Math.round(data.list[i].main.temp_max));
                                    }

                                    if (weather.forecast[currentid].low_imperial > Math.round(data.list[i].main.temp_min)) {
                                        weather.forecast[currentid].low_imperial = Math.round(data.list[i].main.temp_min);
                                        weather.forecast[currentid].low_metric = getAltTemp($scope.units, Math.round(data.list[i].main.temp_min));
                                    }

                                    if (now.getHours() >= customgetHour(data.list[i].dt_txt) && weather.forecast[0].date.getDate() == forecasttemp.date.getDate()) {
                                        weather.temp_imperial = Math.round(data.list[i].main.temp);
                                        weather.temp_metric = getAltTemp($scope.units, Math.round(data.list[i].main.temp));
                                    }
                                        
                                }
                            }

                            weather.high_imperial = Math.round(weather.forecast[0].high_imperial);
                            weather.high_metric = getAltTemp($scope.units, Math.round(weather.forecast[0].high_imperial));

                            weather.low_imperial = Math.round(weather.forecast[0].low_imperial);
                            weather.low_metric = getAltTemp($scope.units, Math.round(weather.forecast[0].low_imperial));

                            //weather.temp_imperial = Math.round((weather.low_imperial + weather.high_imperial) / 2);

                            vm.weather = weather;
                            vm.datenow = now;

                            vm.isErr = false;
                        }
                    }
                ).error(function (msger) {
                    vm.isErr = true;
                    vm.msgError = "WT_MSGERR_INIT";
                });
            }

            function loadWeather(location, units) {
                
                var options = {
                    location: location,
                    unit: units,
                };

                
                //var weatherUrl = 'https://query.yahooapis.com/v1/public/yql?format=json&rnd=' + now.getFullYear() + now.getMonth() + now.getDay() + now.getHours() + '&diagnostics=true&callback=?&q=';
                //if (options.location !== '') {
                //    weatherUrl += 'select * from weather.forecast where woeid in (select woeid from geo.placefinder where text="' + options.location + '" and gflags="R" limit 1) and u="' + options.unit + '"';
                //} else {
                //    options.error({ message: "Could not retrieve weather due to an invalid location." });
                //    vm.msgError = "WT_MSGERR_INIT";
                //    vm.isErr = true;
                //    return false;
                //}

                var weatherUrl = 'https://query.yahooapis.com/v1/public/yql?format=json&rnd=' + now.getFullYear() + now.getMonth() + now.getDay() + now.getHours() + '&diagnostics=true&callback=?&q=';
                if (options.location !== '') {
                    weatherUrl += 'select * from weather.forecast where woeid in (select woeid from geo.placefinder where text="' + options.location + '" and gflags="R" limit 1) and u="' + options.unit + '"';
                } else {
                    options.error({ message: "Could not retrieve weather due to an invalid location." });
                    vm.msgError = "WT_MSGERR_INIT";
                    vm.isErr = true;
                    return false;
                }

                jQuery.getJSON(
                  encodeURI(weatherUrl),
                  function (data) {
                      //if (data !== null && data.query !== null && data.query.results !== null && data.query.results.channel.description !== 'Yahoo! Weather Error') {
                      if (data !== null && data != undefined && data.query !== null && data.query !== undefined) {
                          now = new Date();
                          var result = data.query.results.channel,
                              weather = {},
                              forecast,
                              compass = ['N', 'NNE', 'NE', 'ENE', 'E', 'ESE', 'SE', 'SSE', 'S', 'SSW', 'SW', 'WSW', 'W', 'WNW', 'NW', 'NNW', 'N'],
                              image404 = "https://s.yimg.com/os/mit/media/m/weather/images/icons/l/44d-100567.png";

                          weather.title = result.item.title;

                          weather.temp_imperial = result.item.condition.temp;
                          weather.temp_metric = getAltTemp($scope.units, result.item.condition.temp);

                          weather.code = result.item.condition.code;

                          weather.currently = 'WT_CURRENTLY' + weather.code;
                          weather.high_imperial = result.item.forecast[0].high;
                          weather.high_metric = getAltTemp($scope.units,result.item.forecast[0].high);

                          weather.low_imperial = result.item.forecast[0].low;
                          weather.low_metric = getAltTemp($scope.units, result.item.forecast[0].low);
                        
                       
                          weather.description = result.item.description;
                          weather.city = result.location.city;
                          
                          weather.updated = result.item.pubDate;
                          weather.link = result.item.link;
                          
                     
                          weather.forecast = [];
                          for (var i = 0; i < result.item.forecast.length; i++) {
                              forecast = result.item.forecast[i];
                              forecast.alt = { high: getAltTemp(options.unit, result.item.forecast[i].high), low: getAltTemp(options.unit, result.item.forecast[i].low) };

                              //Set translate 
                              forecast.day = 'WT_' + forecast.day;
                              forecast.date = new Date(result.item.forecast[i].date);

                              forecast.high_imperial = forecast.high;
                              forecast.high_metric = getAltTemp($scope.units, forecast.high);
                              forecast.low_imperial = forecast.low;
                              forecast.low_metric = getAltTemp($scope.units, forecast.low);

                              weather.forecast.push(forecast);
                          }

                          vm.weather = weather;
                          vm.datenow = new Date(result.item.forecast[0].date);
                          vm.datenow.setHours(now.getHours(), now.getMinutes(), now.getSeconds());
                         
                          vm.isErr = false;

                      } else {
                          //options.error({ message: "There was an error retrieving the latest weather information. Please try again.", error: data.query.results.channel.item.title });

                          //Limited get API deferen.
                          loadWeatherOpenWeather(location, units);
                      }
                  }
                )
                .error(function (msger) {
                    //vm.isErr = true;
                    //vm.msgError = "WT_MSGERR_NOSP";
                    loadWeatherOpenWeather(location, units);
                   
                });

            }

            function showLocation(position) {
                vm.isErr = true;
                vm.msgError = "LOADING";
                latitude = position.coords.latitude;
                longitude = position.coords.longitude;

                loadWeather(latitude + ',' + longitude, $scope.units);
                //loadWeatherOpenWeather(latitude + ',' + longitude, $scope.units);
                
            }

            //init form
            //navigator.geolocation.getCurrentPosition(showLocation, errorHandler);

            // event click reload
            vm.reload = function () {
                if (!navigator.geolocation) {
                    vm.msgError = "WT_MSGERR_CF";
                    return;
                }
                navigator.geolocation.getCurrentPosition(showLocation, errorHandler);

            }

            // F or C
            $scope.$watch('units', function (newValue, oldValue) {
                if (oldValue === undefined && $scope.units === undefined) {
                    $scope.units = 'f';
                    if (!navigator.geolocation){
                        vm.msgError = "WT_MSGERR_CF";
                        return;
                    }
                    navigator.geolocation.getCurrentPosition(showLocation, errorHandler);
                
                }
            }, true);

        }
    });
})();