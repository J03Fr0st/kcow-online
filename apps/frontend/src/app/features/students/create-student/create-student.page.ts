import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StudentFormComponent } from '../student-form/student-form.component';
import { type Student } from '@core/services/student.service';

@Component({
    selector: 'app-create-student',
    standalone: true,
    imports: [CommonModule, StudentFormComponent],
    templateUrl: './create-student.page.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateStudentPage {
    private router = inject(Router);

    /**
     * Handle student saved - navigate back to list
     */
    protected onStudentSaved(student: Student): void {
        this.router.navigate(['/students'], {
            queryParams: { created: student.id },
        });
    }

    /**
     * Handle cancel - navigate back to list
     */
    protected onCancel(): void {
        this.router.navigate(['/students']);
    }
}
