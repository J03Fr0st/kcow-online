import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { BreadcrumbService } from '@core/services/breadcrumb.service';
import { PageMetadataService } from '@core/services/page-metadata.service';
import { SidebarService } from '@core/services/sidebar.service';
import { type Theme, ThemeService } from '@core/services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
})
export class NavbarComponent {
  sidebarService = inject(SidebarService);
  themeService = inject(ThemeService);
  breadcrumbService = inject(BreadcrumbService);
  pageMetadataService = inject(PageMetadataService);

  themes = [
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

  setTheme(theme: string): void {
    this.themeService.setTheme(theme as Theme);
  }

  navigateToBreadcrumb(crumb: { label: string; url: string; icon?: string }): void {
    this.breadcrumbService.navigateTo(crumb);
  }
}
