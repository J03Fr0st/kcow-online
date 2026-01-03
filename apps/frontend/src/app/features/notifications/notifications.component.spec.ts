import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationService } from '@core/services/notification.service';
import { NotificationsComponent } from './notifications.component';

describe('NotificationsComponent', () => {
  let component: NotificationsComponent;
  let fixture: ComponentFixture<NotificationsComponent>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationsComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show success notification', () => {
    jest.spyOn(notificationService, 'success');
    component.showSuccess();
    expect(notificationService.success).toHaveBeenCalled();
  });

  it('should show error notification', () => {
    jest.spyOn(notificationService, 'error');
    component.showError();
    expect(notificationService.error).toHaveBeenCalled();
  });

  it('should show warning notification', () => {
    jest.spyOn(notificationService, 'warning');
    component.showWarning();
    expect(notificationService.warning).toHaveBeenCalled();
  });

  it('should show info notification', () => {
    jest.spyOn(notificationService, 'info');
    component.showInfo();
    expect(notificationService.info).toHaveBeenCalled();
  });

  it('should clear all notifications', () => {
    jest.spyOn(notificationService, 'clearAll');
    component.clearAll();
    expect(notificationService.clearAll).toHaveBeenCalled();
  });
});
