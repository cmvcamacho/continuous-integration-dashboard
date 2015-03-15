(function () {
    'use strict';

    var app = angular.module('controllers')
        .controller('ProjectController', ['$scope', '$projects', function ($scope, $projects) {
            $scope.projects = $projects.query();

            var hub = $.connection.ciHub;
            hub.client.hello = function (message) {
                alert(message);
            }
            $.connection.logging = true;

        $.connection.hub.start()
            .done(function() {
                console.log('Now connected, connection ID=' + $.connection.hub.id);
                hub.server.hello();
            })
            .fail(function() { console.log('Could not Connect!'); });

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
