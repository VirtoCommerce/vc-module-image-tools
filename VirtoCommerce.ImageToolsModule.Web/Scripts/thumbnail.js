var moduleName = "virtoCommerce.imageToolsModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}
angular.module(moduleName, ['ui.grid.infiniteScroll'])
    .config(['$stateProvider', function ($stateProvider) {
        $stateProvider
            .state('workspace.thumbnail', {
                url: '/thumbnail',
                templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                controller: ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                    var blade = {
                        id: 'thumbnailList',
                        title: 'imageTools.blades.tasks-list.title',
                        subtitle: 'imageTools.blades.tasks-list.subtitle',
                        controller: 'virtoCommerce.imageToolsModule.taskListController',
                        template: 'Modules/$(VirtoCommerce.ImageTools)/Scripts/blades/task-list.tpl.html',
                        isClosingDisabled: true
                    };
                    bladeNavigationService.showBlade(blade);
                }]
            });
    }])
    .run(
        ['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', 'platformWebApp.authService', function ($rootScope, mainMenuService, widgetService, $state, authService) {
        var menuItem = {
            path: 'browse/thumbnail',
            icon: 'fa fa-picture-o',
            title: 'imageTools.main-menu-title',
            priority: 30,
            action: function () { $state.go('workspace.thumbnail'); },
            permission: 'thumbnail:access'
        };
        mainMenuService.addMenuItem(menuItem);

        // ToDo register notification template
    }]);
