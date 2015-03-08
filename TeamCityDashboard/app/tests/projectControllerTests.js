///<reference path="~/Scripts/jasmine.js"/>
///<reference path="~/Scripts/angular.js"/>
///<reference path="~/Scripts/angular-mocks.js"/>

///<reference path="~/App/app.js"/>
///<reference path="~/App/controllers.js"/>
///<reference path="~/App/controllers/ProjectController.js"/>

describe("Controllers suite", function () {

    beforeEach(module("controllers"));

    describe("Project controller", function () {

        var scope,
            projectsService,
            controller;

        beforeEach(inject(function ($rootScope, $controller) {
            scope = $rootScope.$new();

            projectsService = {
                query: function () {
                    return [
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
                        }
                    ];
                }
            }

            controller = $controller('ProjectController', { $scope: scope, $projects: projectsService });
        }));

        it('should have 1 project', function () {
            expect(scope.projects.length).toBe(1);
        });

    });
});