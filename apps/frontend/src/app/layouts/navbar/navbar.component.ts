import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { BreadcrumbService } from '@core/services/breadcrumb.service';
import { PageMetadataService } from '@core/services/page-metadata.service';
import { SidebarService } from '@core/services/sidebar.service';
import { type Theme, ThemeService } from '@core/services/theme.service';
import { GlobalSearchComponent } from './global-search/global-search.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, GlobalSearchComponent],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent {
  sidebarService = inject(SidebarService);
  themeService = inject(ThemeService);
  breadcrumbService = inject(BreadcrumbService);
  pageMetadataService = inject(PageMetadataService);
  authService = inject(AuthService);
  router = inject(Router);

  themes = this.themeService.AVAILABLE_THEMES;

  setTheme(theme: string): void {
    this.themeService.setTheme(theme as Theme);
  }

  navigateToBreadcrumb(crumb: { label: string; url: string; icon?: string }): void {
    this.breadcrumbService.navigateTo(crumb);
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    this.authService.clearSessionAndRedirect();
  }

  get currentUserInitials(): string {
    const user = this.authService.currentUser();
    if (!user) return 'U';
    const name = user.name;
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  get currentUserName(): string {
    const user = this.authService.currentUser();
    return user ? user.name : 'User';
  }
}
