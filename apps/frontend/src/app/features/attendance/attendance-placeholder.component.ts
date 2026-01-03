import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-attendance-placeholder',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="container mx-auto p-6">
      <div class="card bg-base-100 shadow-xl">
        <div class="card-body">
          <h2 class="card-title text-2xl mb-4">✅ Attendance</h2>
          <div class="alert alert-info">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              class="stroke-current shrink-0 w-6 h-6">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <span>This feature is coming soon. Attendance tracking will be available here.</span>
          </div>
          <p class="mt-4 text-base-content/70">
            This section will allow you to:
          </p>
          <ul class="list-disc list-inside mt-2 space-y-1 text-base-content/70">
            <li>Record attendance per student per class session</li>
            <li>View attendance history for students and class groups</li>
            <li>Correct attendance records with audit trail</li>
            <li>Track attendance patterns and trends</li>
          </ul>
        </div>
      </div>
    </div>
  `,
})
export class AttendancePlaceholderComponent {}




