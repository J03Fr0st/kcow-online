import { CommonModule } from '@angular/common';
import { Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import {
  FormBuilder,
  type FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';

import type {
  LayoutDensity,
  NotificationPreferences,
  Theme,
  WorkspaceSettings,
} from '@core/services/theme.service';
import { ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'app-workspace-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './workspace-settings.component.html',
  styleUrls: ['./workspace-settings.component.css'],
})
export class WorkspaceSettingsComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private themeService = inject(ThemeService);

  // Settings form
  public settingsForm!: FormGroup;

  // Available options
  public readonly themes: Theme[] = [
    'light',
    'dark',
    'cupcake',
    'bumblebee',
    'emerald',
    'corporate',
    'synthwave',
    'retro',
    'cyberpunk',
    'valentine',
    'halloween',
    'garden',
    'forest',
    'aqua',
    'lofi',
    'pastel',
    'fantasy',
    'wireframe',
    'black',
    'luxury',
    'dracula',
  ];

  public readonly layoutDensities: LayoutDensity[] = ['compact', 'comfortable', 'spacious'];
  public readonly notificationPositions = [
    { value: 'top-right', label: 'Top Right' },
    { value: 'top-left', label: 'Top Left' },
    { value: 'bottom-right', label: 'Bottom Right' },
    { value: 'bottom-left', label: 'Bottom Left' },
  ];

  public readonly languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Español' },
    { code: 'fr', name: 'Français' },
    { code: 'de', name: 'Deutsch' },
    { code: 'it', name: 'Italiano' },
    { code: 'pt', name: 'Português' },
    { code: 'ja', name: '日本語' },
    { code: 'ko', name: '한국어' },
    { code: 'zh', name: '中文' },
  ];

  public readonly timezones = [
    { value: 'UTC', label: 'UTC (Coordinated Universal Time)' },
    { value: 'America/New_York', label: 'Eastern Time (ET)' },
    { value: 'America/Chicago', label: 'Central Time (CT)' },
    { value: 'America/Denver', label: 'Mountain Time (MT)' },
    { value: 'America/Los_Angeles', label: 'Pacific Time (PT)' },
    { value: 'Europe/London', label: 'London (GMT/BST)' },
    { value: 'Europe/Paris', label: 'Paris (CET/CEST)' },
    { value: 'Europe/Berlin', label: 'Berlin (CET/CEST)' },
    { value: 'Asia/Tokyo', label: 'Tokyo (JST)' },
    { value: 'Asia/Shanghai', label: 'Shanghai (CST)' },
    { value: 'Asia/Seoul', label: 'Seoul (KST)' },
    { value: 'Australia/Sydney', label: 'Sydney (AEST/AEDT)' },
  ];

  // UI State
  public activeTab = 'appearance';
  public hasChanges = false;
  public isSaving = false;
  public saveSuccess = false;
  public previewMode = false;

  ngOnInit(): void {
    this.initializeForm();
    this.loadCurrentSettings();
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  /**
   * Initialize the settings form
   */
  private initializeForm(): void {
    this.settingsForm = this.fb.group({
      // Appearance Settings
      theme: ['light', Validators.required],
      layoutDensity: ['comfortable', Validators.required],
      sidebarCollapsed: [false],
      showBreadcrumb: [true],
      animations: [true],

      // Notification Settings
      notifications: this.fb.group({
        enabled: [true],
        position: ['top-right', Validators.required],
        duration: [5000, [Validators.min(1000), Validators.max(30000)]],
        showProgress: [true],
        sound: [false],
        desktop: [false],
        types: this.fb.group({
          success: [true],
          error: [true],
          warning: [true],
          info: [true],
        }),
      }),

      // General Settings
      autoSave: [true],
      language: ['en', Validators.required],
      timezone: ['UTC', Validators.required],
    });

    // Listen for form changes
    this.settingsForm.valueChanges.subscribe(() => {
      this.hasChanges = true;
      this.saveSuccess = false;
    });
  }

  /**
   * Load current settings from theme service
   */
  private loadCurrentSettings(): void {
    const settings = this.themeService.workspaceSettings();

    this.settingsForm.patchValue({
      theme: settings.theme,
      layoutDensity: settings.layoutDensity,
      sidebarCollapsed: settings.sidebarCollapsed,
      showBreadcrumb: settings.showBreadcrumb,
      animations: settings.animations,
      notifications: settings.notifications,
      autoSave: settings.autoSave,
      language: settings.language,
      timezone: settings.timezone,
    });

    this.hasChanges = false;
  }

  /**
   * Save all settings
   */
  public saveSettings(): void {
    if (this.settingsForm.invalid) {
      this.markFormGroupTouched(this.settingsForm);
      return;
    }

    this.isSaving = true;

    try {
      const formValue = this.settingsForm.value;

      // Update theme service with new settings
      this.themeService.updateSettings(formValue as WorkspaceSettings);

      // Show success feedback
      this.saveSuccess = true;
      this.hasChanges = false;

      // Hide success message after 3 seconds
      setTimeout(() => {
        this.saveSuccess = false;
      }, 3000);
    } catch (error) {
      console.error('Failed to save settings:', error);
      // Handle error (show toast, etc.)
    } finally {
      this.isSaving = false;
    }
  }

  /**
   * Reset settings to defaults
   */
  public resetToDefaults(): void {
    if (
      confirm(
        'Are you sure you want to reset all settings to their default values? This action cannot be undone.',
      )
    ) {
      this.themeService.resetToDefaults();
      this.loadCurrentSettings();
      this.hasChanges = false;
    }
  }

  /**
   * Preview theme without saving
   */
  public previewTheme(theme: Theme): void {
    this.themeService.setTheme(theme);
    this.settingsForm.patchValue({ theme });
  }

  /**
   * Toggle preview mode
   */
  public togglePreviewMode(): void {
    this.previewMode = !this.previewMode;
    if (!this.previewMode) {
      // Revert to saved settings
      this.loadCurrentSettings();
    }
  }

  /**
   * Test notification settings
   */
  public testNotifications(): void {
    const notificationSettings = this.settingsForm.get('notifications')
      ?.value as NotificationPreferences;

    if (notificationSettings?.enabled) {
      // Create a test notification
      this.createTestNotification(notificationSettings);
    }
  }

  /**
   * Create a test notification
   */
  private createTestNotification(settings: NotificationPreferences): void {
    // This would integrate with your notification service
    const notification = {
      type: 'info' as const,
      title: 'Test Notification',
      message: 'This is a test notification to verify your settings.',
      duration: settings.duration,
      showProgress: settings.showProgress,
      position: settings.position,
    };

    // Emit notification or call notification service
    console.log('Test notification:', notification);

    // Visual feedback
    const button = document.querySelector('[data-test-notification]') as HTMLButtonElement;
    if (button) {
      button.textContent = 'Sent!';
      button.classList.add('success');
      setTimeout(() => {
        button.textContent = 'Test Notifications';
        button.classList.remove('success');
      }, 2000);
    }
  }

  /**
   * Export settings
   */
  public exportSettings(): void {
    const settings = this.themeService.workspaceSettings();
    const dataStr = JSON.stringify(settings, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });

    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `workspace-settings-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  /**
   * Import settings
   */
  public importSettings(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const settings = JSON.parse(e.target?.result as string) as WorkspaceSettings;
        this.themeService.updateSettings(settings);
        this.loadCurrentSettings();
        this.hasChanges = false;

        // Show success feedback
        const button = document.querySelector('[data-import-settings]') as HTMLButtonElement;
        if (button) {
          button.textContent = 'Imported!';
          button.classList.add('success');
          setTimeout(() => {
            button.textContent = 'Import Settings';
            button.classList.remove('success');
          }, 2000);
        }
      } catch (error) {
        console.error('Failed to import settings:', error);
        alert('Invalid settings file. Please upload a valid workspace settings JSON file.');
      }
    };
    reader.readAsText(file);
  }

  /**
   * Switch between tabs
   */
  public switchTab(tab: string): void {
    if (this.hasChanges) {
      if (confirm('You have unsaved changes. Are you sure you want to switch tabs?')) {
        this.activeTab = tab;
      }
    } else {
      this.activeTab = tab;
    }
  }

  /**
   * Mark all form controls as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach((control) => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Get form control for easier template access
   */
  public getFormControl(path: string) {
    return this.settingsForm.get(path);
  }

  /**
   * Check if form control has error
   */
  public hasError(controlName: string, errorName: string): boolean {
    const control = this.settingsForm.get(controlName);
    return (control?.hasError(errorName) && control?.touched) || false;
  }

  /**
   * Get display name for theme
   */
  public getThemeDisplayName(theme: Theme): string {
    return theme
      .split('-')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  /**
   * Get display name for layout density
   */
  public getLayoutDensityDisplayName(density: LayoutDensity): string {
    return density.charAt(0).toUpperCase() + density.slice(1);
  }
}
