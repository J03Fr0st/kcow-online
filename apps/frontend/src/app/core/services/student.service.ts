import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, tap, catchError, finalize, throwError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Student {
    // Primary identifier
    id: number;

    // XSD Field: "Reference" (10 chars max, required) - Unique reference code
    reference: string;

    // XSD Field: "Child_Name" (50 chars max) - Renamed to FirstName for clarity
    firstName?: string;

    // XSD Field: "Child_Surname" (50 chars max) - Renamed to LastName for clarity
    lastName?: string;

    // XSD Field: "Child_birthdate" (datetime)
    dateOfBirth?: string; // ISO date string from DateOnly

    // XSD Field: "Sex" (3 chars max) - Gender (M/F)
    gender?: string;

    // XSD Field: "Language" (3 chars max) - Language preference (Afr/Eng)
    language?: string;

    // Account Person Fields (Responsible Adult)
    // XSD Field: "Account_Person_Name" (50 chars max)
    accountPersonName?: string;

    // XSD Field: "Account_Person_Surname" (50 chars max)
    accountPersonSurname?: string;

    // XSD Field: "Account_Person_Idnumber" (20 chars max)
    accountPersonIdNumber?: string;

    // XSD Field: "Account_Person_Cellphone" (20 chars max)
    accountPersonCellphone?: string;

    // XSD Field: "Account_Person_Office" (20 chars max)
    accountPersonOffice?: string;

    // XSD Field: "Account_Person_Home" (20 chars max)
    accountPersonHome?: string;

    // XSD Field: "Account_Person_Email" (100 chars max)
    accountPersonEmail?: string;

    // XSD Field: "Relation" (20 chars max)
    relation?: string;

    // Mother's Details
    // XSD Field: "Mother_Name" (50 chars max)
    motherName?: string;

    // XSD Field: "Mother_Surname" (50 chars max)
    motherSurname?: string;

    // XSD Field: "Mother_Office" (20 chars max)
    motherOffice?: string;

    // XSD Field: "Mother_Cell" (20 chars max)
    motherCell?: string;

    // XSD Field: "Mother_Home" (20 chars max)
    motherHome?: string;

    // XSD Field: "Mother_Email" (100 chars max)
    motherEmail?: string;

    // Father's Details
    // XSD Field: "Father_Name" (50 chars max)
    fatherName?: string;

    // XSD Field: "Father_Surname" (50 chars max)
    fatherSurname?: string;

    // XSD Field: "Father_Office" (20 chars max)
    fatherOffice?: string;

    // XSD Field: "Father_Cell" (20 chars max)
    fatherCell?: string;

    // XSD Field: "Father_Home" (20 chars max)
    fatherHome?: string;

    // XSD Field: "Father_Email" (100 chars max)
    fatherEmail?: string;

    // Address Fields
    // XSD Field: "Address1" (50 chars max)
    address1?: string;

    // XSD Field: "Address2" (50 chars max)
    address2?: string;

    // XSD Field: "Code" (10 chars max) - Postal code
    postalCode?: string;

    // Enrollment Fields
    // XSD Field: "School_Name" (50 chars max) - Denormalized school name
    schoolName?: string;

    // Foreign key to School entity
    schoolId?: number;

    // XSD Field: "Class_Group" (10 chars max) - Class group code
    classGroupCode?: string;

    // Foreign key to ClassGroup entity
    classGroupId?: number;

    // XSD Field: "Grade" (5 chars max)
    grade?: string;

    // XSD Field: "Teacher" (50 chars max)
    teacher?: string;

    // XSD Field: "Attending_KCOW_at" (50 chars max)
    attendingKcowAt?: string;

    // XSD Field: "Aftercare" (50 chars max)
    aftercare?: string;

    // XSD Field: "Extra" (50 chars max)
    extra?: string;

    // XSD Field: "Home_Time" (datetime)
    homeTime?: string; // ISO datetime string

    // XSD Field: "Start_Classes" (datetime)
    startClasses?: string; // ISO datetime string

    // XSD Field: "Terms" (10 chars max)
    terms?: string;

    // XSD Field: "Seat" (5 chars max)
    seat?: string;

    // XSD Field: "Truck" (3 chars max)
    truck?: string;

    // XSD Field: "Family" (50 chars max) - Family grouping code
    family?: string;

    // XSD Field: "Sequence" (50 chars max)
    sequence?: string;

    // Financial Fields
    // XSD Field: "Financial_Code" (10 chars max)
    financialCode?: string;

    // XSD Field: "Charge" (money)
    charge?: number;

    // XSD Field: "Deposit" (50 chars max)
    deposit?: string;

    // XSD Field: "PayDate" (50 chars max)
    payDate?: string;

    // T-Shirt Order Fields (Set 1)
    // XSD Field: "Tshirt_Code" (5 chars max, required)
    tshirtCode?: string;

    // XSD Field: "Tshirt_Money_1" (30 chars max)
    tshirtMoney1?: string;

    // XSD Field: "Tshirt_MoneyDate_1" (datetime)
    tshirtMoneyDate1?: string; // ISO datetime string

    // XSD Field: "Tshirt_Received_1" (30 chars max)
    tshirtReceived1?: string;

    // XSD Field: "Tshirt_RecDate_1" (datetime)
    tshirtRecDate1?: string; // ISO datetime string

    // XSD Field: "Receive_Note_1" (255 chars max)
    receiveNote1?: string;

    // XSD Field: "TshirtSize1" (10 chars max)
    tshirtSize1?: string;

    // XSD Field: "TshirtColor1" (20 chars max)
    tshirtColor1?: string;

    // XSD Field: "TshirtDesign1" (20 chars max)
    tshirtDesign1?: string;

    // T-Shirt Order Fields (Set 2)
    // XSD Field: "TshirtSize2" (10 chars max)
    tshirtSize2?: string;

    // XSD Field: "Tshirt_Money_2" (30 chars max)
    tshirtMoney2?: string;

    // XSD Field: "Tshirt_MoneyDate_2" (datetime)
    tshirtMoneyDate2?: string; // ISO datetime string

    // XSD Field: "Tshirt_Received_2" (30 chars max)
    tshirtReceived2?: string;

    // XSD Field: "Tshirt_RecDate_2" (datetime)
    tshirtRecDate2?: string; // ISO datetime string

    // XSD Field: "Receive_Note_2" (255 chars max)
    receiveNote2?: string;

    // XSD Field: "TshirtColor2" (20 chars max)
    tshirtColor2?: string;

    // XSD Field: "TshirtDesign2" (20 chars max)
    tshirtDesign2?: string;

    // Status & Tracking Fields
    // XSD Field: "Indicator_1" (3 chars max)
    indicator1?: string;

    // XSD Field: "Indicator_2" (3 chars max)
    indicator2?: string;

    // XSD Field: "General_Note" (255 chars max)
    generalNote?: string;

    // XSD Field: "Print_Id_Card" (bit, required, default false)
    printIdCard: boolean;

    // XSD Field: "AcceptTermsCond" (50 chars max)
    acceptTermsCond?: string;

    // XSD Field: "Status" (20 chars max)
    status?: string;

    // XSD Field: "SmsOrEmail" (10 chars max) - Contact preference
    smsOrEmail?: string;

    // XSD Field: "SchoolClose" (datetime, used as time in legacy)
    schoolClose?: string; // ISO datetime string

    // XSD Field: "Cnt" (float)
    cnt?: number;

    // XSD Field: "OnlineEntry" (int)
    onlineEntry?: number;

    // XSD Field: "Created" (datetime) - Legacy timestamp (distinct from CreatedAt)
    legacyCreated?: string; // ISO datetime string

    // XSD Field: "Submitted" (datetime)
    submitted?: string; // ISO datetime string

    // XSD Field: "Updated" (datetime) - Legacy timestamp (distinct from UpdatedAt)
    legacyUpdated?: string; // ISO datetime string

    // XSD Field: "BookEmail" (255 chars max)
    bookEmail?: string;

    // XSD Field: "Report1GivenOut" (255 chars max)
    report1GivenOut?: string;

    // XSD Field: "AccountGivenOut" (255 chars max)
    accountGivenOut?: string;

    // XSD Field: "CertificatePrinted" (255 chars max)
    certificatePrinted?: string;

    // XSD Field: "Report2GivenOut" (255 chars max)
    report2GivenOut?: string;

    // XSD Field: "Social" (255 chars max)
    social?: string;

    // XSD Field: "ActivityReportGivenOut" (255 chars max)
    activityReportGivenOut?: string;

    // XSD Field: "Photo" (attachment) - Stored as URL/path
    photoUrl?: string;

    // XSD Field: "PhotoUpdated" (datetime)
    photoUpdated?: string; // ISO datetime string

    // Soft delete flag (not in XSD, application-level)
    isActive: boolean;

    // Audit fields (not in XSD, application-level)
    createdAt: string; // ISO datetime string
    updatedAt?: string; // ISO datetime string

    // Computed/denormalized fields (not in backend entity)
    familyName?: string; // Computed from StudentFamilies relationship
}

export interface StudentListItem {
    id: number;
    reference: string;
    firstName?: string;
    lastName?: string;
    grade?: string;
    schoolName?: string;
    familyName?: string;
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
    schoolId?: number;
    classGroupId?: number;
}

export interface CreateStudentRequest {
    reference: string; // Required, 10 chars max
    firstName?: string;
    lastName?: string;
    dateOfBirth?: string;
    gender?: string;
    language?: string;
    grade?: string;
    schoolName?: string; // Legacy XSD field
    photoUrl?: string;
    schoolId?: number;
    classGroupId?: number;
    classGroupCode?: string;
    familyId?: number;
    seat?: string;
    // Include all other optional fields as needed
    [key: string]: unknown;
}

export interface UpdateStudentRequest {
    reference?: string; // Optional on update, but should match existing
    firstName?: string;
    lastName?: string;
    dateOfBirth?: string;
    gender?: string;
    language?: string;
    grade?: string;
    schoolName?: string; // Legacy XSD field
    photoUrl?: string;
    schoolId?: number;
    classGroupId?: number;
    classGroupCode?: string;
    familyId?: number;
    seat?: string;
    isActive?: boolean;
    // Include all other optional fields as needed
    [key: string]: unknown;
}

export interface StudentSearchResult {
    id: number;
    fullName: string;
    schoolName: string;
    grade: string;
    classGroupName: string;
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
        if (params.schoolId !== undefined) {
            httpParams = httpParams.set('schoolId', params.schoolId.toString());
        }
        if (params.classGroupId !== undefined) {
            httpParams = httpParams.set('classGroupId', params.classGroupId.toString());
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
                    reference: newStudent.reference,
                    firstName: newStudent.firstName,
                    lastName: newStudent.lastName,
                    grade: newStudent.grade,
                    schoolName: newStudent.schoolName,
                    familyName: newStudent.familyName,
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
                        reference: updatedStudent.reference,
                        firstName: updatedStudent.firstName,
                        lastName: updatedStudent.lastName,
                        grade: updatedStudent.grade,
                        schoolName: updatedStudent.schoolName,
                        familyName: updatedStudent.familyName,
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

    /**
     * Search for students by name (global search)
     */
    searchStudents(query: string, limit: number = 10): Observable<StudentSearchResult[]> {
        if (!query || query.trim().length < 2) {
            return of([]);
        }

        let httpParams = new HttpParams()
            .set('q', query.trim())
            .set('limit', limit.toString());

        return this.http.get<StudentSearchResult[]>(`${this.apiUrl}/search`, {
            withCredentials: true,
            params: httpParams
        }).pipe(
            catchError((error: HttpErrorResponse) => {
                return this.handleError(error);
            })
        );
    }
}
