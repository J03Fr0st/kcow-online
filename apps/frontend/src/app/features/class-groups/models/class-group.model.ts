export interface School {
  id: number;
  name: string;
  shortName?: string;
}

export interface Truck {
  id: number;
  name: string;
  registrationNumber: string;
}

/**
 * ClassGroup model aligned with backend XSD schema.
 * Includes all 15 XSD fields for legacy compatibility.
 */
export interface ClassGroup {
  id: number;
  name: string;
  dayTruck?: string; // XSD: DayTruck (6 chars) - Legacy composite identifier
  description?: string; // XSD: Description (35 chars)
  schoolId: number;
  schoolName?: string;
  school?: School;
  truckId?: number;
  truckName?: string;
  truck?: Truck;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  evaluate: boolean; // XSD: Evaluate (boolean, required)
  notes?: string;
  importFlag: boolean; // XSD: Import (boolean, required)
  groupMessage?: string; // XSD: GroupMessage (255 chars)
  sendCertificates?: string; // XSD: Send_x0020_Certificates (255 chars)
  moneyMessage?: string; // XSD: Money_x0020_Message (50 chars)
  ixl?: string; // XSD: IXL (3 chars)
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Request model for creating a new class group.
 * Aligned with backend XSD schema constraints.
 */
export interface CreateClassGroupRequest {
  name: string; // Max 10 chars (XSD)
  dayTruck?: string; // Max 6 chars (XSD)
  description?: string; // Max 35 chars (XSD)
  schoolId: number;
  truckId?: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  evaluate?: boolean; // XSD: boolean, default false
  notes?: string; // Max 255 chars (XSD)
  importFlag?: boolean; // XSD: boolean, default false
  groupMessage?: string; // Max 255 chars (XSD)
  sendCertificates?: string; // Max 255 chars (XSD)
  moneyMessage?: string; // Max 50 chars (XSD)
  ixl?: string; // Max 3 chars (XSD)
}

/**
 * Request model for updating an existing class group.
 * Aligned with backend XSD schema constraints.
 */
export interface UpdateClassGroupRequest {
  name: string; // Max 10 chars (XSD)
  dayTruck?: string; // Max 6 chars (XSD)
  description?: string; // Max 35 chars (XSD)
  schoolId: number;
  truckId?: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  evaluate?: boolean; // XSD: boolean, default false
  notes?: string; // Max 255 chars (XSD)
  importFlag?: boolean; // XSD: boolean, default false
  groupMessage?: string; // Max 255 chars (XSD)
  sendCertificates?: string; // Max 255 chars (XSD)
  moneyMessage?: string; // Max 50 chars (XSD)
  ixl?: string; // Max 3 chars (XSD)
  isActive: boolean;
}

// Weekday options for school scheduling (Monday-Friday only)
// Maps to .NET DayOfWeek enum: Sunday=0, Monday=1, ..., Saturday=6
export const DAY_OF_WEEK_OPTIONS: readonly string[] = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
] as const;

export type DayOfWeekOption = typeof DAY_OF_WEEK_OPTIONS[number];

// Map day names to .NET DayOfWeek enum values
const DAY_OF_WEEK_MAP: Record<string, number> = {
  'Sunday': 0,
  'Monday': 1,
  'Tuesday': 2,
  'Wednesday': 3,
  'Thursday': 4,
  'Friday': 5,
  'Saturday': 6,
};

const DAY_NUMBER_TO_NAME: Record<number, string> = {
  0: 'Sunday',
  1: 'Monday',
  2: 'Tuesday',
  3: 'Wednesday',
  4: 'Thursday',
  5: 'Friday',
  6: 'Saturday',
};

export function getDayOfWeekNumber(dayName: string): number {
  return DAY_OF_WEEK_MAP[dayName] ?? 1; // Default to Monday
}

export function getDayOfWeekName(dayNumber: number): string {
  return DAY_NUMBER_TO_NAME[dayNumber] ?? 'Unknown';
}

// Conflict detection types
export interface ScheduleConflict {
  id: number;
  name: string;
  schoolName: string;
  startTime: string;
  endTime: string;
}

export interface CheckConflictsRequest {
  truckId: number;
  dayOfWeek: number;
  startTime: string; // "HH:mm:ss"
  endTime: string;
  excludeId?: number;
}

export interface CheckConflictsResponse {
  hasConflicts: boolean;
  conflicts: ScheduleConflict[];
}
