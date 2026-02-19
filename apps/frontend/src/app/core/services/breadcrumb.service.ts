import { computed, Injectable, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { type ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs/operators';

export interface Breadcrumb {
  label: string;
  url: string;
  icon?: string;
}

export interface BreadcrumbData {
  breadcrumb?: string;
  breadcrumbIcon?: string;
  hideBreadcrumb?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class BreadcrumbService {
  private router = inject(Router);

  // Signal that tracks breadcrumbs from router
  private breadcrumbsSignal = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => {
        const route: ActivatedRoute | null = this.router.routerState.root;
        return this.buildBreadcrumbs(route);
      }),
    ),
    { initialValue: [] as Breadcrumb[] },
  );

  // Public signal for components
  breadcrumbs = computed(() => this.breadcrumbsSignal() || []);

  /**
   * Build breadcrumb trail from route hierarchy
   */
  private buildBreadcrumbs(
    route: ActivatedRoute | null,
    url = '',
    breadcrumbs: Breadcrumb[] = [],
  ): Breadcrumb[] {
    // Guard against null/undefined routes
    if (!route || !route.snapshot) {
      return breadcrumbs;
    }

    // Get route data
    const routeSnapshot = route.snapshot;
    const data = routeSnapshot.data as BreadcrumbData;

    // Skip if breadcrumb is hidden
    if (data?.hideBreadcrumb) {
      return breadcrumbs;
    }

    // Build URL for this segment
    const path = routeSnapshot.url.map((segment) => segment.path).join('/');
    const newUrl = path ? `${url}/${path}` : url;

    // Add breadcrumb if label exists in route data
    if (data?.breadcrumb) {
      const breadcrumb: Breadcrumb = {
        label: data.breadcrumb,
        url: newUrl || '/', // Use '/' for root route
        icon: data.breadcrumbIcon,
      };

      // Avoid duplicate breadcrumbs
      const exists = breadcrumbs.some((b) => b.url === breadcrumb.url);
      if (!exists) {
        breadcrumbs.push(breadcrumb);
      }
    }

    // Recursively build breadcrumbs for child routes
    if (route.firstChild) {
      return this.buildBreadcrumbs(route.firstChild, newUrl, breadcrumbs);
    }

    return breadcrumbs;
  }

  /**
   * Manually set breadcrumbs (useful for dynamic routes)
   */
  private customBreadcrumbs = signal<Breadcrumb[] | null>(null);

  setBreadcrumbs(breadcrumbs: Breadcrumb[]): void {
    this.customBreadcrumbs.set(breadcrumbs);
  }

  clearCustomBreadcrumbs(): void {
    this.customBreadcrumbs.set(null);
  }

  /**
   * Get breadcrumbs with custom override support
   */
  getBreadcrumbs = computed(() => {
    const custom = this.customBreadcrumbs();
    return custom || this.breadcrumbs();
  });

  /**
   * Get current page breadcrumb (last in chain)
   */
  currentPage = computed(() => {
    const crumbs = this.getBreadcrumbs();
    return crumbs.length > 0 ? crumbs[crumbs.length - 1] : null;
  });

  /**
   * Navigate to breadcrumb URL
   */
  navigateTo(breadcrumb: Breadcrumb): void {
    this.router.navigate([breadcrumb.url]);
  }
}
