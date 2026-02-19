import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  type OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivityService } from '@core/services/activity.service';
import { EvaluationService } from '@core/services/evaluation.service';
import { NotificationService } from '@core/services/notification.service';
import type { Activity } from '@features/activities/models/activity.model';
import type {
  CreateEvaluationRequest,
  Evaluation,
  UpdateEvaluationRequest,
} from '@features/evaluations/models/evaluation.model';

interface EditingEvaluation {
  id: number;
  score: number | null;
  speedMetric: number | null;
  accuracyMetric: number | null;
  notes: string;
}

interface NewEvaluationForm {
  activityId: number;
  evaluationDate: string;
  score: number | null;
  speedMetric: number | null;
  accuracyMetric: number | null;
  notes: string;
}

@Component({
  selector: 'app-evaluation-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './evaluation-tab.component.html',
  styleUrls: ['./evaluation-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EvaluationTabComponent implements OnInit {
  private readonly evaluationService = inject(EvaluationService);
  private readonly activityService = inject(ActivityService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Activities signal
  readonly activities = signal<Activity[]>([]);
  readonly isLoadingActivities = signal<boolean>(false);

  // Input signal for student ID
  readonly studentId = input.required<number>();

  // State signals
  readonly evaluationRecords = signal<Evaluation[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  // Edit state
  readonly editingId = signal<number | null>(null);
  readonly editingForm = signal<EditingEvaluation | null>(null);

  // Add form state
  readonly showAddForm = signal<boolean>(false);
  readonly newEvaluationForm = signal<NewEvaluationForm>({
    activityId: 0,
    evaluationDate: new Date().toISOString().split('T')[0], // Default to today
    score: null,
    speedMetric: null,
    accuracyMetric: null,
    notes: '',
  });

  // Computed: sorted evaluation records by date descending
  readonly sortedRecords = computed(() => {
    return [...this.evaluationRecords()].sort((a, b) => {
      return new Date(b.evaluationDate).getTime() - new Date(a.evaluationDate).getTime();
    });
  });

  ngOnInit(): void {
    this.loadEvaluations();
    this.loadActivities();
  }

  /**
   * Load evaluation records for the student
   */
  private loadEvaluations(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.evaluationService
      .getEvaluations(this.studentId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (records) => {
          this.evaluationRecords.set(Array.isArray(records) ? records : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load evaluation records');
          this.isLoading.set(false);
          console.error('Error loading evaluations:', err);
        },
      });
  }

  /**
   * Load available activities for the dropdown
   */
  private loadActivities(): void {
    this.isLoadingActivities.set(true);

    // Use the loading signal from the service
    this.activityService.loadActivities();
    // Copy the activities from the service signal to our component signal
    this.activities.set(this.activityService.activities());
    this.isLoadingActivities.set(this.activityService.loading());
  }

  /**
   * Start editing an evaluation record
   */
  startEdit(record: Evaluation): void {
    this.editingId.set(record.id);
    this.editingForm.set({
      id: record.id,
      score: record.score ?? null,
      speedMetric: record.speedMetric ?? null,
      accuracyMetric: record.accuracyMetric ?? null,
      notes: record.notes ?? '',
    });
  }

  /**
   * Cancel editing
   */
  cancelEdit(): void {
    this.editingId.set(null);
    this.editingForm.set(null);
  }

  /**
   * Cancel add form
   */
  cancelAdd(): void {
    this.showAddForm.set(false);
    this.newEvaluationForm.set({
      activityId: 0,
      evaluationDate: new Date().toISOString().split('T')[0],
      score: null,
      speedMetric: null,
      accuracyMetric: null,
      notes: '',
    });
  }

  /**
   * Show add form
   */
  showAddEvaluationForm(): void {
    this.showAddForm.set(true);
  }

  /**
   * Save edited evaluation
   */
  protected saveEdit(): void {
    const form = this.editingForm();
    if (!form) return;

    this.isSaving.set(true);

    const updateData: UpdateEvaluationRequest = {
      score: form.score,
      speedMetric: form.speedMetric,
      accuracyMetric: form.accuracyMetric,
      notes: form.notes || null,
    };

    this.evaluationService
      .updateEvaluation(form.id, updateData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.updateRecordInList(updated);
          this.editingId.set(null);
          this.editingForm.set(null);
          this.isSaving.set(false);
          this.showSuccessMessage('Evaluation updated successfully');
        },
        error: (err) => {
          this.isSaving.set(false);
          this.showErrorMessage('Failed to update evaluation');
          console.error('Error updating evaluation:', err);
        },
      });
  }

  /**
   * Add new evaluation record
   */
  protected addEvaluation(): void {
    const form = this.newEvaluationForm();

    if (!form.evaluationDate || !form.activityId) {
      this.showErrorMessage('Please fill in all required fields');
      return;
    }

    this.isSaving.set(true);

    const request: CreateEvaluationRequest = {
      studentId: this.studentId(),
      activityId: form.activityId,
      evaluationDate: form.evaluationDate,
      score: form.score,
      speedMetric: form.speedMetric,
      accuracyMetric: form.accuracyMetric,
      notes: form.notes || null,
    };

    this.evaluationService
      .createEvaluation(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (created) => {
          this.evaluationRecords.set([...this.evaluationRecords(), created]);
          this.cancelAdd();
          this.isSaving.set(false);
          this.showSuccessMessage('Evaluation added successfully');
        },
        error: (err) => {
          this.isSaving.set(false);
          this.showErrorMessage('Failed to add evaluation');
          console.error('Error adding evaluation:', err);
        },
      });
  }

  /**
   * Update a record in the list after edit
   */
  private updateRecordInList(updated: Evaluation): void {
    const current = this.evaluationRecords();
    const index = current.findIndex((r) => r.id === updated.id);
    if (index !== -1) {
      const updatedList = [...current];
      updatedList[index] = updated;
      this.evaluationRecords.set(updatedList);
    }
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString();
    } catch {
      return dateString;
    }
  }

  /**
   * Get score class for chip styling
   */
  getScoreClass(score: number | undefined): string {
    if (score === undefined || score === null) return 'badge-ghost';

    if (score >= 80) return 'badge-success';
    if (score >= 60) return 'badge-warning';
    return 'badge-error';
  }

  /**
   * Get score level text
   */
  getScoreLevel(score: number | undefined): string {
    if (score === undefined || score === null) return 'N/A';

    if (score >= 80) return 'Excellent';
    if (score >= 60) return 'Good';
    return 'Needs Work';
  }

  /**
   * Update edit form field
   */
  updateEditForm(field: keyof EditingEvaluation, value: string | number | null): void {
    const current = this.editingForm();
    if (current) {
      this.editingForm.set({ ...current, [field]: value });
    }
  }

  /**
   * Handle blur event during editing - save if form is valid
   */
  protected onEditBlur(): void {
    // Only save if edit mode is still active (prevents double-save with Enter key)
    if (this.editingId() !== null) {
      this.saveEdit();
    }
  }

  /**
   * Update new evaluation form field
   */
  updateNewForm(field: keyof NewEvaluationForm, value: string | number | null): void {
    this.newEvaluationForm.set({ ...this.newEvaluationForm(), [field]: value });
  }

  /**
   * Handle activity dropdown change
   */
  onActivityChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.updateNewForm('activityId', value ? +value : 0);
  }

  /**
   * Handle date input change
   */
  onDateChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateNewForm('evaluationDate', value);
  }

  /**
   * Handle score input change for new form
   */
  onScoreInputChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateNewForm('score', value ? +value : null);
  }

  /**
   * Handle speed metric input change for new form
   */
  onSpeedChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateNewForm('speedMetric', value ? +value : null);
  }

  /**
   * Handle accuracy metric input change for new form
   */
  onAccuracyChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateNewForm('accuracyMetric', value ? +value : null);
  }

  /**
   * Handle notes input change for new form
   */
  onNotesChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateNewForm('notes', value);
  }

  /**
   * Handle edit score input change
   */
  onEditScoreChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateEditForm('score', value ? +value : null);
  }

  /**
   * Handle edit speed input change
   */
  onEditSpeedChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateEditForm('speedMetric', value ? +value : null);
  }

  /**
   * Handle edit accuracy input change
   */
  onEditAccuracyChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateEditForm('accuracyMetric', value ? +value : null);
  }

  /**
   * Handle edit notes input change
   */
  onEditNotesChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.updateEditForm('notes', value);
  }

  /**
   * Check if edit form is valid
   */
  isEditFormValid(): boolean {
    const form = this.editingForm();
    return form !== null;
  }

  /**
   * Check if new form is valid
   */
  isNewFormValid(): boolean {
    const form = this.newEvaluationForm();
    return form.evaluationDate !== '' && form.activityId > 0;
  }

  /**
   * Show success message
   */
  private showSuccessMessage(message: string): void {
    this.notificationService.success(message, undefined, 3000);
  }

  /**
   * Show error message
   */
  private showErrorMessage(message: string): void {
    this.notificationService.error(message, undefined, 5000);
  }
}
