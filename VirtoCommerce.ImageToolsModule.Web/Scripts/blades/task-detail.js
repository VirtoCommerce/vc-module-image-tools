angular.module('virtoCommerce.imageToolsModule')
    .controller('virtoCommerce.imageToolsModule.taskDetailController', ['$rootScope', '$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.imageToolsModule.api', function ($rootScope, $scope, bladeNavigationService, thumbnailApi) {
        var blade = $scope.blade;

        blade.refresh = function (parentRefresh) {
            debugger;
            thumbnailApi.getListOptions().then(function (data) {
                blade.optionList = data;
            });

            if (blade.isNew) {
                initializeBlade({});
            } else {
                thumbnailApi.getTask({ id: blade.currentEntityId }, function (data) {
                    initializeBlade(data);
                    if (parentRefresh) {
                        blade.parentBlade.refresh();
                    }
                });
            }
        };

        function initializeBlade(data) {
            debugger;
            blade.item = angular.copy(data);
            blade.currentEntity = blade.item;
            blade.origEntity = data;
            blade.isLoading = false;
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
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "imageTools.dialogs.task-save.title", "imageTools.dialogs.task-save.message");
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
        function folderPath(folderPath) {
            if (folderPath && folderPath.length === 1 && folderPath[0].type === 'folder') {
                blade.currentEntity.workPath = folderPath[0].relativeUrl;
            } else {
                
            }
        }

        blade.openFolderPath = function () {
            var newBlade = {
                title: 'imageTools.blades.setting-managment.title',
                subtitle: 'imageTools.blades.setting-managment.subtitle',
                onSelect: folderPath,
                controller: 'platformWebApp.assets.assetSelectController'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

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
