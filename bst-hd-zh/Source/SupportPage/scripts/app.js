'use strict';

/**
 * @ngdoc overview
 * @name zendeskApp
 * @description
 * # zendeskApp
 *
 * Main module of the application.
 */
angular
  .module('zendeskApp', [
    'ngAnimate',
    'ngAria',
    'ngCookies',
    'ngMessages',
    'ngResource',
    'ngRoute',
    'ngSanitize',
    'ab-base64',
    'ngTouch'
  ])
  .config(['$routeProvider','$httpProvider','base64',function ($routeProvider,$httpProvider,base64) {
    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'MainCtrl',
        controllerAs: 'main'
      })
      .otherwise({
        redirectTo: '/'
      });
      

  }]);



