(function() {
    'use strict';

    var app = angular.module('controllers')
        .controller('EditController', [
            '$scope', 'editService', 'filterFilter', '$timeout', '$rootScope', function ($scope, $editService, filterFilter, $timeout, $rootScope) {
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

                //helpers
                var findProjectAndBuildByBuildId = function (buildId) {
                    if ($scope.projects) {
                        for (var i = 0; i < $scope.projects.length; i++) {
                            if ($scope.projects[i].Builds) {
                                for (var j = 0; j < $scope.projects[i].Builds.length; j++) {
                                    if ($scope.projects[i].Builds[j].Id === buildId) {
                                        return { "projectIndex": i, "buildIndex": j };
                                    }
                                }
                            }
                        }
                    }
                    return findProjectAndBuildByCiExternalId(buildId);
                }
                var findProjectAndBuildByCiExternalId = function (buildId) {
                    if ($scope.projects) {
                        for (var i = 0; i < $scope.projects.length; i++) {
                            if ($scope.projects[i].Builds) {
                                for (var j = 0; j < $scope.projects[i].Builds.length; j++) {
                                    if ($scope.projects[i].Builds[j].CiExternalId === buildId) {
                                        return { "projectIndex": i, "buildIndex": j };
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }

                var findProjectProjectId = function (projectId) {
                    if ($scope.projects) {
                        for (var i = 0; i < $scope.projects.length; i++) {
                            if ($scope.projects[i].Id === projectId) {
                                return { "projectIndex": i };
                            }
                        }
                    }
                    return null;
                }

                var randomIntFromInterval = function () {
                    var min = -9999;
                    var max = -1;
                    return Math.floor(Math.random() * (max - min + 1) + min);
                }

                $scope.findProjectAndBuildByBuildId = findProjectAndBuildByBuildId;

                $scope.findProjectProjectId = findProjectProjectId;


                //get list of selectabled builds
                $scope.getBuildsToShow = function (search) {
                    var filtered = filterFilter($scope.selectableBuilds, search.viewValue);

                    var results = _(filtered)
                      .groupBy('ProjectName')
                      .map(function (g) {
                            g[0].firstInGroup = true;  // the first item in each group
                            for (var i = 0; i < g.length; i++)
                                g[i].BuildCiExternalId = search.ciExternalId;
                            return g;
                      })
                      .flatten()
                      .value();
                    return results;
                }

                var updateSelectabledBuilds = function (projectBuilds) {
                    $scope.selectableBuilds.splice(0, $scope.selectableBuilds.length);

                    for (var i = 0; i < projectBuilds.length; i++)
                        $scope.selectableBuilds.push(projectBuilds[i]);

                    toastr.info('project build list updated...');
                }


                //add, update projects
                $scope.addProject = function () {
                    var project = { "Id": randomIntFromInterval(), "Name": "Enter project name", "Builds": [] };
                    $scope.projects.push(project);

                    $editService.addNewProject(project);
                }

                $scope.updateProjectName = function (projectId) {
                    var idx = findProjectProjectId(projectId);
                    if (!idx) {
                        toastr.error('Project not found: ' + projectId);
                    }
                    else {
                        var project = $scope.projects[idx.projectIndex];
                        $editService.updateProjectName(projectId, project.Name);
                    }
                }

                $scope.removeProject = function (projectId) {
                    var idx = findProjectProjectId(projectId);
                    if (!idx) {
                        toastr.error('Project not found: ' + projectId);
                    }
                    else {
                        $scope.projects.splice(idx.projectIndex, 1);
                        $editService.removeProject(projectId);
                    }
                }


                //add, update build results
                $scope.addBuildToProject = function (projectId) {
                    var idx = findProjectProjectId(projectId);
                    if (!idx) {
                        toastr.error('Project not found: ' + projectId);
                    }
                    else {
                        var id = randomIntFromInterval();
                        var build = { "Id": id, "Name": "Select a build", "CiExternalId": id };
                        $scope.projects[idx.projectIndex].Builds.push(build);

                        $editService.addBuildToProject(projectId, build);
                    }
                }

                $scope.removeBuild = function (buildId) {
                    var idx = findProjectAndBuildByBuildId(buildId);
                    if (!idx) {
                        toastr.error('Build not found: ' + projectId);
                    }
                    else {
                        $scope.projects[idx.projectIndex].Builds.splice(idx.buildIndex, 1);
                        $editService.removeBuild(buildId);
                    }
                }

                $scope.onBuildSelect = function ($item, buildId) {
                    var idx = findProjectAndBuildByBuildId(buildId);
                    if (idx){
                        var build = $scope.projects[idx.projectIndex].Builds[idx.buildIndex];
                        build.CiExternalId = $item.CiExternalId;
                        build.Name = $item.Name;
                        build.Version = null;
                        build.Status = null;
                        build.Url = null;
                        build.NumberTestPassed = 0;
                        build.NumberTestFailed = 0;
                        build.editThisBuild = false;

                        $editService.updateBuildNameAndExternalId(buildId, $item.Name, $item.CiExternalId);
                    }
                };

                var startRefresh = function () {
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
                        updateSelectabledBuilds(projectBuilds);
                    });
                }); 
            }
        ]);
})();
