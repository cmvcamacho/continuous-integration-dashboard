(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projectsService', function ($scope, $projectsService) {

            $projectsService.initialize();

            $scope.loading = true;
            $scope.projects = [];

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
                for (var i = 0; i < $scope.projects.length; i++) {
                    for (var j = 0; j < $scope.projects[i].Builds.length; j++) {
                        if ($scope.projects[i].Builds[j].BuildId === buildResult.BuildId) {
                            $scope.projects[i].Builds[j] = buildResult;
                            //$('#build_' + buildResult.BuildId).jrumble({ speed: 0 });
                            //$('#build_' + buildResult.BuildId).trigger('startRumble');
                            return;
                        }
                    }
                }
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

            $scope.$parent.$on("sendBuildResult", function (e, buildResult) {
                $scope.$apply(function () {
                    showBuildResult(buildResult);
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
