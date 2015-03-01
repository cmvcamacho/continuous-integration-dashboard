(function () {
    'use strict';

    var app =  angular.module('controllers').controller('ProjectController', ['$scope', 'Project', function ($scope, Project) {
        $scope.projects = Project.query();
    }]);
  
    app.directive('project', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/views/project.html'
        };
    });

    app.directive('build', function () {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/views/build.html'
        };
    });
})();
