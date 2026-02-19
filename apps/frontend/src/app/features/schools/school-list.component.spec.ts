import { signal } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { ModalService } from '../../core/services/modal.service';
import { NotificationService } from '../../core/services/notification.service';
import { SchoolService } from '../../core/services/school.service';
import { SchoolListComponent } from './school-list.component';

describe('SchoolListComponent', () => {
  let component: SchoolListComponent;
  let fixture: ComponentFixture<SchoolListComponent>;
  let mockSchoolService: any;
  let mockModalService: any;
  let mockNotificationService: any;

  const mockSchools = [
    { id: 1, name: 'School A', contactPerson: 'John', phone: '123', isActive: true },
    { id: 2, name: 'School B', contactPerson: 'Jane', phone: '456', isActive: false },
  ];

  beforeEach(async () => {
    mockSchoolService = {
      schools: signal([]),
      isLoading: signal(false),
      error: signal(null),
      getSchools: jest.fn().mockReturnValue(of([])),
      deactivateSchool: jest.fn().mockReturnValue(of({ ...mockSchools[0], isActive: false })),
      reactivateSchool: jest.fn().mockReturnValue(of({ ...mockSchools[1], isActive: true })),
    };

    mockModalService = {
      confirm: jest.fn(),
    };

    mockNotificationService = {
      success: jest.fn(),
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [SchoolListComponent],
      providers: [
        { provide: SchoolService, useValue: mockSchoolService },
        { provide: ModalService, useValue: mockModalService },
        { provide: NotificationService, useValue: mockNotificationService },
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SchoolListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getSchools on init', () => {
    fixture.detectChanges();
    expect(mockSchoolService.getSchools).toHaveBeenCalled();
  });

  it('should render table rows when schools are present', () => {
    mockSchoolService.schools.set(mockSchools);
    fixture.detectChanges();

    const rows = fixture.debugElement.queryAll(By.css('tbody tr'));
    expect(rows.length).toBe(2);
    expect(rows[0].nativeElement.textContent).toContain('School A');
    expect(rows[1].nativeElement.textContent).toContain('School B');
  });

  it('should render empty state when no schools', () => {
    mockSchoolService.schools.set([]);
    fixture.detectChanges();

    const emptyState = fixture.debugElement.query(By.css('.empty-state'));
    expect(emptyState).toBeTruthy();
    expect(emptyState.nativeElement.textContent).toContain('No schools found');
  });

  it('should show loading spinner when loading', () => {
    mockSchoolService.isLoading.set(true);
    fixture.detectChanges();

    const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
    expect(spinner).toBeTruthy();
  });

  it('should toggle sort direction when header is clicked', () => {
    mockSchoolService.schools.set(mockSchools);
    fixture.detectChanges();

    const nameHeaderDiv = fixture.debugElement.query(By.css('thead th div[role="button"]'));

    // Initial state: Ascending (School A, then School B)
    let firstRowName = fixture.debugElement.query(By.css('tbody tr td')).nativeElement.textContent;
    expect(firstRowName).toContain('School A');

    // Click to toggle to Descending
    nameHeaderDiv.nativeElement.click();
    fixture.detectChanges();

    firstRowName = fixture.debugElement.query(By.css('tbody tr td')).nativeElement.textContent;
    expect(firstRowName).toContain('School B');

    // Click to toggle back to Ascending
    nameHeaderDiv.nativeElement.click();
    fixture.detectChanges();

    firstRowName = fixture.debugElement.query(By.css('tbody tr td')).nativeElement.textContent;
    expect(firstRowName).toContain('School A');
  });

  it('should show error alert when error occurs', () => {
    mockSchoolService.error.set({ title: 'Server Error', detail: 'Failed to fetch data' });
    fixture.detectChanges();

    const alert = fixture.debugElement.query(By.css('[role="alert"]'));
    expect(alert).toBeTruthy();
    expect(alert.nativeElement.textContent).toContain('Server Error');
    expect(alert.nativeElement.textContent).toContain('Failed to fetch data');
  });

  // Deactivate/Reactivate tests
  it('should show deactivate button for active schools only', () => {
    mockSchoolService.schools.set(mockSchools);
    fixture.detectChanges();

    // School A is active - should have deactivate button
    const schoolARow = fixture.debugElement.queryAll(By.css('tbody tr'))[0];
    const deactivateButtonA = schoolARow.query(By.css('button[aria-label="Deactivate school"]'));
    expect(deactivateButtonA).toBeTruthy();

    // School A should NOT have reactivate button
    const reactivateButtonA = schoolARow.query(By.css('button[aria-label="Reactivate school"]'));
    expect(reactivateButtonA).toBeFalsy();

    // School B is inactive - should have reactivate button
    const schoolBRow = fixture.debugElement.queryAll(By.css('tbody tr'))[1];
    const reactivateButtonB = schoolBRow.query(By.css('button[aria-label="Reactivate school"]'));
    expect(reactivateButtonB).toBeTruthy();

    // School B should NOT have deactivate button
    const deactivateButtonB = schoolBRow.query(By.css('button[aria-label="Deactivate school"]'));
    expect(deactivateButtonB).toBeFalsy();
  });

  it('should open confirmation dialog when deactivate button is clicked', async () => {
    mockModalService.confirm.mockResolvedValue(false);
    mockSchoolService.schools.set([mockSchools[0]]); // Active school
    fixture.detectChanges();

    const deactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Deactivate school"]'),
    );
    deactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockModalService.confirm).toHaveBeenCalledWith({
      title: 'Deactivate School',
      message:
        'Are you sure you want to deactivate "School A"? It will no longer be available for new student or class assignments.',
      confirmText: 'Deactivate',
      cancelText: 'Cancel',
      confirmClass: 'btn-error',
      size: 'sm',
    });
  });

  it('should open confirmation dialog when reactivate button is clicked', async () => {
    mockModalService.confirm.mockResolvedValue(false);
    mockSchoolService.schools.set([mockSchools[1]]); // Inactive school
    fixture.detectChanges();

    const reactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Reactivate school"]'),
    );
    reactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockModalService.confirm).toHaveBeenCalledWith({
      title: 'Reactivate School',
      message:
        'Are you sure you want to reactivate "School B"? It will become available for new student or class assignments.',
      confirmText: 'Reactivate',
      cancelText: 'Cancel',
      confirmClass: 'btn-success',
      size: 'sm',
    });
  });

  it('should call deactivateSchool API when confirmed', async () => {
    mockModalService.confirm.mockResolvedValue(true);
    mockSchoolService.schools.set([mockSchools[0]]);
    fixture.detectChanges();

    const deactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Deactivate school"]'),
    );
    deactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockSchoolService.deactivateSchool).toHaveBeenCalledWith(1, mockSchools[0]);
    expect(mockNotificationService.success).toHaveBeenCalledWith('School deactivated successfully');
  });

  it('should call reactivateSchool API when confirmed', async () => {
    mockModalService.confirm.mockResolvedValue(true);
    mockSchoolService.schools.set([mockSchools[1]]);
    fixture.detectChanges();

    const reactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Reactivate school"]'),
    );
    reactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockSchoolService.reactivateSchool).toHaveBeenCalledWith(2, mockSchools[1]);
    expect(mockNotificationService.success).toHaveBeenCalledWith('School reactivated successfully');
  });

  it('should not call API when deactivation is cancelled', async () => {
    mockModalService.confirm.mockResolvedValue(false);
    mockSchoolService.schools.set([mockSchools[0]]);
    fixture.detectChanges();

    const deactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Deactivate school"]'),
    );
    deactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockSchoolService.deactivateSchool).not.toHaveBeenCalled();
  });

  it('should show error notification when deactivate fails', async () => {
    mockSchoolService.deactivateSchool.mockReturnValue(
      throwError(() => ({ detail: 'Network error' })),
    );
    mockModalService.confirm.mockResolvedValue(true);
    mockSchoolService.schools.set([mockSchools[0]]);
    fixture.detectChanges();

    const deactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Deactivate school"]'),
    );
    deactivateButton.triggerEventHandler('click', null);

    await fixture.whenStable();

    expect(mockNotificationService.error).toHaveBeenCalledWith('Network error', 'Error');
  });

  it('should disable buttons when processing a request', async () => {
    // Mock a long-running deactivation
    const deactivateSubject = new Subject<any>();
    mockSchoolService.deactivateSchool.mockReturnValue(deactivateSubject.asObservable());
    mockModalService.confirm.mockResolvedValue(true);
    mockSchoolService.schools.set([mockSchools[0]]);
    fixture.detectChanges();

    const deactivateButton = fixture.debugElement.query(
      By.css('button[aria-label="Deactivate school"]'),
    );
    const editButton = fixture.debugElement.query(By.css('button[aria-label="Edit school"]'));

    deactivateButton.triggerEventHandler('click', null);
    await fixture.whenStable();
    fixture.detectChanges();

    // Buttons should be disabled
    expect(deactivateButton.nativeElement.disabled).toBe(true);
    expect(editButton.nativeElement.disabled).toBe(true);
    expect(fixture.debugElement.query(By.css('.loading-spinner'))).toBeTruthy();

    // Complete the request
    const updatedSchool = { ...mockSchools[0], isActive: false };
    mockSchoolService.schools.set([updatedSchool]);
    deactivateSubject.next(updatedSchool);
    deactivateSubject.complete();
    fixture.detectChanges();

    // After completion, buttons should be enabled (and the deactivate one is gone, replaced by reactivate)
    expect(
      fixture.debugElement.query(By.css('button[aria-label="Reactivate school"]')),
    ).toBeTruthy();
  });
});
