import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import {
  type CreateTruckRequest,
  type Truck,
  TruckService,
  type UpdateTruckRequest,
} from './truck.service';

describe('TruckService', () => {
  let service: TruckService;
  let httpMock: HttpTestingController;

  const mockTrucks: Truck[] = [
    {
      id: 1,
      name: 'Truck 1',
      registrationNumber: 'CA 123 456',
      status: 'Active',
      notes: 'Test notes',
      isActive: true,
      createdAt: '2024-01-01T00:00:00',
    },
    {
      id: 2,
      name: 'Truck 2',
      registrationNumber: 'CA 789 012',
      status: 'Maintenance',
      notes: '',
      isActive: true,
      createdAt: '2024-01-02T00:00:00',
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TruckService],
    });

    service = TestBed.inject(TruckService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should have empty trucks array', () => {
      expect(service.trucks()).toEqual([]);
    });

    it('should not be loading initially', () => {
      expect(service.loading()).toBe(false);
    });
  });

  describe('loadTrucks', () => {
    it('should load trucks from API', () => {
      service.loadTrucks();

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      expect(req.request.method).toBe('GET');
      req.flush(mockTrucks);

      expect(service.trucks()).toEqual(mockTrucks);
      expect(service.loading()).toBe(false);
    });

    it('should set loading to true during request', () => {
      service.loadTrucks();

      expect(service.loading()).toBe(true);

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      req.flush(mockTrucks);

      expect(service.loading()).toBe(false);
    });

    it('should handle error and set loading to false', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      service.loadTrucks();

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      req.flush('Error', { status: 500, statusText: 'Server Error' });

      expect(service.loading()).toBe(false);
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('getTruck', () => {
    it('should get single truck by ID', () => {
      const truck = mockTrucks[0];

      service.getTruck(1).subscribe((result) => {
        expect(result).toEqual(truck);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      expect(req.request.method).toBe('GET');
      req.flush(truck);
    });

    it('should handle error when getting truck', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      service.getTruck(1).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error).toBeTruthy();
        },
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('createTruck', () => {
    it('should create new truck', () => {
      const newTruck: CreateTruckRequest = {
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'Active',
        notes: 'Test',
      };

      const createdTruck: Truck = {
        id: 3,
        ...newTruck,
        isActive: true,
        createdAt: '2024-01-03T00:00:00',
      };

      service.createTruck(newTruck).subscribe((result) => {
        expect(result).toEqual(createdTruck);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newTruck);
      req.flush(createdTruck);

      expect(service.trucks()).toContainEqual(createdTruck);
    });

    it('should add truck to local state after creation', () => {
      const newTruck: CreateTruckRequest = {
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'Active',
      };

      const createdTruck: Truck = {
        id: 3,
        ...newTruck,
        isActive: true,
        createdAt: '2024-01-03T00:00:00',
      };

      service.trucks.set(mockTrucks);
      expect(service.trucks().length).toBe(2);

      service.createTruck(newTruck).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      req.flush(createdTruck);

      expect(service.trucks().length).toBe(3);
      expect(service.trucks()).toContainEqual(createdTruck);
    });

    it('should validate status before creating truck', () => {
      const newTruck: CreateTruckRequest = {
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'InvalidStatus',
      };

      expect(() => service.createTruck(newTruck).subscribe()).toThrow(
        'Invalid truck status: "InvalidStatus". Must be one of: Active, Maintenance, Retired',
      );
    });

    it('should handle creation error', () => {
      const newTruck: CreateTruckRequest = {
        name: 'New Truck',
        registrationNumber: 'CA 999 999',
        status: 'Active',
      };

      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      service.createTruck(newTruck).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error).toBeTruthy();
        },
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks`);
      req.flush('Error', { status: 400, statusText: 'Bad Request' });

      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('updateTruck', () => {
    it('should update existing truck', () => {
      const updateData: UpdateTruckRequest = {
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'Maintenance',
        notes: 'Updated notes',
      };

      const updatedTruck: Truck = {
        id: 1,
        ...updateData,
        isActive: true,
        createdAt: '2024-01-01T00:00:00',
        updatedAt: '2024-01-03T00:00:00',
      };

      service.updateTruck(1, updateData).subscribe((result) => {
        expect(result).toEqual(updatedTruck);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush(updatedTruck);
    });

    it('should update local state after successful update', () => {
      service.trucks.set(mockTrucks);

      const updateData: UpdateTruckRequest = {
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'Maintenance',
      };

      const updatedTruck: Truck = {
        id: 1,
        ...updateData,
        isActive: true,
        createdAt: '2024-01-01T00:00:00',
        updatedAt: '2024-01-03T00:00:00',
      };

      service.updateTruck(1, updateData).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      req.flush(updatedTruck);

      expect(service.trucks()[0]).toEqual(updatedTruck);
    });

    it('should validate status before updating truck', () => {
      const updateData: UpdateTruckRequest = {
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'InvalidStatus',
      };

      expect(() => service.updateTruck(1, updateData).subscribe()).toThrow(
        'Invalid truck status: "InvalidStatus". Must be one of: Active, Maintenance, Retired',
      );
    });

    it('should handle update error', () => {
      const updateData: UpdateTruckRequest = {
        name: 'Updated Truck',
        registrationNumber: 'CA 123 456',
        status: 'Maintenance',
      };

      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      service.updateTruck(1, updateData).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error).toBeTruthy();
        },
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      req.flush('Error', { status: 400, statusText: 'Bad Request' });

      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('deleteTruck', () => {
    it('should delete truck', () => {
      service.deleteTruck(1).subscribe((result) => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should remove truck from local state after deletion', () => {
      service.trucks.set(mockTrucks);
      expect(service.trucks().length).toBe(2);

      service.deleteTruck(1).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      req.flush(null);

      expect(service.trucks().length).toBe(1);
      expect(service.trucks().find((t) => t.id === 1)).toBeUndefined();
    });

    it('should handle delete error', () => {
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

      service.deleteTruck(1).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error).toBeTruthy();
        },
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/trucks/1`);
      req.flush('Error', { status: 400, statusText: 'Bad Request' });

      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('validateStatus', () => {
    it('should accept valid status values', () => {
      expect(() => {
        service.validateStatus('Active');
      }).not.toThrow();

      expect(() => {
        service.validateStatus('Maintenance');
      }).not.toThrow();

      expect(() => {
        service.validateStatus('Retired');
      }).not.toThrow();
    });

    it('should reject invalid status values', () => {
      expect(() => {
        service.validateStatus('Invalid');
      }).toThrow('Invalid truck status: "Invalid". Must be one of: Active, Maintenance, Retired');

      expect(() => {
        service.validateStatus('');
      }).toThrow();

      expect(() => {
        service.validateStatus('ACTIVE');
      }).toThrow();
    });
  });
});
