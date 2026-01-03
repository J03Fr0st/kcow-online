import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Student {
    id: number;
    firstName: string;
    lastName: string;
    familyId?: number;
    schoolId: number;
    grade?: string;
    language?: string;
    dateOfBirth?: string; // ISO date string from DateOnly
    gender?: string;
    referenceNumber?: string;
    photoUrl?: string;
    status?: string;
    isActive: boolean;
    createdAt: string; // ISO datetime string
    updatedAt: string; // ISO datetime string
    familyName?: string;
    schoolName: string;
}

export interface StudentListItem {
    id: number;
    firstName: string;
    lastName: string;
    grade?: string;
    schoolName: string;
    familyName?: string;
    referenceNumber?: string;
    photoUrl?: string;
    isActive: boolean;
    status?: string;
}

export interface PaginatedStudentsResponse {
    items: StudentListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
}

export interface ProblemDetails {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    [key: string]: unknown;
}

export interface GetStudentsParams {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: 'asc' | 'desc';
}

export interface CreateStudentRequest {
    firstName: string;
    lastName: string;
    dateOfBirth?: string;
    gender?: string;
    language?: string;
    grade?: string;
    schoolName?: string; // Legacy XSD field
    referenceNumber?: string;
    photoUrl?: string;
    schoolId: number;
    familyId?: number;
}

export interface UpdateStudentRequest {
    firstName: string;
    lastName: string;
    dateOfBirth?: string;
    gender?: string;
    language?: string;
    grade?: string;
    schoolName?: string; // Legacy XSD field
    referenceNumber?: string;
    photoUrl?: string;
    schoolId: number;
    familyId?: number;
    isActive: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class StudentService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/students`;

    // Signals for state
    private studentsSignal = signal<StudentListItem[]>([]);
    private totalCountSignal = signal<number>(0);
    private currentPageSignal = signal<number>(1);
    private currentPageSizeSignal = signal<number>(25);
    private isLoadingSignal = signal<boolean>(false);
    private errorSignal = signal<ProblemDetails | null>(null);

    // Read-only signals
    readonly students = computed(() => this.studentsSignal());
    readonly totalCount = computed(() => this.totalCountSignal());
    readonly currentPage = computed(() => this.currentPageSignal());
    readonly currentPageSize = computed(() => this.currentPageSizeSignal());
    readonly isLoading = computed(() => this.isLoadingSignal());
    readonly error = computed(() => this.errorSignal());

    /**
     * Fetch paginated students from the backend
     */
    getStudents(params: GetStudentsParams = {}): Observable<PaginatedStudentsResponse> {
        this.isLoadingSignal.set(true);
        this.errorSignal.set(null);

        // Build query parameters
        let httpParams = new HttpParams();
        if (params.page !== undefined) {
            httpParams = httpParams.set('page', params.page.toString());
        }
        if (params.pageSize !== undefined) {
            httpParams = httpParams.set('pageSize', params.pageSize.toString());
        }
        if (params.sortBy) {
            httpParams = httpParams.set('sortBy', params.sortBy);
        }
        if (params.sortDirection) {
            httpParams = httpParams.set('sortDirection', params.sortDirection);
        }

        return this.http.get<StudentListItem[]>(this.apiUrl, {
            withCredentials: true,
            params: httpParams,
            observe: 'response'
        }).pipe(
            map((response) => {
                // Extract pagination headers
                const totalCount = response.headers.get('X-Total-Count');
                const page = response.headers.get('X-Page');
                const pageSize = response.headers.get('X-PageSize');

                const result: PaginatedStudentsResponse = {
                    items: response.body || [],
                    totalCount: totalCount ? parseInt(totalCount, 10) : 0,
                    page: page ? parseInt(page, 10) : 1,
                    pageSize: pageSize ? parseInt(pageSize, 10) : 25,
                };

                // Update signals
                this.studentsSignal.set(result.items);
                this.totalCountSignal.set(result.totalCount);
                this.currentPageSignal.set(result.page);
                this.currentPageSizeSignal.set(result.pageSize);

                return result;
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
     * Get a single student by ID
     */
    getStudentById(id: number): Observable<Student> {
        return this.http.get<Student>(`${this.apiUrl}/${id}`, { withCredentials: true }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Create a new student
     */
    createStudent(student: CreateStudentRequest): Observable<Student> {
        return this.http.post<Student>(this.apiUrl, student, { withCredentials: true }).pipe(
            tap((newStudent) => {
                // Add to the current list
                this.studentsSignal.update((students) => [...students, {
                    id: newStudent.id,
                    firstName: newStudent.firstName,
                    lastName: newStudent.lastName,
                    grade: newStudent.grade,
                    schoolName: newStudent.schoolName,
                    familyName: newStudent.familyName,
                    referenceNumber: newStudent.referenceNumber,
                    photoUrl: newStudent.photoUrl,
                    isActive: newStudent.isActive,
                    status: newStudent.status,
                }]);
            }),
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }

    /**
     * Update an existing student
     */
    updateStudent(id: number, student: UpdateStudentRequest): Observable<Student> {
        return this.http.put<Student>(`${this.apiUrl}/${id}`, student, { withCredentials: true }).pipe(
            tap((updatedStudent) => {
                // Update in the current list
                this.studentsSignal.update((students) =>
                    students.map((s) => (s.id === id ? {
                        id: updatedStudent.id,
                        firstName: updatedStudent.firstName,
                        lastName: updatedStudent.lastName,
                        grade: updatedStudent.grade,
                        schoolName: updatedStudent.schoolName,
                        familyName: updatedStudent.familyName,
                        referenceNumber: updatedStudent.referenceNumber,
                        photoUrl: updatedStudent.photoUrl,
                        isActive: updatedStudent.isActive,
                        status: updatedStudent.status,
                    } : s))
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

        // Ensure title is present for standard ProblemDetails consistency
        if (!problemDetails.title && problemDetails.status) {
            problemDetails.title = `Error ${problemDetails.status}`;
        }

        return throwError(() => problemDetails);
    }
}
