angular.module('virtoCommerce.imageToolsModule')
    .factory('virtoCommerce.imageToolsModule.taskApi', ['$resource', function ($resource) {
        return $resource('api/image/thumbnails/tasks/:id', { id: '@id' }, {
            search: { method: 'POST', url: 'api/image/thumbnails/tasks/search' },
            update: { method: 'PUT', url: 'api/image/thumbnails/tasks' }
        });
    }])
    .factory('virtoCommerce.imageToolsModule.optionApi', ['$resource', function ($resource) {
        return $resource('api/image/thumbnails/options/:id', { id: '@id' }, {
            search: { method: 'POST', url: 'api/image/thumbnails/options/search' },
            update: { method: 'PUT', url: 'api/image/thumbnails/options' }
        });
    }])
    .factory('virtoCommerce.imageToolsModule.api', ['$resource', '$timeout', function ($resource, $timeout) {
        return {
            taskRun: function () {
                return $timeout(function () { return {}; }, 1000);
            }
        }
    }]);
