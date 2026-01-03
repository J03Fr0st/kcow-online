import type { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./student-list/student-list.component').then((m) => m.StudentListComponent),
        data: {
            breadcrumb: 'Students',
            breadcrumbIcon: '👥',
            title: 'Students',
            description: 'Manage and view all enrolled students',
        },
    },
    {
        path: 'create',
        loadComponent: () =>
            import('./create-student/create-student.page').then((m) => m.CreateStudentPage),
        data: {
            breadcrumb: 'Add Student',
            title: 'Add New Student',
            description: 'Create a new student record',
        },
    },
    {
        path: ':id',
        loadComponent: () =>
            import('./student-profile/student-profile.page').then((m) => m.StudentProfilePage),
        data: {
            breadcrumb: 'Student Profile',
            title: 'Student Profile',
            description: 'View student details',
        },
    },
    {
        path: ':id/edit',
        loadComponent: () =>
            import('./edit-student/edit-student.page').then((m) => m.EditStudentPage),
        data: {
            breadcrumb: 'Edit Student',
            title: 'Edit Student',
            description: 'Update student information',
        },
    },
];
