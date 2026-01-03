import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { Injectable, inject, signal, PLATFORM_ID } from '@angular/core';

export type Theme =
  | 'light'
  | 'dark'
  | 'cupcake'
  | 'bumblebee'
  | 'emerald'
  | 'corporate'
  | 'synthwave'
  | 'retro'
  | 'cyberpunk'
  | 'valentine'
  | 'halloween'
  | 'garden'
  | 'forest'
  | 'aqua'
  | 'lofi'
  | 'pastel'
  | 'fantasy'
  | 'wireframe'
  | 'black'
  | 'luxury'
  | 'dracula';

export type LayoutDensity = 'compact' | 'comfortable' | 'spacious';

export interface NotificationPreferences {
  enabled: boolean;
  position: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';
  duration: number;
  showProgress: boolean;
  sound: boolean;
  desktop: boolean;
  types: {
    success: boolean;
    error: boolean;
    warning: boolean;
    info: boolean;
  };
}

export interface WorkspaceSettings {
  theme: Theme;
  layoutDensity: LayoutDensity;
  sidebarCollapsed: boolean;
  showBreadcrumb: boolean;
  notifications: NotificationPreferences;
  autoSave: boolean;
  animations: boolean;
  language: string;
  timezone: string;
}

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly SETTINGS_KEY = 'workspace-settings';
  private readonly document = inject(DOCUMENT);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  // Individual signals for reactive UI updates
  currentTheme = signal<Theme>(this.getInitialTheme());
  layoutDensity = signal<LayoutDensity>('comfortable');
  sidebarCollapsed = signal<boolean>(false);
  showBreadcrumb = signal<boolean>(true);
  notifications = signal<NotificationPreferences>(this.getDefaultNotificationSettings());
  autoSave = signal<boolean>(true);
  animations = signal<boolean>(true);
  language = signal<string>('en');
  timezone = signal<string>('UTC');

  // Combined settings signal for batch operations
  workspaceSettings = signal<WorkspaceSettings>(this.getDefaultSettings());

  readonly AVAILABLE_THEMES: Theme[] = [
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

  constructor() {
    // Apply settings immediately if in browser
    if (this.isBrowser) {
      this.loadSettings();
      this.applyCurrentSettings();
    }
  }

  private loadSettings(): void {
    if (!this.isBrowser || typeof localStorage === 'undefined') {
      return;
    }

    try {
      const stored = localStorage.getItem(this.SETTINGS_KEY);
      if (stored) {
        const settings = JSON.parse(stored) as WorkspaceSettings;
        this.workspaceSettings.set(settings);

        // Update individual signals
        this.currentTheme.set(settings.theme);
        this.layoutDensity.set(settings.layoutDensity);
        this.sidebarCollapsed.set(settings.sidebarCollapsed);
        this.showBreadcrumb.set(settings.showBreadcrumb);
        this.notifications.set(settings.notifications);
        this.autoSave.set(settings.autoSave);
        this.animations.set(settings.animations);
        this.language.set(settings.language);
        this.timezone.set(settings.timezone);
      }
    } catch (error) {
      console.warn('Failed to load workspace settings:', error);
    }
  }

  // Theme Management
  setTheme(theme: Theme): void {
    this.currentTheme.set(theme);
    this.applyTheme(theme);
    this.updateSettings({ theme });
  }

  toggleDarkMode(): void {
    const newTheme = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }

  // Layout Density Management
  setLayoutDensity(density: LayoutDensity): void {
    this.layoutDensity.set(density);
    this.applyLayoutDensity(density);
    this.updateSettings({ layoutDensity: density });
  }

  // Sidebar Management
  setSidebarCollapsed(collapsed: boolean): void {
    this.sidebarCollapsed.set(collapsed);
    this.applySidebarState(collapsed);
    this.updateSettings({ sidebarCollapsed: collapsed });
  }

  toggleSidebar(): void {
    this.setSidebarCollapsed(!this.sidebarCollapsed());
  }

  // Breadcrumb Management
  setShowBreadcrumb(show: boolean): void {
    this.showBreadcrumb.set(show);
    this.updateSettings({ showBreadcrumb: show });
  }

  // Notification Settings Management
  updateNotificationSettings(notifications: Partial<NotificationPreferences>): void {
    const currentNotifications = this.notifications();
    const updatedNotifications = { ...currentNotifications, ...notifications };
    this.notifications.set(updatedNotifications);
    this.updateSettings({ notifications: updatedNotifications });
  }

  // General Settings
  setAutoSave(enabled: boolean): void {
    this.autoSave.set(enabled);
    this.updateSettings({ autoSave: enabled });
  }

  setAnimations(enabled: boolean): void {
    this.animations.set(enabled);
    this.applyAnimationSettings(enabled);
    this.updateSettings({ animations: enabled });
  }

  setLanguage(language: string): void {
    this.language.set(language);
    this.updateSettings({ language });
  }

  setTimezone(timezone: string): void {
    this.timezone.set(timezone);
    this.updateSettings({ timezone });
  }

  // Batch Settings Update
  updateSettings(updates: Partial<WorkspaceSettings>): void {
    const currentSettings = this.workspaceSettings();
    const newSettings = { ...currentSettings, ...updates };
    this.workspaceSettings.set(newSettings);

    // Save to localStorage if available
    if (this.isBrowser && typeof localStorage !== 'undefined') {
      localStorage.setItem(this.SETTINGS_KEY, JSON.stringify(newSettings));
    }
  }

  // Reset to Defaults
  resetToDefaults(): void {
    const defaultSettings = this.getDefaultSettings();
    this.updateSettings(defaultSettings);
    this.applyCurrentSettings();
  }

  private getDefaultSettings(): WorkspaceSettings {
    return {
      theme: this.getInitialTheme(),
      layoutDensity: 'comfortable',
      sidebarCollapsed: false,
      showBreadcrumb: true,
      notifications: this.getDefaultNotificationSettings(),
      autoSave: true,
      animations: true,
      language: 'en',
      timezone: 'UTC',
    };
  }

  private getDefaultNotificationSettings(): NotificationPreferences {
    return {
      enabled: true,
      position: 'top-right',
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
    };
  }

  private getInitialTheme(): Theme {
    if (!this.isBrowser) {
      // Default theme for SSR/prerender
      return 'light';
    }

    // Check system preference (fallback since we now use comprehensive settings)
    if (typeof window !== 'undefined' && window.matchMedia) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    // Fallback default
    return 'light';
  }

  private applyCurrentSettings(): void {
    const settings = this.workspaceSettings();

    this.applyTheme(settings.theme);
    this.applyLayoutDensity(settings.layoutDensity);
    this.applySidebarState(settings.sidebarCollapsed);
    this.applyAnimationSettings(settings.animations);
  }

  private applyTheme(theme: Theme): void {
    if (this.isBrowser && this.document?.documentElement) {
      this.document.documentElement.setAttribute('data-theme', theme);
    }
  }

  private applyLayoutDensity(density: LayoutDensity): void {
    if (this.isBrowser && this.document?.documentElement) {
      // Remove existing density classes
      this.document.documentElement.classList.remove(
        'layout-compact',
        'layout-comfortable',
        'layout-spacious',
      );
      // Add new density class
      this.document.documentElement.classList.add(`layout-${density}`);
    }
  }

  private applySidebarState(collapsed: boolean): void {
    if (this.isBrowser && this.document?.documentElement) {
      if (collapsed) {
        this.document.documentElement.classList.add('sidebar-collapsed');
      } else {
        this.document.documentElement.classList.remove('sidebar-collapsed');
      }
    }
  }

  private applyAnimationSettings(enabled: boolean): void {
    if (this.isBrowser && this.document?.documentElement) {
      if (enabled) {
        this.document.documentElement.classList.remove('no-animations');
      } else {
        this.document.documentElement.classList.add('no-animations');
      }
    }
  }
}
