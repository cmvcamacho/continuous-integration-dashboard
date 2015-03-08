///<reference path="~/Scripts/jasmine.js"/>
///<reference path="~/Scripts/angular.js"/>
///<reference path="~/Scripts/angular-mocks.js"/>

///<reference path="~/App/app.js"/>
///<reference path="~/App/services.js"/>
///<reference path="~/App/services/projectService.js"/>

describe("Servicea suite", function () {
    beforeEach(module("services"));

    describe("Project service", function () {

        var projectsService;

        beforeEach(inject(function ($injector) {
            projectsService = $injector.get('$projects');
        }));

        it('should return 2 project when querying', function () {
            expect(projectsService.query().length).toBe(2);
        });

    });
});