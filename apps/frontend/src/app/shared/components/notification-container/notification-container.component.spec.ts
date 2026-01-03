import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NotificationService } from '@core/services/notification.service';
import { NotificationContainerComponent } from './notification-container.component';

describe('NotificationContainerComponent', () => {
  let component: NotificationContainerComponent;
  let fixture: ComponentFixture<NotificationContainerComponent>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationContainerComponent, BrowserAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationContainerComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display notifications', () => {
    notificationService.success('Test message');
    fixture.detectChanges();
    const compiled = fixture.nativeElement;
    expect(compiled.querySelector('.notification-success')).toBeTruthy();
  });

  it('should dismiss notification when close button is clicked', () => {
    const _id = notificationService.info('Test message');
    fixture.detectChanges();
    expect(component.notifications().length).toBe(1);

    const closeButton = fixture.nativeElement.querySelector('.notification-close');
    closeButton.click();
    fixture.detectChanges();

    expect(component.notifications().length).toBe(0);
  });

  it('should return correct icon for notification type', () => {
    expect(component.getIcon('success')).toBe('check-circle');
    expect(component.getIcon('error')).toBe('x-circle');
    expect(component.getIcon('warning')).toBe('alert-triangle');
    expect(component.getIcon('info')).toBe('info');
  });
});
