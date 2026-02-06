import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuditLog {
  id: number;
  entityType: string;
  entityId: number;
  field: string;
  oldValue: string | null;
  newValue: string | null;
  changedBy: string;
  changedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuditLogService {
  private readonly apiUrl = 'api/audit-log';

  constructor(private readonly http: HttpClient) {}

  /**
   * Get audit logs for a specific entity
   */
  getAuditLogs(entityType: string, entityId: number): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(this.apiUrl, {
      params: {
        entityType,
        entityId: entityId.toString(),
      },
    });
  }
}
