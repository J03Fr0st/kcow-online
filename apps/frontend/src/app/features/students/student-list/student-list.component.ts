import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { StudentService, type StudentListItem } from '@core/services/student.service';
import { StudentAvatarComponent } from '@shared/components/student-avatar/student-avatar.component';
import { SchoolService, type School } from '@core/services/school.service';
import { ClassGroupService, type ClassGroup } from '@core/services/class-group.service';

interface SortConfig {
    key: 'name' | 'school';
    label: string;
    direction: WritableSignal<'asc' | 'desc' | null>;
}

@Component({
    selector: 'app-student-list',
    standalone: true,
    imports: [CommonModule, RouterLink, StudentAvatarComponent],
    templateUrl: './student-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentListComponent implements OnInit {
    protected studentService = inject(StudentService);
    private readonly schoolService = inject(SchoolService);
    private readonly classGroupService = inject(ClassGroupService);
    private readonly router = inject(Router);

    // Pagination state
    protected currentPage = signal<number>(1);
    protected pageSize = signal<number>(25);

    // Filter state
    protected schoolFilter = signal<number | null>(null);
    protected classGroupFilter = signal<number | null>(null);

    // Filter options
    protected schools = signal<School[]>([]);
    protected classGroups = signal<ClassGroup[]>([]);

    // Sorting state
    protected sortColumn = signal<'name' | 'school' | null>(null);
    protected sortDirection = signal<'asc' | 'desc'>('asc');

    // Sort configurations
    protected readonly sortableColumns: SortConfig[] = [
        { key: 'name', label: 'Name', direction: signal<'asc' | 'desc' | null>(null) },
        { key: 'school', label: 'School', direction: signal<'asc' | 'desc' | null>(null) },
    ];

    // Computed students from service (server-side sorted)
    protected students = computed(() => this.studentService.students());

    // Computed pagination info
    protected totalPages = computed(() => {
        return Math.ceil(this.studentService.totalCount() / this.pageSize());
    });

    protected pageStart = computed(() => {
        return (this.currentPage() - 1) * this.pageSize() + 1;
    });

    protected pageEnd = computed(() => {
        const end = this.currentPage() * this.pageSize();
        const total = this.studentService.totalCount();
        return end > total ? total : end;
    });

    ngOnInit(): void {
        this.loadFilterOptions();
        this.loadStudents();
    }

    private loadFilterOptions(): void {
        // Load schools for filter dropdown
        this.schoolService.getAllSchools().subscribe(schools => {
            this.schools.set(schools);
        });

        // Load class groups for filter dropdown
        this.classGroupService.getAllClassGroups().subscribe(classGroups => {
            this.classGroups.set(classGroups);
        });
    }

    protected loadStudents(): void {
        this.studentService.getStudents({
            page: this.currentPage(),
            pageSize: this.pageSize(),
            schoolId: this.schoolFilter() ?? undefined,
            classGroupId: this.classGroupFilter() ?? undefined,
            sortBy: this.sortColumn() || undefined,
            sortDirection: this.sortDirection(),
        }).subscribe();
    }

    protected onSchoolFilterChange(schoolId: number): void {
        this.schoolFilter.set(schoolId === 0 ? null : schoolId);
        this.classGroupFilter.set(null); // Reset class group when school changes
        this.currentPage.set(1);
        this.loadStudents();
    }

    protected onClassGroupFilterChange(classGroupId: number): void {
        this.classGroupFilter.set(classGroupId === 0 ? null : classGroupId);
        this.currentPage.set(1);
        this.loadStudents();
    }

    /**
     * Handle sort column click
     */
    protected onSort(columnKey: 'name' | 'school'): void {
        // If clicking the same column, toggle direction
        if (this.sortColumn() === columnKey) {
            this.sortDirection.update(dir => dir === 'asc' ? 'desc' : 'asc');
        } else {
            // New column - set to ascending
            this.sortColumn.set(columnKey);
            this.sortDirection.set('asc');
        }

        // Update sort config UI indicators
        this.sortableColumns.forEach(col => {
            if (col.key === columnKey) {
                col.direction.set(this.sortDirection());
            } else {
                col.direction.set(null);
            }
        });

        // Trigger reload from server
        this.loadStudents();
    }

    /**
     * Handle page change
     */
    protected onPageChange(page: number): void {
        if (page < 1 || page > this.totalPages()) return;
        this.currentPage.set(page);
        this.loadStudents();
    }

    /**
     * Get display name for student
     */
    protected getStudentName(student: StudentListItem): string {
        return `${student.firstName} ${student.lastName}`;
    }

    /**
     * Get value for display, handling null/undefined
     */
    protected getDisplayValue(value: string | null | undefined): string {
        return value || '-';
    }

    /**
     * Navigate to student profile page
     */
    protected navigateToProfile(studentId: number): void {
        this.router.navigate(['/students', studentId]);
    }
}
