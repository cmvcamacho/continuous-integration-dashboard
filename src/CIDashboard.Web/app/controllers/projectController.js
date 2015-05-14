(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projectsService', '$rootScope', function ($scope, $projectsService, $rootScope) {

            $projectsService.initialize();

            $scope.loading = true;
            //$scope.$parent is EditController
            $scope.projects = $scope.$parent.projects;

            //helpers
            var findProjectAndBuildByBuildId = $scope.$parent.findProjectAndBuildByBuildId;

            var findProjectProjectId = $scope.$parent.findProjectProjectId;

            //show info messages
            var showMessage = function (message) {
                if (message.Status === "Info")
                    toastr.info(message.Message);
                else if (message.Status === "Success")
                    toastr.success(message.Message);
                else if (message.Status === "Error")
                    toastr.error(message.Message);
            }


            //show initial projects and builds
            var showProjectsAndBuildConfigs = function (projectList) {
                $scope.loading = false;
                $scope.projects.splice(0, $scope.projects.length);

                for (var i = 0; i < projectList.length; i++)
                    $scope.projects.push(projectList[i]);
            }


            //add, update projects
            var showUpdatedProject = function (projectInfo) {
                var idx = (projectInfo.OldId) 
                        ? findProjectProjectId(projectInfo.OldId)
                        : findProjectProjectId(projectInfo.Project.Id);

                if (!idx) {
                    toastr.error('Project not found: ' + projectInfo.Project.Name);
                }
                else {
                    $scope.projects[idx.projectIndex] = projectInfo.Project;
                    toastr.success(projectInfo.Project.Name + " updated!");
                    //$('#build_' + buildResult.CiExternalId).jrumble({ speed: 0 });
                    //$('#build_' + buildResult.CiExternalId).trigger('startRumble');
                }
            }
            
            $scope.addProject = function () {
                $scope.$parent.addProject();
            }

            $scope.updateProjectName = function (projectId) {
                $scope.$parent.updateProjectName(projectId);
            }

            $scope.removeProject = function (projectId) {
                $scope.$parent.removeProject(projectId);
            }


            //add, update build results
            var showBuildResult = function (buildResult) {
                var idx = findProjectAndBuildByBuildId(buildResult.Id);
                if (!idx)
                    idx = findProjectAndBuildByBuildId(buildResult.CiExternalId);
                if (!idx) {
                    toastr.error('Build not found: ' + buildResult.Name);
                }
                else {
                    buildResult.Id = $scope.projects[idx.projectIndex].Builds[idx.buildIndex].Id;
                    $scope.projects[idx.projectIndex].Builds[idx.buildIndex] = buildResult;
                    toastr.success(buildResult.Name + " updated!");
                    //$('#build_' + buildResult.CiExternalId).jrumble({ speed: 0 });
                    //$('#build_' + buildResult.CiExternalId).trigger('startRumble');
                }
            }

            var showUpdatedBuild = function (buildInfo) {
                var idx = (buildInfo.OldId)
                        ? findProjectAndBuildByBuildId(buildInfo.OldId)
                        : findProjectAndBuildByBuildId(buildInfo.Build.Id);

                if (!idx) {
                    toastr.error('Build not found: ' + buildInfo.Build.Name);
                }
                else {
                    $scope.projects[idx.projectIndex].Builds[idx.buildIndex] = buildInfo.Build;
                    toastr.success(buildInfo.Build.Name + " updated!");
                    //$('#build_' + buildResult.CiExternalId).jrumble({ speed: 0 });
                    //$('#build_' + buildResult.CiExternalId).trigger('startRumble');
                }
            }

            $scope.addBuildToProject = function (projectId) {
                $scope.$parent.addBuildToProject(projectId);
            }

            $scope.removeBuild = function (buildId) {
                $scope.$parent.removeBuild(buildId);
            }

            $scope.getBuildsToShow = function (search) {
                return $scope.$parent.getBuildsToShow(search);
            }

            $scope.onBuildSelect = function ($item, ciExternalId) {
                $scope.$parent.onBuildSelect($item, ciExternalId);
            };


            //event listeners
            $scope.$on("sendMessage", function (e, message) {
                $scope.$apply(function () {
                    showMessage(message);
                });
            });

            $scope.$on("sendProjectsAndBuildConfigs", function (e, projectList) {
                $scope.$apply(function () {
                    showProjectsAndBuildConfigs(projectList);
                });
            });

            $scope.$on("sendBuildResult", function (e, buildResult) {
                $scope.$apply(function () {
                    showBuildResult(buildResult);
                });
            });

            $scope.$on("sendUpdatedProject", function (e, projectInfo) {
                $scope.$apply(function () {
                    showUpdatedProject(projectInfo);
                });
            });

            $scope.$on("sendUpdatedBuild", function (e, buildInfo) {
                $scope.$apply(function () {
                    showUpdatedBuild(buildInfo);
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
