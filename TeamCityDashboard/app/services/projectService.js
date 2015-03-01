(function () {
    'use strict';
    angular.module('services').factory('Project', function () {

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
            query: function () {
                return projects;
            }
        };
    });
}());