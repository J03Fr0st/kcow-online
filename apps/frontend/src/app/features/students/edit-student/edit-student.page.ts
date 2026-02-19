import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import type { Student } from '@core/services/student.service';
import { StudentFormComponent } from '../student-form/student-form.component';

@Component({
  selector: 'app-edit-student',
  standalone: true,
  imports: [CommonModule, StudentFormComponent],
  templateUrl: './edit-student.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditStudentPage {
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  protected readonly studentId = this.route.snapshot.paramMap.get('id');

  /**
   * Handle student saved - navigate back to list
   */
  protected onStudentSaved(student: Student): void {
    this.router.navigate(['/students'], {
      queryParams: { updated: student.id },
    });
  }

  /**
   * Handle cancel - navigate back to list
   */
  protected onCancel(): void {
    this.router.navigate(['/students']);
  }
}
