import { Component, inject, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StudentService, type Student, type ProblemDetails } from '@core/services/student.service';
import { StudentAvatarComponent } from '@shared/components/student-avatar/student-avatar.component';

@Component({
    selector: 'app-student-profile',
    standalone: true,
    imports: [CommonModule, StudentAvatarComponent, RouterLink],
    templateUrl: './student-profile.page.html',
    styleUrls: ['./student-profile.page.scss'],
})
export class StudentProfilePage implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private studentService = inject(StudentService);
    private destroyRef = inject(DestroyRef);

    protected student: Student | null = null;
    protected isLoading = false;
    protected error: ProblemDetails | null = null;

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
        this.isLoading = true;
        this.error = null;

        this.studentService.getStudentById(id).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (student) => {
                this.student = student;
                this.isLoading = false;
            },
            error: (err: ProblemDetails) => {
                this.error = err;
                this.isLoading = false;
            },
        });
    }

    /**
     * Navigate back to list
     */
    protected goBack(): void {
        this.router.navigate(['/students']);
    }
}
