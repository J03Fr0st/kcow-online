import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { ScheduleConflict } from '../models/class-group.model';

@Component({
  selector: 'app-conflict-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './conflict-banner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConflictBannerComponent {
  readonly conflicts = input.required<ScheduleConflict[]>();
}
