import { Component, inject, OnInit, DestroyRef, ChangeDetectionStrategy, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StudentService, type Student, type ProblemDetails } from '@core/services/student.service';
import { StudentAvatarComponent } from '@shared/components/student-avatar/student-avatar.component';
import { ChildInfoTabComponent } from './components/child-info-tab/child-info-tab.component';
import { FamilySectionComponent } from './components/family-section/family-section.component';
import { AttendanceTabComponent } from './components/attendance-tab/attendance-tab.component';

type TabId = 'child-info' | 'financial' | 'attendance' | 'evaluation';

interface Tab {
    id: TabId;
    label: string;
    icon: string;
}

@Component({
    selector: 'app-student-profile',
    standalone: true,
    imports: [CommonModule, StudentAvatarComponent, RouterLink, ChildInfoTabComponent, FamilySectionComponent, AttendanceTabComponent],
    templateUrl: './student-profile.page.html',
    styleUrls: ['./student-profile.page.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentProfilePage implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private studentService = inject(StudentService);
    private destroyRef = inject(DestroyRef);

    protected student: WritableSignal<Student | null> = signal(null);
    protected isLoading = signal(false);
    protected error = signal<ProblemDetails | null>(null);
    protected activeTab: WritableSignal<TabId> = signal('child-info');

    protected readonly tabs: Tab[] = [
        { id: 'child-info', label: 'Child Info', icon: '👤' },
        { id: 'financial', label: 'Financial', icon: '💰' },
        { id: 'attendance', label: 'Attendance', icon: '📋' },
        { id: 'evaluation', label: 'Evaluation', icon: '📊' },
    ];

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadStudent(+id);
        } else {
            this.router.navigate(['/students']);
        }
    }

    /**
     * Load student data
     */
    private loadStudent(id: number): void {
        this.isLoading.set(true);
        this.error.set(null);

        this.studentService.getStudentById(id).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (student) => {
                this.student.set(student);
                this.isLoading.set(false);
            },
            error: (err: ProblemDetails) => {
                this.error.set(err);
                this.isLoading.set(false);
            },
        });
    }

    /**
     * Set active tab
     */
    protected setActiveTab(tabId: TabId): void {
        this.activeTab.set(tabId);
    }

    /**
     * Check if tab is active
     */
    protected isTabActive(tabId: TabId): boolean {
        return this.activeTab() === tabId;
    }

    /**
     * Navigate back to list
     */
    protected goBack(): void {
        this.router.navigate(['/students']);
    }

    /**
     * Format date for display
     */
    protected formatDate(dateString?: string): string {
        if (!dateString) return '-';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString();
        } catch {
            return dateString;
        }
    }

    /**
     * Get display value with fallback
     */
    protected getDisplayValue(value: string | null | undefined): string {
        return value || '-';
    }

    /**
     * Handle student updated event from child info tab
     */
    protected onStudentUpdated(updatedStudent: Student): void {
        this.student.set(updatedStudent);
    }

    /**
     * Handle student updated event from family section
     */
    protected onFamilyUpdated(): void {
        // Reload student data to get updated family info
        const s = this.student();
        if (s) {
            this.loadStudent(s.id);
        }
    }
}
