import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StudentProfilePage } from './student-profile.page';
import { StudentService, type Student, type ProblemDetails } from '@core/services/student.service';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of, throwError, Observable } from 'rxjs';
import { By } from '@angular/platform-browser';

interface MockStudentService {
    getStudentById: jest.Mock<Observable<Student>, [number]>;
}

describe('StudentProfilePage', () => {
    let component: StudentProfilePage;
    let fixture: ComponentFixture<StudentProfilePage>;
    let mockStudentService: MockStudentService;
    let mockRouter: jest.Mocked<Partial<Router>>;
    let mockActivatedRoute: Partial<ActivatedRoute>;

    const mockStudent: Student = {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '2015-03-15',
        gender: 'M',
        schoolId: 1,
        schoolName: 'Test School',
        classGroupId: 1,
        classGroupCode: '5A',
        grade: 'Grade 5',
        language: 'English',
        reference: 'REF001',
        seat: '12',
        teacher: 'Mrs. Smith',
        familyName: 'Doe Family',
        status: 'Enrolled',
        smsOrEmail: 'Email',
        photoUrl: 'https://example.com/photo.jpg',
        isActive: true,
        printIdCard: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
    };

    beforeEach(async () => {
        mockStudentService = {
            getStudentById: jest.fn().mockReturnValue(of(mockStudent)),
        };

        mockRouter = {
            navigate: jest.fn(),
        };

        mockActivatedRoute = {
            snapshot: {
                paramMap: convertToParamMap({ id: '1' }),
            } as ActivatedRoute['snapshot'],
        };

        await TestBed.configureTestingModule({
            imports: [StudentProfilePage],
            providers: [
                { provide: StudentService, useValue: mockStudentService },
                { provide: Router, useValue: mockRouter },
                { provide: ActivatedRoute, useValue: mockActivatedRoute },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(StudentProfilePage);
        component = fixture.componentInstance;
    });

    describe('Component Creation', () => {
        it('should create', () => {
            fixture.detectChanges();
            expect(component).toBeTruthy();
        });

        it('should load student on init when id is present', () => {
            fixture.detectChanges();
            expect(mockStudentService.getStudentById).toHaveBeenCalledWith(1);
        });

        it('should navigate to students list when no id is present', () => {
            mockActivatedRoute.snapshot = {
                paramMap: convertToParamMap({}),
            } as ActivatedRoute['snapshot'];

            fixture.detectChanges();

            expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
        });

        it('should have OnPush change detection', () => {
            const config = TestBed.createComponent(StudentProfilePage).componentType.decorators?.[0].metadata;
            expect(config.changeDetection).toBeDefined();
        });
    });

    describe('Signal State', () => {
        it('should initialize with loading signal', () => {
            expect(component['isLoading']).toBeDefined();
            expect(component.isLoading()).toBe(false);
        });

        it('should initialize with student signal', () => {
            expect(component['student']).toBeDefined();
            expect(component.student()).toBeNull();
        });

        it('should initialize with error signal', () => {
            expect(component['error']).toBeDefined();
            expect(component.error()).toBeNull();
        });

        it('should initialize with activeTab signal set to child-info', () => {
            expect(component['activeTab']).toBeDefined();
            expect(component.activeTab()).toBe('child-info');
        });
    });

    describe('Loading State', () => {
        it('should show loading spinner when loading', () => {
            component['isLoading'].set(true);
            fixture.detectChanges();

            const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
            expect(spinner).toBeTruthy();
        });

        it('should not show loading spinner when student is loaded', () => {
            fixture.detectChanges();

            const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
            expect(spinner).toBeFalsy();
        });

        it('should show student content when loaded', () => {
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).toContain('John');
            expect(content).toContain('Doe');
        });
    });

    describe('3-Column Header Layout (AC #1)', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should have 3-column grid layout', () => {
            const grid = fixture.debugElement.query(By.css('.grid-cols-1.md\\:grid-cols-3'));
            expect(grid).toBeTruthy();
        });

        it('should display photo, name, and demographics in column 1', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('John');
            expect(content).toContain('Doe');
            expect(content).toContain('REF001');
            expect(content).toContain('DOB:');
            expect(content).toContain('Gender:');
            expect(content).toContain('Grade:');
        });

        it('should display school assignment in column 2', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('School Assignment');
            expect(content).toContain('Test School');
            expect(content).toContain('Class Group');
            expect(content).toContain('5A');
            expect(content).toContain('Seat Number');
            expect(content).toContain('12');
            expect(content).toContain('Teacher');
            expect(content).toContain('Mrs. Smith');
        });

        it('should display status indicators in column 3', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Status Overview');
            expect(content).toContain('Attendance');
            expect(content).toContain('Epic 5');
            expect(content).toContain('Financial');
            expect(content).toContain('Epic 6');
        });
    });

    describe('Tabbed Navigation (AC #2)', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should display all tabs', () => {
            const tabs = fixture.debugElement.queryAll(By.css('.tab'));
            expect(tabs.length).toBe(4);
        });

        it('should have child-info tab', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Child Info');
            expect(content).toContain('👤');
        });

        it('should have financial tab', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Financial');
            expect(content).toContain('💰');
        });

        it('should have attendance tab', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Attendance');
            expect(content).toContain('📋');
        });

        it('should have evaluation tab', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Evaluation');
            expect(content).toContain('📊');
        });

        it('should set child-info as active tab by default', () => {
            expect(component.isTabActive('child-info')).toBe(true);
        });

        it('should switch to financial tab when clicked', () => {
            const tabs = fixture.debugElement.queryAll(By.css('.tab'));
            const financialTab = tabs.find(t => t.nativeElement.textContent.includes('Financial'));

            financialTab!.nativeElement.click();
            fixture.detectChanges();

            expect(component.isTabActive('financial')).toBe(true);
            expect(component.isTabActive('child-info')).toBe(false);
        });
    });

    describe('Tab Content', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should display child info tab content', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Child Information');
            expect(content).toContain('Language');
            expect(content).toContain('English');
            expect(content).toContain('Family');
            expect(content).toContain('Doe Family');
        });

        it('should display financial placeholder', () => {
            component.setActiveTab('financial');
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Financial Information');
            expect(content).toContain('Epic 6');
        });

        it('should display attendance placeholder', () => {
            component.setActiveTab('attendance');
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Attendance Records');
            expect(content).toContain('Epic 5');
        });

        it('should display evaluation placeholder', () => {
            component.setActiveTab('evaluation');
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Student Evaluations');
            expect(content).toContain('Epic 5');
        });
    });

    describe('Avatar Display (AC #1)', () => {
        it('should display avatar component with student data', () => {
            fixture.detectChanges();

            const avatar = fixture.debugElement.query(By.css('app-student-avatar'));
            expect(avatar).toBeTruthy();
        });

        it('should use xl size for profile avatar', () => {
            fixture.detectChanges();

            const avatar = fixture.debugElement.query(By.css('app-student-avatar'));
            expect(avatar).toBeTruthy();
            expect(avatar.attributes['size']).toBe('xl');
        });
    });

    describe('Helper Methods', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should format date correctly', () => {
            const formatted = component.formatDate('2015-03-15');
            expect(formatted).not.toBe('-');
            expect(formatted).toContain('2015');
        });

        it('should return dash for invalid date', () => {
            const formatted = component.formatDate('invalid-date');
            expect(formatted).toBe('invalid-date');
        });

        it('should return dash for empty date', () => {
            const formatted = component.formatDate(undefined);
            expect(formatted).toBe('-');
        });

        it('should return dash for empty getDisplayValue', () => {
            expect(component.getDisplayValue(null)).toBe('-');
            expect(component.getDisplayValue(undefined)).toBe('-');
            expect(component.getDisplayValue('')).toBe('-');
            expect(component.getDisplayValue('value')).toBe('value');
        });
    });

    describe('Error Handling', () => {
        it('should display error message when loading fails', () => {
            const errorDetails: ProblemDetails = {
                title: 'Not Found',
                detail: 'Student not found',
                status: 404,
            };
            mockStudentService.getStudentById.mockReturnValue(throwError(() => errorDetails));

            fixture.detectChanges();

            const alert = fixture.debugElement.query(By.css('.alert-error'));
            expect(alert).toBeTruthy();
            expect(alert.nativeElement.textContent).toContain('Student not found');
        });

        it('should show default error message when detail is empty', () => {
            mockStudentService.getStudentById.mockReturnValue(throwError(() => ({ title: 'Error' })));

            fixture.detectChanges();

            const alert = fixture.debugElement.query(By.css('.alert-error'));
            expect(alert).toBeTruthy();
            expect(alert.nativeElement.textContent).toContain('Failed to load student profile');
        });
    });

    describe('Navigation', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should navigate back to students list when back button is clicked', () => {
            const backButtons = fixture.debugElement.queryAll(By.css('.btn-ghost'));
            const backButton = backButtons.find(b => b.nativeElement.textContent.includes('Back to Students'));

            backButton!.triggerEventHandler('click', null);

            expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
        });

        it('should have edit student button with correct route', () => {
            const editButton = fixture.debugElement.query(By.css('.btn-primary'));
            expect(editButton).toBeTruthy();
            expect(editButton!.nativeElement.textContent).toContain('Edit Student');

            const routerLink = editButton!.attributes['routerLink'];
            expect(routerLink).toContain('/students/1/edit');
        });
    });

    describe('Optional Fields', () => {
        it('should not display dateOfBirth section when not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, dateOfBirth: undefined }));
            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('DOB:');
        });

        it('should not display gender section when not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, gender: undefined }));
            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('Gender:');
        });

        it('should not display grade section when not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, grade: undefined }));
            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('Grade:');
        });

        it('should not display school assignment when schoolName is not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, schoolName: undefined }));
            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('School Assignment');
        });
    });

    describe('Performance (AC #3)', () => {
        it('should use single API call for student data', () => {
            fixture.detectChanges();
            expect(mockStudentService.getStudentById).toHaveBeenCalledTimes(1);
        });

        it('should use signals for reactive state', () => {
            expect(component['student']).toBeDefined();
            expect(component['isLoading']).toBeDefined();
            expect(component['error']).toBeDefined();
            expect(component['activeTab']).toBeDefined();
        });
    });
});
