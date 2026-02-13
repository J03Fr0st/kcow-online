import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivityService } from '@core/services/activity.service';
import type { Activity } from '@features/activities/models/activity.model';
import { NotificationService } from '@core/services/notification.service';
import { ActivityFormComponent } from '../activity-form/activity-form.component';

@Component({
  selector: 'app-activities-list',
  standalone: true,
  imports: [CommonModule, ActivityFormComponent],
  templateUrl: './activities-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivitiesListComponent implements OnInit {
  protected activityService = inject(ActivityService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Local component state
  protected deletingId = signal<number | null>(null);
  protected editingId = signal<number | null>(null);
  private showFormFlag = signal<boolean>(false);

  // Computed properties from service
  protected activities = computed(() => this.activityService.activities());
  protected loading = computed(() => this.activityService.loading());

  // Computed: show form when editing or when create flag is set
  protected showForm = computed(() => this.editingId() !== null || this.showFormFlag());

  ngOnInit(): void {
    this.loadActivities();
  }

  protected loadActivities(): void {
    this.activityService.loadActivities();
  }

  /**
   * Open create form
   */
  protected openCreateForm(): void {
    this.editingId.set(null); // null = create mode
    this.showFormFlag.set(true);
  }

  /**
   * Open edit form for an activity
   */
  protected openEditForm(activity: Activity): void {
    this.editingId.set(activity.id);
    this.showFormFlag.set(false); // Clear create flag when editing
  }

  /**
   * Close form (create or edit)
   */
  protected closeForm(): void {
    this.editingId.set(null);
    this.showFormFlag.set(false);
  }

  /**
   * Handle form submission (create or update)
   */
  protected onFormSubmit(event: CustomEvent<{ mode: 'create' | 'update'; activity?: Activity }>): void {
    const { mode, activity } = event.detail;

    if (mode === 'create') {
      this.notificationService.success('Activity created successfully');
      this.closeForm();
    } else if (mode === 'update' && activity) {
      this.notificationService.success('Activity updated successfully');
      this.closeForm();
    }
  }

  /**
   * Start delete flow for an activity
   */
  protected startDelete(activity: Activity): void {
    this.deletingId.set(activity.id);
  }

  /**
   * Cancel delete operation
   */
  protected cancelDelete(): void {
    this.deletingId.set(null);
  }

  /**
   * Confirm delete operation
   */
  protected confirmDelete(activity: Activity): void {
    this.deletingId.set(null);

    this.activityService.deleteActivity(activity.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.success('Activity archived successfully');
      },
      error: (err) => {
        console.error('Delete error:', err);
        this.notificationService.error('Failed to archive activity');
      },
    });
  }

  /**
   * Get icon source (base64 or placeholder)
   */
  protected getIconSrc(activity: Activity): string {
    if (activity.icon) {
      // Detect image type from base64 header bytes
      // BMP: starts with "Qk" (base64 of 0x42 0x4D)
      // JPEG: starts with "/9j/" (base64 of 0xFF 0xD8 0xFF)
      // PNG: starts with "iVBOR" (base64 of 0x89 0x50 0x4E 0x47)
      if (activity.icon.startsWith('Qk')) {
        return `data:image/bmp;base64,${activity.icon}`;
      }
      if (activity.icon.startsWith('/9j/')) {
        return `data:image/jpeg;base64,${activity.icon}`;
      }
      return `data:image/png;base64,${activity.icon}`;
    }
    return 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 40 40"%3E%3Crect width="40" height="40" fill="%23e5e7eb"/%3E%3Ctext x="50%25" y="50%25" text-anchor="middle" dy=".3em" font-size="20" fill="%239ca3af"%3E📄%3C/text%3E%3C/svg%3E';
  }
}
