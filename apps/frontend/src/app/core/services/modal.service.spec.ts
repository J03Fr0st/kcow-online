import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ModalService } from './modal.service';

@Component({
  selector: 'app-test',
  standalone: true,
  template: '<div>Test Component</div>',
})
class TestComponent {}

describe('ModalService', () => {
  let service: ModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModalService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should open a modal', () => {
    const promise = service.open(TestComponent, {
      title: 'Test Modal',
      size: 'md',
    });

    expect(promise).toBeInstanceOf(Promise);
    expect(service.modals$().length).toBe(1);
    expect(service.modals$()[0].component).toBe(TestComponent);
    expect(service.modals$()[0].config.title).toBe('Test Modal');
  });

  it('should open a confirmation dialog', () => {
    const promise = service.confirm({
      title: 'Confirm',
      message: 'Are you sure?',
    });

    expect(promise).toBeInstanceOf(Promise);
    expect(service.modals$().length).toBe(1);
    expect(service.modals$()[0].config.data.type).toBe('confirmation');
    expect(service.modals$()[0].config.data.message).toBe('Are you sure?');
  });

  it('should open an alert dialog', () => {
    const promise = service.alert('This is an alert', 'Alert Title');

    expect(promise).toBeInstanceOf(Promise);
    expect(service.modals$().length).toBe(1);
    expect(service.modals$()[0].config.data.type).toBe('alert');
    expect(service.modals$()[0].config.data.message).toBe('This is an alert');
    expect(service.modals$()[0].config.title).toBe('Alert Title');
  });

  it('should close a modal with result', (done) => {
    const promise = service.open(TestComponent);
    const modalId = service.modals$()[0].id;

    promise.then((result) => {
      expect(result).toBe('test result');
      expect(service.modals$().length).toBe(0);
      done();
    });

    service.close(modalId, 'test result');
  });

  it('should dismiss a modal', (done) => {
    const promise = service.open(TestComponent);
    const modalId = service.modals$()[0].id;

    promise.catch((reason) => {
      expect(reason).toBe('dismissed');
      expect(service.modals$().length).toBe(0);
      done();
    });

    service.dismiss(modalId, 'dismissed');
  });

  it('should close all modals', (done) => {
    const promise1 = service.open(TestComponent);
    const promise2 = service.open(TestComponent);

    expect(service.modals$().length).toBe(2);

    let rejectedCount = 0;
    const checkDone = () => {
      rejectedCount++;
      if (rejectedCount === 2) {
        expect(service.modals$().length).toBe(0);
        done();
      }
    };

    promise1.catch(() => checkDone());
    promise2.catch(() => checkDone());

    service.closeAll();
  });

  it('should apply default config values', () => {
    service.open(TestComponent);

    const modal = service.modals$()[0];
    expect(modal.config.dismissible).toBe(true);
    expect(modal.config.backdropDismiss).toBe(true);
    expect(modal.config.showCloseButton).toBe(true);
    expect(modal.config.size).toBe('md');
  });

  it('should override default config values', () => {
    service.open(TestComponent, {
      dismissible: false,
      backdropDismiss: false,
      showCloseButton: false,
      size: 'lg',
    });

    const modal = service.modals$()[0];
    expect(modal.config.dismissible).toBe(false);
    expect(modal.config.backdropDismiss).toBe(false);
    expect(modal.config.showCloseButton).toBe(false);
    expect(modal.config.size).toBe('lg');
  });

  it('should get a modal by ID', () => {
    service.open(TestComponent);
    const modalId = service.modals$()[0].id;

    const modal = service.getModal(modalId);
    expect(modal).toBeDefined();
    expect(modal?.id).toBe(modalId);
  });

  it('should return undefined for non-existent modal ID', () => {
    const modal = service.getModal('non-existent');
    expect(modal).toBeUndefined();
  });

  it('should generate unique IDs for modals', () => {
    service.open(TestComponent);
    service.open(TestComponent);
    service.open(TestComponent);

    const ids = service.modals$().map((m) => m.id);
    const uniqueIds = new Set(ids);
    expect(uniqueIds.size).toBe(3);
  });
});
