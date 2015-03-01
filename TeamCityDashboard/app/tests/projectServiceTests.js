///<reference path="~/Scripts/jasmine.js"/>
///<reference path="~/Scripts/angular.js"/>
///<reference path="~/Scripts/angular-mocks.js"/>

///<reference path="~/App/app.js"/>
///<reference path="~/App/services.js"/>
///<reference path="~/App/services/projectService.js"/>

describe("Servicea suite", function () {
    beforeEach(module("services"));

    describe("Project service", function () {

        var project;

        beforeEach(inject(function ($injector) {
            project = $injector.get('Project');
        }));

        it('should return 2 project when querying', function () {
            expect(project.query().length).toBe(2);
        });

    });
});