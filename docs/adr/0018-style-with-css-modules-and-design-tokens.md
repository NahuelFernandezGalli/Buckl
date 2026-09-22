# ADR-0018: Style with CSS Modules and design tokens

- **Status:** Accepted
- **Date:** 2026-09-15
- **Deciders:** NahuelFernandezGalli

## Context

Phase 3 builds the first screens and needs a consistent look on a phone and in the browser, with
light and dark themes, without adding weight to a static site that must stay fast on mobile
networks. The design system is small: a handful of form controls, cards and empty states. As a
portfolio project, Buckl also wants to show design judgement rather than a stock component kit.

## Decision

We style the web app with CSS Modules, which Vite supports natively, and a single set of design
tokens declared as CSS custom properties in `apps/web/src/styles/tokens.css` (colors, type scale,
spacing, radii, shadows). Dark mode is a `prefers-color-scheme` override of the color tokens.
Base components live in `apps/web/src/components`, one folder per component with its module,
and expose accessible markup (labels bound with `useId`, errors linked by `aria-describedby`).
In feature components, colors, type sizes, spacing and radii come from tokens; component-specific
layout dimensions (max widths, photo sizes, touch-target minimums) may be literal values.

## Alternatives considered

- **Tailwind CSS** — fast to write, but utility classes in JSX hide the design system and add a
  build step and a large class vocabulary for six screens.
- **A component library (MUI, Radix Themes, Chakra)** — accessible out of the box, but heavy for
  a PWA and it would make the UI look like the library rather than like Buckl.
- **CSS-in-JS (styled-components, Emotion)** — runtime cost and a dependency, for no gain over
  modules on a static site.

## Consequences

### Positive

- Zero styling dependencies; the bundle stays small.
- Tokens are the single place to tune the look; dark mode is free.
- Component tests assert accessible behavior (labels, descriptions, validity), not appearance.

### Negative

- Every component is hand-made, including keyboard and focus behavior.
- No design-time tooling; consistency relies on the token discipline and code review.
