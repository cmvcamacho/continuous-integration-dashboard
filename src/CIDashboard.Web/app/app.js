(function () {
    'use strict';

    var app = angular.module('app', [
        'ngSanitize', 'xeditable', 'ui.select',
        'controllers',
        'services',
        'filters'
    ]);

    app.run(function ($rootScope, editableOptions, editableThemes) {
        editableThemes.bs3.inputClass = 'input-sm';
        editableThemes.bs3.buttonsClass = 'btn-sm';
        editableOptions.theme = 'bs3';

        $rootScope.editMode = false;
    });
    app.config(function (uiSelectConfig) {
        uiSelectConfig.theme = 'select2';
        uiSelectConfig.resetSearchInput = true;
        uiSelectConfig.appendToBody = false;
    });

    app.value('$', $);

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