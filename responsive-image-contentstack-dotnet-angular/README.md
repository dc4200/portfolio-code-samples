# Cross-Framework Responsive Image Pipeline & Art Direction Architecture

A unified, high-performance image delivery architecture engineered to integrate seamlessly with the Contentstack Image Delivery API across mixed server-side rendered (SSR) .NET ViewComponents and client-side Angular SPAs.

Designed to eliminate Cumulative Layout Shift (CLS), enforce editorial art direction across multi-density viewports, and automate responsive asset sizing without inflating client bundle size or developer cognitive overhead.

## Architectural Context & Problem Statement

Media-heavy enterprise websites frequently struggle with performance bottlenecks related to image delivery. In multi-framework architectures (such as .NET backend views paired with client-rendered Angular components), managing responsive imagery introduces severe engineering challenges:

1. **Layout Instability (CLS):** Images rendered without explicit aspect ratios or intrinsic dimensions cause jarring content jumps as they download and paint.

2. **Art Direction Overhead:** Content editors need the ability to serve completely different aspect ratios or cropped variants based on screen size (mobile vs. tablet vs. desktop) without requiring custom frontend engineering for every marketing page.

3. **Duplicated Logic Across Stacks:** Maintaining separate image resizing and srcset generation logic in both server-rendered .NET Razor views and client-side JavaScript components leads to drift, broken assets, and inconsistent Core Web Vitals performance.

## Technical Solution

This module provides an end-to-end image optimization pipeline that bridges Contentstack CMS metadata with modern browser primitives (`<picture>`, `<source>`, `srcset`, and `sizes`), supported by custom .NET ViewComponents and a reactive Angular Signals directive.

### Key Architectural Features

* **Contentstack CDN Integration:** Automatically appends query parameters (`?width=`, `&crop=`, `&format=auto`, `&quality=`) to leverage Contentstack's edge image optimization and next-gen format conversion (AVIF/WebP).

* **Server-Side Art Direction (`.NET Razor & ViewComponents`):** Renders semantic `<picture>` elements with media queries (`min-width: 1024px`, `min-width: 768px`) to serve device-specific responsive image overrides configured directly by content editors.

* **Dynamic Aspect Ratio & CLS Mitigation:** Computes CSS aspect ratio variables (`--ratio-base`, `--ratio-tablet`, `--ratio-desktop`) and intrinsic widths dynamically, locking layout space before the image binary arrives.

* **Client-Side Angular Signals Directive (`[imageHelper]`):** A lightweight, standalone Angular directive utilizing modern `effect()` and `input()` APIs to dynamically compute multi-density `srcset` attributes across 15 standard breakpoint viewports.

## Module Structure

### .NET Backend (`/dotnet`)
* `ImageViewComponent.cs`: Core ViewComponent rendering standalone `<img>` tags with automatic format conversion, preset resolution parsing, and dimension fallbacks.
* `ResponsiveImageViewComponent.cs`: Handles modular CMS content blocks (`responsive_image`), mapping content entries and viewport override containers into view models.
* `ResponsiveImageViewModel.cs`: Encapsulates srcset generation logic, high-DPI quality scaling, aspect ratio conversion (Contentstack colon syntax to CSS slash syntax), and intrinsic dimension bounds.
* `ResponsiveImage-default.cshtml`: Razor template outputting semantic `<picture>`, `<source>`, and `<img>` markup with inline CSS aspect ratio variables.
* `ModularBlockViewComponent.cs` & `ModularBlockViewModel.cs`: Reflection-based registry engine that dynamically discovers and routes modular CMS content blocks on startup.

### Angular Frontend (`/ng`)
* `dynamic-attr.directive.ts` (`[imageHelper]`): Standalone Angular directive that observes signal inputs (`ngSrc`, `dimensions`, `format`, `aspectRatio`) and updates DOM attributes reactively using `Renderer2`.
* `image-grid.html`: Production markup demonstrating responsive masonry and grid layouts leveraging the `[imageHelper]` directive alongside lazy-loaded lightbox triggers.

## Usage Examples

### 1. Server-Side Art Direction & Responsive Image (.NET Razor)

In Contentstack-driven .NET views, editors configure base images and screen-specific overrides. The component renders a responsive `<picture>` stack:

```html
<picture>
    <source media="(min-width: 1024px)" srcset="https://images.contentstack.io/.../image.jpg?width=1024&crop=1:1 1024w, ..." sizes="(max-width: 1760px) 50vw, 880px" />
    <source media="(min-width: 768px)" srcset="https://images.contentstack.io/.../image.jpg?width=768&crop=4:3 768w, ..." sizes="(max-width: 1760px) 50vw, 880px" />
    <img
        class="artDirectedImg__image"
        src="https://images.contentstack.io/.../image.jpg?width=880"
        srcset="https://images.contentstack.io/.../image.jpg?width=320 320w, ..."
        sizes="(max-width: 1760px) 50vw, 880px"
        alt="Hero Marketing Banner"
        loading="lazy"
        decoding="async"
        style="--ratio-base: 16/9; --ratio-desktop: 1/1;"
    />
</picture>
```

### 2. Client-Side Reactive Image Optimization (Angular Directive)

In client-rendered SPA contexts, apply the `[imageHelper]` directive to automatically generate robust `srcset` strings across 15 responsive and retina viewports:

```html
<img
  imageHelper
  class="galleryGrid__img"
  [aspectRatio]="item.aspectRatio"
  [ngSrc]="item.imageUrl"
  [alt]="item.name"
  [width]="item.width || 640"
  [height]="item.height || 427"
  sizes="(min-width: 1760px) 587px, (min-width: 901px) 33vw, 50vw"
/>
```

## Engineering Impact

* **Core Web Vitals:** Reduced Cumulative Layout Shift (CLS) to near-zero by reserving precise aspect-ratio boxes before asset download.
* **Bandwidth Conservation:** Automatically delivers next-generation WebP/AVIF formats and scales down image file sizes via dynamic CDN width parameters based on actual client viewport widths.
* **Developer Experience (DX):** Empowered content editors with total art direction freedom in Contentstack while giving developers a clean, unified abstraction across both .NET server views and Angular client applications.