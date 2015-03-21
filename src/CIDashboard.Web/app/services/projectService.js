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

            proxy.on('sendProjects', function (projects) {
                //$rootScope.$emit('sendProjects', message);
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


        var projects =[
            {
                Name: 'Tru.Common',
                NoBuild: 12,
                Builds: [
                    {
                        Name: "Tru.Common.Messaging",
                        BuildNumber: '1.24.0.231',
                        Tests: {
                            Passed: 1231,
                            Failed: 0
                        },
                        Status: 'success'
                    },
                    {
                        Name: "Tru.Common.System",
                        BuildNumber: '',
                        Tests: {
                            Passed: '',
                            Failed: ''
                        }
                    }
                ]
            },
            {
                Name: 'MNP Framework',
                NoBuild: 12,
                Builds: [
                    {
                        Name: "MNP Web",
                        BuildNumber: '1.24.0.231',
                        Tests: {
                            Passed: 1231,
                            Failed: 430
                        },
                        Status: 'failed'
                    },
                    {
                        Name: "MNP Services",
                        BuildNumber: '2.0.0.54',
                        Tests: {
                            Passed: '453',
                            Failed: '43'
                        },
                        Status: 'building'
                    }
                ]
            }
        ];

        return {
            initialize: initialize,
            //sendRequest: sendRequest,
            query: function () {
                return projects;
            }
        };
    });
}());