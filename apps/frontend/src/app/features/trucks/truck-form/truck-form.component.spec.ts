import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { type ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NotificationService } from '@core/services/notification.service';
import { type Truck, TruckService } from '@core/services/truck.service';
import { environment } from '../../../../../environments/environment';
import { TruckFormComponent } from './truck-form.component';

describe('TruckFormComponent', () => {
  let component: TruckFormComponent;
  let fixture: ComponentFixture<TruckFormComponent>;
  let truckService: TruckService;
  let notificationService: NotificationService;
  let httpMock: HttpTestingController;
  let _fb: FormBuilder;

  const mockTruck: Truck = {
    id: 1,
    name: 'Truck 1',
    registrationNumber: 'CA 123 456',
    status: 'Active',
    notes: 'Test notes',
    isActive: true,
    createdAt: '2024-01-01T00:00:00',
  };

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, FormsModule, TruckFormComponent],
      providers: [FormBuilder, TruckService, NotificationService, provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TruckFormComponent);
    component = fixture.componentInstance;
    truckService = TestBed.inject(TruckService);
    notificationService = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
    _fb = TestBed.inject(FormBuilder);
  }));

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should initialize form with empty values for create mode', () => {
      component.ngOnInit();

      expect(component.form.value).toEqual({
        name: '',
        registrationNumber: '',
        status: 'Active',
        notes: '',
      });
    });

    it('should have required validators on name field', () => {
      component.ngOnInit();
      const nameControl = component.form.get('name');

      nameControl?.setValue('');
      expect(nameControl?.valid).toBe(false);
      expect(nameControl?.errors?.required).toBeTruthy();

      nameControl?.setValue('Truck Name');
      expect(nameControl?.valid).toBe(true);
    });

    it('should have maxlength validator on name field', () => {
      component.ngOnInit();
      const nameControl = component.form.get('name');

      nameControl?.setValue('A'.repeat(101));
      expect(nameControl?.valid).toBe(false);
      expect(nameControl?.errors?.maxlength).toBeTruthy();
    });

    it('should have required validators on registrationNumber field', () => {
      component.ngOnInit();
      const regControl = component.form.get('registrationNumber');

      regControl?.setValue('');
      expect(regControl?.valid).toBe(false);
      expect(regControl?.errors?.required).toBeTruthy();
    });

    it('should have maxlength validator on registrationNumber field', () => {
      component.ngOnInit();
      const regControl = component.form.get('registrationNumber');

      regControl?.setValue('A'.repeat(51));
      expect(regControl?.valid).toBe(false);
      expect(regControl?.errors?.maxlength).toBeTruthy();
    });

    it('should have required validators on status field', () => {
      component.ngOnInit();
      const statusControl = component.form.get('status');

      statusControl?.setValue('');
      expect(statusControl?.valid).toBe(false);
      expect(statusControl?.errors?.required).toBeTruthy();
    });
  });

  describe('Load Truck for Edit', () => {
    it('should load truck data when truckId is provided', () => {
      component.truckId.set(1);
      component.ngOnInit();

      const request = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      request.flush(mockTruck);

      expect(component.form.value).toEqual({
        name: mockTruck.name,
        registrationNumber: mockTruck.registrationNumber,
        status: mockTruck.status,
        notes: mockTruck.notes,
      });
    });

    it('should set loading state during load', () => {
      component.truckId.set(1);
      component.ngOnInit();

      expect(component.isLoading()).toBe(true);

      const request = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      request.flush(mockTruck);

      expect(component.isLoading()).toBe(false);
    });

    it('should handle load error', () => {
      component.truckId.set(1);
      const errorSpy = jest.spyOn(notificationService, 'error');
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      component.ngOnInit();

      const request = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      request.flush({ message: 'Not found' }, { status: 404 });

      expect(component.error()).not.toBeNull();
      expect(errorSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Form Validation Display', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should show error for touched invalid field', () => {
      const nameControl = component.form.get('name');
      nameControl?.setValue('');
      nameControl?.markAsTouched();

      expect(component.hasError('name')).toBe(true);
    });

    it('should not show error for untouched invalid field', () => {
      const nameControl = component.form.get('name');
      nameControl?.setValue('');

      expect(component.hasError('name')).toBe(false);
    });

    it('should return correct error message for required field', () => {
      const nameControl = component.form.get('name');
      nameControl?.setValue('');
      nameControl?.markAsTouched();

      expect(component.getErrorMessage('name')).toBe('This field is required');
    });

    it('should return correct error message for maxlength field', () => {
      const nameControl = component.form.get('name');
      nameControl?.setValue('A'.repeat(101));
      nameControl?.markAsTouched();

      expect(component.getErrorMessage('name')).toBe('Maximum length is 100 characters');
    });
  });

  describe('Submit Form - Create', () => {
    beforeEach(() => {
      component.ngOnInit();
      component.form.setValue({
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'Active',
        notes: 'Test notes',
      });
    });

    it('should call createTruck service method', () => {
      const createSpy = jest.spyOn(truckService, 'createTruck').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockTruck) }),
      } as any);

      component.submitForm();

      expect(createSpy).toHaveBeenCalledWith({
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'Active',
        notes: 'Test notes',
      });
    });

    it('should emit submit event on successful create', () => {
      jest.spyOn(truckService, 'createTruck').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockTruck) }),
      } as any);

      const submitSpy = jest.spyOn(component.submit, 'emit');

      component.submitForm();

      expect(submitSpy).toHaveBeenCalled();
    });

    it('should not submit invalid form', () => {
      component.form.get('name')?.setValue('');

      const createSpy = jest.spyOn(truckService, 'createTruck');

      component.submitForm();

      expect(createSpy).not.toHaveBeenCalled();
    });
  });

  describe('Submit Form - Update', () => {
    beforeEach(() => {
      component.truckId.set(1);
      component.ngOnInit();
      component.form.setValue({
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'Maintenance',
        notes: 'Updated notes',
      });
    });

    it('should call updateTruck service method', () => {
      const updateSpy = jest.spyOn(truckService, 'updateTruck').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockTruck) }),
      } as any);

      component.submitForm();

      expect(updateSpy).toHaveBeenCalledWith(1, {
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'Maintenance',
        notes: 'Updated notes',
      });
    });
  });

  describe('Cancel', () => {
    it('should emit cancel event', () => {
      const cancelSpy = jest.spyOn(component.cancel, 'emit');

      component.onCancel();

      expect(cancelSpy).toHaveBeenCalled();
    });
  });

  describe('Title and Button Text', () => {
    it('should show "Add Truck" title for create mode', () => {
      expect(component.title).toBe('Add Truck');
    });

    it('should show "Edit Truck" title for edit mode', () => {
      component.truckId.set(1);
      expect(component.title).toBe('Edit Truck');
    });

    it('should show "Create Truck" button text for create mode', () => {
      expect(component.submitButtonText).toBe('Create Truck');
    });

    it('should show "Update Truck" button text for edit mode', () => {
      component.truckId.set(1);
      expect(component.submitButtonText).toBe('Update Truck');
    });

    it('should show "Saving..." when form is submitting', () => {
      component.isSaving.set(true);
      expect(component.submitButtonText).toBe('Saving...');
    });
  });
});
