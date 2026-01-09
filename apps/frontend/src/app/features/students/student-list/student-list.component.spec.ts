import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StudentListComponent } from './student-list.component';
import { StudentService, type StudentListItem, type PaginatedStudentsResponse, type ProblemDetails } from '@core/services/student.service';
import { of } from 'rxjs';
import { signal, type WritableSignal, type Signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';

interface MockStudentService {
    students: WritableSignal<StudentListItem[]>;
    totalCount: WritableSignal<number>;
    currentPage: WritableSignal<number>;
    currentPageSize: WritableSignal<number>;
    isLoading: WritableSignal<boolean>;
    error: WritableSignal<ProblemDetails | null>;
    getStudents: jest.Mock;
}

describe('StudentListComponent', () => {
    let component: StudentListComponent;
    let fixture: ComponentFixture<StudentListComponent>;
    let mockStudentService: MockStudentService;

    const mockStudents = [
        {
            id: 1,
            firstName: 'John',
            lastName: 'Doe',
            grade: '5',
            schoolName: 'Test School',
            reference: 'REF001',
            isActive: true,
            status: 'Active'
        },
        {
            id: 2,
            firstName: 'Jane',
            lastName: 'Smith',
            grade: '6',
            schoolName: 'Another School',
            reference: 'REF002',
            isActive: true,
            status: 'Active'
        }
    ];

    beforeEach(async () => {
        mockStudentService = {
            students: signal([]),
            totalCount: signal(0),
            currentPage: signal(1),
            currentPageSize: signal(25),
            isLoading: signal(false),
            error: signal(null),
            getStudents: jest.fn().mockReturnValue(of({ items: [], totalCount: 0, page: 1, pageSize: 25 })),
        };

        await TestBed.configureTestingModule({
            imports: [StudentListComponent],
            providers: [
                { provide: StudentService, useValue: mockStudentService },
                provideRouter([]),
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(StudentListComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call getStudents on init', () => {
        fixture.detectChanges();
        expect(mockStudentService.getStudents).toHaveBeenCalledWith({
            page: 1,
            pageSize: 25,
            sortBy: undefined,
            sortDirection: 'asc',
        });
    });

    it('should render table rows when students are present', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(2);
        fixture.detectChanges();

        const rows = fixture.debugElement.queryAll(By.css('tbody tr'));
        expect(rows.length).toBe(2);
        expect(rows[0].nativeElement.textContent).toContain('John Doe');
        expect(rows[1].nativeElement.textContent).toContain('Jane Smith');
    });

    it('should render empty state when no students', () => {
        mockStudentService.students.set([]);
        mockStudentService.isLoading.set(false);
        mockStudentService.error.set(null);
        fixture.detectChanges();

        const emptyState = fixture.debugElement.query(By.css('.empty-state'));
        expect(emptyState).toBeTruthy();
        expect(emptyState.nativeElement.textContent).toContain('No students found');
    });

    it('should show loading spinner when loading', () => {
        mockStudentService.isLoading.set(true);
        fixture.detectChanges();

        const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
        expect(spinner).toBeTruthy();
    });

    it('should call getStudents with sort params when name header is clicked', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(2);
        fixture.detectChanges();
        mockStudentService.getStudents.mockClear();

        // Get Name header (index 1, after Photo at index 0)
        const nameHeader = fixture.debugElement.queryAll(By.css('thead th'))[1];

        // Click to sort ascending
        nameHeader.triggerEventHandler('click', null);
        fixture.detectChanges();

        expect(mockStudentService.getStudents).toHaveBeenCalledWith({
            page: 1,
            pageSize: 25,
            sortBy: 'name',
            sortDirection: 'asc',
        });

        // Click again to sort descending
        mockStudentService.getStudents.mockClear();
        nameHeader.triggerEventHandler('click', null);
        fixture.detectChanges();

        expect(mockStudentService.getStudents).toHaveBeenCalledWith({
            page: 1,
            pageSize: 25,
            sortBy: 'name',
            sortDirection: 'desc',
        });
    });

    it('should call getStudents with sort params when school header is clicked', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(2);
        fixture.detectChanges();
        mockStudentService.getStudents.mockClear();

        // Get School header (index 2, after Photo and Name)
        const schoolHeader = fixture.debugElement.queryAll(By.css('thead th'))[2];

        schoolHeader.triggerEventHandler('click', null);
        fixture.detectChanges();

        expect(mockStudentService.getStudents).toHaveBeenCalledWith({
            page: 1,
            pageSize: 25,
            sortBy: 'school',
            sortDirection: 'asc',
        });
    });

    it('should show error alert when error occurs', () => {
        mockStudentService.error.set({ title: 'Server Error', detail: 'Failed to fetch students' });
        mockStudentService.isLoading.set(false);
        fixture.detectChanges();

        const alert = fixture.debugElement.query(By.css('[role="alert"]'));
        expect(alert).toBeTruthy();
        expect(alert.nativeElement.textContent).toContain('Server Error');
        expect(alert.nativeElement.textContent).toContain('Failed to fetch students');
    });

    it('should display pagination when total pages > 1', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(50);
        fixture.detectChanges();

        const pagination = fixture.debugElement.query(By.css('.join'));
        expect(pagination).toBeTruthy();
        expect(fixture.nativeElement.textContent).toContain('Showing 1 to 25 of 50 students');
    });

    it('should not display pagination when only one page', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(2);
        fixture.detectChanges();

        const pagination = fixture.debugElement.query(By.css('.join'));
        expect(pagination).toBeFalsy();
    });

    it('should handle page change when clicking next page', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(50);
        mockStudentService.getStudents.mockClear();
        fixture.detectChanges();

        const nextButton = fixture.debugElement.queryAll(By.css('.join-item.btn'))[3]; // Last button (»)
        nextButton.triggerEventHandler('click', null);
        fixture.detectChanges();

        expect(mockStudentService.getStudents).toHaveBeenCalled();
    });

    it('should disable first and previous buttons on first page', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(50);
        fixture.detectChanges();

        const buttons = fixture.debugElement.queryAll(By.css('.join-item.btn'));
        expect(buttons[0].nativeElement.disabled).toBe(true); // «
        expect(buttons[1].nativeElement.textContent.trim()).toBe('1');
    });

    it('should disable last and next buttons on last page', () => {
        mockStudentService.students.set(mockStudents);
        mockStudentService.totalCount.set(50);
        component['currentPage'].set(2);
        fixture.detectChanges();

        const buttons = fixture.debugElement.queryAll(By.css('.join-item.btn'));
        expect(buttons[3].nativeElement.disabled).toBe(true); // »
    });

    it('should display student status badge correctly', () => {
        mockStudentService.students.set([
            { ...mockStudents[0], isActive: true },
            { ...mockStudents[1], isActive: false }
        ]);
        fixture.detectChanges();

        const badges = fixture.debugElement.queryAll(By.css('.badge'));
        expect(badges[0].nativeElement.textContent).toContain('Active');
        expect(badges[0].nativeElement.classList).toContain('badge-success');
        expect(badges[1].nativeElement.textContent).toContain('Inactive');
    });

    it('should call loadStudents when retry button is clicked on error', () => {
        mockStudentService.error.set({ title: 'Error', detail: 'Failed to load' });
        mockStudentService.isLoading.set(false);
        mockStudentService.getStudents.mockClear();
        fixture.detectChanges();

        const retryButton = fixture.debugElement.query(By.css('.btn-ghost'));
        retryButton.triggerEventHandler('click', null);
        fixture.detectChanges();

        expect(mockStudentService.getStudents).toHaveBeenCalled();
    });

    it('should show Add Student CTA in empty state', () => {
        mockStudentService.students.set([]);
        mockStudentService.isLoading.set(false);
        mockStudentService.error.set(null);
        fixture.detectChanges();

        const ctaButton = fixture.debugElement.query(By.css('.empty-state .btn-primary'));
        expect(ctaButton).toBeTruthy();
        expect(ctaButton.nativeElement.textContent).toContain('Add Your First Student');
    });
});
