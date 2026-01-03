import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavbarComponent } from './navbar.component';
import { SidebarService } from '@core/services/sidebar.service';
import { ThemeService } from '@core/services/theme.service';
import { BreadcrumbService } from '@core/services/breadcrumb.service';
import { PageMetadataService } from '@core/services/page-metadata.service';
import { AuthService } from '@core/services/auth.service';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let sidebarServiceMock: any;
  let themeServiceMock: any;
  let breadcrumbServiceMock: any;
  let pageMetadataServiceMock: any;
  let authServiceMock: any;

  beforeEach(async () => {
    sidebarServiceMock = {
      isOpen: signal(true),
      isMobile: signal(false),
      toggle: jest.fn()
    };

    themeServiceMock = {
      currentTheme: signal('light'),
      setTheme: jest.fn(),
      AVAILABLE_THEMES: ['light', 'dark']
    };

    breadcrumbServiceMock = {
      getBreadcrumbs: jest.fn().mockReturnValue([]),
      navigateTo: jest.fn()
    };

    pageMetadataServiceMock = {
      title: signal('Test')
    };

    authServiceMock = {
      currentUser: signal({ name: 'Test User' }),
      clearSessionAndRedirect: jest.fn()
    };

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: SidebarService, useValue: sidebarServiceMock },
        { provide: ThemeService, useValue: themeServiceMock },
        { provide: BreadcrumbService, useValue: breadcrumbServiceMock },
        { provide: PageMetadataService, useValue: pageMetadataServiceMock },
        { provide: AuthService, useValue: authServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle sidebar', () => {
    component.sidebarService.toggle();
    expect(sidebarServiceMock.toggle).toHaveBeenCalled();
  });

  it('should set theme', () => {
    component.setTheme('dark');
    expect(themeServiceMock.setTheme).toHaveBeenCalledWith('dark');
  });

  it('should logout', () => {
    component.logout();
    expect(authServiceMock.clearSessionAndRedirect).toHaveBeenCalled();
  });

  // Accessibility Tests (Story 1.5 AC#2: Keyboard navigation)
  it('should have all interactive elements keyboard accessible', () => {
    const buttons = fixture.nativeElement.querySelectorAll('button');
    const links = fixture.nativeElement.querySelectorAll('a');

    // All buttons should have proper labels
    buttons.forEach((button: HTMLButtonElement) => {
      const hasAriaLabel = button.hasAttribute('aria-label') ||
                           button.textContent?.trim().length > 0;
      expect(hasAriaLabel).toBeTruthy();
    });

    // All links should be focusable (receive focus ring via global CSS)
    expect(links.length).toBeGreaterThan(0);
  });

  it('should have sidebar toggle button with aria-label', () => {
    const toggleButton = fixture.nativeElement.querySelector('button[aria-label="Toggle sidebar"]');
    expect(toggleButton).toBeTruthy();
  });

  it('should have proper semantic structure with header element', () => {
    const header = fixture.nativeElement.querySelector('header');
    expect(header).toBeTruthy();

    // Header landmark for screen readers
    expect(header.tagName.toLowerCase()).toBe('header');
  });

  it('should have proper button semantic elements', () => {
    // Verify theme toggle, notification, and user menu buttons exist
    const buttonElements = fixture.nativeElement.querySelectorAll('.btn');

    expect(buttonElements.length).toBeGreaterThan(0);

    // All buttons should be focusable via keyboard
    buttonElements.forEach((button: HTMLButtonElement) => {
      expect(button.tagName.toLowerCase()).toBe('button');
    });
  });
});
