import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  type OnInit,
  signal,
  type WritableSignal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { type Family, FamilyService } from '@core/services/family.service';
import { ModalService } from '@core/services/modal.service';
import { NotificationService } from '@core/services/notification.service';

interface ColumnConfig {
  key: keyof Family | 'studentCount';
  label: string;
  visible: WritableSignal<boolean>;
}

@Component({
  selector: 'app-family-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './family-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyListComponent implements OnInit {
  protected familyService = inject(FamilyService);
  private modalService = inject(ModalService);
  private notificationService = inject(NotificationService);

  // Column visibility state
  protected readonly columns: ColumnConfig[] = [
    { key: 'familyName', label: 'Family Name', visible: signal(true) },
    { key: 'primaryBillingContactId', label: 'Primary Contact', visible: signal(true) },
    { key: 'studentCount', label: 'Student Count', visible: signal(true) },
    { key: 'notes', label: 'Notes', visible: signal(false) },
  ];

  protected visibleColumns = computed(() => this.columns.filter((col) => col.visible()));

  // Column dropdown state
  protected showColumnMenu = signal<boolean>(false);

  // Local UI state
  protected sortAsc = signal<boolean>(true);
  protected processingIds = signal<Set<number>>(new Set());

  // Computed sorted families
  protected sortedFamilies = computed(() => {
    const families = this.familyService.families();
    const asc = this.sortAsc();

    return [...families].sort((a, b) => {
      const nameA = a.familyName.toLowerCase();
      const nameB = b.familyName.toLowerCase();

      if (nameA < nameB) return asc ? -1 : 1;
      if (nameA > nameB) return asc ? 1 : -1;
      return 0;
    });
  });

  ngOnInit(): void {
    this.familyService.getFamilies().subscribe();
  }

  protected toggleSort(): void {
    this.sortAsc.update((val) => !val);
  }

  protected toggleColumnMenu(): void {
    this.showColumnMenu.update((val) => !val);
  }

  protected toggleColumnVisibility(column: ColumnConfig): void {
    column.visible.update((v) => !v);
  }

  protected getCellValue(family: Family, key: keyof Family | 'studentCount'): string {
    if (key === 'studentCount') {
      // Use studentCount from API if available, otherwise show 0
      return String(family.studentCount ?? 0);
    }
    if (key === 'primaryBillingContactId') {
      // Find guardian name from guardians array
      const primaryGuardian = family.guardians?.find(
        (g) => g.id === family.primaryBillingContactId,
      );
      return primaryGuardian ? `${primaryGuardian.firstName} ${primaryGuardian.lastName}` : '-';
    }
    const value = family[key];
    if (value === null || value === undefined) return '-';
    if (typeof value === 'boolean') return value ? '✓' : '✗';
    return String(value);
  }

  /**
   * Deactivate a family after confirmation
   */
  protected async onDeactivateFamily(family: Family): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: 'Deactivate Family',
      message: `Are you sure you want to deactivate "${family.familyName}"? It will no longer be available for new student assignments.`,
      confirmText: 'Deactivate',
      cancelText: 'Cancel',
      confirmClass: 'btn-error',
      size: 'sm',
    });

    if (confirmed) {
      this.setProcessing(family.id, true);
      this.familyService.deactivateFamily(family.id, family).subscribe({
        next: () => {
          this.notificationService.success('Family deactivated successfully');
          this.setProcessing(family.id, false);
        },
        error: (err) => {
          this.notificationService.error(err.detail || 'Failed to deactivate family', 'Error');
          this.setProcessing(family.id, false);
        },
      });
    }
  }

  /**
   * Reactivate a family after confirmation
   */
  protected async onReactivateFamily(family: Family): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: 'Reactivate Family',
      message: `Are you sure you want to reactivate "${family.familyName}"? It will become available for new student assignments.`,
      confirmText: 'Reactivate',
      cancelText: 'Cancel',
      confirmClass: 'btn-success',
      size: 'sm',
    });

    if (confirmed) {
      this.setProcessing(family.id, true);
      this.familyService.reactivateFamily(family.id, family).subscribe({
        next: () => {
          this.notificationService.success('Family reactivated successfully');
          this.setProcessing(family.id, false);
        },
        error: (err) => {
          this.notificationService.error(err.detail || 'Failed to reactivate family', 'Error');
          this.setProcessing(family.id, false);
        },
      });
    }
  }

  private setProcessing(id: number, isProcessing: boolean): void {
    this.processingIds.update((set) => {
      const newSet = new Set(set);
      if (isProcessing) {
        newSet.add(id);
      } else {
        newSet.delete(id);
      }
      return newSet;
    });
  }
}
