import { TestBed } from '@angular/core/testing';
import { type LayoutDensity, type Theme, ThemeService } from './theme.service';

describe('ThemeService', () => {
  let service: ThemeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    localStorage.clear();
    service = TestBed.inject(ThemeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default theme', () => {
    expect(service.currentTheme()).toBeTruthy();
    expect(['light', 'dark']).toContain(service.currentTheme());
  });

  it('should set theme and update signal', () => {
    const newTheme: Theme = 'dark';
    service.setTheme(newTheme);

    expect(service.currentTheme()).toBe(newTheme);
  });

  it('should persist theme to localStorage', () => {
    const newTheme: Theme = 'dark';
    service.setTheme(newTheme);

    const saved = localStorage.getItem('workspace-settings');
    expect(saved).toBeTruthy();
    if (saved) {
      const settings = JSON.parse(saved);
      expect(settings.theme).toBe(newTheme);
    }
  });

  it('should apply theme to document element', () => {
    const newTheme: Theme = 'cyberpunk';
    service.setTheme(newTheme);

    const appliedTheme = document.documentElement.getAttribute('data-theme');
    expect(appliedTheme).toBe(newTheme);
  });

  it('should toggle between light and dark modes', () => {
    service.setTheme('light');
    expect(service.currentTheme()).toBe('light');

    service.toggleDarkMode();
    expect(service.currentTheme()).toBe('dark');

    service.toggleDarkMode();
    expect(service.currentTheme()).toBe('light');
  });

  it('should load theme from localStorage on init', () => {
    const testSettings = {
      theme: 'retro' as Theme,
      layoutDensity: 'comfortable' as LayoutDensity,
      sidebarCollapsed: false,
      showBreadcrumb: true,
      notifications: {
        enabled: true,
        position: 'top-right' as const,
        duration: 5000,
        showProgress: true,
        sound: false,
        desktop: false,
        types: {
          success: true,
          error: true,
          warning: true,
          info: true,
        },
      },
      autoSave: true,
      animations: true,
      language: 'en',
      timezone: 'UTC',
    };
    localStorage.setItem('workspace-settings', JSON.stringify(testSettings));

    // Create a new TestBed instance to trigger a fresh service initialization
    const newTestBed = TestBed.resetTestingModule();
    newTestBed.configureTestingModule({});
    const newService = newTestBed.inject(ThemeService);

    expect(newService.currentTheme()).toBe('retro');
  });
});
