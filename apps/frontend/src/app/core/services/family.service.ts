import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, map } from 'rxjs';
import { environment } from '../../../environments/environment';

// Import Student interfaces
export interface StudentSummary {
    id: number;
    firstName: string;
    lastName: string;
    grade?: string;
    status?: string;
    isActive: boolean;
}

export interface Guardian {
    id: number;
    familyId: number;
    firstName: string;
    lastName: string;
    relationship?: string;
    phone?: string;
    email?: string;
    isPrimaryContact: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface CreateGuardianRequest {
    firstName: string;
    lastName: string;
    relationship?: string;
    phone?: string;
    email?: string;
    isPrimaryContact?: boolean;
}

export interface UpdateGuardianRequest {
    firstName: string;
    lastName: string;
    relationship?: string;
    phone?: string;
    email?: string;
    isPrimaryContact: boolean;
}

export interface Family {
    id: number;
    familyName: string;
    primaryBillingContactId?: number;
    notes?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
    guardians?: Guardian[];
    studentCount?: number; // Number of students linked to this family
}

export interface CreateFamilyRequest {
    familyName: string;
    primaryBillingContactId?: number;
    notes?: string;
}

export interface UpdateFamilyRequest {
    familyName: string;
    primaryBillingContactId?: number;
    notes?: string;
    isActive: boolean;
}

export interface ProblemDetails {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    [key: string]: unknown;
}

export interface MergeFamiliesRequest {
    primaryFamilyId: number;
    secondaryFamilyId: number;
    keepPrimaryBillingContactId?: number;
}

export interface MergeFamiliesResult {
    primaryFamilyId: number;
    secondaryFamilyId: number;
    studentsMoved: number;
    guardiansConsolidated: number;
    mergedAt: string;
    summary: string;
}

export interface FamilyMergePreview {
    primaryFamilyId: number;
    primaryFamilyName: string;
    secondaryFamilyId: number;
    secondaryFamilyName: string;
    primaryStudentCount: number;
    secondaryStudentCount: number;
    primaryGuardianCount: number;
    secondaryGuardianCount: number;
    duplicateGuardianCount: number;
    hasPrimaryBillingContactConflict: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class FamilyService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/families`;

    // Signals for state
    private familiesSignal = signal<Family[]>([]);
    private isLoadingSignal = signal<boolean>(false);
    private errorSignal = signal<ProblemDetails | null>(null);

    // Read-only signals
    readonly families = computed(() => this.familiesSignal());
    readonly isLoading = computed(() => this.isLoadingSignal());
    readonly error = computed(() => this.errorSignal());

    /**
     * Fetch families from the backend
     */
    getFamilies(skip = 0, take = 100, search?: string): Observable<Family[]> {
        this.isLoadingSignal.set(true);
        this.errorSignal.set(null);

        let params = new HttpParams()
            .set('skip', skip.toString())
            .set('take', take.toString());

        if (search) {
            params = params.set('search', search);
        }

        return this.http.get<Family[]>(this.apiUrl, {
            withCredentials: true,
            params
        }).pipe(
            tap((families) => {
                this.familiesSignal.set(families);
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            }),
            tap({
                error: (err) => this.errorSignal.set(err)
            }),
            finalize(() => {
                this.isLoadingSignal.set(false);
            })
        );
    }

    /**
     * Get a single family by ID
     */
    getFamilyById(id: number): Observable<Family> {
        return this.http.get<Family>(`${this.apiUrl}/${id}`, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Create a new family
     */
    createFamily(family: CreateFamilyRequest): Observable<Family> {
        return this.http.post<Family>(this.apiUrl, family, { withCredentials: true }).pipe(
            tap((newFamily) => {
                this.familiesSignal.update((families) => [...families, newFamily]);
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Update an existing family
     */
    updateFamily(id: number, family: UpdateFamilyRequest): Observable<Family> {
        return this.http.put<Family>(`${this.apiUrl}/${id}`, family, { withCredentials: true }).pipe(
            tap((updatedFamily) => {
                this.familiesSignal.update((families) =>
                    families.map((f) => (f.id === id ? updatedFamily : f))
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Get only active families (for dropdowns/selections)
     */
    getActiveFamilies(search?: string): Observable<Family[]> {
        return this.getFamilies(0, 100, search).pipe(
            map((families) => families.filter((f) => f.isActive))
        );
    }

    /**
     * Deactivate a family (soft delete - sets isActive to false)
     */
    deactivateFamily(id: number, family: Family): Observable<Family> {
        const { id: _, ...rest } = family;
        return this.updateFamily(id, {
            ...rest,
            isActive: false,
        });
    }

    /**
     * Reactivate a family (sets isActive back to true)
     */
    reactivateFamily(id: number, family: Family): Observable<Family> {
        const { id: _, ...rest } = family;
        return this.updateFamily(id, {
            ...rest,
            isActive: true,
        });
    }

    // ========== Guardian Management ==========

    /**
     * Get all guardians for a specific family
     */
    getGuardiansByFamily(familyId: number): Observable<Guardian[]> {
        return this.http.get<Guardian[]>(`${this.apiUrl}/${familyId}/guardians`, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Add a guardian to a family
     */
    addGuardian(familyId: number, guardian: CreateGuardianRequest): Observable<Guardian> {
        return this.http.post<Guardian>(`${this.apiUrl}/${familyId}/guardians`, guardian, { withCredentials: true }).pipe(
            tap((newGuardian) => {
                // Update the family in the list with the new guardian
                this.familiesSignal.update((families) =>
                    families.map((f) => {
                        if (f.id === familyId) {
                            return {
                                ...f,
                                guardians: [...(f.guardians || []), newGuardian]
                            };
                        }
                        return f;
                    })
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Update an existing guardian
     */
    updateGuardian(familyId: number, guardianId: number, guardian: UpdateGuardianRequest): Observable<Guardian> {
        return this.http.put<Guardian>(`${this.apiUrl}/${familyId}/guardians/${guardianId}`, guardian, { withCredentials: true }).pipe(
            tap((updatedGuardian) => {
                // Update the guardian in the family's guardian list
                this.familiesSignal.update((families) =>
                    families.map((f) => {
                        if (f.id === familyId && f.guardians) {
                            return {
                                ...f,
                                guardians: f.guardians.map((g) =>
                                    g.id === guardianId ? updatedGuardian : g
                                )
                            };
                        }
                        return f;
                    })
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Delete a guardian from a family
     */
    deleteGuardian(familyId: number, guardianId: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${familyId}/guardians/${guardianId}`, { withCredentials: true }).pipe(
            tap(() => {
                // Remove the guardian from the family's guardian list
                this.familiesSignal.update((families) =>
                    families.map((f) => {
                        if (f.id === familyId && f.guardians) {
                            return {
                                ...f,
                                guardians: f.guardians.filter((g) => g.id !== guardianId)
                            };
                        }
                        return f;
                    })
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    // ========== Student Management ==========

    /**
     * Get all students for a specific family
     */
    getStudentsByFamily(familyId: number): Observable<StudentSummary[]> {
        return this.http.get<StudentSummary[]>(`${this.apiUrl}/${familyId}/students`, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    // ========== Family Merge Management ==========

    /**
     * Preview a family merge
     */
    previewMerge(request: MergeFamiliesRequest): Observable<FamilyMergePreview> {
        return this.http.post<FamilyMergePreview>(`${this.apiUrl}/merge/preview`, request, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Execute a family merge
     */
    mergeFamilies(request: MergeFamiliesRequest): Observable<MergeFamiliesResult> {
        return this.http.post<MergeFamiliesResult>(`${this.apiUrl}/merge`, request, { withCredentials: true }).pipe(
            tap((result) => {
                // Remove the secondary family from the list
                this.familiesSignal.update((families) =>
                    families.filter((f) => f.id !== request.secondaryFamilyId)
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Handle HTTP errors and map to ProblemDetails
     */
    private handleError(error: HttpErrorResponse): Observable<never> {
        let problemDetails: ProblemDetails = {
            title: 'An error occurred',
            detail: 'An unexpected error occurred',
            status: error.status,
        };

        if (error.error && typeof error.error === 'object') {
            problemDetails = { ...problemDetails, ...error.error };
        } else if (typeof error.error === 'string') {
            problemDetails.detail = error.error;
        }

        if (!problemDetails.title && problemDetails.status) {
            problemDetails.title = `Error ${problemDetails.status}`;
        }

        return throwError(() => problemDetails);
    }
}
