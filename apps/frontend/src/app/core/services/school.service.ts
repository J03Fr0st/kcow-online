import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BillingSettings {
    defaultSessionRate?: number;
    billingCycle?: 'Monthly' | 'Termly';
    billingNotes?: string;
}

export interface School {
    id: number;
    name: string;
    shortName?: string;
    truckId?: number;
    price?: number;
    feeDescription?: string;
    formula?: number;
    visitDay?: string;
    visitSequence?: string;
    contactPerson?: string;
    contactCell?: string;
    phone?: string;
    telephone?: string;
    fax?: string;
    email?: string;
    circularsEmail?: string;
    address?: string;
    address2?: string;
    headmaster?: string;
    headmasterCell?: string;
    isActive: boolean;
    language?: string;
    printInvoice: boolean;
    importFlag: boolean;
    afterschool1Name?: string;
    afterschool1Contact?: string;
    afterschool2Name?: string;
    afterschool2Contact?: string;
    schedulingNotes?: string;
    moneyMessage?: string;
    safeNotes?: string;
    webPage?: string;
    kcowWebPageLink?: string;
    billingSettings?: BillingSettings;
}

export interface CreateSchoolRequest {
    name: string;
    shortName?: string;
    truckId?: number;
    price?: number;
    feeDescription?: string;
    formula?: number;
    visitDay?: string;
    visitSequence?: string;
    contactPerson?: string;
    contactCell?: string;
    phone?: string;
    telephone?: string;
    fax?: string;
    email?: string;
    circularsEmail?: string;
    address?: string;
    address2?: string;
    headmaster?: string;
    headmasterCell?: string;
    language?: string;
    printInvoice: boolean;
    importFlag: boolean;
    afterschool1Name?: string;
    afterschool1Contact?: string;
    afterschool2Name?: string;
    afterschool2Contact?: string;
    schedulingNotes?: string;
    moneyMessage?: string;
    safeNotes?: string;
    webPage?: string;
    kcowWebPageLink?: string;
    billingSettings?: BillingSettings;
}

export interface UpdateSchoolRequest {
    name: string;
    shortName?: string;
    truckId?: number;
    price?: number;
    feeDescription?: string;
    formula?: number;
    visitDay?: string;
    visitSequence?: string;
    contactPerson?: string;
    contactCell?: string;
    phone?: string;
    telephone?: string;
    fax?: string;
    email?: string;
    circularsEmail?: string;
    address?: string;
    address2?: string;
    headmaster?: string;
    headmasterCell?: string;
    isActive: boolean;
    language?: string;
    printInvoice: boolean;
    importFlag: boolean;
    afterschool1Name?: string;
    afterschool1Contact?: string;
    afterschool2Name?: string;
    afterschool2Contact?: string;
    schedulingNotes?: string;
    moneyMessage?: string;
    safeNotes?: string;
    webPage?: string;
    kcowWebPageLink?: string;
    billingSettings?: BillingSettings;
}

export interface ProblemDetails {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    [key: string]: unknown;
}

@Injectable({
    providedIn: 'root',
})
export class SchoolService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/schools`;

    // Signals for state
    private schoolsSignal = signal<School[]>([]);
    private isLoadingSignal = signal<boolean>(false);
    private errorSignal = signal<ProblemDetails | null>(null);

    // Read-only signals
    readonly schools = computed(() => this.schoolsSignal());
    readonly isLoading = computed(() => this.isLoadingSignal());
    readonly error = computed(() => this.errorSignal());

    /**
     * Fetch all schools from the backend
     */
    getSchools(): Observable<School[]> {
        this.isLoadingSignal.set(true);
        this.errorSignal.set(null);

        return this.http.get<School[]>(this.apiUrl, { withCredentials: true }).pipe(
            tap((schools) => {
                this.schoolsSignal.set(schools);
            }),
            catchError((error: HttpErrorResponse) => {
                // Return an observable that emits the mapped error
                return this.handleError(error);
            }),
            // Use tap only if there was an error to update the local error signal
            // This is safer than the nested tap in catchError
            tap({
                error: (err) => this.errorSignal.set(err)
            }),
            finalize(() => {
                this.isLoadingSignal.set(false);
            })
        );
    }

    /**
     * Get a single school by ID
     */
    getSchoolById(id: number): Observable<School> {
        return this.http.get<School>(`${this.apiUrl}/${id}`, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Create a new school
     */
    createSchool(school: CreateSchoolRequest): Observable<School> {
        return this.http.post<School>(this.apiUrl, school, { withCredentials: true }).pipe(
            tap((newSchool) => {
                // Update the schools signal with the new school
                this.schoolsSignal.update((schools) => [...schools, newSchool]);
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Update an existing school
     */
    updateSchool(id: number, school: UpdateSchoolRequest): Observable<School> {
        return this.http.put<School>(`${this.apiUrl}/${id}`, school, { withCredentials: true }).pipe(
            tap((updatedSchool) => {
                // Update the schools signal with the updated school
                this.schoolsSignal.update((schools) =>
                    schools.map((s) => (s.id === id ? updatedSchool : s))
                );
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Deactivate a school (soft delete - sets isActive to false)
     */
    deactivateSchool(id: number, school: School): Observable<School> {
        const { id: _, ...rest } = school;
        return this.updateSchool(id, {
            ...rest,
            isActive: false,
        });
    }

    /**
     * Reactivate a school (sets isActive back to true)
     */
    reactivateSchool(id: number, school: School): Observable<School> {
        const { id: _, ...rest } = school;
        return this.updateSchool(id, {
            ...rest,
            isActive: true,
        });
    }

    /**
     * Get only active schools (for dropdowns/selections)
     */
    getActiveSchools(): Observable<School[]> {
        return this.http.get<School[]>(this.apiUrl, { withCredentials: true }).pipe(
            map((schools: School[]) => schools.filter((s: School) => s.isActive)),
            catchError((error: HttpErrorResponse) => this.handleError(error))
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

        // Ensure title is present for standard ProblemDetails consistency
        if (!problemDetails.title && problemDetails.status) {
            problemDetails.title = `Error ${problemDetails.status}`;
        }

        return throwError(() => problemDetails);
    }
}
