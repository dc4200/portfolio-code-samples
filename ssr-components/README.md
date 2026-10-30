# Native SSR Disclosure Components & Accessibility Primitives

A zero-JavaScript UI component architecture leveraging modern HTML5 `<details>` and `<summary>` disclosure primitives for complex server-side rendered (SSR) patterns, including mega-menus, accordions, media hero overlays, and mobile navigation drawers.

Designed to eliminate client-side JavaScript hydration overhead, prevent Layout Instability (CLS), and maintain native WCAG AA keyboard accessibility out of the box in server-rendered environments (such as .NET ViewComponents, Angular SSR, or Next.js).

---

## Architectural Context & Problem Statement

Modern marketing sites frequently suffer from **hydration mismatches, layout shifts, and delayed interaction readiness (FID/INP)** because essential interactive components—like navigation menus, video controls, and FAQ accordions—rely on JavaScript state machines (`useState`, `active` CSS class toggles, or JS event listeners).

In server-rendered environments, this creates several critical failure points:
1. **Hydration Mismatches:** HTML rendered by the server doesn't match client-side React/Angular state until full hydration finishes.
2. **Flash of Unstyled/Inoperable Content:** Accordions and menus remain frozen or unclickable on slow 3G/mobile networks while heavy JS bundles load and execute.
3. **Accessibility Overhead:** Custom JS accordions require complex ARIA state synchronization (`aria-expanded`, `aria-hidden`, `tabindex` management) that frequently breaks or desynchronizes during client-side state updates.

---

## Technical Solution

This module replaces heavy JS-driven state with **native browser disclosure primitives** (`<details>` and `<summary>`), augmented with CSS grid layout transitions and modern CSS pseudo-classes (`:has()`, `@container`).

### Key Features & Architectural Trade-offs

- **Zero-JS State Management:** Navigation accordions, FAQ drawers, and video hero overlay toggles operate purely on the native browser C++ rendering thread via `<details open>`.
- **Exclusive Accordions (`name` attribute):** Utilizes the modern HTML spec `name` attribute on `<details>` elements (e.g., `<details name="faq">`) to allow native single-open accordion behavior across browsers without writing event listeners.
- **Pure CSS Layout Locking (`:has()`):** Uses parent selector queries like `body:has(.menu-details[open]) { overflow: hidden; }` to lock body scrolling during mobile navigation overlays without requiring JS DOM mutation.
- **Accessible Out of the Box:** Native `<summary>` elements handle `Space` and `Enter` keypresses, focus management, and screen reader announcements automatically without needing explicit JS `aria-expanded` listeners.

---

## Module Structure

- `foundation.html`: Production-grade markup showing native `<details>` mega-menus, video hero controls, sticky header drawers, and FAQ accordions.
- `foundation.css`: Pure CSS styling utilizing `@layer`, CSS variables, and layout controls.
- `demo-grid.css`: Utility dependency providing the responsive subgrid structure.

---

## Usage Examples

### 1. Zero-JS Exclusive Accordion Group
By adding matching `name` attributes, the browser automatically closes other open items in the group when a new summary is clicked:

```html
<details name="faq" class="accordion-details">
  <summary class="accordion-summary">
    <span class="faq-question">What is a custom home?</span>
    <svg class="accordion-summary-icon" aria-hidden="true">...</svg>
  </summary>
  <p class="faq-answer">
    Custom homes are built and designed to a homeowner's unique specifications.
  </p>
</details>

<details name="faq" class="accordion-details">
  <summary class="accordion-summary">
    <span class="faq-question">Do you have models to tour?</span>
    <svg class="accordion-summary-icon" aria-hidden="true">...</svg>
  </summary>
  <p class="faq-answer">
    Yes, our model homes are open to the public daily.
  </p>
</details>
```

### 2. Full-Screen Mega Menu Overlay with Pure CSS Body Scroll Lock
Eliminates JS event listeners on menu open/close by using CSS `:has()`:

```css
/* Lock background scrolling when any mega menu drawer is expanded */
body:has(.menu-details[open]) {
  overflow: hidden;
}

/* Toggle open/close SVG icon states purely in CSS */
.menu-details[open] {
  .icon-open svg { display: none; }
  .icon-close svg { display: block; }
}
```

---

## Engineering Impact

- **Performance:** Reduced Total Blocking Time (TBT) and First Input Delay (FID) to zero for interactive UI controls across high-volume landing pages.
- **Developer Experience:** Established predictable, standardized markup patterns that allowed front-end developers across the team to build new CMS components quickly without rewriting UI state logic or ARIA code.
- **Maintainability:** Reduced component code footprint by over 60% by stripping out framework state hooks and lifecycle handlers.