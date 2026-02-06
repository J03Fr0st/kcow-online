import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '@environments/environment';
import type {
  Attendance,
  CreateAttendanceRequest,
  UpdateAttendanceRequest,
  AttendanceQueryParams,
} from '@features/attendance/models/attendance.model';

@Injectable({
  providedIn: 'root',
})
export class AttendanceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/attendance`;

  /**
   * Get attendance records with optional filters.
   * @param params Query parameters for filtering
   * @returns Observable of attendance records array
   */
  getAttendance(params: AttendanceQueryParams): Observable<Attendance[]> {
    const queryParams = this.buildQueryParams(params);
    return this.http.get<Attendance[]>(`${this.apiUrl}${queryParams}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading attendance records:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Get a single attendance record by ID.
   * @param id Attendance record ID
   * @returns Observable of attendance record
   */
  getAttendanceById(id: number): Observable<Attendance> {
    return this.http.get<Attendance>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading attendance record ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Create a new attendance record.
   * @param data Attendance data to create
   * @returns Observable of created attendance record
   */
  createAttendance(data: CreateAttendanceRequest): Observable<Attendance> {
    return this.http.post<Attendance>(this.apiUrl, data).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error creating attendance record:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing attendance record.
   * @param id Attendance record ID
   * @param data Updated attendance data
   * @returns Observable of updated attendance record
   */
  updateAttendance(id: number, data: UpdateAttendanceRequest): Observable<Attendance> {
    return this.http.put<Attendance>(`${this.apiUrl}/${id}`, data).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error updating attendance record ${id}:`, error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Build query string from params object.
   * @param params Query parameters
   * @returns Query string (including leading '?')
   */
  private buildQueryParams(params: AttendanceQueryParams): string {
    const queryParams: string[] = [];
    if (params.studentId !== undefined) {
      queryParams.push(`studentId=${params.studentId}`);
    }
    if (params.classGroupId !== undefined) {
      queryParams.push(`classGroupId=${params.classGroupId}`);
    }
    if (params.fromDate) {
      queryParams.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
    }
    if (params.toDate) {
      queryParams.push(`toDate=${encodeURIComponent(params.toDate)}`);
    }
    return queryParams.length > 0 ? `?${queryParams.join('&')}` : '';
  }
}
