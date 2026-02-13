import type { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';

export const routes: Routes = [
  // Public route: Login page (no guard)
  {
    path: 'login',
    loadComponent: () =>
      import('@features/auth/login.component').then((m) => m.LoginComponent),
    data: {
      title: 'Admin Login',
      description: 'Sign in to access the admin dashboard',
      keywords: 'login, sign in, authentication',
    },
  },
  // Protected routes: Require authentication
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@layouts/admin-layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    data: {
      breadcrumb: 'Home',
      breadcrumbIcon: '🏠',
    },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('@features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
        data: {
          breadcrumb: 'Dashboard',
          breadcrumbIcon: '📊',
          title: 'Dashboard',
          description: 'Application dashboard with key metrics and insights',
          keywords: 'dashboard, analytics, metrics, overview',
        },
      },
      {
        path: 'tables',
        loadComponent: () =>
          import('@features/tables/tables.component').then((m) => m.TablesComponent),
        data: {
          breadcrumb: 'Tables',
          breadcrumbIcon: '📋',
          title: 'Data Tables',
          description: 'Interactive data tables with sorting and filtering',
          keywords: 'tables, data, sorting, filtering',
        },
      },
      {
        path: 'forms',
        loadComponent: () =>
          import('@features/forms/forms.component').then((m) => m.FormsComponent),
        data: {
          breadcrumb: 'Forms',
          breadcrumbIcon: '📝',
          title: 'Forms',
          description: 'Form examples with validation and reactive patterns',
          keywords: 'forms, validation, input, reactive forms',
        },
      },
      {
        path: 'workspace-settings',
        loadComponent: () =>
          import('@features/workspace-settings/workspace-settings.component').then(
            (m) => m.WorkspaceSettingsComponent,
          ),
        data: {
          breadcrumb: 'Workspace Settings',
          breadcrumbIcon: '⚙️',
          title: 'Workspace Settings',
          description: 'Customize your workspace appearance, notifications, and preferences',
          keywords: 'settings, preferences, theme, notifications, workspace',
        },
      },
      {
        path: 'system-health',
        loadComponent: () =>
          import('@features/system-health/system-health.component').then(
            (m) => m.SystemHealthComponent,
          ),
        data: {
          breadcrumb: 'System Health',
          breadcrumbIcon: '🏥',
          title: 'System Health Monitor',
          description:
            'Real-time monitoring of application performance, metrics, and system health',
          keywords: 'health, monitoring, metrics, performance, alerts',
        },
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('@features/notifications/notifications.component').then(
            (m) => m.NotificationsComponent,
          ),
        data: {
          breadcrumb: 'Notifications',
          breadcrumbIcon: '🔔',
          title: 'Notification System',
          description:
            'Interactive toast notifications with multiple types and customizable options',
          keywords: 'notifications, toasts, alerts, messages',
        },
      },
      {
        path: 'modals',
        loadComponent: () =>
          import('@features/modals/modals.component').then((m) => m.ModalsComponent),
        data: {
          breadcrumb: 'Modals',
          breadcrumbIcon: '🪟',
          title: 'Modal & Dialog System',
          description:
            'Comprehensive modal dialogs with dynamic content, confirmation dialogs, and keyboard shortcuts',
          keywords: 'modals, dialogs, popups, confirmations, alerts',
        },
      },
      {
        path: 'error-handling',
        loadComponent: () =>
          import('@features/error-handling-demo/error-handling-demo.component').then(
            (m) => m.ErrorHandlingDemoComponent,
          ),
        data: {
          breadcrumb: 'Error Handling',
          breadcrumbIcon: '🚨',
          title: 'Error Handling Framework',
          description:
            'Comprehensive error handling with global interceptor, error logging, boundaries, and user-friendly error pages',
          keywords: 'error handling, error logging, error boundary, error pages, interceptor',
        },
      },
      {
        path: 'schools',
        loadChildren: () =>
          import('@features/schools/schools.routes').then((m) => m.routes),
      },
      {
        path: 'students',
        loadChildren: () =>
          import('@features/students/students.routes').then((m) => m.routes),
      },
      {
        path: 'trucks',
        loadComponent: () =>
          import('@features/trucks/trucks-list/trucks-list.component').then((m) => m.TrucksListComponent),
        data: {
          breadcrumb: 'Trucks',
          breadcrumbIcon: '🚚',
          title: 'Trucks Management',
          description: 'Manage and view all fleet trucks',
          keywords: 'trucks, fleet, vehicles, management',
        },
      },
      {
        path: 'families',
        loadChildren: () =>
          import('@features/families/families.routes').then((m) => m.routes),
      },
      {
        path: 'class-groups',
        loadComponent: () =>
          import('@features/class-groups/class-groups-list/class-groups-list.component').then(
            (m) => m.ClassGroupsListComponent,
          ),
        data: {
          breadcrumb: 'Class Groups',
          breadcrumbIcon: '📚',
          title: 'Class Groups',
          description: 'Manage and view all class groups',
        },
      },
      {
        path: 'activities',
        loadComponent: () =>
          import('@features/activities/activities-list/activities-list.component').then((m) => m.ActivitiesListComponent),
        data: {
          breadcrumb: 'Activities',
          breadcrumbIcon: '🧩',
          title: 'Activities Management',
          description: 'Manage and view all educational activities',
          keywords: 'activities, education, programs, management',
        },
      },
      {
        path: 'attendance',
        loadComponent: () =>
          import('@features/attendance/attendance-list.component').then(
            (m) => m.AttendanceListComponent,
          ),
        data: {
          breadcrumb: 'Attendance',
          breadcrumbIcon: '✅',
          title: 'Attendance',
          description: 'View and manage attendance records',
        },
      },
      {
        path: 'evaluations',
        loadComponent: () =>
          import('@features/evaluations/evaluations-list.component').then(
            (m) => m.EvaluationsListComponent,
          ),
        data: {
          breadcrumb: 'Evaluations',
          breadcrumbIcon: '📊',
          title: 'Evaluations',
          description: 'Student evaluations and progress tracking',
        },
      },
      {
        path: 'billing',
        loadComponent: () =>
          import('@features/billing/billing-overview.component').then(
            (m) => m.BillingOverviewComponent,
          ),
        data: {
          breadcrumb: 'Billing',
          breadcrumbIcon: '💰',
          title: 'Billing',
          description: 'Student billing, invoices, and payments',
        },
      },
      {
        path: 'import',
        loadComponent: () =>
          import('@features/import/import-list.component').then(
            (m) => m.ImportListComponent,
          ),
        data: {
          breadcrumb: 'Import',
          breadcrumbIcon: '📥',
          title: 'Import',
          description: 'Legacy data import audit log',
        },
      },
    ],
  },
  // Error pages - outside admin layout for full-screen display
  // These are public routes (no auth required)
  {
    path: 'error',
    children: [
      {
        path: '404',
        loadComponent: () =>
          import('@features/error-pages/not-found/not-found.component').then(
            (m) => m.NotFoundComponent,
          ),
        data: {
          title: '404 - Page Not Found',
          description: 'The requested page could not be found',
        },
      },
      {
        path: '403',
        loadComponent: () =>
          import('@features/error-pages/forbidden/forbidden.component').then(
            (m) => m.ForbiddenComponent,
          ),
        data: {
          title: '403 - Access Forbidden',
          description: 'You do not have permission to access this resource',
        },
      },
      {
        path: '500',
        loadComponent: () =>
          import('@features/error-pages/server-error/server-error.component').then(
            (m) => m.ServerErrorComponent,
          ),
        data: {
          title: '500 - Server Error',
          description: 'An internal server error occurred',
        },
      },
    ],
  },
  // Catch all route - redirect to 404
  {
    path: '**',
    loadComponent: () =>
      import('@features/error-pages/not-found/not-found.component').then(
        (m) => m.NotFoundComponent,
      ),
  },
];
