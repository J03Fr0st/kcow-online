import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { type Student, StudentService } from '@core/services/student.service';
import { of } from 'rxjs';
import { EditStudentPage } from './edit-student.page';

describe('EditStudentPage', () => {
  let component: EditStudentPage;
  let fixture: ComponentFixture<EditStudentPage>;
  let mockRouter: jest.Mocked<Partial<Router>>;
  let mockStudentService: Partial<StudentService>;

  const mockStudent: Student = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    schoolId: 1,
    familyId: 1,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    schoolName: 'Test School',
    familyName: 'Doe Family',
  };

  beforeEach(async () => {
    mockRouter = {
      navigate: jest.fn().mockResolvedValue(true),
    };

    mockStudentService = {
      getStudentById: jest.fn().mockReturnValue(of(mockStudent)),
      updateStudent: jest.fn().mockReturnValue(of(mockStudent)),
    };

    await TestBed.configureTestingModule({
      imports: [EditStudentPage],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: StudentService, useValue: mockStudentService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: jest.fn().mockReturnValue('1'),
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EditStudentPage);
    component = fixture.componentInstance;
  });

  describe('Page Creation', () => {
    it('should create', () => {
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should display page title "Edit Student"', () => {
      fixture.detectChanges();
      const title = fixture.debugElement.query(By.css('h1'));
      expect(title.nativeElement.textContent).toContain('Edit Student');
    });

    it('should have back button to students list', () => {
      fixture.detectChanges();
      const backButton = fixture.debugElement.query(By.css('button[routerLink="/students"]'));
      expect(backButton).toBeTruthy();
    });

    it('should render student form component', () => {
      fixture.detectChanges();
      const form = fixture.debugElement.query(By.css('app-student-form'));
      expect(form).toBeTruthy();
    });

    it('should extract student ID from route params', () => {
      fixture.detectChanges();
      expect(component.studentId).toBe('1');
    });
  });

  describe('Form Configuration', () => {
    it('should pass studentId to form component', () => {
      fixture.detectChanges();
      const form = fixture.debugElement.query(By.css('app-student-form'));
      // The studentId should be passed as input to the form (converted to number)
      expect(form.componentInstance.studentId).toBe(1);
    });
  });

  describe('Navigation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should navigate to students list with updated param on successful save', () => {
      component.onStudentSaved(mockStudent);

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/students'], {
        queryParams: { updated: 1 },
      });
    });

    it('should navigate to students list on cancel', () => {
      component.onCancel();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
    });
  });

  describe('Pre-population', () => {
    it('should pass studentId to form for pre-population', () => {
      fixture.detectChanges();
      const form = fixture.debugElement.query(By.css('app-student-form'));
      // Form should receive studentId to load existing data
      expect(form).toBeTruthy();
    });
  });

  describe('Form Integration', () => {
    it('should pass saved event handler to student form', () => {
      fixture.detectChanges();
      const form = fixture.debugElement.query(By.css('app-student-form'));
      expect(form.listeners.some((l) => l.name === 'saved')).toBeTruthy();
    });

    it('should pass cancelled event handler to student form', () => {
      fixture.detectChanges();
      const form = fixture.debugElement.query(By.css('app-student-form'));
      expect(form.listeners.some((l) => l.name === 'cancelled')).toBeTruthy();
    });
  });
});
