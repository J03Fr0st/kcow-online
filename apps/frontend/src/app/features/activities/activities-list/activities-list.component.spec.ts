import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivityService } from '@core/services/activity.service';
import { NotificationService } from '@core/services/notification.service';
import { ActivitiesListComponent } from './activities-list.component';
import { ActivityFormComponent } from '../activity-form/activity-form.component';
import type { Activity } from '@features/activities/models/activity.model';
import { environment } from '../../../../../environments/environment';

describe('ActivitiesListComponent', () => {
  let component: ActivitiesListComponent;
  let fixture: ComponentFixture<ActivitiesListComponent>;
  let activityService: ActivityService;
  let notificationService: NotificationService;
  let httpMock: HttpTestingController;

  const mockActivities: Activity[] = [
    {
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
    },
    {
      id: 2,
      code: 'ACT002',
      name: 'Reading Activities',
      description: 'Reading comprehension activities',
      folder: '/Activities/Reading',
      gradeLevel: 'Grade 2-4',
      icon: null,
      isActive: true,
      createdAt: '2024-01-02T00:00:00',
      updatedAt: null,
    },
  ];

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [CommonModule, ActivitiesListComponent, ActivityFormComponent],
      providers: [ActivityService, NotificationService, provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ActivitiesListComponent);
    component = fixture.componentInstance;
    activityService = TestBed.inject(ActivityService);
    notificationService = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  }));

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initial Load', () => {
    it('should load activities on init', () => {
      const loadSpy = jest.spyOn(component, 'loadActivities');

      component.ngOnInit();

      expect(loadSpy).toHaveBeenCalled();
    });

    it('should display loading state while loading', () => {
      component.ngOnInit();
      fixture.detectChanges();

      expect(component.loading()).toBe(true);
    });

    it('should display activities after loading', () => {
      const request = httpMock.expectOne(`${environment.apiUrl}/activities`);
      request.flush(mockActivities);

      fixture.detectChanges();

      expect(component.activities()).toEqual(mockActivities);
      expect(component.loading()).toBe(false);
    });

    it('should display empty state when no activities exist', () => {
      const request = httpMock.expectOne(`${environment.apiUrl}/activities`);
      request.flush([]);

      fixture.detectChanges();

      expect(component.activities().length).toBe(0);
    });
  });

  describe('Create Flow', () => {
    it('should open create form when Add Activity button clicked', () => {
      component.openCreateForm();

      expect(component.showForm()).toBe(true);
      expect(component.editingId()).toBeNull();
    });

    it('should close form after create', () => {
      component.openCreateForm();
      expect(component.showForm()).toBe(true);

      component.closeForm();
      expect(component.showForm()).toBe(false);
    });
  });

  describe('Edit Flow', () => {
    it('should open edit form with activity data', () => {
      component.openEditForm(mockActivities[0]);

      expect(component.showForm()).toBe(true);
      expect(component.editingId()).toBe(1);
    });

    it('should close form after edit', () => {
      component.openEditForm(mockActivities[0]);
      expect(component.showForm()).toBe(true);

      component.closeForm();
      expect(component.showForm()).toBe(false);
    });
  });

  describe('Delete Flow', () => {
    it('should show inline confirmation when delete clicked', () => {
      component.startDelete(mockActivities[0]);

      expect(component.deletingId()).toBe(1);
    });

    it('should cancel delete operation', () => {
      component.startDelete(mockActivities[0]);
      expect(component.deletingId()).toBe(1);

      component.cancelDelete();
      expect(component.deletingId()).toBeNull();
    });

    it('should confirm delete and call service', () => {
      const deleteSpy = jest.spyOn(activityService, 'deleteActivity').mockReturnValue({
        subscribe: (callbacks: any) => {
          callbacks.next?.();
          return { unsubscribe: () => {} };
        },
      } as any);
      const successSpy = jest.spyOn(notificationService, 'success');

      component.confirmDelete(mockActivities[0]);

      expect(deleteSpy).toHaveBeenCalledWith(1);
      expect(successSpy).toHaveBeenCalledWith('Activity archived successfully');
      expect(component.deletingId()).toBeNull();
    });

    it('should handle delete error', () => {
      const deleteSpy = jest.spyOn(activityService, 'deleteActivity').mockReturnValue({
        subscribe: (callbacks: any) => {
          callbacks.error?.({ message: 'Error' });
          return { unsubscribe: () => {} };
        },
      } as any);
      const errorSpy = jest.spyOn(notificationService, 'error');
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      component.confirmDelete(mockActivities[0]);

      expect(errorSpy).toHaveBeenCalledWith('Failed to archive activity');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Icon Source Helper', () => {
    it('should return base64 data URL for activity with icon', () => {
      const result = component.getIconSrc(mockActivities[0]);
      expect(result).toBe('data:image/png;base64,base64data');
    });

    it('should return placeholder SVG for activity without icon', () => {
      const result = component.getIconSrc(mockActivities[1]);
      expect(result).toContain('data:image/svg+xml');
      expect(result).toContain('📄');
    });
  });

  describe('Form Submit Handler', () => {
    it('should handle create success', () => {
      const successSpy = jest.spyOn(notificationService, 'success');
      const closeSpy = jest.spyOn(component, 'closeForm');

      component.onFormSubmit(
        new CustomEvent('submit', { detail: { mode: 'create' } })
      );

      expect(successSpy).toHaveBeenCalledWith('Activity created successfully');
      expect(closeSpy).toHaveBeenCalled();
    });

    it('should handle update success', () => {
      const successSpy = jest.spyOn(notificationService, 'success');
      const closeSpy = jest.spyOn(component, 'closeForm');

      component.onFormSubmit(
        new CustomEvent('submit', {
          detail: { mode: 'update', activity: mockActivities[0] },
        })
      );

      expect(successSpy).toHaveBeenCalledWith('Activity updated successfully');
      expect(closeSpy).toHaveBeenCalled();
    });
  });
});
