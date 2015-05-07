(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projectsService', '$rootScope', function ($scope, $projectsService, $rootScope) {

            $projectsService.initialize();

            $scope.loading = true;
            //$scope.$parent is EditController
            $scope.projects = $scope.$parent.projects;

            $scope.selectedBuild = {};
            $scope.selectableBuilds = $scope.$parent.selectableBuilds;

            var findProjectAndBuildByBuildId = $scope.$parent.findProjectAndBuildByBuildId;

            var findProjectProjectId = $scope.$parent.findProjectProjectId;

            var showMessage = function (message) {
                if (message.Status === "Info")
                    toastr.info(message.Message);
                else if (message.Status === "Success")
                    toastr.success(message.Message);
            }

            var showProjects = function (projectList) {
                $scope.loading = false;
                $scope.projects.splice(0, $scope.projects.length);

                var list = JSON.parse(projectList);
                for(var i=0; i<list.length; i++)
                    $scope.projects.push(list[i]);
            }

            var showBuildResult = function (buildResult) {
                var idx = findProjectAndBuildByBuildId(buildResult.CiExternalId);
                if (!idx) {
                    toastr.error('Build not found: ' + buildResult.Name);
                }
                else {
                    $scope.projects[idx.projectIndex].Builds[idx.buildIndex] = buildResult;
                    toastr.success(buildResult.Name + " updated!");
                    //$('#build_' + buildResult.CiExternalId).jrumble({ speed: 0 });
                    //$('#build_' + buildResult.CiExternalId).trigger('startRumble');
                }
            }

            $scope.$on("sendMessage", function (e, message) {
                $scope.$apply(function () {
                    showMessage(message);
                });
            });

            $scope.$on("sendProjects", function (e, projectList) {
                $scope.$apply(function () {
                    showProjects(projectList);
                });
            });

            $scope.$on("sendBuildResult", function (e, buildResult) {
                $scope.$apply(function () {
                    showBuildResult(buildResult);
                });
            });

            $scope.addProject = function () {
                $scope.$parent.addProject();
            }

            $scope.addBuild = function (projectId) {
                $scope.$parent.addBuild(projectId);
            }

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
