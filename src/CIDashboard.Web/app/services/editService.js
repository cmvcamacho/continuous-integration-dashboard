(function () {
    'use strict';
    angular.module('services').factory('editService',  ['$signalrService', '$rootScope', function (signalrService, $rootScope) {

        var initialize = function () {
            $rootScope.editMode = false;

            //Getting the connection object
            signalrService.initialize();

            signalrService.getProxy().on('startRefresh', function (status) {
                $rootScope.$emit('startRefresh', status);
            });

            signalrService.getProxy().on('stopRefresh', function (status) {
                $rootScope.$emit('stopRefresh', status);
            });
        };

        var requestRefresh = function () {
            signalrService.getProxy()
                .invoke('RequestRefresh')
                .fail(function (error) {
                    console.log('RequestRefresh error: ' + error)
                });
        };

        return {
            initialize: initialize,
            requestRefresh: requestRefresh
        };
    }]);
}());