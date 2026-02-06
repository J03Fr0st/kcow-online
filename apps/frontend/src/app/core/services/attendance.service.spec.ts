import { TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { AttendanceService } from './attendance.service';
import type { Attendance, CreateAttendanceRequest, UpdateAttendanceRequest } from '@features/attendance/models/attendance.model';

// Configurable base URL for tests - update if API URL changes
const TEST_API_URL = 'http://localhost:5039/api';

describe('AttendanceService', () => {
  let service: AttendanceService;
  let httpClientSpy: { get: jest.Mock; post: jest.Mock; put: jest.Mock };

  beforeEach(() => {
    httpClientSpy = {
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        AttendanceService,
        { provide: HttpClient, useValue: httpClientSpy },
      ],
    });

    service = TestBed.inject(AttendanceService);
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAttendance', () => {
    it('should return attendance records with no filters', (done) => {
      const mockRecords: Attendance[] = [
        {
          id: 1,
          studentId: 1,
          classGroupId: 1,
          sessionDate: '2026-01-01',
          status: 'Present',
          notes: 'On time',
          createdAt: '2026-01-01T10:00:00Z',
        },
      ];

      httpClientSpy.get.mockReturnValue(of(mockRecords));

      service.getAttendance({}).subscribe((records) => {
        expect(records).toEqual(mockRecords);
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance`);
        done();
      });
    });

    it('should filter by studentId', (done) => {
      httpClientSpy.get.mockReturnValue(of([]));

      service.getAttendance({ studentId: 1 }).subscribe(() => {
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance?studentId=1`);
        done();
      });
    });

    it('should filter by classGroupId', (done) => {
      httpClientSpy.get.mockReturnValue(of([]));

      service.getAttendance({ classGroupId: 2 }).subscribe(() => {
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance?classGroupId=2`);
        done();
      });
    });

    it('should filter by date range', (done) => {
      httpClientSpy.get.mockReturnValue(of([]));

      service.getAttendance({ fromDate: '2026-01-01', toDate: '2026-01-31' }).subscribe(() => {
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance?fromDate=2026-01-01&toDate=2026-01-31`);
        done();
      });
    });

    it('should combine multiple filters', (done) => {
      httpClientSpy.get.mockReturnValue(of([]));

      service.getAttendance({ studentId: 1, classGroupId: 2, fromDate: '2026-01-01' }).subscribe(() => {
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance?studentId=1&classGroupId=2&fromDate=2026-01-01`);
        done();
      });
    });

    it('should handle error response', (done) => {
      const errorResponse = { status: 500, message: 'Server Error' };
      httpClientSpy.get.mockReturnValue(throwError(() => errorResponse));

      service.getAttendance({ studentId: 1 }).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error).toEqual(errorResponse);
          done();
        },
      });
    });
  });

  describe('getAttendanceById', () => {
    it('should return a single attendance record', (done) => {
      const mockRecord: Attendance = {
        id: 1,
        studentId: 1,
        classGroupId: 1,
        sessionDate: '2026-01-01',
        status: 'Present',
        notes: 'On time',
        createdAt: '2026-01-01T10:00:00Z',
      };

      httpClientSpy.get.mockReturnValue(of(mockRecord));

      service.getAttendanceById(1).subscribe((record) => {
        expect(record).toEqual(mockRecord);
        expect(httpClientSpy.get).toHaveBeenCalledWith(`${TEST_API_URL}/attendance/1`);
        done();
      });
    });

    it('should handle error response', (done) => {
      const errorResponse = { status: 404, message: 'Not Found' };
      httpClientSpy.get.mockReturnValue(throwError(() => errorResponse));

      service.getAttendanceById(999).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error).toEqual(errorResponse);
          done();
        },
      });
    });
  });

  describe('createAttendance', () => {
    it('should create a new attendance record', (done) => {
      const newRecord: CreateAttendanceRequest = {
        studentId: 1,
        classGroupId: 1,
        sessionDate: '2026-01-01',
        status: 'Present',
        notes: 'On time',
      };

      const createdRecord: Attendance = {
        id: 1,
        ...newRecord,
        createdAt: '2026-01-01T10:00:00Z',
      };

      httpClientSpy.post.mockReturnValue(of(createdRecord));

      service.createAttendance(newRecord).subscribe((record) => {
        expect(record).toEqual(createdRecord);
        expect(httpClientSpy.post).toHaveBeenCalledWith(`${TEST_API_URL}/attendance`, newRecord);
        done();
      });
    });

    it('should handle error response', (done) => {
      const newRecord: CreateAttendanceRequest = {
        studentId: 1,
        classGroupId: 1,
        sessionDate: '2026-01-01',
        status: 'Present',
      };

      const errorResponse = { status: 400, message: 'Validation Error' };
      httpClientSpy.post.mockReturnValue(throwError(() => errorResponse));

      service.createAttendance(newRecord).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error).toEqual(errorResponse);
          done();
        },
      });
    });
  });

  describe('updateAttendance', () => {
    it('should update an existing attendance record', (done) => {
      const updateData: UpdateAttendanceRequest = {
        status: 'Absent',
        notes: 'Sick leave',
      };

      const updatedRecord: Attendance = {
        id: 1,
        studentId: 1,
        classGroupId: 1,
        sessionDate: '2026-01-01',
        ...updateData,
        createdAt: '2026-01-01T10:00:00Z',
        modifiedAt: '2026-01-01T12:00:00Z',
      };

      httpClientSpy.put.mockReturnValue(of(updatedRecord));

      service.updateAttendance(1, updateData).subscribe((record) => {
        expect(record).toEqual(updatedRecord);
        expect(httpClientSpy.put).toHaveBeenCalledWith(`${TEST_API_URL}/attendance/1`, updateData);
        done();
      });
    });

    it('should handle error response', (done) => {
      const updateData: UpdateAttendanceRequest = {
        status: 'Absent',
      };

      const errorResponse = { status: 404, message: 'Not Found' };
      httpClientSpy.put.mockReturnValue(throwError(() => errorResponse));

      service.updateAttendance(999, updateData).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error).toEqual(errorResponse);
          done();
        },
      });
    });
  });
});
