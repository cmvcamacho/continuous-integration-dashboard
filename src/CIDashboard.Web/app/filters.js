(function () {
    'use strict';

    var app = angular.module('filters', []);
        
    app.filter('buildStatusCss', function () {
            return function (status) {
                return (status === 'success') ? 'panel-success'
                    : (status === 'failed') ? 'panel-danger'
                    : (status === 'building') ? 'panel-primary'
                    : 'panel-info';
            }
    });

    /**
     * AngularJS default filter with the following expression:
     * "person in people | filter: {name: $select.search, age: $select.search}"
     * performs a AND between 'name: $select.search' and 'age: $select.search'.
     * We want to perform a OR.
     */
    app.filter('propsFilter', function () {
        return function (items, props) {
            var out = [];
            var keys = Object.keys(props);

            if (angular.isArray(items) && props[keys[0]] !== "") {
                items.forEach(function (item) {
                    var itemMatches = false;

                    for (var i = 0; i < keys.length; i++) {
                        var prop = keys[i];
                        var text = props[prop].toLowerCase();
                        if (item[prop].toString().toLowerCase().indexOf(text) !== -1) {
                            itemMatches = true;
                            break;
                        }
                    }

                    if (itemMatches) {
                        out.push(item);
                    }
                });
            } else {
                // Let the output be the input untouched
                out = items;
            }

            return out;
        }
    });
})();