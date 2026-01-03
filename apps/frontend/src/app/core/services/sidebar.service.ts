import { Injectable, signal, inject, PLATFORM_ID, DestroyRef } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { fromEvent } from 'rxjs';
import { throttleTime } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly destroyRef = inject(DestroyRef);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

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

  private checkMobile(): void {
    if (this.isBrowser) {
      const isMobile = window.innerWidth < 1024; // lg breakpoint
      this.isMobile.set(isMobile);
      if (!isMobile) {
        this.isOpen.set(true); // Always open on desktop by default
      } else {
        this.isOpen.set(false); // Closed on mobile by default
      }
    }
  }
}