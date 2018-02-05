var imageToolsModuleName = "virtoCommerce.imageToolsModule";

angular.module(imageToolsModuleName, ['ui.grid.infiniteScroll'])
    .config(['$stateProvider', function ($stateProvider) {
        $stateProvider
            .state('workspace.thumbnail', {
                url: '/thumbnail',
                templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                controller: ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                    var blade = {
                        id: 'thumbnailList',
                        title: 'platform.blades.thumbnail.title',
                        subtitle: 'platform.blades.thumbnail.subtitle',
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
        debugger;
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

        //pushNotificationTemplateResolver.register({
        //    priority: 900,
        //    satisfy: function (notify, place) { return place == 'history' && notify.notifyType == 'IndexProgressPushNotification'; },
        //    template: '$(Platform)/Scripts/app/pushNotifications/blade/historyDefault.tpl.html',
        //    action: function (notify) {
        //        var blade = {
        //            id: 'indexProgress',
        //            notification: notify,
        //            controller: 'virtoCommerce.coreModule.indexProgressController',
        //            template: 'Modules/$(VirtoCommerce.Core)/Scripts/SearchIndex/blades/index-progress.tpl.html'
        //        };
        //        bladeNavigationService.showBlade(blade);
        //    }
        //});
    }]);
