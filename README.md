# Virto Commerce Image Tools Module

[![CI status](https://github.com/VirtoCommerce/vc-module-image-tools/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-image-tools/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-image-tools&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-image-tools)

## Overview

The Image Tools Module provides comprehensive image processing capabilities for the Virto Commerce platform. It enables automatic thumbnail generation for product images, category banners, and other visual assets stored in blob storage.

The module features a **format-agnostic architecture** that supports both raster formats (JPEG, PNG, WebP, BMP) and vector formats (SVG) with native XML manipulation, ensuring scalability without quality loss. Resize geometry and the public interfaces are owned by the module; decoding, scaling and encoding of raster images use [SkiaSharp](https://github.com/mono/SkiaSharp) (MIT).

![ImageTools Main Screen](docs/media/main-page.png)

## Key Features

- **Multi-Format Support**: Process raster images (JPEG, PNG, WebP, BMP) and vector graphics (SVG)
- **Flexible Resize Methods**:
  - `FixedSize` - Proportional fit within specified dimensions with optional padding
  - `FixedWidth` - Maintain aspect ratio, resize to target width
  - `FixedHeight` - Maintain aspect ratio, resize to target height
  - `Crop` - Crop to exact dimensions using configurable anchor positions
- **Background Processing**: Batch thumbnail generation via Hangfire jobs with progress tracking
- **Event-Driven Generation**: Automatic thumbnail creation on blob upload events
- **Incremental Processing**: Only process new or modified images since last run
- **Extensible Architecture**: Plugin-based format handlers for easy addition of new formats

## Configuration

### Application Settings

Configure the module in `appsettings.json`:

```json
{
  "ImageTools": {
    "Thumbnails": {
      "EnableImageProcessJob": false,
      "EventBasedThumbnailGeneration": true,
      "ImageProcessJobCronExpression": "0 0 * * *",
      "ProcessBatchSize": 50
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableImageProcessJob` | Boolean | `false` | Enable scheduled background job for thumbnail processing |
| `EventBasedThumbnailGeneration` | Boolean | `true` | Generate thumbnails automatically when images are uploaded |
| `ImageProcessJobCronExpression` | String | `0 0 * * *` | Cron expression for scheduled job (daily at midnight) |
| `ProcessBatchSize` | Integer | `50` | Number of images to process before reporting progress |

### Allowed Image Formats

The **Allowed image formats** setting (`ImageTools.Thumbnails.AllowedImageFormats`) lists the formats thumbnails are generated for. Its default values are `JPEG`, `PNG`, `Webp` and `SVG`. A file is processed when its extension matches one of the values (case-insensitive) and a handler supports it. To process another supported format, add its extension in the Platform Settings UI, for example `bmp`.

### Supported Image Formats

| Format | Extensions | Thumbnail output |
|--------|------------|------------------|
| JPEG | `.jpg`, `.jpeg`, `.jfif` | JPEG at the option's JPEG quality (Low 50, Medium 72, High 85, VeryHigh 92) |
| PNG | `.png`, `.apng` | PNG, 8 bits per channel, with alpha when the source has it |
| WebP | `.webp` | Lossless when the source is lossless, otherwise lossy at quality 75 |
| BMP | `.bmp`, `.bm`, `.dip` | 24-bit BMP |
| SVG | `.svg` | SVG, resized by rewriting its dimensions and viewBox |

A thumbnail is always written in the same format as its source.

### Known Limitations

| Image type | Behavior |
|------------|----------|
| GIF, TIFF, TGA, PBM/PGM/PPM, QOI | Not supported: no thumbnails are generated for these files |
| Animated WebP, APNG | The thumbnail is a static image of the first frame |
| BMP | Thumbnails are written as 24-bit BMP; transparency is not kept |
| Images without transparency (JPEG, RGB PNG, lossy WebP) | Padding without a background color is black, and a background color's transparency is ignored |
| Embedded color profiles, EXIF orientation | Not applied: pixels are used as stored |

Supported formats are listed in `RasterImageFormat.Supported`. Unsupported raster files are skipped: `RasterThumbnailHandler` does not accept them, and `ImageService.LoadImageAsync` returns `null` and logs an error if one reaches it directly.

### Native Libraries

SkiaSharp uses a native library. The module package includes it for these runtimes:

| Runtime | Typical host |
|---------|--------------|
| `linux-x64` | Virto Cloud and Linux containers (glibc, e.g. Debian/Ubuntu images) |
| `linux-arm64` | ARM64 Linux servers |
| `linux-musl-x64` | Alpine Linux containers |
| `osx` | macOS, Intel and Apple Silicon |
| `win-x64` | Windows |

Native debug symbols are not included. To ship another runtime, add its identifier to the `SkiaSharpNativeRuntimes` property in `VirtoCommerce.ImageToolsModule.Web.csproj` and rebuild the package. The Linux library has no system dependencies (`SkiaSharp.NativeAssets.Linux.NoDependencies`), so no `fontconfig` or other packages are needed in the container.

## Architecture

The module implements a **Handler-Based Architecture** for format-agnostic image processing:

```
┌─────────────────────────────────────────────────────────────────┐
│                  ThumbnailGenerationProcessor                    │
│                    (Orchestrates batch jobs)                     │
└─────────────────────────────┬───────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                   IThumbnailHandlerFactory                       │
│              (Routes to format-specific handler)                 │
└─────────────────────────────┬───────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
┌─────────▼─────────┐ ┌───────▼───────┐ ┌────────▼────────┐
│ RasterThumbnail   │ │ SvgThumbnail  │ │ Custom Format   │
│ Handler           │ │ Handler       │ │ Handler         │
│ (JPEG, PNG, WebP, │ │ (SVG)         │ │ (Extensible)    │
│  BMP)             │ │               │ │                 │
└─────────┬─────────┘ └───────┬───────┘ └─────────────────┘
          │                   │
┌─────────▼─────────┐ ┌───────▼───────┐
│ IThumbnailGen     │ │ ISvgService   │
│ IImageService     │ │ ISvgResizer   │
│ IImageResizer     │ │               │
└─────────┬─────────┘ └───────────────┘
          │
┌─────────▼──────────────────────────────────────┐
│ Module-owned imaging (Core.Imaging)            │
│ RasterImage, ImageColor, RasterImageFormat,    │
│ ResizePlanner (fit / pad / crop geometry)      │
├────────────────────────────────────────────────┤
│ SkiaSharp back end (Data.Imaging)              │
│ SkiaImageCodec (decode / encode), Render,      │
│ BmpEncoder, WebpInspector                      │
└────────────────────────────────────────────────┘
```

### Raster Imaging

The module owns its imaging API and logic; [SkiaSharp](https://github.com/mono/SkiaSharp) only handles pixels and file formats:

- **`VirtoCommerce.ImageToolsModule.Core`** has no imaging library dependency. `IImageService` and `IImageResizer` use the module's own types: `RasterImage`, `ImageColor` and `RasterImageFormat`.
- **`ResizePlanner`** computes every resize method as a canvas size plus the rectangle the source is drawn into: fit, pad (centered), crop (with one of nine anchors) and percent scale. It uses single-precision math with half-to-even rounding, so thumbnail sizes match earlier versions of the module.
- **`ImageResizer`** turns a plan into pixels with one method, `Render`: it fills the canvas with the background color and draws the scaled source. Shrinking uses mipmapped linear filtering, which averages fine detail instead of aliasing it. Enlarging uses Catmull-Rom.
- **`ImageService`** reads and writes blobs; `SkiaImageCodec` decodes the first frame and encodes JPEG, PNG and WebP. BMP is written by the module's `BmpEncoder`, because SkiaSharp cannot encode BMP.

### Extension Points

To add support for a new format:

1. Implement `IFormatThumbnailHandler`:
```csharp
public class PdfThumbnailHandler : IFormatThumbnailHandler
{
    public int Priority => 10;

    public Task<bool> CanHandleAsync(string imageUrl)
        => Task.FromResult(imageUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

    public Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
        string source, string destination, IList<ThumbnailOption> options, CancellationToken token)
    {
        // Custom PDF thumbnail generation logic
    }
}
```

2. Register in DI:
```csharp
serviceCollection.AddTransient<IFormatThumbnailHandler, PdfThumbnailHandler>();
```

To change how raster thumbnails are drawn, override `ImageResizer` and register it in place of the default:

```csharp
public class SharperImageResizer : ImageResizer
{
    // Use Catmull-Rom for shrinking too, instead of mipmapped linear filtering
    protected override SKSamplingOptions GetSampling(ImageSize source, ImageRectangle destination)
        => new(SKCubicResampler.CatmullRom);
}

serviceCollection.AddTransient<IImageResizer, SharperImageResizer>();
```

Override `Render` to change how every resize method draws. To use another imaging library, implement `IImageService` and `IImageResizer` with your own `RasterImage` subclass, and reuse `ResizePlanner` for the geometry.

## Components

### Core Interfaces

| Interface | Description |
|-----------|-------------|
| `IImageService` | Load/save raster images from blob storage |
| `IImageResizer` | Raster image resize operations |
| `ISvgService` | Load/save SVG content from blob storage |
| `ISvgResizer` | SVG dimension manipulation |
| `IAllowedImageFormatsService` | Check if image format is in allowed formats list |
| `IFormatThumbnailHandler` | Format-specific thumbnail generation |
| `IThumbnailHandlerFactory` | Route to appropriate format handler |
| `IThumbnailGenerator` | Core thumbnail generation (obsolete - use handlers) |
| `IThumbnailGenerationProcessor` | Batch processing with progress tracking |
| `IImagesChangesProvider` | Detect new/modified images in blob storage |

### Services

| Service | Description |
|---------|-------------|
| `ImageService` | Raster image I/O (SkiaSharp decode/encode, own BMP encoder) |
| `ImageResizer` | Resize methods: module-owned `ResizePlanner` geometry, SkiaSharp rendering |
| `SvgService` | SVG blob storage operations |
| `SvgResizer` | SVG XML manipulation for resizing |
| `AllowedImageFormatsService` | Settings-based format validation |
| `ThumbnailHandlerFactory` | Priority-based handler selection |
| `RasterThumbnailHandler` | Raster format processing via IThumbnailGenerator |
| `SvgThumbnailHandler` | Vector format processing via ISvgService |

### Imaging Types

| Type | Project | Description |
|------|---------|-------------|
| `RasterImage` | Core | Decoded image: size, alpha, source format, lossless flag |
| `RasterImageFormat` | Core | Supported raster formats, their extensions and setting names |
| `ImageColor` | Core | RGBA color; `Parse` accepts `RGB`, `RGBA`, `RRGGBB`, `RRGGBBAA`, with or without `#` |
| `ResizePlanner` | Core | Resize geometry (`FitWithin`, `Pad`, `Crop`, `Scale`) returning a `ResizePlan` |
| `SkiaRasterImage` | Data | `RasterImage` backed by a SkiaSharp `SKBitmap` |
| `SkiaImageCodec` | Data | Decoding and encoding with SkiaSharp |
| `BmpEncoder` | Data | 24-bit BMP writer |
| `WebpInspector` | Data | Detects lossless WebP sources |

### Background Jobs

| Job | Description |
|-----|-------------|
| `ThumbnailProcessJob` | Hangfire job for scheduled/manual thumbnail generation |

### Domain Models

| Model | Description |
|-------|-------------|
| `ThumbnailTask` | Configuration for a thumbnail generation task (work path, options) |
| `ThumbnailOption` | Single thumbnail variant (size, method, quality, suffix) |
| `SvgDimensions` | SVG width, height, and viewBox attributes |

## Upgrading from ImageSharp-Based Versions

Earlier versions used SixLabors.ImageSharp. If your code extends the module's raster services:

- `IImageService` and `IImageResizer` take and return `RasterImage` instead of `Image<Rgba32>`, use `ImageColor` instead of `Rgba32`, and use `RasterImageFormat` instead of `IImageFormat`.
- The protected `ImageResizer.Transform` method is replaced by `Render(RasterImage, ResizePlan, ImageColor)`.
- `ThumbnailGenerator.GenerateThumbnail` takes and returns `RasterImage`.
- `VirtoCommerce.ImageToolsModule.Core` no longer brings in ImageSharp. Reference it directly if your own code still uses it.
- Thumbnails are no longer generated for GIF, TIFF, TGA, PBM/PGM/PPM and QOI; see [Known Limitations](#known-limitations).

## Testing

```bash
dotnet test tests/VirtoCommerce.ImageToolsModule.Tests
```

The raster tests run the real thumbnail pipeline end to end over an in-memory blob store. Sample images for every supported source format and variant are in `tests/VirtoCommerce.ImageToolsModule.Tests/media/formats` (stored as binary in git). Generated thumbnails are checked by file signature, size and pixel colors. Unsupported formats are pinned by `KnownLimitationTests`; their old expectations are kept as skipped tests with a "Known limitation" reason.

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

### Third-Party Components

* [SkiaSharp](https://github.com/mono/SkiaSharp) is licensed under the [MIT License](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md).
