# KCOW Architecture Documentation

> Mobile Computer Literacy Trucks (KCOW) - Admin Application
> Generated: 2025-12-27 | Updated: 2026-01-05 | Scan Level: Deep

---

## ⚠️ CRITICAL: Strict XSD Schema Alignment

**All implementations MUST strictly align with the legacy XSD schemas.** The XSD files define the authoritative data model:

| Entity | XSD Location | Fields |
|--------|--------------|--------|
| School | `docs/legacy/1_School/School.xsd` | 30 |
| Class Group | `docs/legacy/2_Class_Group/Class Group.xsd` | 15 |
| Activity | `docs/legacy/3_Activity/Activity.xsd` | 7 |
| Children | `docs/legacy/4_Children/Children.xsd` | 92 |

**Alignment Requirements:**
- Database columns must match XSD field definitions (names, types, lengths)
- **Use English field names** where XSD uses Afrikaans (e.g., `Trok` → `Truck`, `Taal` → `Language`)
- API DTOs must include all XSD fields
- Frontend models must align with XSD structure
- No fields may be omitted or added without explicit approval

See `docs/domain-models.md` for complete XSD-to-implementation mappings.

---

## 1. System Overview

### 1.1 Purpose
KCOW is an administrative application for managing mobile computer literacy trucks that visit schools. The system handles:
- Truck management
- School management
- Class groups and lesson activities
- Student/Children enrollment and tracking
- Attendance and progress tracking
- Billing and reporting

### 1.2 Architecture Type
**Multi-part Monorepo** with planned frontend-backend separation:

```
kcow/
├── apps/
│   ├── frontend/    # Angular 21 SPA (Active)
│   └── backend/     # ASP.NET Core API (Planned)
├── docs/            # Documentation & legacy data
└── _bmad-output/    # BMM workflow tracking
```

### 1.3 Current State
- **Frontend**: Scaffolded Angular 21 application with demo features
- **Backend**: Empty directory, planned ASP.NET Core with Clean Architecture
- **Legacy Data**: Microsoft Access XML schemas defining domain models

---

## 2. Technology Stack

### 2.1 Frontend Stack

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| Framework | Angular | 21.0.0 | SPA framework |
| Language | TypeScript | 5.9.0 | Type-safe development |
| Build | Vite | 6.0.0 | Fast development/build |
| Styling | Tailwind CSS | 4.1.7 | Utility-first CSS |
| UI Library | DaisyUI | 4.12.14 | Component library |
| Unit Tests | Jest | 30.2.0 | Unit testing |
| E2E Tests | Playwright | 1.56.1 | Browser automation |
| Linting | Biome | - | Code quality |

### 2.2 Planned Backend Stack

| Category | Technology | Purpose |
|----------|------------|---------|
| Framework | ASP.NET Core | Web API |
| Database | SQLite | Data persistence |
| Architecture | Clean Architecture | Separation of concerns |
| ORM | Entity Framework Core | Data access |

### 2.3 Development Environment
- Node.js (Frontend)
- .NET SDK (Backend)
- Windows development platform

---

## 3. Frontend Architecture

### 3.1 Directory Structure

```
apps/frontend/src/app/
├── core/                    # Singleton services & app-wide concerns
│   └── services/           # Core services (theme, notification, etc.)
├── features/               # Feature modules (lazy-loaded)
│   ├── dashboard/
│   ├── tables/
│   ├── forms/
│   ├── workspace-settings/
│   ├── system-health/
│   ├── modals/
│   ├── notifications/
│   ├── error-pages/
│   └── error-handling-demo/
├── layouts/                # Layout components
│   ├── admin-layout/
│   ├── sidebar/
│   └── navbar/
├── models/                 # Shared TypeScript interfaces
└── shared/                 # Shared/reusable components
    └── components/
        ├── card/
        ├── stat-card/
        ├── loading-spinner/
        ├── error-boundary/
        ├── modal/
        ├── modal-container/
        └── notification-container/
```

### 3.2 Path Aliases

```typescript
// tsconfig.json paths
"@core/*"     → "src/app/core/*"
"@shared/*"   → "src/app/shared/*"
"@features/*" → "src/app/features/*"
"@layouts/*"  → "src/app/layouts/*"
"@models/*"   → "src/app/models/*"
```

### 3.3 Component Patterns

**Standalone Components** (Angular 21 standard):
```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ...],
  templateUrl: './example.component.html'
})
```

Key patterns:
- All components are standalone (no NgModules)
- OnPush change detection for performance
- Lazy loading via route-based code splitting
- Signal-based reactivity

### 3.4 State Management

**Angular Signals** for reactive state:
```typescript
@Injectable({ providedIn: 'root' })
export class ThemeService {
  currentTheme = signal<Theme>('light');
  sidebarCollapsed = signal<boolean>(false);

  setTheme(theme: Theme): void {
    this.currentTheme.set(theme);
  }
}
```

**RxJS** for async operations:
```typescript
// Service pattern for data fetching
getData(): Observable<Data[]> {
  return of(data).pipe(delay(300));
}

// Component consumption
this.service.getData().subscribe({
  next: (data) => this.data.set(data),
  error: () => this.error.set('Failed to load')
});
```

**localStorage** for preference persistence:
```typescript
private readonly SETTINGS_KEY = 'workspace-settings';
// Load/save settings to localStorage
```

### 3.5 UI Component Library

**DaisyUI + Tailwind CSS** theming:
- 21 built-in themes supported
- Theme switching via `data-theme` attribute
- Layout density options (compact/comfortable/spacious)

Core shared components:
- `StatCardComponent` - Dashboard statistics cards
- `LoadingSpinnerComponent` - Async loading indicators
- `ModalComponent` - Dialog system
- `NotificationContainerComponent` - Toast notifications
- `ErrorBoundaryComponent` - Error handling

---

## 4. Domain Model (Legacy)

Based on legacy Microsoft Access database schemas. **All implementations must strictly align with XSD definitions.**

### 4.0 Legacy System Overview

The original KCOW system was built in Microsoft Access with a tab-based interface. The application is organized into two workflow types:

#### Core Workflow: Student Management
The **Children (Students)** module is the primary operational interface with 7 interconnected tabs:
- **Child Information** - Student profile, family contacts, enrollment details
- **Child Financial** - Invoice/receipt tracking, balance calculation, statements
- **Class Groups** - Students assigned to class sessions
- **Child Attendance** - Individual attendance tracking
- **Class Groups Attendance** - Group-level attendance overview
- **Child Evaluation** - Progress assessments (14-activity evaluation matrix)
- **Class Groups Evaluation** - Consolidated progress reporting

See [Legacy UI Screenshots](./legacy/4_Children/) for visual reference.

#### Maintenance Workflows
The following entities are primarily **maintenance/lookup tables** supporting the core workflow:
- **School** - School master data (contacts, scheduling, billing)
- **Class Group** - Time slot definitions at schools
- **Activity** - Educational programs/software catalog

### 4.1 School Entity (Maintenance)
Primary entity for schools visited by trucks. **Maintenance workflow** - data is typically set up once and updated infrequently.

| Field | Type | Description |
|-------|------|-------------|
| School_Id | int (PK) | Auto-increment identifier |
| Short_School | nvarchar(50) | Short name |
| School_Description | nvarchar(50) | Full description |
| Price | money | Service price |
| Trok | byte | Truck assignment |
| Day | nvarchar(50) | Scheduled day |
| Sequence | nvarchar(50) | Visit sequence |
| ContactPerson | nvarchar(50) | Primary contact |
| ContactCell | nvarchar(50) | Contact phone |
| E-mail_adress | nvarchar(50) | Contact email |
| Headmaster | nvarchar(50) | Principal name |
| Address1/Address2 | nvarchar(50) | Physical address |
| Taal | nvarchar(50) | Language (Afr/Eng) |

### 4.2 Activity Entity (Maintenance)
Educational activities/programs offered. **Maintenance workflow** - catalog of educational software programs.

| Field | Type | Description |
|-------|------|-------------|
| ActivityID | int (PK) | Activity identifier |
| Program | nvarchar(255) | Program code |
| ProgramName | nvarchar(255) | Display name |
| Educational_Focus | ntext | Learning objectives |
| Folder | nvarchar(255) | Resource folder |
| Grade | nvarchar(255) | Target grade level |
| Icon | image | Visual representation |

### 4.3 Class Group Entity (Maintenance)
Scheduled class sessions at schools. **Maintenance workflow** - defines time slots for truck visits.

| Field | Type | Description |
|-------|------|-------------|
| Class_Group | nvarchar(10) | Group identifier |
| DayTruck | nvarchar(6) | Day-truck combination |
| Description | nvarchar(35) | Group description |
| Start_Time | nvarchar(5) | Session start |
| End_Time | nvarchar(5) | Session end |
| School_Id | smallint (FK) | Associated school |
| DayId | nvarchar(1) | Day code |
| Sequence | nvarchar(50) | Display order |
| Evaluate | bit | Requires evaluation |

### 4.4 Children Entity (Core Workflow)
Student records - the **primary operational interface** (see `docs/legacy/4_Children/Children.xsd`).

This is the main workflow with 6 interconnected UI tabs:

| Tab | Purpose |
|-----|---------|
| Child Information | Personal details, family contacts, class assignment, T-shirt orders |
| Child Financial | Invoice/receipt tracking, balance calculation, certificate printing |
| Class Groups | Grid view of students in a class group |
| Class Groups Attendance | Attendance tracking with planned dates, seat assignments |
| Child Evaluation | Individual 14-activity evaluation matrix, speed/accuracy metrics |
| Class Groups Evaluation | Consolidated evaluation grid for all students |

Key data domains:
- Student identification & demographics
- Guardian/family contact information
- Enrollment & class assignment
- Financial tracking (invoices, receipts, balance)
- Progress evaluation (14 activity scores × 2 evaluation sets)
- Reports & certificates

### 4.5 Entity Relationships

```
School (1) ←——→ (N) Class_Group     # A school hosts multiple class sessions
Class_Group (1) ←——→ (N) Children   # A child belongs to one class group
Activity (1) ←——→ (N) Class_Group   # Activities are delivered during sessions (implicit)
```

---

## 5. Planned Backend Architecture

### 5.1 Clean Architecture Layers

```
apps/backend/
├── src/
│   ├── Api/              # Presentation layer (Controllers, Middleware)
│   ├── Application/      # Use cases, Commands/Queries (CQRS)
│   ├── Domain/           # Entities, Value Objects, Domain Events
│   └── Infrastructure/   # Data access, External services
└── tests/
    ├── Unit/
    └── Integration/
```

### 5.2 API Design Principles
- RESTful endpoints
- JSON:API or similar response format
- Better Auth authentication (TypeScript framework)
- Versioned API paths

### 5.3 Database Approach
- SQLite for initial development
- Entity Framework Core for ORM
- Migrations for schema versioning
- Consider upgrade path to SQL Server/PostgreSQL

---

## 6. Development Patterns

### 6.1 Code Style Rules
From `docs/project_context.md`:

| Rule | Enforcement |
|------|-------------|
| No `any` type | Biome lint error |
| Explicit return types | Required |
| camelCase variables | Standard |
| PascalCase components | Angular convention |
| kebab-case files | Angular convention |

### 6.2 Testing Strategy

**Unit Tests** (Jest):
- Component logic
- Service methods
- Utility functions

**E2E Tests** (Playwright):
- User flows
- Critical paths
- Visual regression

### 6.3 Build & Development

```bash
# Development
npm run dev          # Vite dev server

# Testing
npm run test         # Jest unit tests
npm run e2e          # Playwright E2E

# Production
npm run build        # Vite production build
npm run preview      # Preview production build

# Code Quality
npm run lint         # Biome linting
npm run format       # Code formatting
```

---

## 7. Integration Architecture

### 7.1 Frontend-Backend Communication

```
[Angular SPA] ←—HTTP/REST—→ [ASP.NET Core API] ←—EF Core—→ [SQLite]
     │                              │
     │                              ├── /api/v1/schools
     │                              ├── /api/v1/class-groups
     │                              ├── /api/v1/activities
     │                              └── /api/v1/students
     │
     └── State: Angular Signals + RxJS
```

### 7.2 Authentication
- **Better Auth** - Comprehensive TypeScript authentication framework
- Email & Password authentication
- Session management
- Role-based access control (Admin role)
- Framework agnostic (works with Angular frontend)
- Supports OAuth providers (optional)
- Two-factor authentication support (optional)
- Multi-tenant/organization support (optional)

### 7.3 Error Handling
- Frontend: ErrorBoundaryComponent + GlobalErrorHandler
- Backend: Exception middleware with ProblemDetails
- Structured logging for debugging

---

## 8. Key Architectural Decisions

### 8.1 Decisions Made

| Decision | Rationale |
|----------|-----------|
| Angular 21 standalone components | Simpler architecture, better tree-shaking |
| Signals over RxJS state | Modern Angular pattern, simpler reactivity |
| Vite over Webpack | Faster development builds |
| DaisyUI | Rapid prototyping with built-in themes |
| SQLite initial | Low friction development, portable |

### 8.2 Decisions Pending

| Decision | Options | Considerations |
|----------|---------|----------------|
| API versioning | URL path vs Header | Client compatibility |
| State persistence | localStorage vs IndexedDB | Offline capability |
| Real-time updates | WebSockets vs SSE | Live data requirements |
| File storage | Local vs Cloud | Certificate/report exports |

---

## 9. Security Considerations

### 9.1 Frontend
- XSS prevention via Angular sanitization
- CSRF protection with tokens
- Secure cookie configuration
- Content Security Policy headers

### 9.2 Backend (Planned)
- Input validation on all endpoints
- Parameterized queries (EF Core)
- Better Auth authentication/authorization middleware
- Rate limiting for API endpoints
- Audit logging for sensitive operations

---

## 10. Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| Initial load | < 3s | Lighthouse |
| Route navigation | < 500ms | User perception |
| API response | < 200ms | P95 latency |
| Bundle size | < 500KB | Production build |

---

## Appendix A: File References

| Document | Location | Purpose |
|----------|----------|---------|
| Tech Stack Rules | `docs/project_context.md` | AI agent guidelines |
| UX Design | `docs/ux-design-specification.md` | UI specifications |
| Legacy Schemas | `docs/legacy/*/` | Domain model reference (organized by entity) |
| Legacy UI References | `docs/legacy/4_Children/*.png` | Historical UI screenshots |
| Workflow Status | `_bmad-output/bmm-workflow-status.yaml` | BMM tracking |

---

## Appendix B: Glossary

| Term | Definition |
|------|------------|
| KCOW | Kids Computer On Wheels - Mobile literacy trucks |
| Trok | Truck identifier (Afrikaans term) |
| Class Group | Scheduled session at a school |
| BMM | BMad Method - Development workflow framework |
