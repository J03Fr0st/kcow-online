import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SidebarComponent } from './sidebar.component';
import { SidebarService } from '@core/services/sidebar.service';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;
  let sidebarServiceMock: any;

  beforeEach(async () => {
    sidebarServiceMock = {
      isOpen: signal(true),
      isMobile: signal(false),
      close: jest.fn(),
      toggle: jest.fn()
    };

    await TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        provideRouter([]),
        { provide: SidebarService, useValue: sidebarServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have menu items', () => {
     expect(component.menuItems.length).toBeGreaterThan(0);
  });

  // Accessibility Tests (Story 1.5 AC#2: Keyboard navigation)
  it('should have navigation links for keyboard accessibility', () => {
    const navLinks = fixture.nativeElement.querySelectorAll('nav a[routerLink]');
    expect(navLinks.length).toBeGreaterThan(0);

    // All navigation links should be focusable
    navLinks.forEach((link: HTMLAnchorElement) => {
      expect(link.getAttribute('routerLink')).toBeTruthy();
      // Links receive focus ring via global CSS: styles.css @layer base
    });
  });

  it('should have close button with aria-label for mobile', () => {
    const closeButton = fixture.nativeElement.querySelector('button[aria-label="Close sidebar"]');
    expect(closeButton).toBeTruthy();
  });

  it('should have proper semantic structure for screen readers', () => {
    const aside = fixture.nativeElement.querySelector('aside');
    const nav = fixture.nativeElement.querySelector('nav');

    expect(aside).toBeTruthy();
    expect(nav).toBeTruthy();

    // Aside element provides landmark for screen readers
    // Nav element indicates navigation region
  });
});
