import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Meta, Title } from '@angular/platform-browser';
import { provideRouter, Router, type Routes } from '@angular/router';
import { PageMetadataService } from './page-metadata.service';

@Component({ template: '' })
class DummyComponent {}

describe('PageMetadataService', () => {
  let service: PageMetadataService;
  let router: Router;
  let titleService: Title;
  let metaService: Meta;

  const testRoutes: Routes = [
    {
      path: '',
      component: DummyComponent,
      children: [
        {
          path: 'dashboard',
          component: DummyComponent,
          data: {
            title: 'Dashboard',
            description: 'Dashboard page description',
            keywords: 'dashboard, metrics, analytics',
          },
        },
        {
          path: 'profile',
          component: DummyComponent,
          data: {
            title: 'Profile',
            description: 'User profile page',
            ogTitle: 'User Profile',
            ogDescription: 'View and edit your profile',
            ogImage: 'https://example.com/profile.jpg',
          },
        },
      ],
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PageMetadataService, provideRouter(testRoutes)],
    });

    service = TestBed.inject(PageMetadataService);
    router = TestBed.inject(Router);
    titleService = TestBed.inject(Title);
    metaService = TestBed.inject(Meta);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should extract metadata from route data', async () => {
    await router.navigate(['/dashboard']);

    const metadata = service.metadata();
    expect(metadata.title).toBe('Dashboard');
    expect(metadata.description).toBe('Dashboard page description');
    expect(metadata.keywords).toBe('dashboard, metrics, analytics');
  });

  it('should update document title on navigation', async () => {
    const setTitleSpy = jest.spyOn(titleService, 'setTitle');

    await router.navigate(['/dashboard']);

    expect(setTitleSpy).toHaveBeenCalledWith('KCOW — Dashboard');
  });

  it('should update meta description', async () => {
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    await router.navigate(['/dashboard']);

    expect(updateTagSpy).toHaveBeenCalledWith({
      name: 'description',
      content: 'Dashboard page description',
    });
  });

  it('should update meta keywords', async () => {
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    await router.navigate(['/dashboard']);

    expect(updateTagSpy).toHaveBeenCalledWith({
      name: 'keywords',
      content: 'dashboard, metrics, analytics',
    });
  });

  it('should update Open Graph tags', async () => {
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    await router.navigate(['/profile']);

    expect(updateTagSpy).toHaveBeenCalledWith({
      property: 'og:title',
      content: 'User Profile',
    });

    expect(updateTagSpy).toHaveBeenCalledWith({
      property: 'og:description',
      content: 'View and edit your profile',
    });

    expect(updateTagSpy).toHaveBeenCalledWith({
      property: 'og:image',
      content: 'https://example.com/profile.jpg',
    });
  });

  it('should compute page title signal', async () => {
    await router.navigate(['/dashboard']);

    expect(service.pageTitle()).toBe('Dashboard');
  });

  it('should manually set title', () => {
    const setTitleSpy = jest.spyOn(titleService, 'setTitle');

    service.setTitle('Custom Title');

    expect(setTitleSpy).toHaveBeenCalledWith('KCOW — Custom Title');
  });

  it('should manually set title without app name', () => {
    const setTitleSpy = jest.spyOn(titleService, 'setTitle');

    service.setTitle('Custom Title', false);

    expect(setTitleSpy).toHaveBeenCalledWith('Custom Title');
  });

  it('should manually set meta tag', () => {
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    service.setMetaTag('author', 'John Doe');

    expect(updateTagSpy).toHaveBeenCalledWith({
      name: 'author',
      content: 'John Doe',
    });
  });

  it('should manually set Open Graph tag', () => {
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    service.setOgTag('type', 'website');

    expect(updateTagSpy).toHaveBeenCalledWith({
      property: 'og:type',
      content: 'website',
    });
  });

  it('should remove meta tag', () => {
    const removeTagSpy = jest.spyOn(metaService, 'removeTag');

    service.removeMetaTag('keywords');

    expect(removeTagSpy).toHaveBeenCalledWith("name='keywords'");
  });

  it('should set complete metadata at once', () => {
    const setTitleSpy = jest.spyOn(titleService, 'setTitle');
    const updateTagSpy = jest.spyOn(metaService, 'updateTag');

    const metadata = {
      title: 'Complete Metadata',
      description: 'Full metadata test',
      keywords: 'test, metadata',
      ogTitle: 'OG Complete',
    };

    service.setMetadata(metadata);

    expect(setTitleSpy).toHaveBeenCalledWith('KCOW — Complete Metadata');
    expect(updateTagSpy).toHaveBeenCalledWith({
      name: 'description',
      content: 'Full metadata test',
    });
  });

  it('should use default app title when no route title exists', async () => {
    const setTitleSpy = jest.spyOn(titleService, 'setTitle');

    await router.navigate(['/']);

    expect(setTitleSpy).toHaveBeenCalledWith('KCOW');
  });

  it('should handle routes without metadata', async () => {
    await TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        PageMetadataService,
        provideRouter([
          {
            path: 'no-metadata',
            component: DummyComponent,
          },
        ]),
      ],
    });

    service = TestBed.inject(PageMetadataService);
    router = TestBed.inject(Router);
    titleService = TestBed.inject(Title);

    const setTitleSpy = jest.spyOn(titleService, 'setTitle');

    await router.navigate(['/no-metadata']);

    const metadata = service.metadata();
    expect(Object.keys(metadata).length).toBe(0);
    expect(setTitleSpy).toHaveBeenCalledWith('KCOW');
  });
});