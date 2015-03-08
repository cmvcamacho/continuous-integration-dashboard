///<reference path="~/Scripts/jasmine.js"/>
///<reference path="~/Scripts/boot.js"/>
///<reference path="~/Scripts/angular.js"/>
///<reference path="~/Scripts/angular-mocks.js"/>

///<reference path="~/App/app.js"/>
///<reference path="~/App/filters.js"/>

describe("Filters suite", function () {

    beforeEach(module('filters'));

    describe("buildStatusCss filter", function () {

        it('buildStatusCss filter returns panel-sucess when build is green', inject(function (buildStatusCssFilter) {
            expect(buildStatusCssFilter('success')).toMatch('panel-success');
        }));

        it('buildStatusCss filter returns panel-sucess when build is failed', inject(function (buildStatusCssFilter) {
            expect(buildStatusCssFilter('failed')).toMatch('panel-danger');
        }));

        it('buildStatusCss filter returns panel-sucess when build is building', inject(function (buildStatusCssFilter) {
            expect(buildStatusCssFilter('building')).toMatch('panel-primary');
        }));

        it('buildStatusCss filter returns panel-sucess when build has no status', inject(function (buildStatusCssFilter) {
            expect(buildStatusCssFilter('hello')).toMatch('panel-info');
            expect(buildStatusCssFilter('')).toMatch('panel-info');
            expect(buildStatusCssFilter(null)).toMatch('panel-info');
            expect(buildStatusCssFilter(undefined)).toMatch('panel-info');
        }));
     
    });
});
