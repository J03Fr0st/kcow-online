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
});
