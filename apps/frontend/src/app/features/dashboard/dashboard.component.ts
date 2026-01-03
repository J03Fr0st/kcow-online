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
  loading = signal(true);
  error = signal<string | null>(null);

  recentActivities: RecentActivity[] = [
    { id: 1, user: 'JD', action: 'Created new project', time: '2 minutes ago' },
    { id: 2, user: 'AB', action: 'Updated user profile', time: '15 minutes ago' },
    { id: 3, user: 'CD', action: 'Uploaded new file', time: '1 hour ago' },
    { id: 4, user: 'EF', action: 'Completed task', time: '2 hours ago' },
  ];

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
  }

  private loadStats(): void {
    this.mockDataService.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load dashboard data. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
