(function () {
    'use strict';

    var app = angular.module('filters', [])
        .filter('buildStatusCss', function () {
            return function (status) {
                return (status === 'success') ? 'panel-success'
                    : (status === 'failed') ? 'panel-danger'
                    : (status === 'building') ? 'panel-primary'
                    : 'panel-info';
            }
        });
})();