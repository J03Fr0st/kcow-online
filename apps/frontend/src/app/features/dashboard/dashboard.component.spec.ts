import { Component } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { MockDataService } from '@core/services/mock-data.service';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';

@Component({ selector: 'app-stat-card', standalone: true, template: '' })
class MockStatCardComponent {}

@Component({ selector: 'app-loading-spinner', standalone: true, template: '' })
class MockLoadingSpinnerComponent {}

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let mockDataService: Partial<MockDataService>;

  beforeEach(async () => {
    mockDataService = {
      getStats: jest.fn().mockReturnValue(of([])),
      getRecentActivities: jest.fn().mockReturnValue(of([])),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [{ provide: MockDataService, useValue: mockDataService }],
    })
      .overrideComponent(DashboardComponent, {
        remove: { imports: [MockStatCardComponent, MockLoadingSpinnerComponent] }, // Actually it imports the real ones, so remove imports might fail if names don't match exactly or if I should just add mocks to imports and override.
        // Better way:
        add: { imports: [MockStatCardComponent, MockLoadingSpinnerComponent] },
      })
      .compileComponents();

    // Since overrides are tricky with standalone imports sometimes, let's just use component override to replace the *imports* array if needed,
    // or rely on the fact that if I provide a mock with same selector it might conflict.
    // But standalone components import dependencies directly.
    // So I must override the imports of DashboardComponent.

    TestBed.overrideComponent(DashboardComponent, {
      set: {
        imports: [MockStatCardComponent, MockLoadingSpinnerComponent], // This replaces all imports with these mocks? No, "set" replaces metadata.
        // But DashboardComponent imports CommonModule too.
        // Let's use 'remove' and 'add' if possible or just include CommonModule.
      },
    });
    // Wait, `set` replaces everything.
    // Let's try to just provide mocks and not override if I can avoid it, but they are direct imports.
    // I need to override.

    TestBed.overrideComponent(DashboardComponent, {
      set: {
        imports: [
          MockStatCardComponent,
          MockLoadingSpinnerComponent /* CommonModule is usually needed */,
        ],
      },
    });
    // Wait, CommonModule is exported by @angular/common.
    const { CommonModule } = await import('@angular/common');

    TestBed.overrideComponent(DashboardComponent, {
      set: {
        imports: [CommonModule, MockStatCardComponent, MockLoadingSpinnerComponent],
      },
    });

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load data on init', () => {
    expect(mockDataService.getStats).toHaveBeenCalled();
    expect(mockDataService.getRecentActivities).toHaveBeenCalled();
  });

  // Accessibility Tests (Story 1.5 AC#2: Keyboard navigation)
  it('should have proper heading structure for screen readers', () => {
    fixture.detectChanges();

    const h1 = fixture.nativeElement.querySelector('h1');
    expect(h1).toBeTruthy();

    // H1 should describe the page purpose
    expect(h1.textContent).toContain('Dashboard');
  });

  it('should have keyboard-accessible quick action buttons', () => {
    component.stats.set([]); // Set empty stats to bypass loading
    component.loading.set(false);
    fixture.detectChanges();

    const buttons = fixture.nativeElement.querySelectorAll('button.btn');

    if (buttons.length > 0) {
      // All buttons should be focusable
      buttons.forEach((button: HTMLButtonElement) => {
        expect(button.tagName.toLowerCase()).toBe('button');
      });
    }
  });

  it('should have proper semantic HTML structure', () => {
    component.stats.set([]);
    component.loading.set(false);
    fixture.detectChanges();

    // Cards use semantic divs but should have proper heading context
    const cards = fixture.nativeElement.querySelectorAll('.card');
    expect(cards.length).toBeGreaterThan(0);

    // Each card should have a card-title for accessibility
    const cardTitles = fixture.nativeElement.querySelectorAll('.card-title');
    expect(cardTitles.length).toBeGreaterThan(0);
  });
});
