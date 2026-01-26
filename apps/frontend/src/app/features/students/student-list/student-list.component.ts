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
    template: `
<div class="p-6 space-y-6">
  <!-- Header -->
  <div class="flex justify-between items-center">
    <div>
      <h1 class="text-2xl font-bold text-base-content">Students</h1>
      <p class="text-base-content/60">Manage and view all enrolled students</p>
    </div>
    <div class="flex items-center space-x-2">
      <button class="btn btn-primary" routerLink="create">
        <span class="mr-2" aria-hidden="true">➕</span> Add Student
      </button>
    </div>
  </div>

  <!-- Filters & Search -->
  <div class="card bg-base-100 shadow-sm border border-base-200">
    <div class="card-body p-4">
      <div class="flex flex-wrap items-center gap-4">
            <!-- School Filter -->
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">School</span></label>
              <select class="select select-sm select-bordered min-w-[200px]" 
                      [value]="schoolFilter() ?? 0"
                      (change)="onSchoolFilterChange(+$any($event.target).value)">
                <option [value]="0">All Schools</option>
                @for (school of schools(); track school.id) {
                  <option [value]="school.id">{{ school.name }}</option>
                }
              </select>
            </div>

            <!-- Class Group Filter -->
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Class Group</span></label>
              <select class="select select-sm select-bordered min-w-[200px]" 
                      [disabled]="!schoolFilter()"
                      [value]="classGroupFilter() ?? 0"
                      (change)="onClassGroupFilterChange(+$any($event.target).value)">
                <option [value]="0">All Class Groups</option>
                @for (cg of classGroups(); track cg.id) {
                  <option [value]="cg.id">{{ cg.name }}</option>
                }
              </select>
            </div>

        <!-- Global Search Placeholder (Actual search in Navbar) -->
        <div class="flex-1"></div>
        <div class="text-xs text-base-content/50 italic">
          Use global search above to find students across all criteria
        </div>
      </div>
    </div>
  </div>

  <!-- Students Table -->
  <div class="card bg-base-100 shadow-sm border border-base-200 overflow-hidden">
    <div class="overflow-x-auto">
      <table class="table table-zebra w-full">
        <thead>
          <tr class="bg-base-200/50">
            <th class="w-12"></th>
            <th class="cursor-pointer hover:bg-base-300 transition-colors" (click)="onSort('name')">
              <div class="flex items-center gap-2">
                Name
                <span class="text-xs" *ngIf="sortColumn() === 'name'">{{ sortDirection() === 'asc' ? '↑' : '↓' }}</span>
              </div>
            </th>
            <th class="cursor-pointer hover:bg-base-300 transition-colors" (click)="onSort('school')">
              <div class="flex items-center gap-2">
                School
                <span class="text-xs" *ngIf="sortColumn() === 'school'">{{ sortDirection() === 'asc' ? '↑' : '↓' }}</span>
              </div>
            </th>
            <th>Grade</th>
            <th>Class Group</th>
            <th>Status</th>
            <th class="w-20">Actions</th>
          </tr>
            </thead>
            <tbody>
              @for (student of students(); track student.id) {
                <tr class="hover:bg-base-200/30 cursor-pointer" (click)="navigateToProfile(student.id)">
                  <td>
                    <app-student-avatar [firstName]="student.firstName || ''" [lastName]="student.lastName || ''" [photoUrl]="student.photoUrl" size="sm" />
                  </td>
                  <td>
                    <div class="font-bold">{{ getStudentName(student) }}</div>
                    <div class="text-xs opacity-50">{{ student.reference }}</div>
                  </td>
                  <td>{{ getDisplayValue(student.schoolName) }}</td>
                  <td>
                    <span class="badge badge-ghost badge-sm">{{ getDisplayValue(student.grade) }}</span>
                  </td>
                  <td>{{ getDisplayValue(student.classGroupName) }}</td>
                  <td>
                    <span class="badge badge-sm" [class.badge-success]="student.isActive" [class.badge-ghost]="!student.isActive">
                      {{ student.status || (student.isActive ? 'Active' : 'Inactive') }}
                    </span>
                  </td>
                  <td (click)="$event.stopPropagation()">
                    <div class="flex items-center gap-1">
                      <a [routerLink]="[student.id, 'edit']" class="btn btn-ghost btn-xs btn-square" title="Edit">
                        ✏️
                      </a>
                    </div>
                  </td>
                </tr>
              }
              
              @if (studentService.isLoading()) {
                <tr>
                  <td colspan="7" class="text-center py-10">
                    <span class="loading loading-spinner loading-lg text-primary"></span>
                  </td>
                </tr>
              }
              
              @if (!studentService.isLoading() && students().length === 0) {
                <tr>
                  <td colspan="7" class="text-center py-10 text-base-content/50 italic">
                    No students found matching your criteria.
                  </td>
                </tr>
              }
            </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="p-4 flex flex-col sm:flex-row justify-between items-center gap-4 border-t border-base-200 bg-base-100">
      <div class="text-sm text-base-content/60">
        Showing <span class="font-medium text-base-content">{{ pageStart() }}</span> 
        to <span class="font-medium text-base-content">{{ pageEnd() }}</span> 
        of <span class="font-medium text-base-content">{{ studentService.totalCount() }}</span> students
      </div>
      
      <div class="join">
        <button class="join-item btn btn-sm" 
                [disabled]="currentPage() === 1"
                (click)="onPageChange(currentPage() - 1)">
          Previous
        </button>
        <button class="join-item btn btn-sm no-animation bg-base-200">
          Page {{ currentPage() }} of {{ totalPages() }}
        </button>
        <button class="join-item btn btn-sm"
                [disabled]="currentPage() === totalPages() || totalPages() === 0"
                (click)="onPageChange(currentPage() + 1)">
          Next
        </button>
      </div>
    </div>
  </div>
</div>
    `,
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
    protected classGroups = computed(() => {
        const groups = this.classGroupService.classGroups();
        return Array.isArray(groups) ? groups : [];
    });

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
        this.schoolService.getSchools().subscribe(schools => {
            this.schools.set(schools);
        });

        // Load all class groups for filter dropdown
        // Note: The service updates its internal signal, but we want our local one too
        this.classGroupService.loadClassGroups();
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
