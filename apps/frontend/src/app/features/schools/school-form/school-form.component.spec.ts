import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { NotificationService } from '@core/services/notification.service';
import { SchoolService } from '@core/services/school.service';
import { of, throwError } from 'rxjs';
import { SchoolFormComponent } from './school-form.component';

describe('SchoolFormComponent', () => {
  let component: SchoolFormComponent;
  let fixture: ComponentFixture<SchoolFormComponent>;
  let mockSchoolService: any;
  let mockNotificationService: any;
  let router: Router;
  let mockActivatedRoute: any;

  const mockSchool = {
    id: 1,
    name: 'Test School',
    address: '123 Main St',
    contactPerson: 'John Doe',
    phone: '123-456-7890',
    email: 'test@example.com',
    isActive: true,
    schedulingNotes: 'Test notes',
    printInvoice: false,
    importFlag: false,
  };

  beforeEach(async () => {
    mockSchoolService = {
      createSchool: jest.fn().mockReturnValue(of(mockSchool)),
      updateSchool: jest.fn().mockReturnValue(of(mockSchool)),
      getSchoolById: jest.fn().mockReturnValue(of(mockSchool)),
    };

    mockNotificationService = {
      success: jest.fn(),
      error: jest.fn(),
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: jest.fn().mockReturnValue(null),
        },
      },
    };

    await TestBed.configureTestingModule({
      imports: [
        SchoolFormComponent,
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([
          { path: 'schools', component: SchoolFormComponent },
          { path: 'schools/create', component: SchoolFormComponent },
          { path: 'schools/edit/:id', component: SchoolFormComponent },
        ]),
      ],
      providers: [
        { provide: SchoolService, useValue: mockSchoolService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SchoolFormComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render form with all required fields', () => {
    fixture.detectChanges();

    const nameField = fixture.debugElement.query(By.css('input[formControlName="name"]'));
    const addressField = fixture.debugElement.query(By.css('input[formControlName="address"]'));
    const contactPersonField = fixture.debugElement.query(
      By.css('input[formControlName="contactPerson"]'),
    );
    const phoneField = fixture.debugElement.query(By.css('input[formControlName="phone"]'));
    const emailField = fixture.debugElement.query(By.css('input[formControlName="email"]'));
    const schedulingNotesField = fixture.debugElement.query(
      By.css('textarea[formControlName="schedulingNotes"]'),
    );

    expect(nameField).toBeTruthy();
    expect(addressField).toBeTruthy();
    expect(contactPersonField).toBeTruthy();
    expect(phoneField).toBeTruthy();
    expect(emailField).toBeTruthy();
    expect(schedulingNotesField).toBeTruthy();
  });

  it('should show validation error when name is empty', () => {
    fixture.detectChanges();
    const form = (component as any).form;
    const nameControl = form.get('name');

    nameControl?.setValue('');
    nameControl?.markAsTouched();
    fixture.detectChanges();

    expect(nameControl?.hasError('required')).toBe(true);
    const errorElement = fixture.debugElement.query(By.css('.text-error'));
    expect(errorElement).toBeTruthy();
  });

  it('should show validation error for invalid email format', () => {
    fixture.detectChanges();
    const form = (component as any).form;
    const emailControl = form.get('email');

    emailControl?.setValue('invalid-email');
    emailControl?.markAsTouched();
    fixture.detectChanges();

    expect(emailControl?.hasError('email')).toBe(true);
  });

  it('should disable save button when form is invalid', () => {
    fixture.detectChanges();
    const form = (component as any).form;
    form.get('name')?.setValue('');

    fixture.detectChanges();
    const saveButton = fixture.debugElement.query(By.css('button[type="submit"]'));
    expect(saveButton.nativeElement.disabled).toBe(true);
  });

  it('should enable save button when form is valid', () => {
    fixture.detectChanges();
    const form = (component as any).form;
    form.patchValue({
      name: 'Test School',
      address: '123 Main St',
      contactPerson: 'John Doe',
      phone: '123-456-7890',
      email: 'test@example.com',
      schedulingNotes: 'Notes',
    });

    fixture.detectChanges();
    const saveButton = fixture.debugElement.query(By.css('button[type="submit"]'));
    expect(saveButton.nativeElement.disabled).toBe(false);
  });

  it('should call createSchool and show success on valid create submission', () => {
    fixture.detectChanges();
    const form = (component as any).form;
    form.patchValue({
      name: 'New School',
      address: '456 Oak St',
      contactPerson: 'Jane Smith',
      phone: '987-654-3210',
      email: 'new@example.com',
      schedulingNotes: 'New notes',
    });

    (component as any).onSubmit();
    fixture.detectChanges();

    expect(mockSchoolService.createSchool).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'New School',
        address: '456 Oak St',
        contactPerson: 'Jane Smith',
        phone: '987-654-3210',
        email: 'new@example.com',
        schedulingNotes: 'New notes',
      }),
    );
    expect(mockNotificationService.success).toHaveBeenCalledWith(
      'School created successfully',
      'Success',
    );
    expect(router.navigate).toHaveBeenCalledWith(['/schools']);
  });

  it('should load and pre-populate form in edit mode', () => {
    mockActivatedRoute.snapshot.paramMap.get.mockReturnValue('1');
    fixture = TestBed.createComponent(SchoolFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(mockSchoolService.getSchoolById).toHaveBeenCalledWith(1);
    expect((component as any).isEditMode()).toBe(true);

    // Wait for async load
    fixture.detectChanges();

    const form = (component as any).form;
    expect(form.get('name')?.value).toBe('Test School');
    expect(form.get('address')?.value).toBe('123 Main St');
    expect(form.get('contactPerson')?.value).toBe('John Doe');
  });

  it('should call updateSchool and show success on valid edit submission', () => {
    mockActivatedRoute.snapshot.paramMap.get.mockReturnValue('1');
    fixture = TestBed.createComponent(SchoolFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // Wait for async load
    fixture.detectChanges();

    const form = (component as any).form;
    form.patchValue({
      name: 'Updated School',
    });

    (component as any).onSubmit();
    fixture.detectChanges();

    expect(mockSchoolService.updateSchool).toHaveBeenCalledWith(
      1,
      expect.objectContaining({
        name: 'Updated School',
      }),
    );
    expect(mockNotificationService.success).toHaveBeenCalledWith(
      'School updated successfully',
      'Success',
    );
    expect(router.navigate).toHaveBeenCalledWith(['/schools']);
  });

  it('should show error notification on create failure', () => {
    mockSchoolService.createSchool.mockReturnValue(throwError(() => ({ detail: 'Server error' })));

    fixture.detectChanges();
    const form = (component as any).form;
    form.patchValue({
      name: 'Test School',
    });

    (component as any).onSubmit();
    fixture.detectChanges();

    expect(mockNotificationService.error).toHaveBeenCalledWith('Server error', 'Error');
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
