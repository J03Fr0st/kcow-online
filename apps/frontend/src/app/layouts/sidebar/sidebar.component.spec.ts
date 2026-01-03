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
});
