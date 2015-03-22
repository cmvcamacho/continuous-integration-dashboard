(function () {
    'use strict';
    angular.module('services').factory('$projectsService', function ($, $rootScope) {
        var proxy;
        var connection;

        var initialize = function () {
            //Getting the connection object
            connection = $.hubConnection();
            proxy = connection.createHubProxy('ciDashboardHub');

            proxy.on('sendMessage', function (message) {
                $rootScope.$emit('sendMessage', message);
            });

            proxy.on('sendProjects', function (projectList) {
                $rootScope.$emit('sendProjects', projectList);
            });

            //Starting connection
            connection
                .start()
                .done(function () {
                    console.log('Now connected, connection ID=' + $.connection.hub.id);
                    toastr.success('Connection established');
                })
                .fail(function() {
                     console.log('Could not Connect!');
                     toastr.error('Connection failed');
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
    });
}());