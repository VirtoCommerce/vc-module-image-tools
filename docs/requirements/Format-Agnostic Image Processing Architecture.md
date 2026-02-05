# Format-Agnostic Image Processing Architecture

## Author: Oleg Zhuk (CTO Review)
## Date: 2026-02-05
## Module: VirtoCommerce.ImageTools

---

## Executive Summary

This document proposes an architecture to make the ImageTools module format-agnostic and extensible to support new image formats like SVG, while **minimizing breaking changes** to existing code and custom implementations.

**Key Challenge:** Current interfaces use `Image<Rgba32>` (raster-only type from SixLabors.ImageSharp), making it impossible to support vector formats like SVG without architectural changes.

**Recommended Approach:** Hybrid Handler + Routing Pattern that keeps existing interfaces intact while adding a new abstraction layer for format-specific processing.

---

## Current Architecture Analysis

### Existing Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `IImageService` | Core/Services | Load/save images using `Image<Rgba32>` |
| `IImageResizer` | Core/Services | Resize operations on `Image<Rgba32>` |
| `IThumbnailGenerator` | Core/ThumbnailGeneration | Generate thumbnails from source to destination |
| `IThumbnailGenerationProcessor` | Core/ThumbnailGeneration | Orchestrates batch thumbnail processing |
| `DefaultImageService` | Data/Services | ImageSharp-based implementation |
| `DefaultImageResizer` | Data/Services | ImageSharp resize operations |
| `DefaultThumbnailGenerator` | Data/ThumbnailGeneration | Coordinates load → resize → save |
| `ThumbnailGenerationProcessor` | Data/ThumbnailGeneration | Main processing loop |

### Current Format Support
- **JPEG** (raster)
- **PNG** (raster)
- **WebP** (raster)

### Problem: Tight Coupling to Raster Types

```csharp
// Current interface - tightly coupled to Image<Rgba32>
public interface IImageService
{
    Task<Image<Rgba32>> LoadImageAsync(string imageUrl);
    Task SaveImageAsync(string imageUrl, Image<Rgba32> image, IImageFormat format, JpegQuality jpegQuality);
}
```

SVG is a **vector format** that cannot be represented as `Image<Rgba32>`. Changing these interfaces would break all existing implementations.

---

## Proposed Architecture: Hybrid Handler + Routing Pattern

### Design Principles

1. **Zero breaking changes** for existing raster format handling
2. **Format detection at entry point** - route to appropriate handler early
3. **New abstractions only for new capabilities** - SVG-specific interfaces
4. **Composition over inheritance** - wrap existing services
5. **Easy extensibility** - add new formats via DI registration

### Architecture Diagram

```
                                    ┌─────────────────────────┐
                                    │  ThumbnailGeneration    │
                                    │      Processor          │
                                    └───────────┬─────────────┘
                                                │
                                    ┌───────────▼─────────────┐
                                    │  IThumbnailHandler      │
                                    │      Factory            │
                                    └───────────┬─────────────┘
                                                │
                         ┌──────────────────────┼──────────────────────┐
                         │                      │                      │
              ┌──────────▼──────────┐ ┌────────▼────────┐ ┌───────────▼───────────┐
              │ RasterThumbnail     │ │ SvgThumbnail    │ │ Future Format         │
              │ Handler             │ │ Handler         │ │ Handler               │
              └──────────┬──────────┘ └────────┬────────┘ └───────────────────────┘
                         │                      │
              ┌──────────▼──────────┐ ┌────────▼────────┐
              │ IThumbnailGenerator │ │ ISvgService     │
              │ (existing)          │ │ ISvgResizer     │
              └─────────────────────┘ └─────────────────┘
```

---

## New Interfaces and Classes

### Phase 1: Core Abstractions

#### 1.1 Image Format Type Enum
**File:** `Core/Services/ImageFormatType.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public enum ImageFormatType
    {
        Raster,   // JPEG, PNG, WebP, GIF, BMP, TIFF
        Vector    // SVG, SVGZ
    }
}
```

#### 1.2 Format Detector Interface
**File:** `Core/Services/IImageFormatDetector.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IImageFormatDetector
    {
        ImageFormatType DetectFormatType(string imageUrl);
        Task<ImageFormatType> DetectFormatTypeAsync(Stream stream);
        Task<bool> IsFormatSupportedAsync(string imageUrl);
    }
}
```

#### 1.3 Format Thumbnail Handler Interface
**File:** `Core/ThumbnailGeneration/IFormatThumbnailHandler.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IFormatThumbnailHandler
    {
        ImageFormatType SupportedFormatType { get; }
        int Priority { get; }
        Task<bool> CanHandleAsync(string imageUrl);
        Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(
            string source,
            string destination,
            IList<ThumbnailOption> options,
            ICancellationToken token);
    }
}
```

#### 1.4 Handler Factory Interface
**File:** `Core/ThumbnailGeneration/IThumbnailHandlerFactory.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailHandlerFactory
    {
        Task<IFormatThumbnailHandler> GetHandlerAsync(string imageUrl);
        void RegisterHandler(IFormatThumbnailHandler handler);
    }
}
```

### Phase 2: SVG-Specific Interfaces

#### 2.1 SVG Service Interface
**File:** `Core/Services/ISvgService.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface ISvgService
    {
        Task<string> LoadSvgAsync(string svgUrl);
        Task SaveSvgAsync(string svgUrl, string svgContent);
        Task<SvgDimensions> GetDimensionsAsync(string svgUrl);
    }

    public class SvgDimensions
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string ViewBox { get; set; }
    }
}
```

#### 2.2 SVG Resizer Interface
**File:** `Core/Services/ISvgResizer.cs`

```csharp
namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface ISvgResizer
    {
        string Resize(string svgContent, int? width, int? height, ResizeMethod method);
        string SetDimensions(string svgContent, int width, int height);
        string EnsureViewBox(string svgContent);
    }
}
```

### Phase 3: Implementations

#### 3.1 Default Format Detector
**File:** `Data/Services/DefaultImageFormatDetector.cs`

- Detects format by file extension (.svg, .svgz = Vector, others = Raster)
- Validates against allowed formats configuration

#### 3.2 Raster Thumbnail Handler
**File:** `Data/ThumbnailGeneration/RasterThumbnailHandler.cs`

- Wraps existing `IThumbnailGenerator`
- Zero changes to existing raster processing logic
- Handles JPEG, PNG, WebP (and any future raster formats)

#### 3.3 SVG Thumbnail Handler
**File:** `Data/ThumbnailGeneration/SvgThumbnailHandler.cs`

- Uses `ISvgService` and `ISvgResizer`
- SVG "thumbnails" remain as SVG files with modified dimensions
- Preserves viewBox for proper scaling

#### 3.4 Default SVG Service
**File:** `Data/Services/DefaultSvgService.cs`

- Loads/saves SVG content from blob storage as text
- Parses dimensions from SVG attributes

#### 3.5 Default SVG Resizer
**File:** `Data/Services/DefaultSvgResizer.cs`

- Modifies SVG width/height attributes
- Preserves viewBox for scalability
- Handles all ResizeMethod enum values

#### 3.6 Default Thumbnail Handler Factory
**File:** `Data/ThumbnailGeneration/DefaultThumbnailHandlerFactory.cs`

- Receives all handlers via DI
- Selects handler by format detection and priority

---

## Files to Modify

### 1. ThumbnailGenerationProcessor.cs
**Location:** `Data/ThumbnailGeneration/ThumbnailGenerationProcessor.cs`

**Change:** Add `IThumbnailHandlerFactory` dependency and use it for routing.

```csharp
// Before (line 63):
var result = await _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);

// After:
var handler = await _handlerFactory.GetHandlerAsync(fileChange.Url);
var result = handler != null
    ? await handler.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token)
    : await _generator.GenerateThumbnailsAsync(fileChange.Url, task.WorkPath, task.ThumbnailOptions, token);
```

### 2. ModuleConstants.cs
**Location:** `Core/ModuleConstants.cs`

**Change:** Add "Svg" to AllowedImageFormats (line 70-75).

```csharp
AllowedValues =
[
    JpegFormat.Instance.Name,
    PngFormat.Instance.Name,
    WebpFormat.Instance.Name,
    "Svg",  // NEW
],
```

### 3. Module.cs
**Location:** `Web/Module.cs`

**Change:** Add DI registrations for new services.

```csharp
// NEW: Format detection
serviceCollection.AddTransient<IImageFormatDetector, DefaultImageFormatDetector>();

// NEW: SVG support
serviceCollection.AddTransient<ISvgService, DefaultSvgService>();
serviceCollection.AddTransient<ISvgResizer, DefaultSvgResizer>();

// NEW: Format handlers (registered as collection)
serviceCollection.AddTransient<IFormatThumbnailHandler, RasterThumbnailHandler>();
serviceCollection.AddTransient<IFormatThumbnailHandler, SvgThumbnailHandler>();

// NEW: Handler factory
serviceCollection.AddTransient<IThumbnailHandlerFactory, DefaultThumbnailHandlerFactory>();
```

### 4. BlobImagesChangesProvider.cs
**Location:** `Data/ThumbnailGeneration/BlobImagesChangesProvider.cs`

**Change:** Update `IsSupportedImage` method to use `IImageFormatDetector` instead of hardcoded extensions.

---

## Backward Compatibility Matrix

| Component | Breaking Change? | Notes |
|-----------|-----------------|-------|
| `IImageService` | **No** | Unchanged |
| `IImageResizer` | **No** | Unchanged |
| `IThumbnailGenerator` | **No** | Unchanged |
| `DefaultImageService` | **No** | Unchanged |
| `DefaultImageResizer` | **No** | Unchanged |
| `DefaultThumbnailGenerator` | **No** | Unchanged |
| `ThumbnailGenerationProcessor` | **Minor** | New constructor parameter, fallback for compatibility |
| Custom implementations | **No** | Continue to work, just won't handle SVG |

**Key Guarantee:** All existing custom implementations of `IImageService`, `IImageResizer`, and `IThumbnailGenerator` will continue to work without modifications.

---

## SVG-Specific Considerations

1. **No Rasterization:** SVG thumbnails remain as SVG files (not converted to PNG/JPEG)
2. **ViewBox Preservation:** Always preserve/ensure viewBox for proper browser scaling
3. **Crop Handling:** For SVG, "crop" modifies viewBox rather than pixel cropping
4. **Background Color:** May require adding a `<rect>` element (differs from raster)
5. **File Extension:** SVG thumbnails keep `.svg` extension
6. **SVGZ Support:** Consider supporting gzip-compressed SVG files

---

## Implementation Plan

### Step 1: Create Core Interfaces (Core Layer)
- [ ] Create `ImageFormatType.cs` enum
- [ ] Create `IImageFormatDetector.cs` interface
- [ ] Create `IFormatThumbnailHandler.cs` interface
- [ ] Create `IThumbnailHandlerFactory.cs` interface
- [ ] Create `ISvgService.cs` interface
- [ ] Create `ISvgResizer.cs` interface
- [ ] Create `SvgDimensions.cs` model

### Step 2: Implement Format Detection (Data Layer)
- [ ] Implement `DefaultImageFormatDetector.cs`

### Step 3: Implement Raster Handler Wrapper (Data Layer)
- [ ] Implement `RasterThumbnailHandler.cs` (wraps existing logic)

### Step 4: Implement SVG Support (Data Layer)
- [ ] Implement `DefaultSvgService.cs`
- [ ] Implement `DefaultSvgResizer.cs`
- [ ] Implement `SvgThumbnailHandler.cs`

### Step 5: Implement Handler Factory (Data Layer)
- [ ] Implement `DefaultThumbnailHandlerFactory.cs`

### Step 6: Integrate with Processor
- [ ] Modify `ThumbnailGenerationProcessor.cs` to use factory
- [ ] Modify `BlobImagesChangesProvider.cs` to support SVG detection

### Step 7: Configuration Updates
- [ ] Update `ModuleConstants.cs` with SVG format
- [ ] Update `Module.cs` with DI registrations

### Step 8: Testing
- [ ] Unit tests for new components
- [ ] Integration tests for SVG thumbnail generation
- [ ] Regression tests for existing raster functionality

---

## Extension Guide for Future Formats

To add support for a new format (e.g., PDF, HEIC):

1. **Create format-specific interfaces** (if needed):
   ```csharp
   public interface IPdfService { ... }
   public interface IPdfResizer { ... }
   ```

2. **Implement format handler**:
   ```csharp
   public class PdfThumbnailHandler : IFormatThumbnailHandler
   {
       public ImageFormatType SupportedFormatType => ImageFormatType.Raster; // or Vector
       public int Priority => 10; // Higher = preferred

       public Task<bool> CanHandleAsync(string imageUrl)
       {
           return Task.FromResult(imageUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
       }
       // ...
   }
   ```

3. **Register in DI**:
   ```csharp
   serviceCollection.AddTransient<IFormatThumbnailHandler, PdfThumbnailHandler>();
   ```

4. **Update allowed formats** in `ModuleConstants.cs`

---

## Verification Plan

### Unit Tests
- Test `DefaultImageFormatDetector` with various file extensions
- Test `DefaultSvgResizer` resize methods
- Test `DefaultThumbnailHandlerFactory` handler selection

### Integration Tests
1. Upload SVG file → verify thumbnail generated with correct dimensions
2. Upload JPEG file → verify existing behavior unchanged
3. Test resize methods: FixedSize, FixedWidth, FixedHeight, Crop
4. Test viewBox preservation in SVG output

### Manual Testing
1. Upload SVG to blob storage via VirtoCommerce Admin
2. Configure thumbnail task with SVG in scope
3. Run thumbnail generation job
4. Verify SVG thumbnail created with correct dimensions
5. Verify existing JPEG/PNG/WebP thumbnails still work

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking existing implementations | Low | High | Fallback to `_generator` in processor |
| SVG parsing edge cases | Medium | Low | Robust error handling, skip invalid SVGs |
| Performance impact | Low | Low | SVG processing is fast (text manipulation) |
| Security (SVG XSS) | Medium | High | Sanitize SVG content before saving |

---

## Conclusion

This architecture provides a clean, extensible solution for format-agnostic image processing while maintaining full backward compatibility with existing code. The handler-based approach allows adding new formats through simple DI registration without modifying core interfaces.

**Key Benefits:**
- Zero breaking changes for existing implementations
- Clean separation of concerns (format detection → routing → processing)
- Easy extensibility for future formats
- SVG support without rasterization (preserves vector scalability)
