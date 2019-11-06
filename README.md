


# VirtoCommerce.ImageTools
VirtoCommerce.ImageTools module represents a functionality that helps working with images.

## Key features:
1. Possibility to make different image thumbnails and use them, instead of using the  original images. For example,this functionality can be used in listing or for previews;
1. Possibility to resize Group images into tasks and run them against an asset catalog in the background;
1. Supports all image formats.

![image](https://user-images.githubusercontent.com/20122385/36428926-e483c5e6-1659-11e8-88aa-e4dc2b95b50b.png)

## Generating Thumbnails

In order to generate Thumbnails you need to:

1. Specify what kind of thumbnails you need by creating a Task. Each task should have a name, a path to folder with images and a number of options;
2. Put an image into the platform blob storage;
3. Run the selected Task. Tasks are executed in the background job with hangfire.

The system will generate all thumbnails in the folder according to your options.

The thumbnail url is obtained by adding a suffix to the original image url separated by "_" symbol.

In order to create a task open the Thumbnails menu and click the 'Add' button on the Tasks blade:

![image](https://user-images.githubusercontent.com/20122385/36429401-481d02e2-165b-11e8-8061-c0d9a6c88b31.png)

* **Name**: displays the task name
* **Path to images**: assets path to the folder with images. Clicking the folder button will result in opening the asset search blade where you can select a folder you want to run the task . Each task can refer only to one folder.
* **Settings**: you can select a number of options for this task

In order to create an option you should click on the 'Edit' button located next to the Thumbnail settings:

![image](https://user-images.githubusercontent.com/20122385/36429547-a431bbfe-165b-11e8-9607-291cc511878f.png)

* **Name**: display setting name
* **Thumbnail file name suffix**: The suffix that will be added to the back of the original file name, separated by '_', for example for 'large' and the original file

```
  https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723.jpg will be
```
Thumbnail name is going to be
```
  https://virtocommercedemo1.blob.core.windows.net/catalog/1428965138000_1133723_large.jpg
```

* **Width**: Thumbnail width in pixels
* **Height**: Thumbnail height in pixels
* **Resize method**.

# Resize method options

**1. "FixedSize" method.**

This method resizes the original image to the desired size while maintaining the aspect ratio without cropping. If the aspect ratio of the original image and the thumbnail don't match, the white space will be colored with the color of the background.

**2. "FixedHeight" method.**

Resizes the image to the desired size while maintaining the aspect ratio without cropping. The width of thumbnail will be calculated according to the aspect ratio.

**3. "FixedWidth" method.**

Resizes the image to the desired width while maintaining the aspect ratio.  If the aspect ratio of the original image and the thumbnail don't match , then the white space will be colored with the background color.

**4. "Crop" method.**

This method resizes the image until one of the sides matches the given dimensions while maintaining the aspect ratio. A part of the image will be cut off. The thumbnail will be resized and cropped according to the anchor position. You can specify an anchor position in the o'sption Anchor Position: TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight.

Now you can use the two thumbnails wherever you want to display a smaller version of the original image.
You can do it, just add a needed suffix (grande or medium) to product image url.

## Running the task

You can select multiple task and run them in the task list or run just one task from the task blade.

![image](https://user-images.githubusercontent.com/20122385/36430796-b68279c6-165e-11e8-8e02-ae75e7a11f66.png)

Available modes:

* **Regenerate** - thumbnails will be generated for all images.
* **Process changes** - tries to find changed or added images and generates thumbnails only for them. Also will detect added options. Thumbnails generated for removed options will not be deleted.

# Installation

Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Image tools module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-image-tools. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# [Deprecated] JSON Settings
* **ImageTools.Thumbnails.Parameters** -  manually defined rules to resize images
Use settings to define each thumbnail (width, height, method for generating, etc.)

There are four methods of resizing image. You can specify  its own for any thumbnail.

![image](https://cloud.githubusercontent.com/assets/7059355/16445900/38c49044-3de5-11e6-94d5-bb71de59444c.png)
Settings of four thumbnails with different resize methods.

Here are description of the settings:

**1. "FixedSize" method example:**

To resize the image to the desired size while maintaining the aspect ratio without cropping.
```
  {method: "FixedSize", alias: "grande", width: 800, height:600, background: "#B20000"} 
```
In this case will generate thumbnail with width = 100, height = 75. Image will not be cropped. If aspect ratio of original image and the thumbnail doesn't match then white space will be filled with color of background. Url of thumbnail will receive the suffix by alias value, here is "red".

**2. "FixedHeight" method example:**

Resize the image to the desired height while maintaining the aspect ratio.
```
  {method: "FixedHeight", alias: "medium", height:240}
```
In this case will generate thumbnail with height = 75. Image will not be cropped. Width of thumbnail will be calculated according aspect ratio. Url of thumbnail will receive the suffix by alias value, here is "small".

**3. "FixedWidth" method example:**

Resize the image to the desired width while maintaining the aspect ratio.
```
  {method: "FixedWidth", alias: "large", width: 480}
```
In this case will generate thumbnail with width = 800. Image will not be cropped. Height of thumbnail will be calculated according aspect ratio. Url of thumbnail will receive the suffix by alias value, here is "large".

**4. "Crop" method example:**

Resize the image until one of the sides will match to the given dimensions while maintaining the aspect ratio. Part of the image will be cut off.
```
  {method: "Crop", alias: "compact", width: 160, height:160, anchorposition:"TopLeft"}
```
In this case will generate thumbnail with width = 160 and height = 160. The thumbnail will be resized and cropped from bottom or right. 

You can specify an anchor to change cropping area: TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight.  Url of thumbnail will receive the suffix by alias value, here is "topleft".


# Available resources
* API client documentation http://demo.virtocommerce.com/admin/docs/ui/index#!/Image_tools_module

# License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
