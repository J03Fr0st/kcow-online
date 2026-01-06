import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ClassGroupService, type ClassGroup } from '@core/services/class-group.service';
import { SchoolService, type School } from '@core/services/school.service';
import { TruckService, type Truck } from '@core/services/truck.service';
import { NotificationService } from '@core/services/notification.service';
import { ClassGroupFormComponent } from '../class-group-form/class-group-form.component';
import { WeeklyScheduleComponent } from '../weekly-schedule/weekly-schedule.component';
import { getDayOfWeekName } from '../models/class-group.model';

type ViewMode = 'list' | 'weekly';

@Component({
  selector: 'app-class-groups-list',
  standalone: true,
  imports: [CommonModule, ClassGroupFormComponent, WeeklyScheduleComponent],
  templateUrl: './class-groups-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassGroupsListComponent implements OnInit {
  protected classGroupService = inject(ClassGroupService);
  private readonly schoolService = inject(SchoolService);
  private readonly truckService = inject(TruckService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Local component state
  protected deletingId = signal<number | null>(null);
  protected editingId = signal<number | null>(null);
  private showFormFlag = signal<boolean>(false);
  protected viewMode = signal<ViewMode>('list');

  // Filters
  protected filterSchoolId = signal<number | null>(null);
  protected filterTruckId = signal<number | null>(null);

  // Dropdown data for filters
  protected availableSchools = signal<School[]>([]);
  protected availableTrucks = signal<Truck[]>([]);

  // Computed properties from service
  protected classGroups = computed(() => {
    const groups = this.classGroupService.classGroups();
    // Sort by day of week, then by sequence
    return [...groups].sort((a, b) => {
      if (a.dayOfWeek !== b.dayOfWeek) {
        return a.dayOfWeek - b.dayOfWeek;
      }
      return a.sequence - b.sequence;
    });
  });
  protected loading = computed(() => this.classGroupService.loading());

  // Computed: show form when editing or when create flag is set
  protected showForm = computed(() => this.editingId() !== null || this.showFormFlag());

  ngOnInit(): void {
    this.loadClassGroups();
    this.loadSchools();
    this.loadTrucks();
  }

  protected loadClassGroups(): void {
    this.classGroupService.loadClassGroups(this.filterSchoolId() ?? undefined, this.filterTruckId() ?? undefined);
  }

  /**
   * Load schools for filter dropdown
   */
  private loadSchools(): void {
    this.schoolService.getSchools().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (schools) => this.availableSchools.set(schools.filter((s) => s.isActive)),
      error: (err) => {
        console.error('Error loading schools:', err);
        this.notificationService.error('Failed to load schools');
      },
    });
  }

  /**
   * Load trucks for filter dropdown
   */
  private loadTrucks(): void {
    this.truckService.trucks.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (trucks) => this.availableTrucks.set(trucks),
      error: (err) => {
        console.error('Error loading trucks:', err);
        this.notificationService.error('Failed to load trucks');
      },
    });
    this.truckService.loadTrucks();
  }

  /**
   * Open create form
   */
  protected openCreateForm(): void {
    this.editingId.set(null); // null = create mode
    this.showFormFlag.set(true);
  }

  /**
   * Open edit form for a class group
   */
  protected openEditForm(classGroup: ClassGroup): void {
    this.editingId.set(classGroup.id);
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
  protected onFormSubmit(event: CustomEvent<{ mode: 'create' | 'update'; classGroup?: ClassGroup }>): void {
    const { mode, classGroup } = event.detail;

    if (mode === 'create') {
      this.notificationService.success('Class group created successfully');
      this.closeForm();
    } else if (mode === 'update' && classGroup) {
      this.notificationService.success('Class group updated successfully');
      this.closeForm();
    }
  }

  /**
   * Start delete flow for a class group
   */
  protected startDelete(classGroup: ClassGroup): void {
    this.deletingId.set(classGroup.id);
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
  protected confirmDelete(classGroup: ClassGroup): void {
    this.deletingId.set(null);

    this.classGroupService.deleteClassGroup(classGroup.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.success('Class group archived successfully');
      },
      error: (err) => {
        console.error('Delete error:', err);
        this.notificationService.error('Failed to archive class group');
      },
    });
  }

  /**
   * Handle school filter change
   */
  protected onSchoolFilter(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterSchoolId.set(select.value ? Number(select.value) : null);
    this.loadClassGroups();
  }

  /**
   * Handle truck filter change
   */
  protected onTruckFilter(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterTruckId.set(select.value ? Number(select.value) : null);
    this.loadClassGroups();
  }

  /**
   * Get day of week name
   */
  protected getDayOfWeekName(dayNumber: number): string {
    return getDayOfWeekName(dayNumber);
  }

  /**
   * Format time for display
   */
  protected formatTime(time: string): string {
    return time.substring(0, 5); // "HH:mm:ss" -> "HH:mm"
  }

  /**
   * Switch to list view
   */
  protected showListView(): void {
    this.viewMode.set('list');
  }

  /**
   * Switch to weekly view
   */
  protected showWeeklyView(): void {
    this.viewMode.set('weekly');
  }

  /**
   * Check if list view is active
   */
  protected isListView(): boolean {
    return this.viewMode() === 'list';
  }

  /**
   * Check if weekly view is active
   */
  protected isWeeklyView(): boolean {
    return this.viewMode() === 'weekly';
  }
}
