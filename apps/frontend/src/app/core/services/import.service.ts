import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '@environments/environment';
import type { ImportAuditLog } from '@features/import/models/import-log.model';

@Injectable({
  providedIn: 'root',
})
export class ImportService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/import/audit-log`;

  /**
   * Get recent import audit log entries.
   * @param count Number of entries to return (default 10, max 100)
   */
  getRecentImports(count: number = 20): Observable<ImportAuditLog[]> {
    return this.http.get<ImportAuditLog[]>(`${this.apiUrl}?count=${count}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading import audit logs:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Get a single import audit log entry by ID.
   * @param id Import audit log ID
   */
  getImportById(id: number): Observable<ImportAuditLog> {
    return this.http.get<ImportAuditLog>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading import audit log ${id}:`, error);
        return throwError(() => error);
      })
    );
  }
}
