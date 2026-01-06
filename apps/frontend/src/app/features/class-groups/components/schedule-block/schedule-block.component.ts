import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import type { ClassGroup } from '../../models/class-group.model';
import './schedule-block.component.css';

@Component({
  selector: 'app-schedule-block',
  standalone: true,
  templateUrl: './schedule-block.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScheduleBlockComponent {
  readonly classGroup = input.required<ClassGroup>();
  readonly hasConflict = input(false);
  readonly blockClick = output<ClassGroup>();

  protected get blockStyle(): { [key: string]: string } {
    // Color-code by truck for visual distinction
    const truckId = this.classGroup().truckId || 0;
    const hue = (truckId * 137.5) % 360; // Golden angle for distinct colors
    return {
      'background-color': `hsl(${hue}, 70%, 85%)`,
      'border-left-color': `hsl(${hue}, 70%, 45%)`,
    };
  }

  protected onClick(): void {
    this.blockClick.emit(this.classGroup());
  }
}
