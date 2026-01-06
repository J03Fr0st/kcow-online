import { Component, inject, OnInit, computed, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ClassGroupService } from '@core/services/class-group.service';
import type { ClassGroup } from '@features/class-groups/models/class-group.model';
import { ScheduleBlockComponent } from '../components/schedule-block/schedule-block.component';
import { DAY_OF_WEEK_OPTIONS } from '../models/class-group.model';
import './weekly-schedule.component.css';

interface ScheduleBlock {
  classGroup: ClassGroup;
  column: number;
  rowStart: number;
  rowEnd: number;
  hasConflict: boolean;
}

interface TimeSlot {
  hour: number;
  minute: number;
  label: string;
}

@Component({
  selector: 'app-weekly-schedule',
  standalone: true,
  imports: [CommonModule, ScheduleBlockComponent],
  templateUrl: './weekly-schedule.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WeeklyScheduleComponent implements OnInit {
  private readonly classGroupService = inject(ClassGroupService);
  private readonly router = inject(Router);

  // Grid configuration
  readonly startHour = 7; // 7:00 AM
  readonly endHour = 17; // 5:00 PM
  readonly slotDurationMinutes = 30;

  // Days of week (Monday-Friday)
  readonly days = DAY_OF_WEEK_OPTIONS;

  // Computed: time slots for the grid
  protected readonly timeSlots = computed<TimeSlot[]>(() => {
    const slots: TimeSlot[] = [];
    for (let hour = this.startHour; hour < this.endHour; hour++) {
      slots.push({ hour, minute: 0, label: this.formatTime(hour, 0) });
      slots.push({ hour, minute: 30, label: this.formatTime(hour, 30) });
    }
    return slots;
  });

  // Computed: schedule blocks positioned on the grid
  protected readonly scheduleBlocks = computed<ScheduleBlock[]>(() => {
    const classGroups = this.classGroupService.classGroups();
    const blocks: ScheduleBlock[] = [];

    // Group by day and check for conflicts
    const byDay: Record<number, ClassGroup[]> = {};
    for (const day of DAY_OF_WEEK_OPTIONS) {
      const dayNum = this.getDayNumber(day);
      byDay[dayNum] = classGroups.filter(cg => cg.dayOfWeek === dayNum);
    }

    // Detect conflicts and create blocks
    for (const day of DAY_OF_WEEK_OPTIONS) {
      const dayNum = this.getDayNumber(day);
      const dayGroups = byDay[dayNum];

      // Check for conflicts (overlapping times for same truck)
      const conflicts = this.detectConflicts(dayGroups);

      for (const classGroup of dayGroups) {
        const position = this.calculateBlockPosition(classGroup);
        const hasConflict = conflicts.has(classGroup.id);

        blocks.push({
          classGroup,
          column: this.getDayNumber(classGroup.dayOfWeek) + 1, // +1 for time label column
          rowStart: position.rowStart,
          rowEnd: position.rowEnd,
          hasConflict,
        });
      }
    }

    return blocks;
  });

  // Loading state
  protected readonly isLoading = computed(() => this.classGroupService.loading());

  ngOnInit(): void {
    // Load class groups if not already loaded
    if (this.classGroupService.classGroups().length === 0) {
      this.classGroupService.loadClassGroups();
    }
  }

  /**
   * Calculate grid position for a class group block
   */
  private calculateBlockPosition(classGroup: ClassGroup): { rowStart: number; rowEnd: number } {
    const gridStartTime = this.startHour * 60; // Convert to minutes
    const startMinutes = this.timeToMinutes(classGroup.startTime);
    const endMinutes = this.timeToMinutes(classGroup.endTime);

    // Calculate row position (each slot is slotDurationMinutes)
    const rowStart = Math.floor((startMinutes - gridStartTime) / this.slotDurationMinutes) + 2; // +2 for header row
    const rowEnd = Math.floor((endMinutes - gridStartTime) / this.slotDurationMinutes) + 2;

    return { rowStart, rowEnd };
  }

  /**
   * Detect conflicts (overlapping class groups for the same truck on the same day)
   */
  private detectConflicts(classGroups: ClassGroup[]): Set<number> {
    const conflicts = new Set<number>();

    for (let i = 0; i < classGroups.length; i++) {
      for (let j = i + 1; j < classGroups.length; j++) {
        const a = classGroups[i];
        const b = classGroups[j];

        // Check if same truck and times overlap
        if (a.truckId && a.truckId === b.truckId) {
          const startA = this.timeToMinutes(a.startTime);
          const endA = this.timeToMinutes(a.endTime);
          const startB = this.timeToMinutes(b.startTime);
          const endB = this.timeToMinutes(b.endTime);

          // Overlap: startA < endB && startB < endA
          if (startA < endB && startB < endA) {
            conflicts.add(a.id);
            conflicts.add(b.id);
          }
        }
      }
    }

    return conflicts;
  }

  /**
   * Convert "HH:mm:ss" to minutes since midnight
   */
  private timeToMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
  }

  /**
   * Format time as "HH:MM AM/PM"
   */
  private formatTime(hour: number, minute: number): string {
    const period = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minute.toString().padStart(2, '0')} ${period}`;
  }

  /**
   * Get day number from day name
   */
  private getDayNumber(dayName: string): number {
    const dayMap: Record<string, number> = {
      'Monday': 1,
      'Tuesday': 2,
      'Wednesday': 3,
      'Thursday': 4,
      'Friday': 5,
    };
    return dayMap[dayName] || 1;
  }

  /**
   * Get CSS grid style for a block
   */
  protected getBlockStyle(block: ScheduleBlock): { [key: string]: string } {
    return {
      'grid-column': `${block.column} / span 1`,
      'grid-row': `${block.rowStart} / span ${block.rowEnd - block.rowStart}`,
    };
  }

  /**
   * Handle block click - navigate to edit
   */
  protected onBlockClick(classGroup: ClassGroup): void {
    this.router.navigate(['/class-groups', classGroup.id]);
  }

  /**
   * Get class groups for a specific day and time slot
   */
  protected getBlocksForSlot(day: string, slotIndex: number): ScheduleBlock[] {
    const dayNum = this.getDayNumber(day);
    return this.scheduleBlocks().filter(block => {
      return block.column === dayNum + 1 &&
        block.rowStart <= slotIndex + 2 &&
        block.rowEnd > slotIndex + 2;
    });
  }
}
