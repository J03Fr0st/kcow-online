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

export const DAY_OF_WEEK_OPTIONS: readonly string[] = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
] as const;

export type DayOfWeekOption = typeof DAY_OF_WEEK_OPTIONS[number];

export function getDayOfWeekNumber(dayName: string): number {
  return DAY_OF_WEEK_OPTIONS.indexOf(dayName);
}

export function getDayOfWeekName(dayNumber: number): string {
  return DAY_OF_WEEK_OPTIONS[dayNumber] || 'Unknown';
}
