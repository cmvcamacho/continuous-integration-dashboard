(function() {
    'use strict';

    var app = angular.module('controllers')
        .controller('EditController', [
            '$scope', 'editService', '$rootScope', function ($scope, $editService, $rootScope) {
                $editService.initialize();

                $scope.refreshing = false;

                $scope.toggleEditMode = function() {
                    $rootScope.editMode = !$rootScope.editMode;
                }

                $scope.startRefresh = function () {
                    startRefresh();
                }

                var startRefresh = function() {
                    $editService.requestRefresh();
                }

                var stopRefresh = function () {
                    $scope.$apply(function () {
                        $scope.refreshing = false;
                    });
                }

                $scope.$parent.$on("startRefresh", function (e, status) {
                    $scope.$apply(function () {
                        $scope.refreshing = true;
                    });
                });

                $scope.$parent.$on("stopRefresh", function (e, status) {
                    setTimeout(stopRefresh, 500);
                });
            }
        ]);
})();
