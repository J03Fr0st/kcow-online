import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Truck {
  id: number;
  name: string;
  registrationNumber: string;
  status: 'Active' | 'Maintenance' | 'Retired';
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTruckRequest {
  name: string;
  registrationNumber: string;
  status: string;
  notes?: string;
}

export interface UpdateTruckRequest {
  name: string;
  registrationNumber: string;
  status: string;
  notes?: string;
}

export const TRUCK_STATUS_OPTIONS: readonly ['Active', 'Maintenance', 'Retired'] = ['Active', 'Maintenance', 'Retired'] as const;

@Injectable({
  providedIn: 'root'
})
export class TruckService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/trucks`;

  readonly trucks = signal<Truck[]>([]);
  readonly loading = signal(false);

  /**
   * Load all trucks from the API
   */
  loadTrucks(): void {
    this.loading.set(true);

    this.http.get<Truck[]>(this.apiUrl).pipe(
      tap((trucks) => this.trucks.set(trucks)),
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading trucks:', error);
        return throwError(() => error);
      }),
      finalize(() => this.loading.set(false)),
      shareReplay({ bufferSize: 1, refCount: true })
    ).subscribe();
  }

  /**
   * Get a single truck by ID
   */
  getTruck(id: number): Observable<Truck> {
    return this.http.get<Truck>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading truck ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Create a new truck
   */
  createTruck(data: CreateTruckRequest): Observable<Truck> {
    return this.http.post<Truck>(this.apiUrl, data).pipe(
      tap((newTruck) => {
        // Add to local state
        const current = this.trucks();
        this.trucks.set([...current, newTruck]);
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Error creating truck:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing truck
   */
  updateTruck(id: number, data: UpdateTruckRequest): Observable<Truck> {
    return this.http.put<Truck>(`${this.apiUrl}/${id}`, data).pipe(
      tap((updatedTruck) => {
        // Update local state
        const current = this.trucks();
        const index = current.findIndex((t) => t.id === id);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = updatedTruck;
          this.trucks.set(updated);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error updating truck ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Delete (archive) a truck
   */
  deleteTruck(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        // Remove from local state
        const current = this.trucks();
        this.trucks.set(current.filter((t) => t.id !== id));
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error deleting truck ${id}:`, error);
        return throwError(() => error);
      })
    );
  }
}
