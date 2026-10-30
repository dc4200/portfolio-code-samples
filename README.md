# Enterprise Frontend Architecture & Code Samples

A collection of core architectural patterns, modular UI scaffolding, and performance pipelines developed during the initial build-out for <a href="https://www.schumacherhomes.com" rel="noreferrer noopener" target="_blank">Schumacher Homes</a>.

These samples demonstrate a browser-first engineering philosophy, focusing on native web standards, framework-agnostic design systems, zero-JS performance optimizations, and developer ergonomics that scale across multi-framework teams.

---

## Architecture Modules

### 1. [Containerless Grid System & CSS Subgrid Abstraction](./containerless-grid-system-with-selective-bleed)
* **Tech:** CSS Grid Level 2 (`subgrid`), `@layer`, `@property`, CSS Custom Properties.
* **Overview:** A layout engine allowing content editors total design flexibility for marketing breakouts without wrapper `div` inflation or heavy layout frameworks.

### 2. [SSR Component Architecture & Zero-JS Native Primitives](./ssr-components)
* **Tech:** Semantic HTML (`<details>`, `<summary>`), pure CSS state toggling.
* **Overview:** High-performance server-rendered UI components (mega-menus, accordions, overlays) achieving zero-hydration layout shift and instant initial loads.

### 3. [Cross-Framework Responsive Image Pipeline](./responsive-image-contentstack-dotnet-angular)
* **Tech:** ASP.NET Core (`ImageViewComponent`, `ModularBlockViewComponent`), Contentstack Image API, Angular Signals (`ImageHelperDirective`).
* **Overview:** A unified asset delivery pipeline that automates device-specific sizing, enforces aspect-ratio reservation to mitigate CLS, and serves next-gen formats (`avif`/`webp`) across SSR and SPA contexts.

### 4. [Vector Icon Asset Management](./icon-sprite-sheet)
* **Tech:** SVG `<symbol>`, `<use>`, CSS `currentColor`.
* **Overview:** A zero-dependency SVG sprite system keeping network requests to a single cached asset while ensuring icons scale crisply and inherit theme states natively.

---

## Production Context
* **Live Site:** <a href="https://www.schumacherhomes.com" rel="noreferrer noopener" target="_blank">Schumacher Homes</a>
* **Tech Stack:** Headless Contentstack CMS, .NET View Components (Server-Side Rendered), and Angular.
