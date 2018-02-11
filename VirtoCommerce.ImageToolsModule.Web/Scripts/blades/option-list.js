angular.module('virtoCommerce.imageToolsModule')
    .controller('virtoCommerce.imageToolsModule.optionListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.imageToolsModule.optionApi',
        function ($scope, bladeNavigationService, optionApi) {
            var blade = $scope.blade;

            blade.refresh = function (parentRefresh) {

                blade.isLoading = true;
                optionApi.search({
                    skip: 0
                }, function (data) {
                    blade.isLoading = false;
                    blade.currentEntities = data.result;
                });

                if (parentRefresh && blade.parentBlade.refresh) {
                    blade.parentBlade.refresh();
                }

            };

            blade.setSelectedId = function (selectedNodeId) {
                $scope.selectedNodeId = selectedNodeId;
            };

            function showDetailBlade(bladeData) {
                var newBlade = {
                    id: 'optionDetail',
                    controller: 'virtoCommerce.imageToolsModule.optionDetailController',
                    template: 'Modules/$(VirtoCommerce.ImageTools)/Scripts/blades/option-detail.tpl.html'
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.selectNode = function (listItem) {
                blade.setSelectedId(listItem.id);
                showDetailBlade({ currentEntityId: listItem.id });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh", icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        blade.setSelectedId(null);
                        showDetailBlade({ isNew: true });
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            blade.refresh();
        }]);
