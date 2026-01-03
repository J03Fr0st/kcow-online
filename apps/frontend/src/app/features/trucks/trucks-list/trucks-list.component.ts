import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TruckService, type Truck } from '@core/services/truck.service';
import { NotificationService } from '@core/services/notification.service';
import { TruckFormComponent } from '../truck-form/truck-form.component';

@Component({
  selector: 'app-trucks-list',
  standalone: true,
  imports: [CommonModule, TruckFormComponent],
  templateUrl: './trucks-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrucksListComponent implements OnInit {
  protected truckService = inject(TruckService);
  private readonly notificationService = inject(NotificationService);

  // Local component state
  protected deletingId = signal<number | null>(null);
  protected editingId = signal<number | null>(null);
  private showFormFlag = signal<boolean>(false);

  // Computed properties from service
  protected trucks = computed(() => this.truckService.trucks());
  protected loading = computed(() => this.truckService.loading());
  
  // Computed: show form when editing or when create flag is set
  protected showForm = computed(() => this.editingId() !== null || this.showFormFlag());

  ngOnInit(): void {
    this.loadTrucks();
  }

  protected loadTrucks(): void {
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
   * Open edit form for a truck
   */
  protected openEditForm(truck: Truck): void {
    this.editingId.set(truck.id);
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
  protected onFormSubmit(event: CustomEvent<{ mode: 'create' | 'update'; truck?: Truck }>): void {
    const { mode, truck } = event.detail;

    if (mode === 'create') {
      this.notificationService.success('Truck created successfully');
      this.closeForm();
    } else if (mode === 'update' && truck) {
      this.notificationService.success('Truck updated successfully');
      this.closeForm();
    }
  }

  /**
   * Start delete flow for a truck
   */
  protected startDelete(truck: Truck): void {
    this.deletingId.set(truck.id);
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
  protected confirmDelete(truck: Truck): void {
    this.deletingId.set(null);

    this.truckService.deleteTruck(truck.id).subscribe({
      next: () => {
        this.notificationService.success('Truck archived successfully');
      },
      error: (err) => {
        console.error('Delete error:', err);
        this.notificationService.error('Failed to archive truck');
      },
    });
  }

  /**
   * Get status badge class
   */
  protected getStatusClass(status: string): string {
    switch (status) {
      case 'Active':
        return 'badge-success';
      case 'Maintenance':
        return 'badge-warning';
      case 'Retired':
        return 'badge-error';
      default:
        return 'badge-ghost';
    }
  }
}
