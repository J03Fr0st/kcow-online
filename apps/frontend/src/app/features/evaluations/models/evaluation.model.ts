/**
 * Evaluation record as returned from the API.
 */
export interface Evaluation {
  id: number;
  studentId: number;
  studentName?: string;
  activityId: number;
  activityName?: string;
  evaluationDate: string; // ISO date string (YYYY-MM-DD)
  score?: number | null;
  speedMetric?: number | null;
  accuracyMetric?: number | null;
  notes?: string | null;
  createdAt: string; // ISO datetime string
  modifiedAt?: string; // ISO datetime string
}

/**
 * Request payload for creating a new evaluation record.
 */
export interface CreateEvaluationRequest {
  studentId: number;
  activityId: number;
  evaluationDate: string; // ISO date string (YYYY-MM-DD)
  score?: number | null;
  speedMetric?: number | null;
  accuracyMetric?: number | null;
  notes?: string | null;
}

/**
 * Request payload for updating an existing evaluation record.
 */
export interface UpdateEvaluationRequest {
  score?: number | null;
  speedMetric?: number | null;
  accuracyMetric?: number | null;
  notes?: string | null;
}
