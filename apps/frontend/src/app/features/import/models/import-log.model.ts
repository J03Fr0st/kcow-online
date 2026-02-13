/**
 * Import audit log entry as returned from the API.
 * Matches backend ImportAuditLogDto.
 */
export interface ImportAuditLog {
  id: number;
  startedAt: string; // ISO datetime string
  completedAt: string | null; // ISO datetime string
  runBy: string;
  sourcePath: string;
  status: string; // 'Completed' | 'Failed' | 'Running'
  schoolsCreated: number;
  classGroupsCreated: number;
  activitiesCreated: number;
  studentsCreated: number;
  totalCreated: number;
  totalFailed: number;
  totalSkipped: number;
  exceptionsFilePath: string | null;
  notes: string | null;
}
