import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportService } from '@core/services/import.service';
import type { ImportAuditLog } from '@features/import/models/import-log.model';

@Component({
  selector: 'app-import-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './import-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImportListComponent implements OnInit {
  private readonly importService = inject(ImportService);

  protected readonly records = signal<ImportAuditLog[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly expandedId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadImports();
  }

  protected loadImports(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.importService.getRecentImports(50).subscribe({
      next: (records) => {
        this.records.set(records);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.detail || err.message || 'Failed to load import logs');
        this.isLoading.set(false);
      },
    });
  }

  protected toggleExpand(id: number): void {
    this.expandedId.update((current) => (current === id ? null : id));
  }

  protected getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Completed':
        return 'badge-success';
      case 'Failed':
        return 'badge-error';
      case 'Running':
        return 'badge-warning';
      default:
        return 'badge-ghost';
    }
  }

  protected formatDateTime(value: string | null): string {
    if (!value) return '-';
    try {
      return new Date(value).toLocaleString();
    } catch {
      return value;
    }
  }
}
