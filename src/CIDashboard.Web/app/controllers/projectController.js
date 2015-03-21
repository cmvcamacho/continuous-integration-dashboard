(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projectsService', function ($scope, $projectsService) {

            $projectsService.initialize();


            $scope.projects = $projectsService.query();


            var showMessage = function (message) {
                toastr.success(message);
            }
            $scope.$parent.$on("sendMessage", function (e, message) {
                $scope.$apply(function () {
                    showMessage(message);
                });
            });

            //var hub = $.connection.ciDashboardHub;
            //hub.client.sendMessage = function (message) {
            //    toastr.success(message);
            //}
            //$.connection.logging = true;

            //$.connection.hub.start()
            //    .done(function() {
            //        console.log('Now connected, connection ID=' + $.connection.hub.id);
            //        toastr.success('Connection established');
            //    })
            //    .fail(function() { console.log('Could not Connect!'); });

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
