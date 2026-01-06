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

export interface ClassGroup {
  id: number;
  name: string;
  schoolId: number;
  school?: School;
  truckId?: number;
  truck?: Truck;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateClassGroupRequest {
  name: string;
  schoolId: number;
  truckId?: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  notes?: string;
}

export interface UpdateClassGroupRequest {
  name: string;
  schoolId: number;
  truckId?: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  sequence: number;
  notes?: string;
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
