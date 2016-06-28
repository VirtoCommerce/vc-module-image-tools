


# VirtoCommerce.ImageTools
VirtoCommerce.ImageTools module represents functionality for working with images.
Key features:
* Make image thumbnails in different ways
* All image formats supported


# Documentation
Use API to make thumbnails POST /api/image/thumbnails.
There are two parameters:

1. Url of original image
2. isRegenerateAll - if the parameter set as true all thumbnails will replace with new. If false - will generate missed only.

Use settings to define each thumbnail (width, height, method for generating, etc.)

**"FixedSize" method example:**

To resize the image to the desired size while maintaining the aspect ratio without cropping.
{method: "FixedSize", width: 100 and height:75, alias: "red", background: "#B20000"} 
In this case will generate thumbnail with width = 100, height = 75. Image will not be cropped. If aspect ratio of original image and the thumbnail doesn't match then white space will be filled with color of background. Url of thumbnail will receive the suffix by alias value, here is "red".

**"FixedHeight" method example:**

Resize the image to the desired height while maintaining the aspect ratio.
```
{method: "FixedHeight", height:75, alias: "small"}
```
In this case will generate thumbnail with height = 75. Image will not be cropped. Width of thumbnail will be calculated according aspect ratio. Url of thumbnail will receive the suffix by alias value, here is "small".

**"FixedWidth" method example:**

Resize the image to the desired width while maintaining the aspect ratio.
```
{method: "FixedWidth", width: 800, alias: "large"}
```
In this case will generate thumbnail with width = 800. Image will not be cropped. Height of thumbnail will be calculated according aspect ratio. Url of thumbnail will receive the suffix by alias value, here is "large".

**"Cut" method example:**

Cut a part of image without change aspect ratio or resize.
```
{method: "Cut", width: 100, height:75, alias: "topleft", anchorposition:"TopLeft"}
```
In this case will generate thumbnail with width = 100 and height = 75. The thumbnail will be cutted from original image from top left angle. You can specify other place of cutting: TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight.  Url of thumbnail will receive the suffix by alias value, here is "topleft".


# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Image tools module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-image-tools. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
* **VirtoCommerce.ImageTools.Thumbnails** -  manually defined rules to resize images

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
