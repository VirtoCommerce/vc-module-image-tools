# Virto Commerce Image Tools Module

[![CI status](https://github.com/VirtoCommerce/vc-module-image-tools/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-image-tools/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools)

The Image Tools (or Thumbnails) module provides functionality for working with images. It allows generating thumbnails that can be used for uploading instead of the original images.

## Key features

* Generate different image thumbnails and use them instead of the original images. This functionality is useful for listings or previews.
* Resize group images into tasks and run them against an asset catalog in the background.
* Utilize the [Sixlabors.ImageSharp library](https://docs.sixlabors.com/articles/imagesharp/index.html) for image processing.
* Default image formats supported:
   - Jpeg
   - Png
   - WebP
* Additional image formats supported (can be enabled in thumbnail settings):
   - Jpeg
   - Bmp
   - WebP
   - Gif
   - Pbm
   - Png
   - Tiff
   - Tga

![ImageTools Main Screen](docs/media/main-page.png)

## Documentation

* [Image Tools module user documentation](https://docs.virtocommerce.org/platform/user-guide/thumbnails/overview/)
* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.ImageTools)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-image-tools)


## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-image-tools/releases/latest)

## License

Copyright (c) Virto Solutions LTD. All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
