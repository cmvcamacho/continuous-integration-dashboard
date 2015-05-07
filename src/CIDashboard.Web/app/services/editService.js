(function () {
    'use strict';
    angular.module('services').factory('editService',  ['$signalrService', '$rootScope', function (signalrService, $rootScope) {

        var initialize = function () {
            $rootScope.editMode = false;

            //Getting the connection object
            signalrService.initialize();

            setTimeout(requesProjectBuilds, 200);

            signalrService.getProxy().on('startRefresh', function (status) {
                $rootScope.$broadcast('startRefresh', status);
            });

            signalrService.getProxy().on('stopRefresh', function (status) {
                $rootScope.$broadcast('stopRefresh', status);
            });

            signalrService.getProxy().on('sendProjectBuilds', function (projectBuilds) {
                $rootScope.$broadcast('sendProjectBuilds', projectBuilds);
            }); 
        };

        var requestRefresh = function () {
            signalrService.getProxy()
                .invoke('RequestRefresh')
                .fail(function (error) {
                console.log('RequestRefresh error: ' + error);
            });
        };

        var requesProjectBuilds = function () {
            if (!signalrService.isConnected())
                setTimeout(requesProjectBuilds, 200);

            signalrService.getProxy()
                .invoke('RequestAllProjectBuilds')
                .fail(function (error) {
                console.log('RequestAllProjectBuilds error: ' + error);
            });
        };

        return {
            initialize: initialize,
            requestRefresh: requestRefresh,
            requesProjectBuilds: requesProjectBuilds
        };
    }]);
}());