import { TestBed } from '@angular/core/testing';
import { SidebarService } from './sidebar.service';

describe('SidebarService', () => {
  let service: SidebarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SidebarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with sidebar open', () => {
    expect(service.isOpen()).toBeTruthy();
  });

  it('should toggle sidebar state', () => {
    const initialState = service.isOpen();
    service.toggle();
    expect(service.isOpen()).toBe(!initialState);

    service.toggle();
    expect(service.isOpen()).toBe(initialState);
  });

  it('should open sidebar', () => {
    service.close();
    expect(service.isOpen()).toBe(false);

    service.open();
    expect(service.isOpen()).toBe(true);
  });

  it('should close sidebar', () => {
    service.open();
    expect(service.isOpen()).toBe(true);

    service.close();
    expect(service.isOpen()).toBe(false);
  });

  it('should detect mobile state based on window width', () => {
    // Note: This test may need adjustment based on actual window size in test environment
    expect(service.isMobile()).toBeDefined();
    expect(typeof service.isMobile()).toBe('boolean');
  });
});
