(function () {
    'use strict';
    angular.module('services').factory('$projectsService', ['$signalrService', '$rootScope', function (signalrService, $rootScope) {

        var initialize = function () {
            //Getting the connection object
            signalrService.initialize();

            signalrService.getProxy().on('sendMessage', function (message) {
                $rootScope.$emit('sendMessage', message);
            });

            signalrService.getProxy().on('sendProjects', function (projectList) {
                $rootScope.$emit('sendProjects', projectList);
            });

            signalrService.getProxy().on('sendBuildResult', function (buildResult) {
                $rootScope.$emit('sendBuildResult', buildResult);
            });

        };

        //var sendRequest = function () {
        //    //Invoking greetAll method defined in hub
        //    proxy.invoke('greetAll', 'ddd');
        //};


        return {
            initialize: initialize,
            //sendRequest: sendRequest,
        };
    }]);
}());