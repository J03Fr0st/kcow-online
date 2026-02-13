import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, type OnDestroy, type OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import type { HealthMetric } from '@core/services/system-health.service';
import { SystemHealthService } from '@core/services/system-health.service';
import { interval, type Subscription } from 'rxjs';

@Component({
  selector: 'app-system-health',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './system-health.component.html',
  styleUrls: ['./system-health.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemHealthComponent implements OnInit, OnDestroy {
  private healthService = inject(SystemHealthService);
  private subscriptions: Subscription[] = [];

  // Service data
  public readonly metrics = this.healthService.metrics;
  public readonly systemStatus = this.healthService.systemStatus;
  public readonly alerts = this.healthService.alerts;
  public readonly requestMetrics = this.healthService.requestMetrics;
  public readonly isMonitoring = this.healthService.isMonitoring;

  // Computed properties
  public readonly unacknowledgedAlerts = computed(() =>
    this.alerts().filter((alert) => !alert.acknowledged),
  );

  public readonly criticalMetrics = computed(() =>
    this.metrics().filter((metric) => metric.status === 'critical'),
  );

  public readonly warningMetrics = computed(() =>
    this.metrics().filter((metric) => metric.status === 'warning'),
  );

  public readonly healthyMetrics = computed(() =>
    this.metrics().filter((metric) => metric.status === 'healthy'),
  );

  // UI State
  public selectedTimeRange: '1m' | '5m' | '15m' | '1h' = '5m';
  public autoRefresh = true;
  public refreshInterval = 5000; // 5 seconds
  public showDetails = false;

  ngOnInit(): void {
    this.initializeHealthMonitoring();
    this.setupAutoRefresh();
  }

  ngOnDestroy(): void {
    this.cleanup();
  }

  /**
   * Initialize health monitoring if not already started
   */
  private initializeHealthMonitoring(): void {
    if (!this.isMonitoring()) {
      this.healthService.startHealthMonitoring();
    }
  }

  /**
   * Setup auto-refresh functionality
   */
  private setupAutoRefresh(): void {
    if (this.autoRefresh) {
      const subscription = interval(this.refreshInterval).subscribe(() => {
        this.refreshData();
      });
      this.subscriptions.push(subscription);
    }
  }

  /**
   * Refresh health data
   */
  public refreshData(): void {
    // Force a health check
    this.healthService.triggerHealthCheck();
  }

  /**
   * Toggle monitoring state
   */
  public toggleMonitoring(): void {
    if (this.isMonitoring()) {
      this.healthService.stopHealthMonitoring();
    } else {
      this.healthService.startHealthMonitoring();
      this.setupAutoRefresh();
    }
  }

  /**
   * Toggle auto-refresh
   */
  public toggleAutoRefresh(): void {
    this.autoRefresh = !this.autoRefresh;

    // Clear existing subscriptions
    this.cleanup();

    // Restart with new settings
    if (this.autoRefresh) {
      this.setupAutoRefresh();
    }
  }

  /**
   * Acknowledge an alert
   */
  public acknowledgeAlert(alertId: string): void {
    this.healthService.acknowledgeAlert(alertId);
  }

  /**
   * Acknowledge all alerts
   */
  public acknowledgeAllAlerts(): void {
    this.unacknowledgedAlerts().forEach((alert) => {
      this.healthService.acknowledgeAlert(alert.id);
    });
  }

  /**
   * Clear acknowledged alerts
   */
  public clearAcknowledgedAlerts(): void {
    this.healthService.clearAcknowledgedAlerts();
  }

  /**
   * Get status color class
   */
  public getStatusClass(status: string): string {
    switch (status) {
      case 'healthy':
      case 'up':
        return 'status-healthy';
      case 'warning':
      case 'degraded':
        return 'status-warning';
      case 'critical':
      case 'down':
        return 'status-critical';
      default:
        return 'status-unknown';
    }
  }

  /**
   * Get alert type class
   */
  public getAlertTypeClass(type: string): string {
    switch (type) {
      case 'error':
        return 'alert-error';
      case 'warning':
        return 'alert-warning';
      case 'info':
        return 'alert-info';
      default:
        return 'alert-default';
    }
  }

  /**
   * Get metric value with appropriate formatting
   */
  public formatMetricValue(metric: HealthMetric): string {
    switch (metric.unit) {
      case '%':
        return `${metric.value.toFixed(1)}%`;
      case 'ms':
        return `${metric.value.toFixed(0)}ms`;
      default:
        return metric.value.toString();
    }
  }

  /**
   * Get progress bar width for metrics
   */
  public getProgressWidth(metric: HealthMetric): number {
    // For percentage metrics (CPU, Memory, Error Rate)
    if (metric.unit === '%' || metric.id === 'error-rate') {
      return Math.min(metric.value, 100);
    }

    // For response time, scale logarithmically
    if (metric.unit === 'ms') {
      return Math.min((metric.value / 2000) * 100, 100);
    }

    return 0;
  }

  /**
   * Get overall health summary
   */
  public getHealthSummary(): { status: string; issues: number; uptime: number } {
    return this.healthService.getHealthSummary();
  }

  /**
   * Format uptime
   */
  public formatUptime(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);

    if (hours > 0) {
      return `${hours}h ${minutes}m ${secs}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${secs}s`;
    } else {
      return `${secs}s`;
    }
  }

  /**
   * Format relative time
   */
  public formatRelativeTime(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(date).getTime();
    const seconds = Math.floor(diff / 1000);

    if (seconds < 60) {
      return `${seconds}s ago`;
    } else if (seconds < 3600) {
      return `${Math.floor(seconds / 60)}m ago`;
    } else {
      return `${Math.floor(seconds / 3600)}h ago`;
    }
  }

  /**
   * Export health data
   */
  public exportHealthData(): void {
    const data = {
      timestamp: new Date().toISOString(),
      metrics: this.metrics(),
      systemStatus: this.systemStatus(),
      requestMetrics: this.requestMetrics(),
      alerts: this.alerts(),
    };

    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `health-data-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  /**
   * Cleanup subscriptions
   */
  private cleanup(): void {
    this.subscriptions.forEach((sub) => {
      sub.unsubscribe();
    });
    this.subscriptions = [];
  }
}
