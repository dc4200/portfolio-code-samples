# Containerless Grid System with Subgrid

A framework-agnostic CSS layout abstraction built with CSS Grid (`subgrid`), `@layer`, and CSS Typed Custom Properties (`@property`).

Designed for CMS-driven enterprise marketing sites (e.g., Contentstack) where content editors require full-bleed sections, asymmetric column layouts, and multi-card grids without breaking the DOM hierarchy or wrapping every section in container `<div>` elements.

## Architectural Problem & Context

Traditional layout frameworks (like standard Bootstrap or Tailwind container utilities) rely on deeply nested wrapper divs (`.container > .row > .col`) to constrain text content while using negative margins or separate wrapper blocks for full-width background colors and media breakouts.

In a CMS editorial workflow, this approach causes two major issues:

1. **DOM Inflation & Hydration Overhead:** Constant wrapper elements bloat the HTML payload and slow down initial rendering.

2. **Layout Fragility:** Editors cannot easily toggle a section from centered to left-bleed or full-bleed without altering the parent DOM markup structure.

This system solves those issues by establishing a single top-level 14-track grid on the page container. All direct children act as subgrids, inheriting track alignments directly while allowing independent background bleeds and content locks.

## Technical Highlights

* **CSS Subgrid (`grid-template-columns: subgrid`):** Ensures all rows and multi-column cards align perfectly to the master 12-column content grid, even across deeply nested components.

* **Cascade Layering (`@layer`):** Uses CSS cascade layers (`base`, `layout`, `components`, `demo`, `utilities`) to guarantee layout rules maintain predictable specificity across client and server rendering passes.

* **Typed CSS Variables (`@property`):** Defines `--content-col-count`, `--content-col-marker`, and `--child-grid-origin` as typed CSS properties to ensure mathematical calculations inside `calc()` remain reliable and fall back cleanly.

* **Zero-DOM Bleed Utilities:** Full-bleed, left-bleed, and right-bleed variations are driven entirely by CSS utility classes on the section wrapper rather than DOM structural changes.

## Grid Architecture & Track Layout

The root `.grid` container defines a 14-track layout:

* **12 Central Tracks:** Dedicated to primary page content.

* **2 Outer Tracks:** Serve as fluid responsive gutters that scale dynamically across viewports.

```
[ Outer Gutter ] | [1] [2] [3] [4] [5] [6] [7] [8] [9] [10] [11] [12] | [ Outer Gutter ]
                 ^-- Start Content                      End Content --^
^-- Start Edge                                                              End Edge --^
```

## Usage Examples

### Standard Centered Section (12 Columns)

```html
<div class="grid">
  <div class="start-content end-content">
    <div>
      <h2>Centered Section Title</h2>
      <p>Content automatically aligns to the 12 central tracks.</p>
    </div>
  </div>
</div>
```

### Full-Bleed Section with Content Lock

Locks the text content to the central 12 columns while extending the background styling to the absolute viewport edges:

```html
<div class="grid">
  <div class="start-edge end-edge content-lock" data-full-bleed>
    <div>
      <h2>Full-Width Background Header</h2>
      <p>Background touches screen edges, but text remains aligned with standard grid columns.</p>
    </div>
  </div>
</div>
```

### Asymmetric Breakouts (Left or Right Bleed)

Useful for hero components, split image-text rows, and editorial callouts:

```html
<!-- Right Bleed: Starts at content margin, extends to right edge -->
<div class="start-content end-edge" data-right-bleed>
  <div>...</div>
</div>

<!-- Left Bleed: Starts at left screen edge, ends at content margin -->
<div class="start-edge end-content" data-left-bleed>
  <div>...</div>
</div>
```

### Custom Grid Resolution Opt-Out (`.custom-grid-cols`)

For specialized components requiring non-standard track resolutions (e.g., 16 tracks), the subgrid link can be overridden locally via inline CSS variables:

```html
<div class="start-content end-content" data-custom-grid-cols>
  <div class="custom-grid-cols" style="--content-col-count: 16;">
    <div style="--col-width: 8;">Span 8 of 16 tracks</div>
    <div style="--col-width: 8;">Span 8 of 16 tracks</div>
  </div>
</div>
```

## Browser Support & Fallbacks

* **Primary Target:** Evergreen desktop and mobile browsers supporting CSS Subgrid (`grid-template-columns: subgrid`) and CSS `@layer`.

* **Legacy Fallbacks:** For older browser engines, explicit `grid-template-columns: repeat(...)` fallbacks are declared inline prior to the `subgrid` directive to maintain responsive columnar layouts without breaking page flow.

## Acknowledgments & Architecture Extensions

The foundational subgrid layout concept builds upon modern CSS explorations popularized by <a href="https://www.kevinpowell.co/" rel="noreferrer noopener" target="_blank">Kevin Powell</a>. 

This implementation significantly extends those concepts into a production-ready enterprise architecture by adding:
- **Variable Bleed Utility Combinations:** Decoupling background bleeds from content locks using multi-directional class flags (`.start-edge`, `.end-content`, etc.).
- **Nested & Custom Grid Support:** Full support for nested subgrids, hybrid layouts, and dynamic track opt-outs (`.custom-grid-cols`) via inline CSS variables.
- **Strict Cascade & Type Safety:** Enterprise-grade code organization utilizing CSS Cascade Layers (`@layer`) and Typed Custom Properties (`@property`) to ensure cross-browser stability in server-rendered and client-rendered environments.
