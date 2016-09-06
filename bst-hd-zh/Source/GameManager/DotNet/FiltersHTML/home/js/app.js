var app = angular.module('BluestacksTvFilters', ['ngAnimate', 'ngRoute', '720kb.tooltips', 'ngResource', 'ngCookies', 'LocalStorageModule']);

app.config(function ($routeProvider, $locationProvider, localStorageServiceProvider) {
    localStorageServiceProvider
        .setPrefix('');
    $routeProvider
        .when('/filters', {
            controller: 'FilterController',
            templateUrl: 'static/filters/home/partials/filters.html'
        })
        .otherwise({
            controller: 'FilterController',
            templateUrl: '../../filters/home/partials/filters.html'
        });
    $locationProvider.html5Mode(true).hashPrefix('!');
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });
});
