import { computed, Injectable, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs/operators';

export interface PageMetadata {
  title?: string;
  description?: string;
  keywords?: string;
  ogTitle?: string;
  ogDescription?: string;
  ogImage?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PageMetadataService {
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private titleService = inject(Title);
  private metaService = inject(Meta);

  private readonly APP_TITLE = 'Angular Template';
  private readonly DEFAULT_SEPARATOR = ' | ';

  // Listen to navigation events and extract metadata
  private navigationEnd$ = this.router.events.pipe(
    filter((event) => event instanceof NavigationEnd),
    map(() => this.extractMetadata(this.activatedRoute.root)),
  );

  // Convert to signal
  private metadataSignal = toSignal(this.navigationEnd$, {
    initialValue: this.extractMetadata(this.activatedRoute.root),
  });

  // Public signal for current page metadata
  metadata = computed(() => this.metadataSignal() || {});

  // Computed page title
  pageTitle = computed(() => {
    const meta = this.metadata();
    return meta.title || this.APP_TITLE;
  });

  constructor() {
    // Auto-update document title and meta tags when metadata changes
    this.navigationEnd$.subscribe((metadata) => {
      this.updateDocumentMetadata(metadata);
    });
  }

  /**
   * Extract metadata from route hierarchy
   */
  private extractMetadata(route: ActivatedRoute): PageMetadata {
    let metadata: PageMetadata = {};

    // Traverse route tree to find metadata
    let currentRoute: ActivatedRoute | null = route;
    while (currentRoute) {
      // Check if snapshot exists before accessing data
      if (currentRoute.snapshot) {
        const routeMetadata = currentRoute.snapshot.data as PageMetadata;
        // Merge metadata (child routes override parent)
        metadata = { ...metadata, ...routeMetadata };
      }
      currentRoute = currentRoute.firstChild;
    }

    return metadata;
  }

  /**
   * Update document title and meta tags
   */
  private updateDocumentMetadata(metadata: PageMetadata): void {
    // Update title
    const title = metadata.title
      ? `${metadata.title}${this.DEFAULT_SEPARATOR}${this.APP_TITLE}`
      : this.APP_TITLE;
    this.titleService.setTitle(title);

    // Update meta description
    if (metadata.description) {
      this.metaService.updateTag({
        name: 'description',
        content: metadata.description,
      });
    }

    // Update meta keywords
    if (metadata.keywords) {
      this.metaService.updateTag({
        name: 'keywords',
        content: metadata.keywords,
      });
    }

    // Update Open Graph tags
    if (metadata.ogTitle) {
      this.metaService.updateTag({
        property: 'og:title',
        content: metadata.ogTitle,
      });
    }

    if (metadata.ogDescription) {
      this.metaService.updateTag({
        property: 'og:description',
        content: metadata.ogDescription,
      });
    }

    if (metadata.ogImage) {
      this.metaService.updateTag({
        property: 'og:image',
        content: metadata.ogImage,
      });
    }
  }

  /**
   * Manually set page title
   */
  setTitle(title: string, appendAppName = true): void {
    const fullTitle = appendAppName ? `${title}${this.DEFAULT_SEPARATOR}${this.APP_TITLE}` : title;
    this.titleService.setTitle(fullTitle);
  }

  /**
   * Manually set meta tag
   */
  setMetaTag(name: string, content: string): void {
    this.metaService.updateTag({ name, content });
  }

  /**
   * Manually set Open Graph tag
   */
  setOgTag(property: string, content: string): void {
    this.metaService.updateTag({ property: `og:${property}`, content });
  }

  /**
   * Remove meta tag
   */
  removeMetaTag(name: string): void {
    this.metaService.removeTag(`name='${name}'`);
  }

  /**
   * Set complete metadata at once
   */
  setMetadata(metadata: PageMetadata): void {
    this.updateDocumentMetadata(metadata);
  }
}
