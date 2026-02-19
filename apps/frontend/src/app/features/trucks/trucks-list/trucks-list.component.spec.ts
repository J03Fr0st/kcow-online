import { CommonModule } from '@angular/common';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { type ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { NotificationService } from '@core/services/notification.service';
import { TruckService } from '@core/services/truck.service';
import { environment } from '../../../../../environments/environment';
import { TruckFormComponent } from '../truck-form/truck-form.component';
import { TrucksListComponent } from './trucks-list.component';

describe('TrucksListComponent', () => {
  let component: TrucksListComponent;
  let fixture: ComponentFixture<TrucksListComponent>;
  let truckService: TruckService;
  let notificationService: NotificationService;
  let httpMock: HttpTestingController;

  const mockTrucks = [
    {
      id: 1,
      name: 'Truck 1',
      registrationNumber: 'CA 123 456',
      status: 'Active' as const,
      notes: 'Test notes',
      isActive: true,
      createdAt: '2024-01-01T00:00:00',
    },
    {
      id: 2,
      name: 'Truck 2',
      registrationNumber: 'CA 789 012',
      status: 'Maintenance' as const,
      notes: '',
      isActive: true,
      createdAt: '2024-01-02T00:00:00',
    },
  ];

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [CommonModule, TrucksListComponent, TruckFormComponent],
      providers: [TruckService, NotificationService, provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TrucksListComponent);
    component = fixture.componentInstance;
    truckService = TestBed.inject(TruckService);
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
    it('should load trucks on init', () => {
      const loadSpy = jest.spyOn(component, 'loadTrucks');

      component.ngOnInit();

      expect(loadSpy).toHaveBeenCalled();
    });

    it('should display loading state while loading', () => {
      component.ngOnInit();
      fixture.detectChanges();

      expect(component.loading()).toBe(true);
    });

    it('should display trucks after loading', () => {
      const request = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      request.flush(mockTrucks);

      fixture.detectChanges();

      expect(component.trucks()).toEqual(mockTrucks);
      expect(component.loading()).toBe(false);
    });

    it('should display empty state when no trucks exist', () => {
      const request = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      request.flush([]);

      fixture.detectChanges();

      expect(component.trucks().length).toBe(0);
    });
  });

  describe('Create Flow', () => {
    it('should open create form when Add Truck button clicked', () => {
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
    it('should open edit form with truck data', () => {
      component.openEditForm(mockTrucks[0]);

      expect(component.showForm()).toBe(true);
      expect(component.editingId()).toBe(1);
    });

    it('should close form after edit', () => {
      component.openEditForm(mockTrucks[0]);
      expect(component.showForm()).toBe(true);

      component.closeForm();
      expect(component.showForm()).toBe(false);
    });
  });

  describe('Delete Flow', () => {
    it('should show inline confirmation when delete clicked', () => {
      component.startDelete(mockTrucks[0]);

      expect(component.deletingId()).toBe(1);
    });

    it('should cancel delete operation', () => {
      component.startDelete(mockTrucks[0]);
      expect(component.deletingId()).toBe(1);

      component.cancelDelete();
      expect(component.deletingId()).toBeNull();
    });

    it('should confirm delete and call service', () => {
      const deleteSpy = jest.spyOn(truckService, 'deleteTruck').mockReturnValue({
        subscribe: (callbacks: any) => {
          callbacks.next?.();
          return { unsubscribe: () => {} };
        },
      } as any);
      const successSpy = jest.spyOn(notificationService, 'success');

      component.confirmDelete(mockTrucks[0]);

      expect(deleteSpy).toHaveBeenCalledWith(1);
      expect(successSpy).toHaveBeenCalledWith('Truck archived successfully');
      expect(component.deletingId()).toBeNull();
    });

    it('should handle delete error', () => {
      const _deleteSpy = jest.spyOn(truckService, 'deleteTruck').mockReturnValue({
        subscribe: (callbacks: any) => {
          callbacks.error?.({ message: 'Error' });
          return { unsubscribe: () => {} };
        },
      } as any);
      const errorSpy = jest.spyOn(notificationService, 'error');
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      component.confirmDelete(mockTrucks[0]);

      expect(errorSpy).toHaveBeenCalledWith('Failed to archive truck');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Status Badge Classes', () => {
    it('should return success class for Active status', () => {
      expect(component.getStatusClass('Active')).toBe('badge-success');
    });

    it('should return warning class for Maintenance status', () => {
      expect(component.getStatusClass('Maintenance')).toBe('badge-warning');
    });

    it('should return error class for Retired status', () => {
      expect(component.getStatusClass('Retired')).toBe('badge-error');
    });

    it('should return ghost class for unknown status', () => {
      expect(component.getStatusClass('Unknown')).toBe('badge-ghost');
    });
  });

  describe('Form Submit Handler', () => {
    it('should handle create success', () => {
      const successSpy = jest.spyOn(notificationService, 'success');
      const closeSpy = jest.spyOn(component, 'closeForm');

      component.onFormSubmit(new CustomEvent('submit', { detail: { mode: 'create' } }));

      expect(successSpy).toHaveBeenCalledWith('Truck created successfully');
      expect(closeSpy).toHaveBeenCalled();
    });

    it('should handle update success', () => {
      const successSpy = jest.spyOn(notificationService, 'success');
      const closeSpy = jest.spyOn(component, 'closeForm');

      component.onFormSubmit(
        new CustomEvent('submit', {
          detail: { mode: 'update', truck: mockTrucks[0] },
        }),
      );

      expect(successSpy).toHaveBeenCalledWith('Truck updated successfully');
      expect(closeSpy).toHaveBeenCalled();
    });
  });
});
