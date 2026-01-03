# Story 1.5: Theme System & Accessibility Baseline

Status: ready-for-dev

## Story

As an **admin**,
I want **consistent theming and accessible focus states**,
so that **the UI is readable and keyboard-navigable**.

## Acceptance Criteria

1. **Given** the application loads
   **When** I view any page
   **Then** the DaisyUI dark theme is applied consistently

2. **Given** I use keyboard navigation
   **When** I tab through interactive elements
   **Then** visible focus rings appear on buttons, inputs, and links

3. **And** form fields have proper labels and error states

4. **And** contrast ratios meet basic readability standards

## Tasks / Subtasks

- [ ] Task 1: Configure DaisyUI dark theme (AC: #1)
  - [ ] Set dark theme as default in tailwind.config.js
  - [ ] Apply theme attribute to html element
  - [ ] Create ThemeService if theme switching needed later
  - [ ] Verify theme consistency across all existing components
- [ ] Task 2: Create global focus ring styles (AC: #2)
  - [ ] Define focus ring utility classes in styles.css
  - [ ] Apply focus:ring styles to buttons
  - [ ] Apply focus:ring styles to inputs
  - [ ] Apply focus:ring styles to links
  - [ ] Use visible, high-contrast focus rings
- [ ] Task 3: Audit and fix form accessibility (AC: #3)
  - [ ] Review login form labels
  - [ ] Add aria-labels where needed
  - [ ] Ensure error messages are associated with fields
  - [ ] Add aria-invalid for invalid states
- [ ] Task 4: Check contrast ratios (AC: #4)
  - [ ] Verify text contrast on dark backgrounds
  - [ ] Verify button text contrast
  - [ ] Adjust colors if needed for readability
  - [ ] Document color palette decisions
- [ ] Task 5: Create shared form components (AC: #3)
  - [ ] Create form-field wrapper component
  - [ ] Create input component with label and error
  - [ ] Ensure consistent styling and accessibility
- [ ] Task 6: Document accessibility patterns
  - [ ] Add accessibility notes to project context
  - [ ] Create component patterns for future development

## Dev Notes

### Architecture Compliance

- **ThemeService in `core/services/`** - If needed for theme switching
- **Shared components in `shared/components/`** - Form field wrappers
- **Global styles in `styles.css`** - Focus rings, theme variables
- **OnPush change detection** for all components

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Tailwind CSS | Latest | Utility classes |
| DaisyUI | Latest | Component library, dark theme |

### Critical Rules

- **Dark theme as default** - DaisyUI's dark theme
- **Visible focus rings** - High contrast, obvious when tabbing
- **No `any` type** in TypeScript
- **OnPush change detection** for all components

### Theme Configuration

**tailwind.config.js**:
```javascript
module.exports = {
  // ...
  daisyui: {
    themes: ["dark"],
  },
}
```

**index.html**:
```html
<html data-theme="dark">
```

### Focus Ring Styles

**styles.css**:
```css
/* Global focus ring utilities */
.focus-visible-ring {
  @apply focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 focus-visible:ring-offset-base-100;
}

/* Or using Tailwind directly on elements */
button, 
a, 
input, 
select, 
textarea {
  @apply focus:outline-none focus-visible:ring-2 focus-visible:ring-primary;
}
```

### Form Field Component Pattern

```typescript
@Component({
  selector: 'app-form-field',
  template: `
    <div class="form-control w-full">
      <label class="label" [for]="inputId">
        <span class="label-text">{{ label }}</span>
      </label>
      <ng-content></ng-content>
      @if (error) {
        <label class="label">
          <span class="label-text-alt text-error">{{ error }}</span>
        </label>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormFieldComponent {
  @Input() label = '';
  @Input() inputId = '';
  @Input() error = '';
}
```

### File Structure

```
apps/frontend/src/
├── styles.css (global theme and focus styles)
├── app/
│   ├── core/
│   │   └── services/
│   │       └── theme.service.ts (optional)
│   └── shared/
│       └── components/
│           ├── form-field/
│           │   └── form-field.component.ts
│           └── input/
│               └── input.component.ts
```

### Previous Story Dependencies

- **Story 1.4** provides: AdminLayout, Sidebar, Navbar to verify theme consistency

### Testing Requirements

- Visual test: Dark theme applied consistently
- Keyboard test: Tab through all interactive elements
- Accessibility audit: Use browser devtools accessibility panel
- Contrast check: Use WCAG contrast checker

### Accessibility Requirements (NFR9)

- Keyboard operability: All forms navigable via Tab
- Visible focus states: Clear visual indication of focus
- Form labels: Every input has an associated label
- Error states: Errors linked to fields via aria-describedby
- Contrast: 4.5:1 minimum for body text

### DaisyUI Color Palette

| Usage | DaisyUI Class | Notes |
|-------|---------------|-------|
| Primary actions | btn-primary | Main CTA buttons |
| Error states | text-error | Form errors, warnings |
| Focus rings | ring-primary | Keyboard focus indicator |
| Background | base-100 | Main content area |
| Cards | base-200 | Card backgrounds |

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Frontend Architecture]
- [Source: _bmad-output/planning-artifacts/prd.md#Accessibility]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.5]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
