# KCOW Development Guide

> Development setup, commands, and conventions
> Generated: 2025-12-27

## Quick Start

```bash
# Navigate to frontend directory
cd apps/frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Open browser at http://localhost:4200
```

---

## NPM Scripts

### Development

| Command | Description |
|---------|-------------|
| `npm run dev` | Start Vite development server (port 4200) |
| `npm run build` | Production build to `dist/` |
| `npm run preview` | Preview production build |

### Testing

| Command | Description |
|---------|-------------|
| `npm run test` | Run Jest unit tests |
| `npm run test:watch` | Run tests in watch mode |
| `npm run test:coverage` | Run tests with coverage report |
| `npm run test:e2e` | Run Playwright E2E tests |
| `npm run test:e2e:ui` | Run E2E with UI inspector |
| `npm run test:e2e:headed` | Run E2E in headed browser |
| `npm run test:e2e:debug` | Debug E2E tests |
| `npm run test:e2e:chromium` | E2E on Chromium only |
| `npm run test:e2e:firefox` | E2E on Firefox only |
| `npm run test:e2e:webkit` | E2E on WebKit only |
| `npm run test:e2e:report` | Show Playwright HTML report |

### Code Quality

| Command | Description |
|---------|-------------|
| `npm run lint` | Biome lint check |
| `npm run format` | Biome format (auto-fix) |
| `npm run check` | Biome lint + format (auto-fix) |

---

## Path Aliases

Configure in both `tsconfig.json` and `vite.config.ts`:

| Alias | Path | Usage |
|-------|------|-------|
| `@core/*` | `src/app/core/*` | Core services, interceptors |
| `@shared/*` | `src/app/shared/*` | Shared components |
| `@features/*` | `src/app/features/*` | Feature modules |
| `@layouts/*` | `src/app/layouts/*` | Layout components |
| `@models/*` | `src/app/models/*` | TypeScript interfaces |
| `@environments/*` | `src/environments/*` | Environment configs |

```typescript
// Example usage
import { ThemeService } from '@core/services/theme.service';
import { StatCardComponent } from '@shared/components/stat-card/stat-card.component';
import type { Notification } from '@models/notification.model';
```

---

## Code Style (Biome)

### Formatting Rules

| Rule | Value |
|------|-------|
| Indent | 2 spaces |
| Line ending | LF |
| Line width | 100 characters |
| Quotes | Single quotes |
| Trailing commas | All |
| Semicolons | Always |
| Arrow parens | Always |

### Key Lint Rules

```json
{
  "useConst": "error",           // Prefer const
  "noVar": "error",              // No var declarations
  "noDebugger": "warn",          // No debugger statements
  "noExplicitAny": "off",        // any type allowed (consider enabling)
  "noDangerouslySetInnerHtml": "warn"  // Security warning
}
```

### Running Checks

```bash
# Check for issues
npm run lint

# Auto-fix issues
npm run check
```

---

## TypeScript Configuration

### Compiler Options

| Option | Value | Description |
|--------|-------|-------------|
| `target` | ES2023 | Modern JavaScript output |
| `module` | ES2022 | ESM modules |
| `strict` | true | Full strict mode |
| `strictTemplates` | true | Angular template checking |
| `noImplicitReturns` | true | Require return statements |
| `noFallthroughCasesInSwitch` | true | Prevent switch fallthrough |

### Strict Mode Implications

```typescript
// Must handle potential undefined
const user = getUser(); // Type: User | undefined
if (user) {
  console.log(user.name); // OK - narrowed to User
}

// Must use explicit return types when ambiguous
function calculate(x: number): number {
  return x * 2;
}
```

---

## Testing

### Unit Tests (Jest)

**Location**: `*.spec.ts` files alongside source

**Coverage Thresholds**:
- Branches: 80%
- Functions: 80%
- Lines: 80%
- Statements: 80%

**Example Test**:
```typescript
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    service = new NotificationService();
  });

  it('should show notification', () => {
    const id = service.show({ type: 'success', message: 'Test' });
    expect(service.notifications$()).toHaveLength(1);
  });
});
```

### E2E Tests (Playwright)

**Location**: `e2e/` directory

**Browsers Tested**:
- Chromium (Desktop Chrome)
- Firefox (Desktop Firefox)
- WebKit (Desktop Safari)
- Mobile Chrome (Pixel 5)
- Mobile Safari (iPhone 12)

**CI Configuration**:
- Retries: 2
- Workers: 1 (sequential)
- Trace: On first retry
- Screenshot: On failure only
- Video: Retain on failure

**Example Test**:
```typescript
import { test, expect } from '@playwright/test';

test('should navigate to dashboard', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL('/dashboard');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
});
```

---

## Routes

| Path | Component | Description |
|------|-----------|-------------|
| `/` | AdminLayout | Root layout with sidebar |
| `/dashboard` | Dashboard | Main dashboard |
| `/tables` | Tables | Data table demo |
| `/forms` | Forms | Form demo |
| `/workspace-settings` | WorkspaceSettings | User preferences |
| `/system-health` | SystemHealth | System monitoring |
| `/notifications` | Notifications | Toast demo |
| `/modals` | Modals | Modal demo |
| `/error-handling` | ErrorHandlingDemo | Error patterns |
| `/error/404` | NotFound | 404 page |
| `/error/403` | Forbidden | 403 page |
| `/error/500` | ServerError | 500 page |

All feature routes are **lazy-loaded** for optimal bundle splitting.

---

## Component Patterns

### Standalone Component Template

```typescript
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'app-example',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div class="card bg-base-100 shadow-lg">
      <div class="card-body">
        <h2 class="card-title">{{ title() }}</h2>
        <p>{{ description() }}</p>
      </div>
    </div>
  `,
})
export class ExampleComponent {
  title = signal('Default Title');
  description = signal('Default description');
}
```

### Service Template

```typescript
import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ExampleService {
  private readonly _data = signal<Item[]>([]);
  readonly data$ = this._data.asReadonly();

  addItem(item: Item): void {
    this._data.update((items) => [...items, item]);
  }

  removeItem(id: number): void {
    this._data.update((items) => items.filter((i) => i.id !== id));
  }
}
```

---

## Styling (Tailwind + DaisyUI)

### Theme Configuration

21 themes available via DaisyUI. Theme is controlled by `ThemeService`:

```typescript
// Available themes
const themes = [
  'light', 'dark', 'cupcake', 'bumblebee', 'emerald',
  'corporate', 'synthwave', 'retro', 'cyberpunk', 'valentine',
  'halloween', 'garden', 'forest', 'aqua', 'lofi',
  'pastel', 'fantasy', 'wireframe', 'black', 'luxury', 'dracula'
];

// Change theme
themeService.setTheme('dark');
```

### DaisyUI Component Classes

```html
<!-- Card -->
<div class="card bg-base-100 shadow-lg">
  <div class="card-body">
    <h2 class="card-title">Title</h2>
    <p>Content</p>
  </div>
</div>

<!-- Button variants -->
<button class="btn btn-primary">Primary</button>
<button class="btn btn-secondary">Secondary</button>
<button class="btn btn-accent">Accent</button>
<button class="btn btn-ghost">Ghost</button>

<!-- Stats -->
<div class="stats shadow">
  <div class="stat">
    <div class="stat-title">Total</div>
    <div class="stat-value">100</div>
    <div class="stat-desc">Description</div>
  </div>
</div>
```

---

## Environment Configuration

### Development (environment.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  // Add other dev-specific settings
};
```

### Production (environment.prod.ts)

```typescript
export const environment = {
  production: true,
  apiUrl: '/api',
  // Add other prod-specific settings
};
```

---

## Build Output

### Development

- Hot module replacement enabled
- Source maps included
- No minification
- Server runs on `http://localhost:4200`

### Production

```bash
npm run build
```

- Output: `dist/` directory
- Target: ES2023
- Tree-shaking enabled
- Code splitting by route
- Minification enabled

---

## Troubleshooting

### Common Issues

**Port 4200 in use**:
```bash
# Kill process on port 4200
npx kill-port 4200
```

**Node modules corrupted**:
```bash
rm -rf node_modules package-lock.json
npm install
```

**TypeScript errors after update**:
```bash
rm -rf dist .angular
npm run build
```

**Playwright browsers not installed**:
```bash
npx playwright install
```

### Debugging

**Browser DevTools**: F12 → Sources → Angular DevTools extension

**Jest Debug**:
```bash
node --inspect-brk node_modules/.bin/jest --runInBand
```

**Playwright Debug**:
```bash
npm run test:e2e:debug
```

---

## IDE Setup

### VS Code Extensions

- Angular Language Service
- Tailwind CSS IntelliSense
- Biome (formatting/linting)
- Playwright Test for VSCode

### Recommended Settings

```json
{
  "editor.formatOnSave": true,
  "editor.defaultFormatter": "biomejs.biome",
  "[typescript]": {
    "editor.defaultFormatter": "biomejs.biome"
  },
  "typescript.preferences.importModuleSpecifier": "relative"
}
```
