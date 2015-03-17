(function () {
    'use strict';

    var app = angular.module('app', [
        'controllers',
        'services',
        'filters'
    ]);

    toastr.options = {
        "debug": false,
        "positionClass": "toast-bottom-right",
        "onclick": null,
        "fadeIn": 300,
        "fadeOut": 1000,
        "newestOnTop": false,
        "preventDuplicates": true,
        "showDuration": "150",
        "hideDuration": "500",
        "timeOut": "2500",
        "extendedTimeOut": "500",
    };
})();