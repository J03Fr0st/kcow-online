import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MENU_ICONS } from '@core/constants/icons.constants';
import { SidebarService } from '@core/services/sidebar.service';
import { MENU_ITEMS } from '@models/menu-item.model';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
})
export class SidebarComponent {
  sidebarService = inject(SidebarService);
  menuItems = MENU_ITEMS;

  onMobileClick(): void {
    if (this.sidebarService.isMobile()) {
      this.sidebarService.close();
    }
  }

  getIcon(iconName: string): string {
    return MENU_ICONS[iconName] || '📄';
  }
}
