(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projectsService', function ($scope, $projectsService) {

            $projectsService.initialize();


            $scope.projects = [];

            var showMessage = function (message) {
                if (message.Status === "Info")
                    toastr.info(message.Message);
                else if (message.Status === "Success")
                    toastr.success(message.Message);
            }

            var showProjects = function (projectList) {
                $('.spinner').remove();
                $scope.projects.splice(0, $scope.projects.length);

                var list = JSON.parse(projectList);
                for(var i=0; i<list.length; i++)
                    $scope.projects.push(list[i]);
            }

            $scope.$parent.$on("sendMessage", function (e, message) {
                $scope.$apply(function () {
                    showMessage(message);
                });
            });

            $scope.$parent.$on("sendProjects", function (e, projectList) {
                $scope.$apply(function () {
                    showProjects(projectList);
                });
            });
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
