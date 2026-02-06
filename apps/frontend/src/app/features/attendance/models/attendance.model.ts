/**
 * Attendance status enumeration matching backend enum.
 */
export type AttendanceStatus = 'Present' | 'Absent' | 'Late';

/**
 * Attendance record as returned from the API.
 */
export interface Attendance {
  id: number;
  studentId: number;
  studentName?: string;
  classGroupId: number;
  classGroupName?: string;
  sessionDate: string; // ISO date string (YYYY-MM-DD)
  status: AttendanceStatus;
  notes?: string;
  createdAt: string; // ISO datetime string
  modifiedAt?: string; // ISO datetime string
}

/**
 * Request payload for creating a new attendance record.
 */
export interface CreateAttendanceRequest {
  studentId: number;
  classGroupId: number;
  sessionDate: string; // ISO date string (YYYY-MM-DD)
  status: AttendanceStatus;
  notes?: string;
}

/**
 * Request payload for updating an existing attendance record.
 */
export interface UpdateAttendanceRequest {
  status: AttendanceStatus;
  notes?: string;
}

/**
 * Query parameters for filtering attendance records.
 */
export interface AttendanceQueryParams {
  studentId?: number;
  classGroupId?: number;
  fromDate?: string; // ISO date string (YYYY-MM-DD)
  toDate?: string; // ISO date string (YYYY-MM-DD)
}
