import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { Router } from '@angular/router';
import {
  type CreateFamilyRequest,
  type CreateGuardianRequest,
  type Family,
  FamilyService,
  type Guardian,
  type StudentSummary,
  type UpdateFamilyRequest,
} from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import { StudentService } from '@core/services/student.service';
import { type Observable, of } from 'rxjs';
import { FamilySectionComponent } from './family-section.component';

interface MockFamilyService {
  getFamilyById: jest.Mock<Observable<Family>, [number]>;
  updateFamily: jest.Mock<Observable<Family>, [number, UpdateFamilyRequest]>;
  createFamily: jest.Mock<Observable<Family>, [CreateFamilyRequest]>;
  getStudentsByFamily: jest.Mock<Observable<StudentSummary[]>, [number]>;
  addGuardian: jest.Mock<Observable<Guardian>, [number, CreateGuardianRequest]>;
  updateGuardian: jest.Mock<Observable<Guardian>, [number, number, CreateGuardianRequest]>;
  deleteGuardian: jest.Mock<Observable<void>, [number, number]>;
}

interface MockStudentService {
  updateStudent: jest.Mock<Observable<any>, [number, any]>;
}

describe('FamilySectionComponent', () => {
  let component: FamilySectionComponent;
  let fixture: ComponentFixture<FamilySectionComponent>;
  let mockFamilyService: MockFamilyService;
  let mockStudentService: MockStudentService;
  let mockRouter: jest.Mocked<Partial<Router>>;
  let mockNotificationService: jest.Mocked<Partial<NotificationService>>;

  const mockFamily: Family = {
    id: 1,
    familyName: 'Smith Family',
    primaryBillingContactId: 1,
    notes: 'Test notes',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    guardians: [
      {
        id: 1,
        familyId: 1,
        firstName: 'John',
        lastName: 'Smith Sr.',
        relationship: 'Father',
        phone: '0123456789',
        email: 'john@example.com',
        isPrimaryContact: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
      {
        id: 2,
        familyId: 1,
        firstName: 'Jane',
        lastName: 'Smith',
        relationship: 'Mother',
        phone: '0123456790',
        email: 'jane@example.com',
        isPrimaryContact: false,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    studentCount: 2,
  };

  const mockSiblings: StudentSummary[] = [
    {
      id: 2,
      firstName: 'Sarah',
      lastName: 'Smith',
      grade: 'Grade 6',
      status: 'Enrolled',
      isActive: true,
    },
    {
      id: 3,
      firstName: 'Tom',
      lastName: 'Smith',
      grade: 'Grade 4',
      status: 'Enrolled',
      isActive: true,
    },
  ];

  const mockStudent = {
    id: 1,
    familyId: 1,
    familyName: 'Smith Family',
  };

  beforeEach(async () => {
    mockFamilyService = {
      getFamilyById: jest.fn().mockReturnValue(of(mockFamily)),
      updateFamily: jest.fn().mockReturnValue(of(mockFamily)),
      createFamily: jest.fn().mockReturnValue(of(mockFamily)),
      getStudentsByFamily: jest.fn().mockReturnValue(of(mockSiblings)),
      addGuardian: jest.fn().mockReturnValue(of(mockFamily.guardians?.[0])),
      updateGuardian: jest.fn().mockReturnValue(of(mockFamily.guardians?.[0])),
      deleteGuardian: jest.fn().mockReturnValue(of(undefined)),
    };

    mockStudentService = {
      updateStudent: jest.fn().mockReturnValue(of({})),
    };

    mockRouter = {
      navigate: jest.fn(),
    };

    mockNotificationService = {
      success: jest.fn().mockReturnValue('notification-id'),
      error: jest.fn().mockReturnValue('notification-id'),
    };

    await TestBed.configureTestingModule({
      imports: [FamilySectionComponent, ReactiveFormsModule],
      providers: [
        { provide: FamilyService, useValue: mockFamilyService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: Router, useValue: mockRouter },
        { provide: NotificationService, useValue: mockNotificationService },
        FormBuilder,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FamilySectionComponent);
    component = fixture.componentInstance;

    // Set the required input
    component.student.set(mockStudent);
  });

  describe('Component Creation', () => {
    it('should create', () => {
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should load family data on init', () => {
      fixture.detectChanges();
      expect(mockFamilyService.getFamilyById).toHaveBeenCalledWith(1);
      expect(component.family()).toEqual(mockFamily);
    });
  });

  describe('Display Family (AC #1)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display family name', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('Smith Family');
    });

    it('should display guardians', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('John Smith Sr.');
      expect(content).toContain('Jane Smith');
      expect(content).toContain('Father');
      expect(content).toContain('Mother');
    });

    it('should display contact information', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('0123456789');
      expect(content).toContain('john@example.com');
    });
  });

  describe('Edit Family (AC #3)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have edit button', () => {
      const editButton = fixture.debugElement.query(By.css('button.btn-ghost'));
      expect(editButton).toBeTruthy();
      expect(editButton.nativeElement.textContent).toContain('Edit Family');
    });

    it('should enable edit mode when edit button is clicked', () => {
      const editButton = fixture.debugElement.query(By.css('button.btn-ghost'));
      editButton.nativeElement.click();
      fixture.detectChanges();

      expect(component.isEditingFamily()).toBe(true);
    });

    it('should save family changes', () => {
      component.editFamily();
      fixture.detectChanges();

      component.familyForm.patchValue({ familyName: 'Updated Family' });
      const saveButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Save'));
      saveButton?.nativeElement.click();
      fixture.detectChanges();

      expect(mockFamilyService.updateFamily).toHaveBeenCalledWith(
        1,
        expect.objectContaining({
          familyName: 'Updated Family',
        }),
      );
    });
  });

  describe('Add Guardian (AC #2)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have add guardian button', () => {
      const addButton = fixture.debugElement.query(By.css('button.btn-primary'));
      expect(addButton).toBeTruthy();
      expect(addButton.nativeElement.textContent).toContain('Add Guardian');
    });

    it('should show guardian form when add is clicked', () => {
      component.showAddGuardian();
      fixture.detectChanges();

      expect(component.isAddingGuardian()).toBe(true);
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('Add Guardian');
    });

    it('should create new guardian', () => {
      component.showAddGuardian();
      fixture.detectChanges();

      component.guardianForm.patchValue({
        firstName: 'New',
        lastName: 'Guardian',
        relationship: 'Aunt',
      });

      component.saveGuardian();
      fixture.detectChanges();

      expect(mockFamilyService.addGuardian).toHaveBeenCalledWith(
        1,
        expect.objectContaining({
          firstName: 'New',
          lastName: 'Guardian',
        }),
      );
    });
  });

  describe('Edit Guardian (AC #3)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should show edit form when guardian edit is clicked', () => {
      const editButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Edit'));
      editButton?.nativeElement.click();
      fixture.detectChanges();

      expect(component.isEditingGuardian()).toBe(1);
    });

    it('should update guardian', () => {
      component.editGuardian(mockFamily.guardians?.[0]);
      fixture.detectChanges();

      component.guardianForm.patchValue({ firstName: 'Updated' });
      component.saveGuardian();
      fixture.detectChanges();

      expect(mockFamilyService.updateGuardian).toHaveBeenCalledWith(
        1,
        1,
        expect.objectContaining({
          firstName: 'Updated',
        }),
      );
    });
  });

  describe('Delete Guardian (AC #3)', () => {
    beforeEach(() => {
      fixture.detectChanges();
      window.confirm = jest.fn().mockReturnValue(true);
    });

    it('should delete guardian when delete is clicked', () => {
      const deleteButton = fixture.debugElement.queryAll(By.css('button.text-error'));
      const firstDelete = deleteButton[0];
      firstDelete.nativeElement.click();
      fixture.detectChanges();

      expect(mockFamilyService.deleteGuardian).toHaveBeenCalledWith(1, 1);
      expect(component.guardians().length).toBe(1);
    });

    it('should not delete if cancelled', () => {
      (window.confirm as jest.Mock).mockReturnValue(false);

      const deleteButton = fixture.debugElement.queryAll(By.css('button.text-error'));
      const firstDelete = deleteButton[0];
      firstDelete.nativeElement.click();
      fixture.detectChanges();

      expect(mockFamilyService.deleteGuardian).not.toHaveBeenCalled();
      expect(component.guardians().length).toBe(2);
    });
  });

  describe('Display Siblings (AC #4)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display siblings', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('Siblings');
      expect(content).toContain('Sarah Smith');
      expect(content).toContain('Tom Smith');
    });

    it('should show grade for siblings', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('(Grade 6)');
      expect(content).toContain('(Grade 4)');
    });

    it('should navigate to sibling profile when clicked', () => {
      const siblingButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Sarah'));
      siblingButton?.nativeElement.click();
      fixture.detectChanges();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/students', 2]);
    });
  });

  describe('No Family State', () => {
    beforeEach(() => {
      component.student.set({ id: 1 });
      fixture.detectChanges();
    });

    it('should show no family message', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('No Family Linked');
    });

    it('should have create family button', () => {
      const createButton = fixture.debugElement.query(By.css('button.btn-primary'));
      expect(createButton).toBeTruthy();
      expect(createButton.nativeElement.textContent).toContain('Create Family');
    });
  });
});
