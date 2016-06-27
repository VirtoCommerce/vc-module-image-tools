

# VirtoCommerce.ImageTools
VirtoCommerce.ImageTools module represents functionality for working with images.
Key features:
* It allows you to make thumbnails of images in different ways

# Documentation
Thumbnails
Use API to make thumbnails.
There are several ways to make thumbnail:
1. Resize the image to the desired size while maintaining the aspect ratio without cropping.
2. Resize the image to the desired height while maintaining the aspect ratio.
3. Resize the image to the desired width while maintaining the aspect ratio.
4. Cut a part of image without change aspect ratio or resize.

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Image tools module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-image-tools. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
* **VirtoCommerce.ImageTools.Thumbnails** -  manually defined ruls to resize images

# Available resources
* Module related service implementations as a <a href="https://www.nuget.org/packages/VirtoCommerce.ImageTools.Data" target="_blank">NuGet package</a>
* API client as a <a href="https://www.nuget.org/packages/VirtoCommerce.ImageToolsModule.Client" target="_blank">NuGet package</a>
* API client documentation http://demo.virtocommerce.com/admin/docs/ui/index#!/Image_tools_module

# License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
Currently the module allows to make thumbnails from any image files.
 
