import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  ClassGroup,
  CreateClassGroupRequest,
  UpdateClassGroupRequest,
  CheckConflictsRequest,
  CheckConflictsResponse,
} from '@features/class-groups/models/class-group.model';

@Injectable({
  providedIn: 'root',
})
export class ClassGroupService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/class-groups`;

  readonly classGroups = signal<ClassGroup[]>([]);
  readonly loading = signal(false);

  /**
   * Load all class groups with optional filters
   */
  loadClassGroups(schoolId?: number, truckId?: number): void {
    this.loading.set(true);

    let params = '';
    if (schoolId) params += `${params ? '&' : '?'}schoolId=${schoolId}`;
    if (truckId) params += `${params ? '&' : '?'}truckId=${truckId}`;

    this.http.get<ClassGroup[]>(`${this.apiUrl}${params}`).pipe(
      tap((classGroups) => this.classGroups.set(Array.isArray(classGroups) ? classGroups : [])),
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading class groups:', error);
        return throwError(() => error);
      }),
      finalize(() => this.loading.set(false))
    ).subscribe();
  }

  /**
   * Get a single class group by ID
   */
  getClassGroup(id: number): Observable<ClassGroup> {
    return this.http.get<ClassGroup>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading class group ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Create a new class group
   */
  createClassGroup(data: CreateClassGroupRequest): Observable<ClassGroup> {
    return this.http.post<ClassGroup>(this.apiUrl, data).pipe(
      tap((newClassGroup) => {
        // Add to local state
        const current = this.classGroups();
        this.classGroups.set([...current, newClassGroup]);
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Error creating class group:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing class group
   */
  updateClassGroup(id: number, data: UpdateClassGroupRequest): Observable<ClassGroup> {
    return this.http.put<ClassGroup>(`${this.apiUrl}/${id}`, data).pipe(
      tap((updatedClassGroup) => {
        // Update local state
        const current = this.classGroups();
        const index = current.findIndex((cg) => cg.id === id);
        if (index !== -1) {
          const updated = [...current];
          updated[index] = updatedClassGroup;
          this.classGroups.set(updated);
        }
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error updating class group ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Delete (archive) a class group
   */
  deleteClassGroup(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        // Remove from local state
        const current = this.classGroups();
        this.classGroups.set(current.filter((cg) => cg.id !== id));
      }),
      catchError((error: HttpErrorResponse) => {
        console.error(`Error deleting class group ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Check for scheduling conflicts
   */
  checkConflicts(request: CheckConflictsRequest): Observable<CheckConflictsResponse> {
    return this.http.post<CheckConflictsResponse>(`${this.apiUrl}/check-conflicts`, request).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error checking conflicts:', error);
        return throwError(() => error);
      })
    );
  }
}
