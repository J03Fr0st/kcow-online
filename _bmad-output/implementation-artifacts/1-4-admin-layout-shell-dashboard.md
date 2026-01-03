# Story 1.4: Admin Layout Shell & Dashboard

Status: ready-for-dev

## Story

As an **admin**,
I want **a dashboard with sidebar navigation after login**,
so that **I can navigate to different modules of the system**.

## Acceptance Criteria

1. **Given** I am authenticated
   **When** I access the root path (`/`)
   **Then** I see the AdminLayout with:
   - Sidebar navigation with module links (Dashboard, Trucks, Schools, Class Groups, Students)
   - Top navbar with user info and logout button
   - Main content area showing the Dashboard

2. **Given** I am on the dashboard
   **When** I view the page
   **Then** I see a placeholder welcome message and quick stats area

3. **And** the layout uses DaisyUI components with dark theme

4. **And** sidebar is persistent on desktop

5. **And** keyboard navigation works for all interactive elements (NFR9)

## Tasks / Subtasks

- [ ] Task 1: Create AdminLayout component (AC: #1, #3)
  - [ ] Create admin-layout component in `layouts/`
  - [ ] Structure with sidebar + main content area
  - [ ] Apply DaisyUI dark theme styling
  - [ ] Use OnPush change detection
- [ ] Task 2: Create Sidebar component (AC: #1, #4)
  - [ ] Create sidebar component in `layouts/sidebar/`
  - [ ] Add navigation links: Dashboard, Trucks, Schools, Class Groups, Students
  - [ ] Add icons for each menu item
  - [ ] Style as persistent desktop sidebar
  - [ ] Highlight active route
- [ ] Task 3: Create Navbar component (AC: #1)
  - [ ] Create navbar component in `layouts/navbar/`
  - [ ] Display current user info
  - [ ] Add logout button
  - [ ] Add global search placeholder (for later implementation)
- [ ] Task 4: Create Dashboard page (AC: #2)
  - [ ] Create dashboard component in `features/dashboard/`
  - [ ] Add welcome message
  - [ ] Add placeholder stats cards
  - [ ] Use DaisyUI card components
- [ ] Task 5: Configure routing (AC: #1)
  - [ ] Set up AdminLayout as wrapper route
  - [ ] Configure dashboard as default route
  - [ ] Add placeholder routes for other modules
  - [ ] Apply AuthGuard to all admin routes
- [ ] Task 6: Implement keyboard navigation (AC: #5)
  - [ ] Ensure all links are focusable
  - [ ] Add visible focus rings
  - [ ] Test tab navigation through sidebar and navbar
- [ ] Task 7: Wire logout functionality (AC: #1)
  - [ ] Call AuthService.logout() on button click
  - [ ] Navigate to login page

## Dev Notes

### Architecture Compliance

- **AdminLayout in `layouts/`** - Wrapper component for authenticated routes
- **Sidebar in `layouts/sidebar/`** - Navigation component
- **Navbar in `layouts/navbar/`** - Top bar component
- **Dashboard in `features/dashboard/`** - Feature module (lazy-loaded)
- **OnPush change detection mandatory** for all components

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Angular | 21 | Standalone components |
| DaisyUI | Latest | Dark theme, drawer, navbar components |
| Angular Router | - | Lazy-loaded routes |

### Critical Rules

- **Lazy-load feature routes** under AdminLayout shell
- **OnPush change detection** for all components
- **No global loading overlays** - use local spinners only
- **Components are presentation-only** - services own state

### Routing Structure

```typescript
export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { 
        path: 'dashboard', 
        loadComponent: () => import('./features/dashboard/dashboard.component')
      },
      { path: 'trucks', loadChildren: () => import('./features/trucks/routes') },
      { path: 'schools', loadChildren: () => import('./features/schools/routes') },
      { path: 'class-groups', loadChildren: () => import('./features/class-groups/routes') },
      { path: 'students', loadChildren: () => import('./features/students/routes') },
    ]
  }
];
```

### Sidebar Navigation Items

| Label | Route | Icon (suggestion) |
|-------|-------|-------------------|
| Dashboard | `/dashboard` | Home |
| Trucks | `/trucks` | Truck |
| Schools | `/schools` | Building |
| Class Groups | `/class-groups` | Calendar |
| Students | `/students` | Users |

### File Structure

```
apps/frontend/src/app/
├── layouts/
│   ├── admin-layout/
│   │   ├── admin-layout.component.ts
│   │   └── admin-layout.component.html
│   ├── sidebar/
│   │   ├── sidebar.component.ts
│   │   └── sidebar.component.html
│   └── navbar/
│       ├── navbar.component.ts
│       └── navbar.component.html
├── features/
│   └── dashboard/
│       ├── dashboard.component.ts
│       └── dashboard.component.html
└── app.routes.ts
```

### Previous Story Dependencies

- **Story 1.3** provides: AuthService, AuthGuard, login flow

### Testing Requirements

- Component test: AdminLayout renders sidebar and navbar
- Component test: Sidebar navigation links work
- Component test: Logout button calls AuthService.logout()
- E2E test: Full navigation flow after login

### UI/UX Requirements

- DaisyUI dark theme throughout
- Persistent sidebar on desktop (not collapsible)
- Comfortable 8px spacing scale
- Dense, data-rich dashboard cards
- Visible focus rings on all interactive elements

### Accessibility Requirements (NFR9)

- All nav links are focusable via Tab key
- Visible focus rings on buttons and links
- Proper semantic HTML (nav, main, button)
- Readable typography

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Frontend Architecture]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Design System]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.4]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
