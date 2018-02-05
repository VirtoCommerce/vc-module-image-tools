angular.module('virtoCommerce.imageToolsModule')
    .controller('virtoCommerce.imageToolsModule.taskListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils', 'virtoCommerce.imageToolsModule.api', 'platformWebApp.uiGridHelper', 'platformWebApp.dialogService',
        function ($scope, bladeNavigationService, bladeUtils, thumbnailApi, uiGridHelper, dialogService) {
            var blade = $scope.blade;

            $scope.uiGridConstants = uiGridHelper.uiGridConstants;
            $scope.hasMore = true;
            $scope.items = [];

            blade.refresh = function () {
                blade.isLoading = true;

                thumbnailApi.getTaskList().then(function (results) {

                    blade.isLoading = false;
                    $scope.items = results;
                    $scope.hasMore = results.length === $scope.pageSettings.itemsPerPageCount;
                });
            };

            blade.setSelectedItem = function (listItem) {
                $scope.selectedNodeId = listItem.id;
            };

            $scope.selectItem = function (e, listItem) {
                blade.setSelectedItem(listItem);
                var newBlade = {
                    id: "listTaskDetail",
                    itemId: listItem.id,
                    title: 'imageTools.blades.task-detail.title',
                    subtitle: 'imageTools.blades.task-detail.subtitle',
                    controller: 'virtoCommerce.imageToolsModule.taskDetailController',
                    template: 'Modules/$(VirtoCommerce.ImageTools)/Scripts/blades/task-detail.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };


            $scope.selectNode = function (node, isNew) {
                $scope.selectedNodeId = node.id;

                var newBlade = {
                    id: 'listTaskDetail',
                    controller: 'virtoCommerce.imageToolsModule.taskDetailController',
                    template: 'Modules/$(VirtoCommerce.ImageTools)/Scripts/blades/task-detail.tpl.html'
                };

                if (isNew) {
                    angular.extend(newBlade, {
                        title: 'pricing.blades.pricelist-detail.title-new',
                        isNew: true,
                        saveCallback: function (newPricelist) {
                            newBlade.isNew = false;
                            blade.refresh(true).then(function () {
                                newBlade.currentEntityId = newPricelist.id;
                                bladeNavigationService.showBlade(newBlade, blade);
                            });
                        }
                        // onChangesConfirmedFn: callback,
                    });
                } else {
                    angular.extend(newBlade, {
                        currentEntityId: node.id,
                        title: node.name,
                        subtitle: 'imageTools.blades.task-detail.subtitle'
                    });
                }

                bladeNavigationService.showBlade(newBlade, blade);
            };


            $scope.taskRun = function (itemsSelect) {
                var dialog = {
                    id: "confirmTaskRun",
                    callback: function (doReindex) {
                        var options = _.map(documentTypes, function (x) {
                            return {
                                documentType: x.documentType,
                                deleteExistingIndex: doReindex
                            };
                        });
                        thumbnailApi.taskRun(options).then(function openProgressBlade(data) {
                            var newBlade = {
                                id: 'thumbnailProgress',
                                notification: data,
                                parentRefresh: blade.parentRefresh,
                                controller: 'virtoCommerce.coreModule.indexProgressController',
                                template: '$(Platform)/Scripts/app/thumbnail/blades/task-detail.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade.parentBlade || blade);
                        });
                    }
                }
                dialogService.showDialog(dialog, '$(Platform)/Scripts/app/thumbnail/dialogs/run-dialog.tpl.html', 'platformWebApp.confirmDialogController');
            }

            function isItemsChecked() {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }

            function getSelectedItems() {
                return $scope.gridApi.selection.getSelectedRows();
            }


            blade.headIcon = 'fa fa-picture-o';
            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        $scope.selectNode({}, true);
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "imageTools.commands.run",
                    icon: 'fa fa-exclamation',
                    canExecuteMethod: isItemsChecked,
                    executeMethod: function () {
                        $scope.taskRun(getSelectedItems());
                    }
                },
                {
                    name: "platform.commands.delete", icon: 'fa fa-trash-o',
                    executeMethod: function () {
                        $scope.taskDelete(getSelectedItems());
                    },
                    canExecuteMethod: isItemsChecked
                }
            ];

            // ui-grid
            $scope.setGridOptions = function (gridOptions) {

                //disable watched
                bladeUtils.initializePagination($scope, true);
                //сhoose the optimal amount that ensures the appearance of the scroll
                $scope.pageSettings.itemsPerPageCount = 50;

                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    //update gridApi for current grid
                    $scope.gridApi = gridApi;

                    uiGridHelper.bindRefreshOnSortChanged($scope);
                    //$scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);
                });

                blade.refresh();
            };
        }]);
