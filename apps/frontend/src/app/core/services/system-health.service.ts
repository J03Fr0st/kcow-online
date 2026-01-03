import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { BehaviorSubject, interval, type Observable } from 'rxjs';

export interface HealthMetric {
  id: string;
  name: string;
  status: 'healthy' | 'warning' | 'critical' | 'unknown';
  value: number;
  unit: string;
  threshold: {
    warning: number;
    critical: number;
  };
  lastUpdated: Date;
}

export interface RequestMetrics {
  totalRequests: number;
  successfulRequests: number;
  failedRequests: number;
  averageResponseTime: number;
  requestsPerMinute: number;
  errorRate: number;
}

export interface SystemStatus {
  overall: 'healthy' | 'degraded' | 'down';
  uptime: number;
  lastCheck: Date;
  services: ServiceStatus[];
}

export interface ServiceStatus {
  name: string;
  url: string;
  status: 'up' | 'down' | 'degraded';
  responseTime?: number;
  lastCheck: Date;
  error?: string;
}

export interface HealthAlert {
  id: string;
  type: 'error' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  acknowledged: boolean;
  source: string;
}

@Injectable({
  providedIn: 'root',
})
export class SystemHealthService {
  private readonly HEALTH_CHECK_INTERVAL = 30000; // 30 seconds
  private readonly METRICS_RETENTION_PERIOD = 300000; // 5 minutes

  // Signals for reactive state management
  public readonly metrics = signal<HealthMetric[]>([]);
  public readonly requestMetrics = signal<RequestMetrics>({
    totalRequests: 0,
    successfulRequests: 0,
    failedRequests: 0,
    averageResponseTime: 0,
    requestsPerMinute: 0,
    errorRate: 0,
  });
  public readonly systemStatus = signal<SystemStatus>({
    overall: 'healthy',
    uptime: 0,
    lastCheck: new Date(),
    services: [],
  });
  public readonly alerts = signal<HealthAlert[]>([]);
  public readonly isMonitoring = signal<boolean>(false);

  // Subjects for real-time updates
  private readonly alertSubject = new BehaviorSubject<HealthAlert[]>([]);
  private readonly metricsSubject = new BehaviorSubject<HealthMetric[]>([]);

  // Request tracking
  private readonly requestTimes: number[] = [];
  private readonly requestTimestamps: Date[] = [];
  private readonly maxRequestHistory = 1000;

  constructor(_http: HttpClient) {
    this.initializeMetrics();
    this.startHealthMonitoring();
  }

  /**
   * Initialize default health metrics
   */
  private initializeMetrics(): void {
    const initialMetrics: HealthMetric[] = [
      {
        id: 'cpu-usage',
        name: 'CPU Usage',
        status: 'healthy',
        value: 0,
        unit: '%',
        threshold: {
          warning: 70,
          critical: 90,
        },
        lastUpdated: new Date(),
      },
      {
        id: 'memory-usage',
        name: 'Memory Usage',
        status: 'healthy',
        value: 0,
        unit: '%',
        threshold: {
          warning: 80,
          critical: 95,
        },
        lastUpdated: new Date(),
      },
      {
        id: 'response-time',
        name: 'Response Time',
        status: 'healthy',
        value: 0,
        unit: 'ms',
        threshold: {
          warning: 500,
          critical: 1000,
        },
        lastUpdated: new Date(),
      },
      {
        id: 'error-rate',
        name: 'Error Rate',
        status: 'healthy',
        value: 0,
        unit: '%',
        threshold: {
          warning: 5,
          critical: 10,
        },
        lastUpdated: new Date(),
      },
    ];

    this.metrics.set(initialMetrics);
  }

  /**
   * Start automated health monitoring
   */
  public startHealthMonitoring(): void {
    if (this.isMonitoring()) return;

    this.isMonitoring.set(true);

    // Set up periodic health checks
    interval(this.HEALTH_CHECK_INTERVAL).subscribe(() => {
      this.performHealthCheck();
      this.cleanupOldData();
    });

    // Initial health check
    this.performHealthCheck();
  }

  /**
   * Stop health monitoring
   */
  public stopHealthMonitoring(): void {
    this.isMonitoring.set(false);
  }

  /**
   * Perform comprehensive health check
   */
  private async performHealthCheck(): Promise<void> {
    try {
      // Update system metrics
      await this.updateSystemMetrics();

      // Check external services
      await this.checkExternalServices();

      // Update overall system status
      this.updateSystemStatus();

      // Check for alerts
      this.checkForAlerts();
    } catch (error) {
      console.error('Health check failed:', error);
      this.createAlert(
        'error',
        'Health Check Failed',
        'Unable to complete system health check',
        'system',
      );
    }
  }

  /**
   * Update system performance metrics
   */
  private async updateSystemMetrics(): Promise<void> {
    // Simulate metrics collection (in real app, this would call actual monitoring endpoints)
    const cpuUsage = this.simulateMetric(45, 25);
    const memoryUsage = this.simulateMetric(60, 20);
    const responseTime = this.calculateAverageResponseTime();
    const errorRate = this.calculateErrorRate();

    this.updateMetric('cpu-usage', cpuUsage);
    this.updateMetric('memory-usage', memoryUsage);
    this.updateMetric('response-time', responseTime);
    this.updateMetric('error-rate', errorRate);
  }

  /**
   * Check health of external services
   */
  private async checkExternalServices(): Promise<void> {
    const services: ServiceStatus[] = [
      {
        name: 'API Gateway',
        url: '/api/health',
        status: 'up',
        responseTime: Math.random() * 200 + 50,
        lastCheck: new Date(),
      },
      {
        name: 'Database',
        url: '/api/health/db',
        status: Math.random() > 0.95 ? 'degraded' : 'up',
        responseTime: Math.random() * 100 + 20,
        lastCheck: new Date(),
      },
      {
        name: 'Auth Service',
        url: '/api/health/auth',
        status: 'up',
        responseTime: Math.random() * 150 + 30,
        lastCheck: new Date(),
      },
    ];

    this.systemStatus.update((status) => ({
      ...status,
      services,
      lastCheck: new Date(),
    }));
  }

  /**
   * Update overall system status based on metrics and services
   */
  private updateSystemStatus(): void {
    const metrics = this.metrics();
    const services = this.systemStatus().services;

    let overallStatus: 'healthy' | 'degraded' | 'down' = 'healthy';

    // Check metrics for issues
    const hasCriticalMetrics = metrics.some((m) => m.status === 'critical');
    const hasWarningMetrics = metrics.some((m) => m.status === 'warning');

    // Check services for issues
    const hasDownServices = services.some((s) => s.status === 'down');
    const hasDegradedServices = services.some((s) => s.status === 'degraded');

    if (hasCriticalMetrics || hasDownServices) {
      overallStatus = 'down';
    } else if (hasWarningMetrics || hasDegradedServices) {
      overallStatus = 'degraded';
    }

    this.systemStatus.update((status) => ({
      ...status,
      overall: overallStatus,
      uptime: performance.now() / 1000, // Simple uptime simulation
    }));
  }

  /**
   * Track HTTP request start
   */
  public trackRequestStart(_url: string, _method: string): void {
    // Implementation for tracking request start
    const timestamp = new Date();
    this.requestTimestamps.push(timestamp);

    // Cleanup old timestamps
    this.cleanupOldTimestamps();
  }

  /**
   * Track successful HTTP response
   */
  public trackRequestSuccess(url: string, method: string, status: number, duration: number): void {
    this.requestTimes.push(duration);
    this.updateRequestMetrics();

    // Log successful request
    console.debug(`Request success: ${method} ${url} - ${status} (${duration.toFixed(2)}ms)`);
  }

  /**
   * Track failed HTTP response
   */
  public trackRequestError(
    url: string,
    method: string,
    status: number,
    error: string,
    duration: number,
  ): void {
    this.requestTimes.push(duration);
    this.updateRequestMetrics();

    // Create alert for high-priority errors
    if (status >= 500) {
      this.createAlert('error', 'Server Error', `${method} ${url} failed with ${status}`, 'http');
    }

    console.error(
      `Request error: ${method} ${url} - ${status} (${duration.toFixed(2)}ms) - ${error}`,
    );
  }

  /**
   * Update request-related metrics
   */
  private updateRequestMetrics(): void {
    const currentMetrics = this.requestMetrics();
    const totalRequests = currentMetrics.totalRequests + 1;

    // Calculate average response time
    const avgResponseTime =
      this.requestTimes.length > 0
        ? this.requestTimes.reduce((a, b) => a + b, 0) / this.requestTimes.length
        : 0;

    // Calculate requests per minute
    const now = new Date();
    const oneMinuteAgo = new Date(now.getTime() - 60000);
    const recentRequests = this.requestTimestamps.filter((t) => t > oneMinuteAgo).length;

    this.requestMetrics.set({
      totalRequests,
      successfulRequests: currentMetrics.successfulRequests,
      failedRequests: currentMetrics.failedRequests,
      averageResponseTime: avgResponseTime,
      requestsPerMinute: recentRequests,
      errorRate: this.calculateErrorRate(),
    });
  }

  /**
   * Calculate average response time from recent requests
   */
  private calculateAverageResponseTime(): number {
    if (this.requestTimes.length === 0) return 0;

    const recentTimes = this.requestTimes.slice(-100); // Last 100 requests
    return recentTimes.reduce((a, b) => a + b, 0) / recentTimes.length;
  }

  /**
   * Calculate error rate percentage
   */
  private calculateErrorRate(): number {
    const metrics = this.requestMetrics();
    if (metrics.totalRequests === 0) return 0;

    return (metrics.failedRequests / metrics.totalRequests) * 100;
  }

  /**
   * Update a specific health metric
   */
  private updateMetric(metricId: string, value: number): void {
    this.metrics.update((metrics) =>
      metrics.map((metric) => {
        if (metric.id === metricId) {
          let status: 'healthy' | 'warning' | 'critical' = 'healthy';

          if (value >= metric.threshold.critical) {
            status = 'critical';
          } else if (value >= metric.threshold.warning) {
            status = 'warning';
          }

          return {
            ...metric,
            value,
            status,
            lastUpdated: new Date(),
          };
        }
        return metric;
      }),
    );
  }

  /**
   * Check for alerts based on current metrics
   */
  private checkForAlerts(): void {
    const metrics = this.metrics();

    metrics.forEach((metric) => {
      if (metric.status === 'critical') {
        this.createAlert(
          'error',
          'Critical Metric Alert',
          `${metric.name} has reached critical level: ${metric.value}${metric.unit}`,
          'metrics',
        );
      } else if (metric.status === 'warning') {
        this.createAlert(
          'warning',
          'Warning Metric Alert',
          `${metric.name} is above threshold: ${metric.value}${metric.unit}`,
          'metrics',
        );
      }
    });
  }

  /**
   * Create a new health alert
   */
  public createAlert(
    type: 'error' | 'warning' | 'info',
    title: string,
    message: string,
    source: string,
  ): void {
    const alert: HealthAlert = {
      id: this.generateId(),
      type,
      title,
      message,
      timestamp: new Date(),
      acknowledged: false,
      source,
    };

    this.alerts.update((alerts) => [alert, ...alerts].slice(0, 100)); // Keep last 100 alerts

    // Also emit for real-time subscribers
    this.alertSubject.next([...this.alerts()]);
  }

  /**
   * Acknowledge an alert
   */
  public acknowledgeAlert(alertId: string): void {
    this.alerts.update((alerts) =>
      alerts.map((alert) => (alert.id === alertId ? { ...alert, acknowledged: true } : alert)),
    );
  }

  /**
   * Clear all acknowledged alerts
   */
  public clearAcknowledgedAlerts(): void {
    this.alerts.update((alerts) => alerts.filter((alert) => !alert.acknowledged));
  }

  /**
   * Get alerts as observable
   */
  public getAlerts$(): Observable<HealthAlert[]> {
    return this.alertSubject.asObservable();
  }

  /**
   * Get metrics as observable
   */
  public getMetrics$(): Observable<HealthMetric[]> {
    return this.metricsSubject.asObservable();
  }

  /**
   * Cleanup old data to prevent memory leaks
   */
  private cleanupOldData(): void {
    this.cleanupOldTimestamps();
    this.cleanupOldRequestTimes();
  }

  private cleanupOldTimestamps(): void {
    const cutoff = new Date(Date.now() - this.METRICS_RETENTION_PERIOD);
    this.requestTimestamps.splice(
      0,
      this.requestTimestamps.findIndex((t) => t > cutoff),
    );
  }

  private cleanupOldRequestTimes(): void {
    if (this.requestTimes.length > this.maxRequestHistory) {
      this.requestTimes.splice(0, this.requestTimes.length - this.maxRequestHistory);
    }
  }

  /**
   * Simulate metric value with some randomness
   */
  private simulateMetric(base: number, variance: number): number {
    return Math.max(0, base + (Math.random() - 0.5) * variance);
  }

  /**
   * Generate unique ID
   */
  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  /**
   * Get system health summary
   */
  public getHealthSummary(): { status: string; issues: number; uptime: number } {
    const status = this.systemStatus();
    const alerts = this.alerts();
    const unacknowledgedIssues = alerts.filter((a) => !a.acknowledged && a.type !== 'info').length;

    return {
      status: status.overall,
      issues: unacknowledgedIssues,
      uptime: status.uptime,
    };
  }

  /**
   * Public method to trigger health check
   */
  public triggerHealthCheck(): void {
    this.performHealthCheck();
  }
}
