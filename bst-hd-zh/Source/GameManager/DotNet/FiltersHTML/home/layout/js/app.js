var app = angular.module('BluestacksTvLayouts', ['ngAnimate', 'ngRoute', '720kb.tooltips', 'ngResource', 'ngCookies', 'LocalStorageModule']);

app.config(function ($routeProvider, $locationProvider, localStorageServiceProvider) {
    localStorageServiceProvider
        .setPrefix('');
    $routeProvider
        .when('/layout', {
            controller: 'LayoutController',
            templateUrl: './partials/layout.html'
        })
        .otherwise({
            controller: 'LayoutController',
            templateUrl: './partials/layout.html'
        });
    $locationProvider.html5Mode(true).hashPrefix('!');
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false
    });
});
