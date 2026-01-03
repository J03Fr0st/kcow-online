# E2E Tests with Playwright

This directory contains comprehensive end-to-end tests for the Angular Admin Template using Playwright.

## Test Coverage

### Core Features
- **Navigation & Routing** - Page navigation, breadcrumbs, deep linking, browser history
- **Sidebar** - Menu navigation, collapse/expand, mobile responsive
- **Breadcrumbs** - Dynamic breadcrumb updates, clickable navigation

### Component Features
- **Notifications** - All 4 types (success, error, warning, info), auto-dismiss, stacking
- **Modals** - Different sizes, confirmation dialogs, alerts, nested modals, keyboard support
- **Tables** - Filtering, sorting, column management, pagination, saved views
- **Forms** - Validation, text/email/number inputs, checkboxes, radio buttons, dynamic fields
- **Dashboard** - Widgets, statistics, activity feeds, responsive layout

### System Features
- **Theme Switching** - 20+ themes, localStorage persistence, visual updates
- **Settings** - Preferences, notification settings, layout density
- **Error Handling** - 404 pages, network errors, form validation, error boundaries

## Test Structure

```
e2e/
├── helpers/
│   └── test-helpers.ts       # Utility functions for tests
├── page-objects/
│   ├── base.page.ts          # Base page object with common elements
│   ├── notifications.page.ts # Notification-specific page object
│   ├── modals.page.ts        # Modal-specific page object
│   ├── tables.page.ts        # Table-specific page object
│   └── settings.page.ts      # Settings-specific page object
├── navigation.spec.ts        # Navigation and routing tests
├── sidebar.spec.ts           # Sidebar tests
├── breadcrumbs.spec.ts       # Breadcrumb tests
├── notifications.spec.ts     # Notification system tests
├── modals.spec.ts            # Modal/dialog tests
├── tables.spec.ts            # Table functionality tests
├── forms.spec.ts             # Form validation tests
├── dashboard.spec.ts         # Dashboard tests
├── theme.spec.ts             # Theme switching tests
├── settings.spec.ts          # Settings and preferences tests
└── error-handling.spec.ts    # Error handling tests
```

## Running Tests

### Run all tests
```bash
npm run test:e2e
```

### Run with UI mode (interactive)
```bash
npm run test:e2e:ui
```

### Run in headed mode (see browser)
```bash
npm run test:e2e:headed
```

### Debug tests
```bash
npm run test:e2e:debug
```

### Run specific browser
```bash
npm run test:e2e:chromium
npm run test:e2e:firefox
npm run test:e2e:webkit
```

### View test report
```bash
npm run test:e2e:report
```

## Test Browsers

Tests run on multiple browsers by default:
- **Chromium** (Desktop Chrome)
- **Firefox** (Desktop Firefox)
- **WebKit** (Desktop Safari)
- **Mobile Chrome** (Pixel 5)
- **Mobile Safari** (iPhone 12)

## Page Object Pattern

Tests use the Page Object pattern for better maintainability:

```typescript
// Example usage
const page = new NotificationsPage(testPage);
await page.gotoNotifications();
await page.showSuccessNotification();
expect(await page.getNotificationCount()).toBeGreaterThan(0);
```

## Test Helpers

Common test utilities available in `helpers/test-helpers.ts`:

```typescript
const helpers = new TestHelpers(page);

// Wait for Angular to stabilize
await helpers.waitForAngular();

// Work with notifications
await helpers.waitForNotification('Success message');

// Work with modals
await helpers.waitForModal('Modal Title');

// LocalStorage operations
const theme = await helpers.getCurrentTheme();
await helpers.clearLocalStorage();
```

## Best Practices

1. **Use Page Objects** - Encapsulate page interactions in page objects
2. **Wait for Angular** - Always wait for Angular to stabilize after navigation
3. **Isolate Tests** - Each test should be independent and clean up after itself
4. **Clear State** - Clear localStorage/sessionStorage between tests when needed
5. **Descriptive Names** - Use clear, descriptive test names
6. **Assertions** - Use meaningful assertions with helpful error messages

## CI/CD Integration

Tests are configured to run in CI with:
- Automatic retries (2 retries in CI)
- Sequential execution (1 worker in CI)
- JUnit XML reports
- HTML reports
- Screenshots on failure
- Videos on failure

## Debugging

### Take screenshots
```typescript
await helpers.screenshot('my-screenshot');
```

### Use debug mode
```bash
npm run test:e2e:debug
```

### Use headed mode
```bash
npm run test:e2e:headed
```

### Use UI mode
```bash
npm run test:e2e:ui
```

## Coverage

Current test coverage includes:
- ✅ Navigation and routing
- ✅ Sidebar navigation
- ✅ Breadcrumb navigation
- ✅ Notification system (all 4 types)
- ✅ Modal/Dialog system
- ✅ Table functionality (filtering, sorting, columns)
- ✅ Form validation
- ✅ Dashboard widgets
- ✅ Theme switching (20+ themes)
- ✅ Settings and preferences
- ✅ Error handling

## Contributing

When adding new features:
1. Create tests in the appropriate spec file
2. Add page objects if needed in `page-objects/`
3. Add helper functions if needed in `helpers/`
4. Update this README with coverage information
