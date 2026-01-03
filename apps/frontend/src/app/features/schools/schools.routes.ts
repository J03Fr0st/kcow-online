import type { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./school-list.component').then((m) => m.SchoolListComponent),
        data: {
            breadcrumb: 'Schools',
            breadcrumbIcon: '🏫',
            title: 'School Registry',
            description: 'Manage and view all registered schools',
        },
    },
    {
        path: 'create',
        loadComponent: () =>
            import('./school-form/school-form.component').then((m) => m.SchoolFormComponent),
        data: {
            breadcrumb: 'Add School',
            breadcrumbIcon: '➕',
            title: 'Add School',
            description: 'Create a new school registry entry',
        },
    },
    {
        path: 'edit/:id',
        loadComponent: () =>
            import('./school-form/school-form.component').then((m) => m.SchoolFormComponent),
        data: {
            breadcrumb: 'Edit School',
            breadcrumbIcon: '✏️',
            title: 'Edit School',
            description: 'Update school information',
        },
    },
];
