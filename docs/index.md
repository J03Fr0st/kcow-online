# KCOW Documentation Index

> Mobile Computer Literacy Trucks - Admin Application
> Generated: 2025-12-27 | Deep Scan Documentation

---

## Quick Links

| Document | Purpose |
|----------|---------|
| [Architecture](./architecture.md) | System design and patterns |
| [Source Tree](./source-tree.md) | File structure reference |
| [Development Guide](./development-guide.md) | Setup and commands |
| [Domain Models](./domain-models.md) | Data entities |
| [UX Design](./ux-design-specification.md) | UI specifications |
| [Tech Rules](./project_context.md) | AI agent guidelines |

---

## Project Overview

**KCOW** (Kids Computer On Wheels) is an administrative application for managing mobile computer literacy trucks that visit schools in South Africa.

### Core Functionality

- **School Management**: Register schools,track contacts
- **Class Group Management**: Coordinate truck visits and session times
- **Student Enrollment**: Manage student records and class group assignment
- **Activity Tracking**: Monitor educational program delivery
- **Attendance & Progress**: Track student participation and outcomes
- **Billing & Reporting**: Generate invoices and operational reports

### Current Status

| Component | Status | Location |
|-----------|--------|----------|
| Frontend | Active | `apps/frontend/` |
| Backend | Planned | `apps/backend/` |
| Legacy Data | Reference | `docs/legacy/` |

---

## Documentation Map

### Architecture & Design

```
docs/
├── architecture.md        # System architecture, patterns, decisions
├── domain-models.md       # Entity definitions and relationships
├── ux-design-specification.md  # UI/UX design specs (partial)
└── project_context.md     # Tech stack rules for AI agents
```

### Development

```
docs/
├── development-guide.md   # Commands, setup, patterns
└── source-tree.md         # Complete file structure
```

### Reference Data (Legacy System)

The legacy Microsoft Access system is organized by entity type. **Maintenance entities** (School, Class Group, Activity) are lookup/configuration tables, while **Children** is the core operational module.

```
docs/legacy/
├── 1_School/                    # [Maintenance] School master data
│   ├── School.xsd               # Schema: contacts, scheduling, billing
│   └── School.xml               # Sample data
├── 2_Class_Group/               # [Maintenance] Time slots at schools
│   ├── Class Group.xsd          # Schema: schedule, truck assignment
│   └── Class Group.xml          # Sample data
├── 3_Activity/                  # [Maintenance] Educational programs catalog
│   ├── Activity.xsd             # Schema: program code, grade level
│   └── Activity.xml             # Sample data
├── 4_Children/                  # [Core] Student management system
│   ├── Children.xsd             # Schema: 92 fields
│   ├── Children.xml             # Sample data
│   ├── 1_Child_Information.png  # Tab 1: Personal info, enrollment
│   ├── 2_Child_Financial.png    # Tab 2: Invoices, receipts, balance
│   ├── 3_Class_Group.png        # Tab 3: Students in class group
│   ├── 4_Class_Group_Attendance.png  # Tab 4: Attendance tracking
│   ├── 5_Child_Evaluation.png   # Tab 5: Progress evaluation matrix
│   └── 6_Class_Groups_Evaluation.png # Tab 6: Group evaluation grid
├── kcow_logo.svg                # Brand assets
└── kcow_logo.png
```

#### Legacy Workflow Summary

| Entity | Type | Purpose |
|--------|------|---------|
| School | Maintenance | Configure schools visited by trucks |
| Class Group | Maintenance | Define time slots and schedules |
| Activity | Maintenance | Manage educational program catalog |
| **Children** | **Core** | **Full student lifecycle management** |

### Workflow Tracking

```
_bmad-output/
└── bmm-workflow-status.yaml   # BMM phase tracking

docs/
└── project-scan-report.json   # Scan workflow state
```

---

## Technology Stack

### Frontend (Active)

| Category | Technology | Version |
|----------|------------|---------|
| Framework | Angular | 21.0.0 |
| Language | TypeScript | 5.9.0 |
| Build | Vite | 6.0.0 |
| Styling | Tailwind CSS | 3.4.1 |
| Components | DaisyUI | 4.12.14 |
| Unit Tests | Jest | 30.2.0 |
| E2E Tests | Playwright | 1.56.1 |
| Linting | Biome | 2.0.0 |

### Backend (Planned)

| Category | Technology |
|----------|------------|
| Framework | ASP.NET Core |
| Database | SQLite (initial) |
| ORM | Entity Framework Core |
| Architecture | Clean Architecture |

---

## Key Patterns

### State Management
- **Angular Signals** for reactive state
- **RxJS** for async operations
- **localStorage** for user preferences

### Component Architecture
- Standalone components (no NgModules)
- OnPush change detection
- Lazy-loaded routes

### Theming
- 21 DaisyUI themes
- Theme switching via `ThemeService`
- Layout density options

---

## Getting Started

### Prerequisites
- Node.js 20+
- npm 10+
- (Future) .NET SDK 8+

### Quick Start

```bash
cd apps/frontend
npm install
npm run dev
# Open http://localhost:4200
```

### Key Commands

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server |
| `npm run test` | Run unit tests |
| `npm run test:e2e` | Run E2E tests |
| `npm run lint` | Check code quality |
| `npm run build` | Production build |

See [Development Guide](./development-guide.md) for full details.

---

## Domain Entities

| Entity | Description | Details |
|--------|-------------|---------|
| **School** | Schools visited by trucks | [View](./domain-models.md#school) |
| **Activity** | Educational programs | [View](./domain-models.md#activity) |
| **Class Group** | Scheduled sessions | [View](./domain-models.md#class-group) |
| **Children** | Student records | [View](./domain-models.md#children-students) |

---

## Application Routes

| Path | Feature | Component |
|------|---------|-----------|
| `/dashboard` | Main overview | DashboardComponent |
| `/tables` | Data table demo | TablesComponent |
| `/forms` | Form demo | FormsComponent |
| `/workspace-settings` | User preferences | WorkspaceSettingsComponent |
| `/system-health` | System monitoring | SystemHealthComponent |
| `/notifications` | Toast demo | NotificationsComponent |
| `/modals` | Modal demo | ModalsComponent |
| `/error-handling` | Error patterns | ErrorHandlingDemoComponent |

---

## BMM Workflow Status

Current phase in the BMad Method workflow:

| Phase | Status | Next Action |
|-------|--------|-------------|
| Prerequisite: Document Project | Complete | - |
| Phase 0: Discovery (Research) | Required | `/bmad:bmm:workflows:research` |
| Phase 1: Planning (PRD) | Required | After research |
| Phase 2: Solutioning | Required | After PRD |
| Phase 3: Implementation | Pending | After solutioning |

See `_bmad-output/bmm-workflow-status.yaml` for detailed tracking.

---

## Document Generation

This documentation was generated by the `document-project` workflow:

- **Scan Level**: Deep
- **Generated**: 2025-12-27
- **Outputs**: 6 documents

### Regeneration

To regenerate after significant changes:
```
/bmad:bmm:workflows:document-project
```

---

## Contributing

### Code Standards
- Follow patterns in [Development Guide](./development-guide.md)
- Use Biome for formatting: `npm run format`
- All tests must pass: `npm run test`

### Documentation Updates
- Keep architecture.md current with design decisions
- Update domain-models.md when entities change
- Regenerate source-tree.md after structural changes
