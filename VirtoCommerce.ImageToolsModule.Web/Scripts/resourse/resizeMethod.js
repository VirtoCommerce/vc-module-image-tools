angular.module('virtoCommerce.imageToolsModule')
    .factory('virtoCommerce.imageToolsModule.resizeMethod', function () {
        return {
            get: function () {
                return [
                    {
                        value: "FixedSize",
                        title: "imageTools.blades.setting-detail.resize-method.fixed-size"
                    },
                    {
                        value: "FixedWidth",
                        title: "imageTools.blades.setting-detail.resize-method.fixed-width"
                    },
                    {
                        value: "FixedHeight",
                        title: "imageTools.blades.setting-detail.resize-method.fixed-height"
                    },
                    {
                        value: "Crop",
                        title: "imageTools.blades.setting-detail.resize-method.crop"
                    }
                ];
            }
        };
    });
