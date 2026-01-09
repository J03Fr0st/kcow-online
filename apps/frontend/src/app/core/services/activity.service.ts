import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { Activity, CreateActivityRequest, UpdateActivityRequest } from '@features/activities/models/activity.model';

@Injectable({
  providedIn: 'root'
})
export class ActivityService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/activities`;

  readonly activities = signal<Activity[]>([]);
  readonly loading = signal(false);

  /**
   * Load all activities from the API
   */
  loadActivities(): void {
    this.loading.set(true);

    this.http.get<Activity[]>(this.apiUrl).pipe(
      tap((activities) => this.activities.set(activities)),
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading activities:', error);
        return throwError(() => error);
      }),
      finalize(() => this.loading.set(false)),
      shareReplay({ bufferSize: 1, refCount: true })
    ).subscribe();
  }

  /**
   * Get a single activity by ID
   */
  getActivity(id: number): Observable<Activity> {
    return this.http.get<Activity>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading activity ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Create a new activity
   */
  createActivity(data: CreateActivityRequest): Observable<Activity> {
    return this.http.post<Activity>(this.apiUrl, data).pipe(
      tap((newActivity) => {
        // Add to local state
        const current = this.activities();
        this.activities.set([...current, newActivity]);
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Error creating activity:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing activity
   */
  updateActivity(id: number, data: UpdateActivityRequest): Observable<Activity> {
    return this.http.put<Activity>(`${this.apiUrl}/${id}`, data).pipe(
      tap((updatedActivity) => {
        // Update local state
        const current = this.activities();
        const index = current.findIndex((a) => a.id === id);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = updatedActivity;
          this.activities.set(updated);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error updating activity ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Delete (archive) an activity
   */
  deleteActivity(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        // Remove from local state
        const current = this.activities();
        this.activities.set(current.filter((a) => a.id !== id));
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error deleting activity ${id}:`, error);
        return throwError(() => error);
      })
    );
  }
}
