# KCOW Source Tree

> Complete directory structure with annotations
> Generated: 2025-12-27

## Repository Overview

```
kcow/                              # Project root
├── apps/                          # Application workspaces
│   ├── frontend/                  # Angular 21 SPA (Active)
│   └── backend/                   # ASP.NET Core API (Planned - Empty)
├── docs/                          # Documentation & legacy data
├── _bmad-output/                  # BMM workflow tracking
├── LICENSE                        # Project license
└── README.md                      # Project readme
```

---

## Frontend Application

### Configuration Files

```
apps/frontend/
├── biome.json                     # Biome linter configuration
├── index.html                     # SPA entry point
├── jest.config.js                 # Jest test configuration
├── package.json                   # NPM dependencies & scripts
├── package-lock.json              # Dependency lock file
├── playwright.config.ts           # E2E test configuration
├── postcss.config.js              # PostCSS for Tailwind
├── setup-jest.ts                  # Jest setup file
├── tailwind.config.js             # Tailwind CSS configuration
├── tsconfig.json                  # Root TypeScript config
├── tsconfig.app.json              # App-specific TS config
├── tsconfig.spec.json             # Test-specific TS config
├── vite.config.ts                 # Vite build configuration
├── dist/                          # Production build output
├── node_modules/                  # Dependencies
├── e2e/                           # Playwright E2E tests
└── src/                           # Source code
```

### Source Code Structure

```
apps/frontend/src/
├── main.ts                        # Application bootstrap
├── styles.css                     # Global styles (Tailwind imports)
├── environments/                  # Environment configs
│   ├── environment.ts             # Development environment
│   └── environment.prod.ts        # Production environment
└── app/                           # Angular application
    ├── app.component.ts           # Root component
    ├── app.component.html         # Root template
    ├── app.component.css          # Root styles
    ├── app.config.ts              # Application configuration
    ├── app.routes.ts              # Route definitions
    ├── core/                      # Singleton services & app-wide concerns
    ├── features/                  # Feature modules (lazy-loaded)
    ├── layouts/                   # Layout components
    ├── models/                    # Shared TypeScript interfaces
    ├── shared/                    # Shared/reusable components
    └── styles/                    # Shared stylesheets
```

### Core Module

```
apps/frontend/src/app/core/
├── services/                      # Application-wide services
│   ├── breadcrumb.service.ts      # Navigation breadcrumbs
│   ├── breadcrumb.service.spec.ts
│   ├── error-logging.service.ts   # Error logging utility
│   ├── global-error-handler.service.ts # Global error handler
│   ├── mock-data.service.ts       # Demo data provider
│   ├── mock-data.service.spec.ts
│   ├── modal.service.ts           # Modal dialog management
│   ├── modal.service.spec.ts
│   ├── notification.service.ts    # Toast notifications
│   ├── notification.service.spec.ts
│   ├── page-metadata.service.ts   # Page title/meta management
│   ├── page-metadata.service.spec.ts
│   ├── sidebar.service.ts         # Sidebar state management
│   ├── sidebar.service.spec.ts
│   ├── system-health.service.ts   # System health monitoring
│   └── theme.service.ts           # Theme & workspace settings
│       └── theme.service.spec.ts
└── interceptors/                  # HTTP interceptors
    ├── error.interceptor.ts       # Error handling interceptor
    └── http-monitoring.interceptor.ts # HTTP request monitoring
```

### Feature Modules

```
apps/frontend/src/app/features/
├── dashboard/                     # Main dashboard
│   ├── dashboard.component.ts
│   ├── dashboard.component.html
│   └── dashboard.component.css
├── tables/                        # Data table demo
│   ├── tables.component.ts        # Complex table with sorting/pagination
│   ├── tables.component.html
│   └── tables.component.css
├── forms/                         # Form demo
│   ├── forms.component.ts
│   ├── forms.component.html
│   └── forms.component.css
├── workspace-settings/            # User preferences
│   ├── workspace-settings.component.ts
│   ├── workspace-settings.component.html
│   └── workspace-settings.component.css
├── system-health/                 # System status monitoring
│   ├── system-health.component.ts
│   ├── system-health.component.html
│   └── system-health.component.css
├── modals/                        # Modal demo
│   ├── modals.component.ts
│   ├── modals.component.html
│   ├── modals.component.scss
│   ├── simple-modal/
│   │   └── simple-modal.component.ts
│   └── form-modal/
│       └── form-modal.component.ts
├── notifications/                 # Notification demo
│   ├── notifications.component.ts
│   ├── notifications.component.html
│   ├── notifications.component.css
│   └── notifications.component.spec.ts
├── error-handling-demo/           # Error handling examples
│   ├── error-handling-demo.component.ts
│   ├── error-handling-demo.component.html
│   └── error-handling-demo.component.css
└── error-pages/                   # Error page components
    ├── forbidden/
    │   └── forbidden.component.ts # 403 page
    ├── not-found/
    │   └── not-found.component.ts # 404 page
    └── server-error/
        └── server-error.component.ts # 500 page
```

### Layout Components

```
apps/frontend/src/app/layouts/
├── admin-layout/                  # Main admin shell
│   ├── admin-layout.component.ts  # Router outlet + sidebar/navbar
│   └── admin-layout.component.html
├── sidebar/                       # Navigation sidebar
│   ├── sidebar.component.ts
│   ├── sidebar.component.html
│   └── sidebar.component.css
└── navbar/                        # Top navigation bar
    ├── navbar.component.ts
    ├── navbar.component.html
    └── navbar.component.css
```

### Shared Components

```
apps/frontend/src/app/shared/
└── components/
    ├── card/
    │   └── card.component.ts      # Generic card container
    ├── stat-card/
    │   └── stat-card.component.ts # Dashboard stat card
    ├── loading-spinner/
    │   └── loading-spinner.component.ts # Loading indicator
    ├── error-boundary/
    │   └── error-boundary.component.ts # Error boundary wrapper
    ├── modal/
    │   ├── modal.component.ts     # Modal dialog
    │   ├── modal.component.html
    │   └── modal.component.scss
    ├── modal-container/
    │   └── modal-container.component.ts # Modal host
    └── notification-container/
        ├── notification-container.component.ts # Toast host
        ├── notification-container.component.html
        ├── notification-container.component.css
        └── notification-container.component.spec.ts
```

### Models

```
apps/frontend/src/app/models/
├── error.model.ts                 # Error type definitions
├── menu-item.model.ts             # Navigation menu types
├── modal.model.ts                 # Modal configuration types
├── notification.model.ts          # Notification types
├── recent-activity.model.ts       # Activity feed types
├── stats.model.ts                 # Dashboard stat types
└── table-data.model.ts            # Table/user data types
```

### E2E Tests

```
apps/frontend/e2e/
├── README.md                      # E2E test documentation
├── helpers/
│   └── test-helpers.ts            # Shared test utilities
├── page-objects/
│   └── base.page.ts               # Base page object
├── breadcrumbs.spec.ts            # Breadcrumb tests
├── dashboard.spec.ts              # Dashboard tests
├── error-handling.spec.ts         # Error handling tests
├── forms.spec.ts                  # Form tests
├── modals.spec.ts                 # Modal tests
├── navigation.spec.ts             # Navigation tests
├── notifications.spec.ts          # Notification tests
├── settings.spec.ts               # Settings tests
├── sidebar.spec.ts                # Sidebar tests
├── tables.spec.ts                 # Table tests
└── theme.spec.ts                  # Theme tests
```

---

## Documentation

```
docs/
├── architecture.md                # Architecture documentation
├── project_context.md             # AI agent guidelines/tech rules
├── project-scan-report.json       # Workflow state tracking
├── source-tree.md                 # This file
├── ux-design-specification.md     # UI/UX specifications
└── legacy/                        # Legacy system data
    ├── 1_School/                  # School entity data
    │   ├── School.xml             # School sample data
    │   └── School.xsd             # School schema
    ├── 2_Class_Group/             # Class group entity data
    │   ├── Class Group.xml        # Class group sample data
    │   └── Class Group.xsd        # Class group schema
    ├── 3_Activity/                # Activity entity data
    │   ├── Activity.xml           # Activity sample data
    │   └── Activity.xsd           # Activity schema
    ├── 4_Children/                # Student entity data + UI references
    │   ├── Children.xml           # Student sample data
    │   ├── Children.xsd           # Student schema (large)
    │   ├── 1_Child_Information.png    # Legacy UI - Child info tab
    │   ├── 2_Child_Financial.png      # Legacy UI - Financial tab
    │   ├── 3_Class_Group.png          # Legacy UI - Class group tab
    │   ├── 4_Class_Group_Attendance.png # Legacy UI - Attendance tab
    │   ├── 5_Child_Evaluation.png     # Legacy UI - Evaluation tab
    │   └── 6_Class_Groups_Evaluation.png # Legacy UI - Group evaluation tab
    ├── kcow_logo.png              # Logo asset (PNG)
    └── kcow_logo.svg              # Logo asset (SVG)
```

---

## Workflow Tracking

```
_bmad-output/
└── bmm-workflow-status.yaml       # BMM workflow progress
```

---

## File Counts Summary

| Directory | Files | Description |
|-----------|-------|-------------|
| `apps/frontend/src/app/core/services/` | 18 | Core services + specs |
| `apps/frontend/src/app/core/interceptors/` | 2 | HTTP interceptors |
| `apps/frontend/src/app/features/` | 28 | Feature components |
| `apps/frontend/src/app/layouts/` | 8 | Layout components |
| `apps/frontend/src/app/shared/` | 10 | Shared components |
| `apps/frontend/src/app/models/` | 7 | TypeScript models |
| `apps/frontend/e2e/` | 14 | E2E tests + helpers |
| `docs/` | 5 | Documentation |
| `docs/legacy/` | 16 | Legacy data/schemas/UI references |

**Total Source Files**: ~108 (excluding node_modules, dist)

---

## Key File Purposes

### Entry Points
| File | Purpose |
|------|---------|
| `src/main.ts` | Application bootstrap |
| `src/app/app.component.ts` | Root component |
| `src/app/app.routes.ts` | Route configuration |
| `src/app/app.config.ts` | Provider configuration |

### State Management
| File | Purpose |
|------|---------|
| `core/services/theme.service.ts` | Theme + workspace settings (signals) |
| `core/services/sidebar.service.ts` | Sidebar state (signals) |
| `core/services/notification.service.ts` | Notification queue (signals) |
| `core/services/modal.service.ts` | Modal state management |

### Demo Features (Scaffold)
| File | Purpose |
|------|---------|
| `features/tables/tables.component.ts` | Advanced table patterns |
| `features/forms/forms.component.ts` | Form handling patterns |
| `features/dashboard/dashboard.component.ts` | Dashboard layout patterns |

### Legacy Domain Reference
| File | Purpose |
|------|---------|
| `docs/legacy/1_School/School.xsd` | School entity schema |
| `docs/legacy/3_Activity/Activity.xsd` | Activity/program schema |
| `docs/legacy/2_Class_Group/Class Group.xsd` | Class session schema |
| `docs/legacy/4_Children/Children.xsd` | Student data schema |

---

## Planned Structure (Backend)

```
apps/backend/                      # (Empty - Planned)
├── src/
│   ├── Api/                       # Controllers, Middleware
│   ├── Application/               # Use cases, CQRS
│   ├── Domain/                    # Entities, Value Objects
│   └── Infrastructure/            # Data access, EF Core
└── tests/
    ├── Unit/
    └── Integration/
```
