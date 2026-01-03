import { Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { fromEvent } from 'rxjs';
import { throttleTime } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
  isOpen = signal<boolean>(true);
  isMobile = signal<boolean>(false);

  constructor() {
    if (this.isBrowser && typeof window !== 'undefined') {
      // Initial check
      this.checkMobile();

      // Subscribe to resize events with throttling and automatic cleanup
      fromEvent(window, 'resize')
        .pipe(
          throttleTime(150), // Throttle resize events to improve performance
          takeUntilDestroyed(this.destroyRef),
        )
        .subscribe(() => this.checkMobile());
    }
  }

  toggle(): void {
    this.isOpen.set(!this.isOpen());
  }

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }
}
