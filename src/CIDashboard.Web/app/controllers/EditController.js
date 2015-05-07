(function() {
    'use strict';

    var app = angular.module('controllers')
        .controller('EditController', [
            '$scope', 'editService', '$rootScope', function ($scope, $editService, $rootScope) {
                $editService.initialize();

                $scope.refreshing = false;
                $scope.projects = [];
                $scope.selectableBuilds = [];

                $scope.toggleEditMode = function() {
                    $rootScope.editMode = !$rootScope.editMode;
                }

                $scope.startRefresh = function () {
                    startRefresh();
                }

                var findProjectAndBuildByBuildId = function (buildId) {
                    for (var i = 0; i < $scope.projects.length; i++) {
                        for (var j = 0; j < $scope.projects[i].Builds.length; j++) {
                            if ($scope.projects[i].Builds[j].CiExternalId === buildId) {
                                return { "projectIndex": i, "buildIndex": j };
                            }
                        }
                    }
                    return null;
                }

                var findProjectProjectId = function (projectId) {
                    for (var i = 0; i < $scope.projects.length; i++) {
                        if ($scope.projects[i].Id === projectId) {
                            return { "projectIndex": i };
                        }
                    }
                    return null;
                }

                $scope.findProjectAndBuildByBuildId = findProjectAndBuildByBuildId;

                $scope.findProjectProjectId = findProjectProjectId;

                $scope.addBuild = function (projectId) {
                    var idx = findProjectProjectId(projectId);
                    if (!idx) {
                        toastr.error('Project not found: ' + projectId);
                    }
                    else {
                        $scope.projects[idx.projectIndex].Builds.push({ "Name": "Select a build" });
                    }
                }

                $scope.addProject = function () {
                    $scope.projects.push({ "Name": "Enter project name", "Builds": [] });
                }

                var startRefresh = function() {
                    $editService.requestRefresh();
                }

                var stopRefresh = function () {
                    $scope.$apply(function () {
                        $scope.refreshing = false;
                    });
                }

                $scope.$on("startRefresh", function (e, status) {
                    $scope.$apply(function () {
                        $scope.refreshing = true;
                    });
                });

                $scope.$on("stopRefresh", function (e, status) {
                    setTimeout(stopRefresh, 500);
                });

                $scope.$on("sendProjectBuilds", function (e, projectBuilds) {
                    $scope.$apply(function () {
                        $scope.selectableBuilds.splice(0, $scope.selectableBuilds.length);

                        for (var i = 0; i < projectBuilds.length; i++)
                            $scope.selectableBuilds.push(projectBuilds[i]);

                        toastr.info('project build list updated...');
                    });
                }); 
            }
        ]);
})();
