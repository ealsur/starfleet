'use strict';

(function () {
    angular.module('starfleet', ['uiGmapgoogle-maps','ui.bootstrap'])
       .config(function(uiGmapGoogleMapApiProvider) {
            uiGmapGoogleMapApiProvider.configure({
                key: 'AIzaSyAKqbTtW-dJqHYd-beYltCUAI6lZmIZQHU',
                v: '3.25', 
                libraries: 'weather,geometry,visualization'
            });
        })
        .config(['$httpProvider', function ($httpProvider) {
		    /*CONFIGURO EL PROVIDER PARA NUNCA CACHEAR RESPUESTAS DE GET*/
		    if (!$httpProvider.defaults.headers.get) {
		        $httpProvider.defaults.headers.get = {};
		    }
		    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
		    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
		    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
		    $httpProvider.defaults.headers.get['X-Requested-With'] = 'XMLHttpRequest';
        }])
        .factory('myCoordinates', ['$q', function myCoordinates($q) {

            var deferred = $q.defer();

            // Check your browser support HTML5 Geolocation API
            if (window.navigator && window.navigator.geolocation) {
                window.navigator.geolocation.getCurrentPosition(getCoordinates);
            } else {
                deferred.reject({msg: "Browser does not supports HTML5 geolocation"});
            }

            function getCoordinates(coordinates){
                var myCoordinates = {};
                myCoordinates.lat = coordinates.coords.latitude;
                myCoordinates.lng = coordinates.coords.longitude;
                deferred.resolve(myCoordinates);
            }

            return deferred.promise;

        }])
        .controller('MapController', ['$http','uiGmapGoogleMapApi','myCoordinates','$rootScope','$uibModal',
            function ($http,uiGmapGoogleMapApi,myCoordinates,$rootScope,$uibModal) {
                var ctrl = this;
                ctrl.loading = true;
                ctrl.markers=[];
                myCoordinates.then(function(myLocation){
                    ctrl.loading=false;
                    ctrl.map = { center: { latitude: myLocation.lat, longitude: myLocation.lng }, zoom: 2 };
                });
                ctrl.onClick= function(marker, eventName, model) {
                        $http({
                            method: 'POST',
                            url: '/airlines/bycountry',
                            data: '"'+model.country+'"'
                        }).then(function (response) {
                            var modalInstance = $uibModal.open({
                                templateUrl: 'airlines.html',
                                controller: function(airlines,airport){
                                    this.airlines = airlines;
                                    this.airport = airport;
                                },
                                controllerAs: '$ctrl',
                                size: 'lg',
                                resolve: {
                                    airlines: function () {
                                    return response.data.results;
                                    },
                                    airport: function () {
                                    return model;
                                    }
                                }
                                });

                        });

                    };
                uiGmapGoogleMapApi.then(function(maps) {
                    $rootScope.$on('newResults', function(event,data){
                        ctrl.markers=data;
                    });
                });

                
                
            }
        ])
		.controller('SearchController', ['$http','uiGmapGoogleMapApi','myCoordinates','$rootScope',
            function ($http,uiGmapGoogleMapApi,myCoordinates,$rootScope) {
                var ctrl = this;
                /******** MODEL ************/
                ctrl.searching = false;
                ctrl.loadingSuggestions = false;
                ctrl.results = [];
                var lastSearch='';
                ctrl.filters=null;
                ctrl.searchText = '';
                ctrl.page = 1;
                
                var myLocation = null;
                myCoordinates.then(function(_myLocation){
                    myLocation = _myLocation;
                });
                /******** /MODEL ***********/
                ctrl.filter = function($event, filter, value){
                    $event.preventDefault();
                    if(ctrl.filters !== null && ctrl.filters.hasOwnProperty(filter)){
                        delete ctrl.filters[filter];
                    }
                    else{
                        if(ctrl.filters===null){
                            ctrl.filters={};
                        }
                        ctrl.filters[filter] = value;
                    }
                    ctrl.search($event);
                };
                ctrl.getSuggestions = function(text){
                    return $http({
                        method: 'GET',
                        url: '/search/suggest?term='+text
                    }).then(function (response) {
                        var map= response.data.results.map(function(item){
                            return item;
                        });
                        return map;
                    });
                };
                
                ctrl.search = function($event){
                    $event.preventDefault();  
                    ctrl.searching=true;
                    ctrl.results=null;
                    if(lastSearch!==ctrl.searchText){
                        ctrl.page = 1;
                    }
                    var facets = ["country","city"];
                    if(!ctrl.filters || !ctrl.filters.hasOwnProperty('country')){
                        facets = ["country"];
                    }
                    $http({
                        method: 'POST',
                        url: '/search/search',
                        data: {
                            Text: ctrl.searchText,
                            Filters:ctrl.filters,
                            IncludeFacets:true,
                            Page:ctrl.page,
                            Facets:facets,
                            PageSize:50
                        }
                    }).then(function(response) {
                        ctrl.results= response.data;
                        ctrl.searching=false;
                        $rootScope.$broadcast('newResults',response.data.results.map(function(item){
                            return item.document;
                        }));
                    });
                };
                 ctrl.next = function($event){

                    ctrl.page++;

                    ctrl.search($event);

                };

                ctrl.prev = function($event){

                    ctrl.page--;

                    ctrl.search($event);

                };
            }]);
})();