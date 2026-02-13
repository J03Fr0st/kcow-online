import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EvaluationService } from '@core/services/evaluation.service';
import type { Evaluation } from '@features/evaluations/models/evaluation.model';

@Component({
  selector: 'app-evaluations-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './evaluations-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EvaluationsListComponent implements OnInit {
  private readonly evaluationService = inject(EvaluationService);

  protected readonly records = signal<Evaluation[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly error = signal<string | null>(null);

  // Filter state
  protected readonly filterFromDate = signal('');
  protected readonly filterToDate = signal('');

  protected readonly filteredRecords = computed(() => {
    let data = [...this.records()];
    const from = this.filterFromDate();
    const to = this.filterToDate();

    if (from) {
      data = data.filter((r) => r.evaluationDate >= from);
    }
    if (to) {
      data = data.filter((r) => r.evaluationDate <= to);
    }

    return data.sort(
      (a, b) => new Date(b.evaluationDate).getTime() - new Date(a.evaluationDate).getTime()
    );
  });

  ngOnInit(): void {
    this.loadEvaluations();
  }

  protected loadEvaluations(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.evaluationService.getAllEvaluations().subscribe({
      next: (records) => {
        this.records.set(records);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.detail || err.message || 'Failed to load evaluations');
        this.isLoading.set(false);
      },
    });
  }

  protected onFromDateChange(value: string): void {
    this.filterFromDate.set(value);
  }

  protected onToDateChange(value: string): void {
    this.filterToDate.set(value);
  }

  protected clearFilters(): void {
    this.filterFromDate.set('');
    this.filterToDate.set('');
  }

  protected formatScore(value: number | null | undefined): string {
    return value != null ? value.toString() : '-';
  }
}
