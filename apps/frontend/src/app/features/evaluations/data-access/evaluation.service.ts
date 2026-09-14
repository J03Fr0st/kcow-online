import { HttpClient, type HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '@environments/environment';
import type {
  CreateEvaluationRequest,
  Evaluation,
  UpdateEvaluationRequest,
} from '@features/evaluations/models/evaluation.model';
import { catchError, type Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EvaluationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/evaluations`;

  /**
   * Get all evaluation records (no student filter).
   * @returns Observable of all evaluation records
   */
  getAllEvaluations(): Observable<Evaluation[]> {
    return this.http.get<Evaluation[]>(this.apiUrl).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading all evaluation records:', error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Get evaluation records with optional filters.
   * @param studentId Student ID to filter by
   * @returns Observable of evaluation records array
   */
  getEvaluations(studentId: number): Observable<Evaluation[]> {
    return this.http.get<Evaluation[]>(`/api/students/${studentId}/evaluations`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading evaluation records:', error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Get a single evaluation record by ID.
   * @param id Evaluation record ID
   * @returns Observable of evaluation record
   */
  getEvaluationById(id: number): Observable<Evaluation> {
    return this.http.get<Evaluation>(`${this.apiUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading evaluation record ${id}:`, error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Create a new evaluation record.
   * @param data Evaluation data to create
   * @returns Observable of created evaluation record
   */
  createEvaluation(data: CreateEvaluationRequest): Observable<Evaluation> {
    return this.http.post<Evaluation>(this.apiUrl, data).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error creating evaluation record:', error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Update an existing evaluation record.
   * @param id Evaluation record ID
   * @param data Updated evaluation data
   * @returns Observable of updated evaluation record
   */
  updateEvaluation(id: number, data: UpdateEvaluationRequest): Observable<Evaluation> {
    return this.http.put<Evaluation>(`${this.apiUrl}/${id}`, data).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error updating evaluation record ${id}:`, error);
        return throwError(() => error);
      }),
    );
  }
}
