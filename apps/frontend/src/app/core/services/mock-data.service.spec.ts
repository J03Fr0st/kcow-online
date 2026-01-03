import { TestBed } from '@angular/core/testing';
import { MockDataService } from './mock-data.service';

describe('MockDataService', () => {
  let service: MockDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MockDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getStats', () => {
    it('should return an observable of stats', (done) => {
      service.getStats().subscribe((stats) => {
        expect(stats).toBeDefined();
        expect(Array.isArray(stats)).toBeTruthy();
        expect(stats.length).toBeGreaterThan(0);
        done();
      });
    });

    it('should return stats with correct properties', (done) => {
      service.getStats().subscribe((stats) => {
        const stat = stats[0];
        expect(stat).toHaveProperty('title');
        expect(stat).toHaveProperty('value');
        expect(stat).toHaveProperty('change');
        expect(stat).toHaveProperty('changeType');
        expect(stat).toHaveProperty('icon');
        expect(stat).toHaveProperty('color');
        done();
      });
    });
  });

  describe('getUsers', () => {
    it('should return an observable of users', (done) => {
      service.getUsers(1, 10).subscribe((users) => {
        expect(users).toBeDefined();
        expect(Array.isArray(users)).toBeTruthy();
        expect(users.length).toBeLessThanOrEqual(10);
        done();
      });
    });

    it('should return users with correct properties', (done) => {
      service.getUsers(1, 5).subscribe((users) => {
        const user = users[0];
        expect(user).toHaveProperty('id');
        expect(user).toHaveProperty('name');
        expect(user).toHaveProperty('email');
        expect(user).toHaveProperty('role');
        expect(user).toHaveProperty('status');
        expect(user).toHaveProperty('joinDate');
        done();
      });
    });

    it('should handle pagination correctly', (done) => {
      service.getUsers(2, 10).subscribe((users) => {
        expect(users.length).toBeLessThanOrEqual(10);
        expect(users[0].id).toBeGreaterThan(10);
        done();
      });
    });
  });
});
