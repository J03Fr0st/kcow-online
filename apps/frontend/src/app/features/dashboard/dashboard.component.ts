import { CommonModule } from '@angular/common';
import { Component, inject, type OnInit, signal } from '@angular/core';
import { MockDataService } from '@core/services/mock-data.service';
import type { RecentActivity } from '@models/recent-activity.model';
import type { StatCard } from '@models/stats.model';
import { LoadingSpinnerComponent } from '@shared/components/loading-spinner/loading-spinner.component';
import { StatCardComponent } from '@shared/components/stat-card/stat-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, StatCardComponent, LoadingSpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  private mockDataService = inject(MockDataService);

  stats = signal<StatCard[]>([]);
  recentActivities = signal<RecentActivity[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  retryLoad(): void {
    this.loading.set(true);
    this.error.set(null);
    this.loadData();
  }

  private loadData(): void {
    this.loadStats();
    this.loadRecentActivities();
  }

  private loadStats(): void {
    this.mockDataService.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.checkLoading();
      },
      error: () => {
        this.error.set('Failed to load dashboard data. Please try again.');
        this.loading.set(false);
      },
    });
  }

  private loadRecentActivities(): void {
    this.mockDataService.getRecentActivities().subscribe({
      next: (data) => {
        this.recentActivities.set(data);
        this.checkLoading();
      },
      error: () => {
        // Non-critical, just log or ignore
        console.error('Failed to load recent activities');
      },
    });
  }

  private checkLoading(): void {
    if (this.stats().length > 0) {
      this.loading.set(false);
    }
  }
}
