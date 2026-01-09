export interface Activity {
  id: number;
  code: string | null;
  name: string | null;
  description: string | null;
  folder: string | null;
  gradeLevel: string | null;
  icon: string | null; // base64 encoded
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateActivityRequest {
  code?: string;
  name?: string;
  description?: string;
  folder?: string;
  gradeLevel?: string;
  icon?: string; // base64 encoded
}

export interface UpdateActivityRequest {
  code?: string;
  name?: string;
  description?: string;
  folder?: string;
  gradeLevel?: string;
  icon?: string;
}
