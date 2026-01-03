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

export type TruckStatus = 'Active' | 'Maintenance' | 'Retired';

export const TRUCK_STATUS_OPTIONS: readonly TruckStatus[] = ['Active', 'Maintenance', 'Retired'] as const;
