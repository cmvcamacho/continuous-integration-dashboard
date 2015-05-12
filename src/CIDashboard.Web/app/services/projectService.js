(function () {
    'use strict';
    angular.module('services').factory('$projectsService', ['$signalrService', '$rootScope', function (signalrService, $rootScope) {

        var initialize = function () {
            //Getting the connection object
            signalrService.initialize();

            signalrService.getProxy().on('sendMessage', function (message) {
                $rootScope.$broadcast('sendMessage', message);
            });

            signalrService.getProxy().on('sendProjectsAndBuildConfigs', function (projectList) {
                $rootScope.$broadcast('sendProjectsAndBuildConfigs', projectList);
            });

            signalrService.getProxy().on('sendBuildResult', function (buildResult) {
                $rootScope.$broadcast('sendBuildResult', buildResult);
            });

            signalrService.getProxy().on('sendUpdatedProject', function (projectInfo) {
                $rootScope.$broadcast('sendUpdatedProject', projectInfo);
            });

            signalrService.getProxy().on('sendUpdatedBuild', function (buildInfo) {
                $rootScope.$broadcast('sendUpdatedBuild', buildInfo);
            }); 

        };

        return {
            initialize: initialize,
            //sendRequest: sendRequest,
        };
    }]);
}());