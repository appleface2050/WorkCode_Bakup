	'use strict';

	/**
	 * @ngdoc function
	 * @name zendeskApp.controller:MainCtrl
	 * @description
	 * # MainCtrl
	 * Controller of the zendeskApp
	 */
	 angular.module('zendeskApp')
	 .controller('MainCtrl',['$scope', 'base64', '$http', '$interval','$timeout','$window',
	 	function ($scope,base64,$http,$interval ,$timeout, $window) {

	 		$scope.authSet         		= false;
	 		$scope.language 			= {"locale":"","name":""};
	 		$scope.popularArticles 		= [];
	 		$scope.gettingStarted  		= [];
	 		$scope.isDataLoading   		= true;
	 		$scope.auth;                 
	 		$scope.categoryData			= "";
	 		$scope.searchQuery          = "";
	 		$scope.searchResults        = [];
	 		$scope.searchTimer;
	 		$scope.ticketRequestReady 	= false;
	 		$scope.locValues  = {
									"report_problem": "",
									"ask_question": "",
									"search_bluestacks_help": "",
									"please_wait":"",
									"no_results_found": ""    
								};;
	 		
	 		
	 		$scope.langOptions = loc;

	 		//Zendesk Game Manager Category ID
	 		var ZENDESK_GAME_MANAGER_CATEGORY       = 200931716;

	 		//Zendesk Section IDs	
	 		var ZENDESK_SECTION_POPULAR_ARTICLES_ID = 201495786;
	 		var ZENDESK_SECTION_GETTING_STARTED_ID  = 201488586;

	 		//Zendesk Stats
	 		$scope.user_agent     = navigator.userAgent.split('/');
	 		$scope.prod_ver_index = $scope.user_agent.length-1;
	 		$scope.prod_ver       = $scope.user_agent[$scope.prod_ver_index];
	 		

	 		//Get auth token	
	 		$http.get("http://bluestacks-cloud.appspot.com/api/zenid").
	 		then(function(response) {
	 			$http.defaults.headers.common.Authorization = "Basic " + response.data;
	 			$scope.auth = response.data;
	 			$scope.authSet = true;
	 		});

	 		//Get all the data from Zendesk REST API
	 		$scope.getData = function() {
	 			if(!$scope.authSet) {
	 				$http.defaults.headers.common.Authorization = "Basic " + $scope.auth;
	 				$scope.authSet = true;
	 			}

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/categories/"+ ZENDESK_GAME_MANAGER_CATEGORY + ".json")
	 			.then(function(response) {
	 				if(response.status == 401) {
	 					console.log("Authentication failed, using token instead for subsequent Requests Authentication");
	 					$http.defaults.headers.common.Authorization = "Basic " + "emVuYWRtaW5AYmx1ZXN0YWNrcy5jb20vdG9rZW46a0x4ZUFhZmhjMjRVdm5UYjlhQk56UUtyUm93UXZ3cURoR1c5REFiQQ==";	
	 				}
	 			});

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/categories/"+ ZENDESK_GAME_MANAGER_CATEGORY + ".json")
	 			.then(function(response) {
	 				$scope.categoryData = response.data.category;
	 			});

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/sections/"+ ZENDESK_SECTION_POPULAR_ARTICLES_ID +"/articles.json")
	 			.then(function(response) {
	 				$scope.popularArticles = response.data.articles;
	 				$scope.isDataLoading = false;
	 			});

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/sections/"+ ZENDESK_SECTION_GETTING_STARTED_ID +"/articles.json")
	 			.then(function(response) {
	 				$scope.gettingStarted = response.data.articles;
	 				$scope.isDataLoading = false;
	 			});

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/sections/" + ZENDESK_SECTION_POPULAR_ARTICLES_ID + ".json").
	 			then(function(response) {
	 				$scope.popularArticlesSection = response.data.section;
	 			});

	 			$http.get("https://bluestacks.zendesk.com/api/v2/help_center/"+ $scope.language.locale +"/sections/" + ZENDESK_SECTION_GETTING_STARTED_ID + ".json").
	 			then(function(response) {
	 				$scope.gettingStartedSection = response.data.section;	
	 			});
	 		}

	 		//Zendesk REST API search
	 		$scope.searchArticles = function(query) {
	 			$timeout.cancel($scope.searchTimer);
	 			console.log("Delaying")
	 			$scope.searchTimer = $timeout(function () {
	 				console.log("Send stats");
	 				if(query.length > 1) {
	 					$scope.keyword_searched(query); 
	 					$http.get("https://bluestacks.zendesk.com/api/v2/help_center/articles/search.json?query=" + query + "&category=" + ZENDESK_GAME_MANAGER_CATEGORY)
	 					.then(function(response) {
	 						$scope.searc
	 						$scope.isSearching = false;
	 						$scope.searchResults = response.data.results;
	 						if($scope.searchResults.length == 0) {
	 							$scope.searchResultsEmpty = true;
	 						}else {
	 							$scope.searchResultsEmpty = false;
	 						}
	 					});
	 				}	
	 			}, 500);

	 			$scope.searchResultsEmpty = false;
	 			$scope.isSearching = true;
	 			if(query == "") {
	 				return;
	 			}
	 		}

	 		//Call back for GmApi.gmGetGuid()
	 		$window.getGuid = function(guid) {
	 			$scope.guid = guid;
	 		};

	 		//Set current locale and initialise all params
	 		$scope.setLocale = function (locale,name,client_locale){
	 			$scope.isDataLoading = true;
	 			if($scope.language.locale == angular.lowercase(locale)) {
	 				return;
	 			}
	 			$scope.language.locale = angular.lowercase(locale);
	 			$scope.language.name = name;
	 			$scope.locValues = client_locale;
	 			console.log(client_locale);
	 			GmApi.gmGetGuid("getGuid");
	 			$scope.getData();
	 		}	

	 		//Initialise locale config
	 		var checkAuth = $interval(function () {
	 			if($scope.authSet == true) {
	 				$interval.cancel(checkAuth);
	 				if (document.location.hostname == "localhost") {
    						$scope.userLang = "en-US";
						}
					else {
    						$scope.userLang = navigator.language || navigator.userLanguage;
    						if ($scope.userLang === "fil-PH") {
        						$scope.userLang = "tl-PH";
    						}
    						else if (!($scope.userLang in $scope.langOptions)) {
        					$scope.userLang = "en-US";
    					}

				}
	 			$scope.setLocale($scope.langOptions[$scope.userLang].locale,$scope.langOptions[$scope.userLang].name ,$scope.langOptions[$scope.userLang].client_locale );
	 			}
	 		} , 1000);


	 		

	 		//GmApi call for report problem workflow	
	 		$scope.reportProblem = function() {
	 			GmApi.gmReportProblem();
	 			$scope.buttonClicked("report_problem");
	 		}

	 		$scope.askQuestion = function() {
	 			$scope.buttonClicked("ask_question");
	 		}

	 		/*---------------------- Stats Helpers ----------------------*/

	 		$scope.linkClicked = function(value,keyword) {
	 			$scope.sendZendeskFunnelStats("link_clicked",value,keyword);	
	 		}

	 		$scope.buttonClicked = function(value) {
	 			$scope.sendZendeskFunnelStats("button_clicked",value,"blank");
	 		}

	 	
	 		$scope.keyword_searched = function(value) {
	 			$scope.sendZendeskFunnelStats("keyword_searched",value,"blank");
	 		}

	   		//Zendesk FAQ stats object
	   		$scope.sendZendeskFunnelStats = function(event_type,data,keyword_searched) {
	   			var obj_to_send =   {
	   				"event_type"	:  event_type,
	   				"oem"			: "gamemanager",
	   				"guid"      	:  $scope.guid,
	   				"prod_ver"  	:  $scope.prod_ver,
	   				"id" 			:  data,
	   				"locale"		:  $scope.userLang,
	   				"keyword_searched": keyword_searched
	   			};
	   			$http.defaults.headers.post	= {};
	   			$http.defaults.headers.common = {};
	   			$scope.authSet = false;

	   			$http({
	   				method : "POST",
	   				url    : "https://bluestacks-cloud.appspot.com/stats/support_faq_stats",
	   				params : obj_to_send
	   			});
	   			console.log(obj_to_send);
	   		}

	   	}]);
	var request_index = 0;
	var moveNext = function(index) {
				if(request_index<3) {
					request_index++;
				}
	   			$('#myCarousel').carousel(request_index);	
	   		}

	   
