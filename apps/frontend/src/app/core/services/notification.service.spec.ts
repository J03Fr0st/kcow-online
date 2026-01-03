import { TestBed } from '@angular/core/testing';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add a success notification', () => {
    const id = service.success('Test success message');
    expect(service.notifications$().length).toBe(1);
    expect(service.notifications$()[0].type).toBe('success');
    expect(service.notifications$()[0].message).toBe('Test success message');
    expect(service.notifications$()[0].id).toBe(id);
  });

  it('should add an error notification', () => {
    const _id = service.error('Test error message', 'Error');
    expect(service.notifications$().length).toBe(1);
    expect(service.notifications$()[0].type).toBe('error');
    expect(service.notifications$()[0].title).toBe('Error');
  });

  it('should dismiss a notification', () => {
    const id = service.info('Test message');
    expect(service.notifications$().length).toBe(1);
    service.dismiss(id);
    expect(service.notifications$().length).toBe(0);
  });

  it('should clear all notifications', () => {
    service.success('Message 1');
    service.error('Message 2');
    service.warning('Message 3');
    expect(service.notifications$().length).toBe(3);
    service.clearAll();
    expect(service.notifications$().length).toBe(0);
  });

  it('should auto-dismiss notification after duration', (done) => {
    service.success('Test message', undefined, 100);
    expect(service.notifications$().length).toBe(1);

    setTimeout(() => {
      expect(service.notifications$().length).toBe(0);
      done();
    }, 150);
  });
});
