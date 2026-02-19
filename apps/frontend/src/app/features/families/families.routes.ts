import type { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./family-list/family-list.component').then((m) => m.FamilyListComponent),
    data: {
      breadcrumb: 'Families',
      breadcrumbIcon: '👨‍👩‍👧‍👦',
      title: 'Families',
      description: 'Manage families and guardians',
    },
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./family-form/family-form.component').then((m) => m.FamilyFormComponent),
    data: {
      breadcrumb: 'Add Family',
      breadcrumbIcon: '➕',
      title: 'Add Family',
      description: 'Create a new family registry entry',
    },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./family-detail/family-detail.component').then((m) => m.FamilyDetailComponent),
    data: {
      breadcrumb: 'Family Details',
      breadcrumbIcon: '👁️',
      title: 'Family Details',
      description: 'View family information and linked students',
    },
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./family-form/family-form.component').then((m) => m.FamilyFormComponent),
    data: {
      breadcrumb: 'Edit Family',
      breadcrumbIcon: '✏️',
      title: 'Edit Family',
      description: 'Update family information',
    },
  },
];
