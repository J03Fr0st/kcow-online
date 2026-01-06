import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
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
