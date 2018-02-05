angular.module('virtoCommerce.imageToolsModule')
    .controller('virtoCommerce.imageToolsModule.taskDetailController', ['$rootScope', '$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.imageToolsModule.api', function ($rootScope, $scope, bladeNavigationService, thumbnailApi) {
        var blade = $scope.blade;

        blade.refresh = function (parentRefresh) {

            thumbnailApi.getListOptions().then(function (data) {
                blade.optionList = data;
            });

            thumbnailApi.getTask(blade.itemId).then(function (item) {

                initializeBlade(item);

                if (blade.childrenBlades) {
                    _.each(blade.childrenBlades, function (x) {
                        if (x.refresh) {
                            x.refresh(blade.currentEntity);
                        }
                    });
                }

                if (parentRefresh) {
                    blade.parentBlade.refresh();
                }
            }
            );
        };

        function initializeBlade(data) {
            blade.item = angular.copy(data);
            blade.currentEntity = blade.item;
            blade.origEntity = data;
            blade.isLoading = false;
        };

        blade.codeValidator = function (value) {
            var pattern = /[$+;=%{}[\]|\\\/@ ~!^*&()?:'<>,]/;
            return !pattern.test(value);
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        };

        function canSave() {
            return isDirty() && blade.formScope && blade.formScope.$valid;
        }

        function saveChanges() {
            blade.isLoading = true;
            categories.update({}, blade.currentEntity, function (data, headers) {
                blade.refresh(true);
            },
                function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog.dialogs.category-save.title", "catalog.dialogs.category-save.message");
        };

        blade.formScope = null;
        $scope.setForm = function (form) { blade.formScope = form; }

        blade.toolbarCommands = [
            {
                name: "platform.commands.save",
                icon: 'fa fa-save',
                executeMethod: blade.refresh,
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "platform.commands.reset",
                icon: 'fa fa-undo',
                executeMethod: blade.refresh,
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "imageTools.commands.run",
                icon: 'fa fa-exclamation',
                executeMethod: blade.refresh,
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "platform.commands.delete",
                icon: 'fa fa-trash-o',
                executeMethod: blade.refresh,
                canExecuteMethod: function () {
                    return true;
                }
            }
        ];

        blade.openSettingManagement = function () {
            var newBlade = {
                id: 'optionListDetail',
                currentEntityId: blade.itemId,
                title: 'imageTools.blades.setting-managment.title',
                subtitle: 'imageTools.blades.setting-managment.subtitle',
                controller: 'virtoCommerce.imageToolsModule.optionListController',
                template: 'Modules/$(VirtoCommerce.ImageTools)/Scripts/blades/option-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        blade.refresh();

    }]);
