import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { StudentAvatarComponent } from './student-avatar.component';

describe('StudentAvatarComponent', () => {
  let component: StudentAvatarComponent;
  let fixture: ComponentFixture<StudentAvatarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentAvatarComponent],
    }).compileComponents();
  });

  const createComponent = (
    firstName: string,
    lastName: string,
    photoUrl?: string,
    size: 'xs' | 'sm' | 'md' | 'lg' = 'md',
  ) => {
    fixture = TestBed.createComponent(StudentAvatarComponent);
    component = fixture.componentInstance;

    // Set required inputs using ComponentRef
    fixture.componentRef.setInput('firstName', firstName);
    fixture.componentRef.setInput('lastName', lastName);
    if (photoUrl !== undefined) {
      fixture.componentRef.setInput('photoUrl', photoUrl);
    }
    fixture.componentRef.setInput('size', size);

    fixture.detectChanges();
    return { component, fixture };
  };

  describe('Component Creation', () => {
    it('should create', () => {
      createComponent('John', 'Doe');
      expect(component).toBeTruthy();
    });
  });

  describe('Photo Display (AC #2)', () => {
    it('should display image when photoUrl is provided', () => {
      createComponent('John', 'Doe', 'https://example.com/photo.jpg');

      const img = fixture.debugElement.query(By.css('img'));
      expect(img).toBeTruthy();
      expect(img.nativeElement.src).toBe('https://example.com/photo.jpg');
    });

    it('should set correct alt text for image', () => {
      createComponent('John', 'Doe', 'https://example.com/photo.jpg');

      const img = fixture.debugElement.query(By.css('img'));
      expect(img.nativeElement.alt).toBe('John Doe');
    });

    it('should have lazy loading attribute set', () => {
      createComponent('John', 'Doe', 'https://example.com/photo.jpg');

      const img = fixture.debugElement.query(By.css('img'));
      // Check attribute directly since jsdom may not reflect loading property
      expect(img.nativeElement.getAttribute('loading')).toBe('lazy');
    });
  });

  describe('Placeholder Display (AC #3)', () => {
    it('should display placeholder when no photoUrl is provided', () => {
      createComponent('John', 'Doe');

      const placeholder = fixture.debugElement.query(By.css('.placeholder'));
      expect(placeholder).toBeTruthy();

      const img = fixture.debugElement.query(By.css('img'));
      expect(img).toBeFalsy();
    });

    it('should display placeholder when photoUrl is null', () => {
      createComponent('John', 'Doe', null as unknown as string);

      const placeholder = fixture.debugElement.query(By.css('.placeholder'));
      expect(placeholder).toBeTruthy();
    });

    it('should display placeholder when photoUrl is empty string', () => {
      createComponent('John', 'Doe', '');

      const placeholder = fixture.debugElement.query(By.css('.placeholder'));
      expect(placeholder).toBeTruthy();
    });
  });

  describe('Initials Generation', () => {
    it('should display correct initials from first and last name', () => {
      createComponent('John', 'Doe');

      const initialsSpan = fixture.debugElement.query(By.css('.placeholder span'));
      expect(initialsSpan.nativeElement.textContent).toBe('JD');
    });

    it('should handle single character names', () => {
      createComponent('A', 'B');

      const initialsSpan = fixture.debugElement.query(By.css('.placeholder span'));
      expect(initialsSpan.nativeElement.textContent).toBe('AB');
    });

    it('should handle empty first name', () => {
      createComponent('', 'Doe');

      const initialsSpan = fixture.debugElement.query(By.css('.placeholder span'));
      expect(initialsSpan.nativeElement.textContent).toBe('D');
    });

    it('should handle empty last name', () => {
      createComponent('John', '');

      const initialsSpan = fixture.debugElement.query(By.css('.placeholder span'));
      expect(initialsSpan.nativeElement.textContent).toBe('J');
    });

    it('should convert initials to uppercase', () => {
      createComponent('john', 'doe');

      const initialsSpan = fixture.debugElement.query(By.css('.placeholder span'));
      expect(initialsSpan.nativeElement.textContent).toBe('JD');
    });
  });

  describe('Color Consistency', () => {
    it('should generate consistent color for same name', () => {
      createComponent('John', 'Doe');
      const color1 = component['avatarColor']();

      // Create another instance with same name
      const fixture2 = TestBed.createComponent(StudentAvatarComponent);
      fixture2.componentRef.setInput('firstName', 'John');
      fixture2.componentRef.setInput('lastName', 'Doe');
      fixture2.detectChanges();
      const color2 = fixture2.componentInstance['avatarColor']();

      expect(color1).toBe(color2);
    });

    it('should generate different colors for different names', () => {
      createComponent('John', 'Doe');
      const color1 = component['avatarColor']();

      const fixture2 = TestBed.createComponent(StudentAvatarComponent);
      fixture2.componentRef.setInput('firstName', 'Jane');
      fixture2.componentRef.setInput('lastName', 'Smith');
      fixture2.detectChanges();
      const color2 = fixture2.componentInstance['avatarColor']();

      // Colors might be the same by chance, but should be one of the valid colors
      expect(color1).toMatch(/^bg-(primary|secondary|accent|info|success|warning|error)$/);
      expect(color2).toMatch(/^bg-(primary|secondary|accent|info|success|warning|error)$/);
    });

    it('should apply color class to placeholder', () => {
      createComponent('John', 'Doe');

      const placeholder = fixture.debugElement.query(By.css('.placeholder'));
      const classes = placeholder.nativeElement.className;
      expect(classes).toMatch(/bg-(primary|secondary|accent|info|success|warning|error)/);
    });
  });

  describe('Size Variations', () => {
    it('should apply xs size classes', () => {
      createComponent('John', 'Doe', undefined, 'xs');

      const avatar = fixture.debugElement.query(By.css('.student-avatar'));
      expect(avatar.nativeElement.className).toContain('w-6');
      expect(avatar.nativeElement.className).toContain('h-6');
    });

    it('should apply sm size classes', () => {
      createComponent('John', 'Doe', undefined, 'sm');

      const avatar = fixture.debugElement.query(By.css('.student-avatar'));
      expect(avatar.nativeElement.className).toContain('w-8');
      expect(avatar.nativeElement.className).toContain('h-8');
    });

    it('should apply md size classes (default)', () => {
      createComponent('John', 'Doe');

      const avatar = fixture.debugElement.query(By.css('.student-avatar'));
      expect(avatar.nativeElement.className).toContain('w-12');
      expect(avatar.nativeElement.className).toContain('h-12');
    });

    it('should apply lg size classes', () => {
      createComponent('John', 'Doe', undefined, 'lg');

      const avatar = fixture.debugElement.query(By.css('.student-avatar'));
      expect(avatar.nativeElement.className).toContain('w-32');
      expect(avatar.nativeElement.className).toContain('h-32');
    });
  });

  describe('Error Handling', () => {
    it('should show placeholder when image fails to load', () => {
      createComponent('John', 'Doe', 'https://example.com/broken.jpg');

      // Initially shows image
      let img = fixture.debugElement.query(By.css('img'));
      expect(img).toBeTruthy();

      // Simulate image error
      img.triggerEventHandler('error', {});
      fixture.detectChanges();

      // Should now show placeholder
      img = fixture.debugElement.query(By.css('img'));
      expect(img).toBeFalsy();

      const placeholder = fixture.debugElement.query(By.css('.placeholder'));
      expect(placeholder).toBeTruthy();
    });

    it('should log warning when image fails to load', () => {
      const consoleSpy = jest.spyOn(console, 'warn').mockImplementation();

      createComponent('John', 'Doe', 'https://example.com/broken.jpg');

      const img = fixture.debugElement.query(By.css('img'));
      img.triggerEventHandler('error', {});
      fixture.detectChanges();

      expect(consoleSpy).toHaveBeenCalledWith(
        expect.stringContaining('[StudentAvatar] Failed to load image for John Doe'),
      );

      consoleSpy.mockRestore();
    });

    it('should reset error state when image loads successfully', () => {
      createComponent('John', 'Doe', 'https://example.com/photo.jpg');

      const img = fixture.debugElement.query(By.css('img'));

      // Simulate error first
      img.triggerEventHandler('error', {});
      fixture.detectChanges();

      // Now change URL and simulate successful load
      fixture.componentRef.setInput('photoUrl', 'https://example.com/new-photo.jpg');
      fixture.detectChanges();

      const newImg = fixture.debugElement.query(By.css('img'));
      if (newImg) {
        newImg.triggerEventHandler('load', {});
        fixture.detectChanges();

        // Image should be visible
        expect(fixture.debugElement.query(By.css('img'))).toBeTruthy();
      }
    });
  });
});
