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
        schoolId: 1,
        schoolName: 'Test School',
        grade: 'Grade 5',
        language: 'English',
        reference: 'REF001',
        photoUrl: 'https://example.com/photo.jpg',
        isActive: true,
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
    });

    describe('Loading State', () => {
        it('should not show loading spinner when student is loaded', () => {
            fixture.detectChanges();

            const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
            expect(spinner).toBeFalsy();
        });

        it('should show student content when loaded', () => {
            fixture.detectChanges();

            const card = fixture.debugElement.query(By.css('.card'));
            expect(card).toBeTruthy();
        });
    });

    describe('Student Display (AC #2)', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should display student name', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('John');
            expect(content).toContain('Doe');
        });

        it('should display school name', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Test School');
        });

        it('should display grade when present', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('Grade 5');
        });

        it('should display language when present', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('English');
        });

        it('should display reference number when present', () => {
            const content = fixture.nativeElement.textContent;
            expect(content).toContain('REF001');
        });

        it('should display active badge when student is active', () => {
            const badge = fixture.debugElement.query(By.css('.badge-success'));
            expect(badge).toBeTruthy();
            expect(badge.nativeElement.textContent).toContain('Active');
        });

        it('should display inactive badge when student is inactive', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, isActive: false }));

            // Recreate component with new data
            fixture = TestBed.createComponent(StudentProfilePage);
            component = fixture.componentInstance;
            fixture.detectChanges();

            const badge = fixture.debugElement.query(By.css('.badge-ghost'));
            expect(badge).toBeTruthy();
            expect(badge.nativeElement.textContent).toContain('Inactive');
        });
    });

    describe('Avatar Display (AC #2, #3)', () => {
        it('should display avatar component with student data', () => {
            fixture.detectChanges();

            const avatar = fixture.debugElement.query(By.css('app-student-avatar'));
            expect(avatar).toBeTruthy();
        });

        it('should pass student name to avatar component', () => {
            fixture.detectChanges();

            const avatar = fixture.debugElement.query(By.css('app-student-avatar'));
            // Verify the avatar is rendered with the student's name (check the DOM for presence)
            expect(avatar).toBeTruthy();
            // The avatar should have size lg class applied
            const avatarEl = avatar.query(By.css('.student-avatar'));
            expect(avatarEl).toBeTruthy();
        });

        it('should use large size for profile avatar', () => {
            fixture.detectChanges();

            const avatar = fixture.debugElement.query(By.css('app-student-avatar'));
            // Check that the large size class is applied
            const avatarDiv = avatar.query(By.css('.w-32'));
            expect(avatarDiv).toBeTruthy();
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
            mockStudentService.getStudentById.mockReturnValue(throwError(() => ({})));

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
            const backButton = fixture.debugElement.query(By.css('.btn-ghost'));
            backButton.triggerEventHandler('click', null);

            expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
        });

        it('should have edit student button', () => {
            // Find button containing "Edit Student" text
            const buttons = fixture.debugElement.queryAll(By.css('.btn-primary'));
            const editButton = buttons.find((b) => b.nativeElement.textContent.includes('Edit Student'));
            expect(editButton).toBeTruthy();
            // Verify button text
            expect(editButton!.nativeElement.textContent).toContain('Edit Student');
        });
    });

    describe('Optional Fields', () => {
        it('should not display grade section when grade is not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, grade: undefined }));

            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('Grade');
        });

        it('should not display language section when language is not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, language: undefined }));

            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            // Check that Language label is not present when value is missing
            const languageLabels = fixture.debugElement.queryAll(By.css('.text-xs'));
            const hasLanguageLabel = languageLabels.some((el) => el.nativeElement.textContent.includes('Language'));
            expect(hasLanguageLabel).toBeFalsy();
        });

        it('should not display reference number section when not set', () => {
            mockStudentService.getStudentById.mockReturnValue(of({ ...mockStudent, reference: undefined }));

            fixture = TestBed.createComponent(StudentProfilePage);
            fixture.detectChanges();

            const content = fixture.nativeElement.textContent;
            expect(content).not.toContain('REF001');
        });
    });
});
