import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { FamilyService } from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import { GuardianFormComponent } from '@shared/components/guardian-form/guardian-form.component';
import { of } from 'rxjs';
import { FamilyFormComponent } from './family-form.component';

describe('FamilyFormComponent', () => {
  let component: FamilyFormComponent;
  let fixture: ComponentFixture<FamilyFormComponent>;
  let mockFamilyService: any;
  let mockNotificationService: any;
  let mockRouter: any;
  let mockActivatedRoute: any;

  const mockFamily = {
    id: 1,
    familyName: 'Smith Family',
    primaryBillingContactId: 1,
    notes: 'Some notes',
    isActive: true,
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
    guardians: [
      {
        id: 1,
        familyId: 1,
        firstName: 'John',
        lastName: 'Smith',
        relationship: 'Father',
        phone: '555-1234',
        email: 'john@example.com',
        isPrimaryContact: true,
        createdAt: '2024-01-01',
        updatedAt: '2024-01-01',
      },
    ],
  };

  beforeEach(async () => {
    mockFamilyService = {
      getFamilyById: jest.fn().mockReturnValue(of(mockFamily)),
      createFamily: jest.fn().mockReturnValue(of(mockFamily)),
      updateFamily: jest.fn().mockReturnValue(of(mockFamily)),
      addGuardian: jest.fn().mockReturnValue(of(mockFamily.guardians[0])),
    };

    mockNotificationService = {
      success: jest.fn(),
      error: jest.fn(),
      warning: jest.fn(),
    };

    mockRouter = {
      navigate: jest.fn(),
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: jest.fn(),
        },
      },
    };

    await TestBed.configureTestingModule({
      imports: [FamilyFormComponent, GuardianFormComponent],
      providers: [
        { provide: FamilyService, useValue: mockFamilyService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FamilyFormComponent);
    component = fixture.componentInstance;
  });

  describe('Create Mode', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.mockReturnValue(null);
      fixture.detectChanges();
    });

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with empty form', () => {
      expect((component as any).form.value).toEqual({
        familyName: '',
        notes: '',
      });
    });

    it('should show initial guardian form for new family', () => {
      expect((component as any).isAddingFirstGuardian()).toBe(true);
      expect((component as any).isGuardianFormVisible()).toBe(true);
    });

    it('should add guardian to list when guardian form is submitted', () => {
      const guardianData = {
        firstName: 'Jane',
        lastName: 'Doe',
        relationship: 'Mother',
        phone: '555-5678',
        email: 'jane@example.com',
        isPrimaryContact: false,
      };

      (component as any).onGuardianSubmit(guardianData);
      fixture.detectChanges();

      // In create mode, guardians array starts empty
      expect((component as any).guardians().length).toBe(1);
      expect((component as any).guardians()[0].firstName).toBe('Jane');
    });

    it('should show validation error when submitting without family name', () => {
      (component as any).form.setValue({ familyName: '', notes: '' });
      (component as any).onSubmit();

      expect((component as any).form.invalid).toBe(true);
    });

    it('should show warning when submitting without guardians', () => {
      (component as any).form.setValue({ familyName: 'Test Family', notes: '' });
      (component as any).guardians.set([]);
      (component as any).onSubmit();

      expect(mockNotificationService.warning).toHaveBeenCalledWith(
        'Please add at least one guardian to create a family',
        'Validation',
      );
    });

    it('should create family and guardian on submit', () => {
      (component as any).form.setValue({ familyName: 'Test Family', notes: 'Test notes' });
      (component as any).onSubmit();

      expect(mockFamilyService.createFamily).toHaveBeenCalledWith({
        familyName: 'Test Family',
        notes: 'Test notes',
      });
    });

    it('should navigate to families list on cancel', () => {
      (component as any).onCancel();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/families']);
    });
  });

  describe('Edit Mode', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.mockReturnValue('1');
      fixture.detectChanges();
    });

    it('should load family data on init', () => {
      expect(mockFamilyService.getFamilyById).toHaveBeenCalledWith(1);
      expect((component as any).isEditMode()).toBe(true);
      expect((component as any).form.value.familyName).toBe('Smith Family');
    });

    it('should populate guardians from loaded family', () => {
      expect((component as any).guardians().length).toBe(1);
      expect((component as any).guardians()[0].firstName).toBe('John');
    });

    it('should update family on submit', () => {
      (component as any).form.setValue({ familyName: 'Updated Family', notes: 'Updated notes' });
      (component as any).onSubmit();

      expect(mockFamilyService.updateFamily).toHaveBeenCalledWith(1, {
        familyName: 'Updated Family',
        notes: 'Updated notes',
        primaryBillingContactId: 1,
        isActive: true,
      });
    });
  });

  describe('Guardian Management', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.mockReturnValue(null);
      fixture.detectChanges();
    });

    it('should edit existing guardian', () => {
      (component as any).onEditGuardian(0);
      expect((component as any).isEditingGuardian()).toBe(true);
      expect((component as any).editingGuardianIndex()).toBe(0);
    });

    it('should remove guardian from list', () => {
      // First add a second guardian so we can remove one
      (component as any).onGuardianSubmit({
        firstName: 'Jane',
        lastName: 'Doe',
        relationship: 'Mother',
        phone: '555-5678',
        email: 'jane@example.com',
        isPrimaryContact: false,
      });
      const countBefore = (component as any).guardians().length;

      (component as any).onRemoveGuardian(0);
      expect((component as any).guardians().length).toBe(countBefore - 1);
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot.paramMap.get.mockReturnValue(null);
      fixture.detectChanges();
    });

    it('should show error for empty family name', () => {
      (component as any).form.patchValue({ familyName: '' });
      (component as any).form.markAllAsTouched();
      fixture.detectChanges();

      const error = (component as any).getErrorMessage('familyName');
      expect(error).toContain('required');
    });

    it('should show error for family name exceeding max length', () => {
      const longName = 'A'.repeat(201);
      (component as any).form.patchValue({ familyName: longName });
      (component as any).form.markAllAsTouched();
      fixture.detectChanges();

      const error = (component as any).getErrorMessage('familyName');
      expect(error).toContain('cannot exceed');
    });

    it('should show no error for valid family name', () => {
      (component as any).form.patchValue({ familyName: 'Smith Family' });
      (component as any).form.markAllAsTouched();
      fixture.detectChanges();

      const error = (component as any).getErrorMessage('familyName');
      expect(error).toBe('');
    });
  });
});
