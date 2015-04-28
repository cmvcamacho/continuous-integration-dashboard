(function () {
    'use strict';
    angular.module('services').factory('$signalrService', function ($) {
        var proxy;
        var connection;

        var initialize = function () {
            if (!connection) {
                //Getting the connection object
                connection = $.hubConnection();
                proxy = connection.createHubProxy('ciDashboardHub');

                //Starting connection
                startConnection();

                connection.disconnected(function () {
                    toastr.error("not able to connect to server...");
                    setTimeout(startConnection, 500); // Restart connection after half a second.
                });
            }
        };

        var startConnection = function () {
            connection
                .start()
                .done(function () {
                    console.log('Now connected, connection ID=' + connection.id);
                    toastr.success('Connection established');
                })
                .fail(function () {
                    console.log('Could not Connect!');
                    toastr.error('Connection failed');
                });
        }

        var getProxy = function () {
            return proxy;
        }

        return {
            initialize: initialize,
            getProxy: getProxy
        };
    });
}());