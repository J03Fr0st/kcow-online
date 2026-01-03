import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import type { StatCard, StatIcon } from '@models/stats.model';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div class="stats shadow-lg bg-base-100">
      <div class="stat">
        <div class="stat-figure" [class]="'text-' + stat.color">
          <span class="text-4xl">{{ getIcon(stat.icon) }}</span>
        </div>
        <div class="stat-title">{{ stat.title }}</div>
        <div class="stat-value" [class]="'text-' + stat.color">{{ stat.value }}</div>
        <div class="stat-desc flex items-center gap-1">
          <span
            class="font-semibold"
            [class.text-success]="stat.changeType === 'increase'"
            [class.text-error]="stat.changeType === 'decrease'">
            {{ stat.changeType === 'increase' ? '↗︎' : '↘︎' }}
            {{ stat.change }}%
          </span>
          <span>from last month</span>
        </div>
      </div>
    </div>
  `,
})
export class StatCardComponent {
  @Input({ required: true }) stat!: StatCard;

  getIcon(iconName: StatIcon): string {
    const icons: Record<StatIcon, string> = {
      dollar: '💵',
      users: '👥',
      shopping: '🛒',
      trending: '📈',
    };
    return icons[iconName] || '📊';
  }
}
