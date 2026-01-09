import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivityService } from '@core/services/activity.service';
import type { Activity } from '@features/activities/models/activity.model';
import { NotificationService } from '@core/services/notification.service';
import { ActivityFormComponent } from './activity-form.component';
import { environment } from '../../../../../environments/environment';

describe('ActivityFormComponent', () => {
  let component: ActivityFormComponent;
  let fixture: ComponentFixture<ActivityFormComponent>;
  let activityService: ActivityService;
  let notificationService: NotificationService;
  let httpMock: HttpTestingController;
  let fb: FormBuilder;

  const mockActivity: Activity = {
    id: 1,
    code: 'ACT001',
    name: 'Math Activities',
    description: 'Math learning activities',
    folder: '/Activities/Math',
    gradeLevel: 'Grade 1-3',
    icon: 'base64data',
    isActive: true,
    createdAt: '2024-01-01T00:00:00',
    updatedAt: '2024-01-01T00:00:00',
  };

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, FormsModule, ActivityFormComponent],
      providers: [FormBuilder, ActivityService, NotificationService, provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ActivityFormComponent);
    component = fixture.componentInstance;
    activityService = TestBed.inject(ActivityService);
    notificationService = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
    fb = TestBed.inject(FormBuilder);
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
        code: '',
        name: '',
        description: '',
        folder: '',
        gradeLevel: '',
        icon: '',
      });
    });

    it('should have maxlength validator on code field', () => {
      component.ngOnInit();
      const codeControl = component.form.get('code');

      codeControl?.setValue('A'.repeat(256));
      expect(codeControl?.valid).toBe(false);
      expect(codeControl?.errors?.['maxlength']).toBeTruthy();
    });

    it('should have maxlength validator on name field', () => {
      component.ngOnInit();
      const nameControl = component.form.get('name');

      nameControl?.setValue('A'.repeat(256));
      expect(nameControl?.valid).toBe(false);
      expect(nameControl?.errors?.['maxlength']).toBeTruthy();
    });

    it('should have maxlength validator on folder field', () => {
      component.ngOnInit();
      const folderControl = component.form.get('folder');

      folderControl?.setValue('A'.repeat(256));
      expect(folderControl?.valid).toBe(false);
      expect(folderControl?.errors?.['maxlength']).toBeTruthy();
    });

    it('should have maxlength validator on gradeLevel field', () => {
      component.ngOnInit();
      const gradeLevelControl = component.form.get('gradeLevel');

      gradeLevelControl?.setValue('A'.repeat(256));
      expect(gradeLevelControl?.valid).toBe(false);
      expect(gradeLevelControl?.errors?.['maxlength']).toBeTruthy();
    });
  });

  describe('Load Activity for Edit', () => {
    it('should load activity data when activityId is provided', () => {
      component.activityId.set(1);
      component.ngOnInit();

      const request = httpMock.expectOne(`${environment.apiUrl}/activities/1`);
      request.flush(mockActivity);

      expect(component.form.value).toEqual({
        code: mockActivity.code,
        name: mockActivity.name,
        description: mockActivity.description,
        folder: mockActivity.folder,
        gradeLevel: mockActivity.gradeLevel,
        icon: mockActivity.icon,
      });
    });

    it('should set icon preview when activity has icon', () => {
      component.activityId.set(1);
      component.ngOnInit();

      const request = httpMock.expectOne(`${environment.apiUrl}/activities/1`);
      request.flush(mockActivity);

      expect(component.iconPreview()).toBe('data:image/png;base64,base64data');
    });

    it('should set loading state during load', () => {
      component.activityId.set(1);
      component.ngOnInit();

      expect(component.isLoading()).toBe(true);

      const request = httpMock.expectOne(`${environment.apiUrl}/activities/1`);
      request.flush(mockActivity);

      expect(component.isLoading()).toBe(false);
    });

    it('should handle load error', () => {
      component.activityId.set(1);
      const errorSpy = jest.spyOn(notificationService, 'error');
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      component.ngOnInit();

      const request = httpMock.expectOne(`${environment.apiUrl}/activities/1`);
      request.flush({ message: 'Not found' }, { status: 404 });

      expect(component.error()).not.toBeNull();
      expect(errorSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Icon Upload', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should reject non-image files', () => {
      const errorSpy = jest.spyOn(notificationService, 'error');
      const file = new File(['content'], 'test.txt', { type: 'text/plain' });
      const input = { files: [file] } as HTMLInputElement;

      component.onIconSelected({ target: input } as Event);

      expect(errorSpy).toHaveBeenCalledWith('Please select an image file');
    });

    it('should reject files larger than 5MB', () => {
      const errorSpy = jest.spyOn(notificationService, 'error');
      const largeContent = new Array(6 * 1024 * 1024).fill('x').join('');
      const file = new File([largeContent], 'large.png', { type: 'image/png' });
      const input = { files: [file] } as HTMLInputElement;

      component.onIconSelected({ target: input } as Event);

      expect(errorSpy).toHaveBeenCalledWith('Image file must be less than 5MB');
    });

    it('should accept valid image file and set preview', (done) => {
      const file = new File(['x'], 'test.png', { type: 'image/png' });
      const input = { files: [file] } as HTMLInputElement;

      component.onIconSelected({ target: input } as Event);

      setTimeout(() => {
        expect(component.iconPreview()).not.toBeNull();
        expect(component.form.get('icon')?.value).not.toBe('');
        done();
      }, 100);
    });

    it('should clear icon', () => {
      component.form.patchValue({ icon: 'base64data' });
      component.iconPreview.set('data:image/png;base64,base64data');

      component.clearIcon();

      expect(component.form.get('icon')?.value).toBe('');
      expect(component.iconPreview()).toBeNull();
    });
  });

  describe('Form Validation Display', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should show error for touched invalid field', () => {
      const codeControl = component.form.get('code');
      codeControl?.setValue('A'.repeat(256));
      codeControl?.markAsTouched();

      expect(component.hasError('code')).toBe(true);
    });

    it('should not show error for untouched invalid field', () => {
      const codeControl = component.form.get('code');
      codeControl?.setValue('A'.repeat(256));

      expect(component.hasError('code')).toBe(false);
    });

    it('should return correct error message for maxlength field', () => {
      const codeControl = component.form.get('code');
      codeControl?.setValue('A'.repeat(256));
      codeControl?.markAsTouched();

      expect(component.getErrorMessage('code')).toBe('Maximum length is 255 characters');
    });
  });

  describe('Submit Form - Create', () => {
    beforeEach(() => {
      component.ngOnInit();
      component.form.setValue({
        code: 'ACT002',
        name: 'New Activity',
        description: 'New description',
        folder: '/Activities/New',
        gradeLevel: 'Grade K',
        icon: '',
      });
    });

    it('should call createActivity service method', () => {
      const createSpy = jest.spyOn(activityService, 'createActivity').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockActivity) }),
      } as any);

      component['submitForm']();

      expect(createSpy).toHaveBeenCalledWith({
        code: 'ACT002',
        name: 'New Activity',
        description: 'New description',
        folder: '/Activities/New',
        gradeLevel: 'Grade K',
      });
    });

    it('should emit submit event on successful create', () => {
      jest.spyOn(activityService, 'createActivity').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockActivity) }),
      } as any);

      const submitSpy = jest.spyOn(component.submit, 'emit');

      component['submitForm']();

      expect(submitSpy).toHaveBeenCalled();
    });
  });

  describe('Submit Form - Update', () => {
    beforeEach(() => {
      component.activityId.set(1);
      component.ngOnInit();
      component.form.setValue({
        code: 'ACT001',
        name: 'Updated Activity',
        description: 'Updated description',
        folder: '/Activities/Updated',
        gradeLevel: 'Grade 4-6',
        icon: '',
      });
    });

    it('should call updateActivity service method', () => {
      const updateSpy = jest.spyOn(activityService, 'updateActivity').mockReturnValue({
        pipe: () => ({ subscribe: (callbacks: any) => callbacks.next?.(mockActivity) }),
      } as any);

      component['submitForm']();

      expect(updateSpy).toHaveBeenCalledWith(1, {
        code: 'ACT001',
        name: 'Updated Activity',
        description: 'Updated description',
        folder: '/Activities/Updated',
        gradeLevel: 'Grade 4-6',
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
    it('should show "Add Activity" title for create mode', () => {
      expect(component.title).toBe('Add Activity');
    });

    it('should show "Edit Activity" title for edit mode', () => {
      component.activityId.set(1);
      expect(component.title).toBe('Edit Activity');
    });

    it('should show "Create Activity" button text for create mode', () => {
      expect(component.submitButtonText).toBe('Create Activity');
    });

    it('should show "Update Activity" button text for edit mode', () => {
      component.activityId.set(1);
      expect(component.submitButtonText).toBe('Update Activity');
    });

    it('should show "Saving..." when form is submitting', () => {
      component['isSaving'].set(true);
      expect(component.submitButtonText).toBe('Saving...');
    });
  });
});
