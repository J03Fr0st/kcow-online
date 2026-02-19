import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

type AvatarSize = 'xs' | 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-student-avatar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './student-avatar.component.html',
  styleUrls: ['./student-avatar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentAvatarComponent {
  // Inputs
  readonly photoUrl = input<string | null | undefined>(null);
  readonly firstName = input.required<string>();
  readonly lastName = input.required<string>();
  readonly size = input<AvatarSize>('md');

  // Internal state for image error handling
  protected imageLoadFailed = signal<boolean>(false);

  // Computed size classes
  protected readonly sizeClasses = computed(() => {
    const sizeMap: Record<AvatarSize, string> = {
      xs: 'w-6 h-6 text-xs',
      sm: 'w-8 h-8 text-sm',
      md: 'w-12 h-12 text-base',
      lg: 'w-32 h-32 text-4xl',
    };
    return sizeMap[this.size()];
  });

  /**
   * Computed initials from first and last name
   */
  protected readonly initials = computed(() => {
    const first = (this.firstName() || '')[0] || '';
    const last = (this.lastName() || '')[0] || '';
    return (first + last).toUpperCase();
  });

  /**
   * Resolved image source — handles URLs, data URIs, and raw base64 with OLE headers.
   * Legacy MS Access exports wrap images in an OLE header:
   *   12 bytes fixed + UTF-16LE extension string, then actual image data.
   * We scan for JPEG/PNG/BMP signatures and build a proper data URI.
   */
  protected readonly resolvedPhotoSrc = computed(() => {
    const url = this.photoUrl();
    if (!url) return null;

    // Already a URL or data URI
    if (url.startsWith('http') || url.startsWith('data:')) return url;

    // Treat as base64 — legacy data has trailing metadata after newlines:
    //   <base64 of OLE+image>//9k=\n\n20150335 BOKAMOSO.jpg\njpg
    // Split on newlines FIRST to isolate actual base64 from filename/metadata.
    try {
      const base64Part = url
        .split('\n')
        .filter((line) => line.trim().length > 20)
        .join('');
      const cleanBase64 = base64Part.replace(/[^A-Za-z0-9+/=]/g, '');
      if (!cleanBase64) return null;
      const raw = Uint8Array.from(atob(cleanBase64), (c) => c.charCodeAt(0));

      // Check for image signatures at offset 0
      if (raw.length >= 2 && raw[0] === 0xff && raw[1] === 0xd8) {
        return `data:image/jpeg;base64,${cleanBase64}`;
      }
      if (raw.length >= 4 && raw[0] === 0x89 && raw[1] === 0x50) {
        return `data:image/png;base64,${cleanBase64}`;
      }
      if (raw.length >= 2 && raw[0] === 0x42 && raw[1] === 0x4d) {
        return `data:image/bmp;base64,${cleanBase64}`;
      }

      // Scan for image signature within OLE wrapper (first 512 bytes)
      const scanLimit = Math.min(raw.length - 1, 512);
      for (let i = 1; i < scanLimit; i++) {
        let mime: string | null = null;
        if (raw[i] === 0xff && raw[i + 1] === 0xd8) mime = 'image/jpeg';
        else if (raw[i] === 0x89 && raw[i + 1] === 0x50) mime = 'image/png';
        else if (raw[i] === 0x42 && raw[i + 1] === 0x4d) mime = 'image/bmp';

        if (mime) {
          const imageBytes = raw.slice(i);
          // Chunk btoa to avoid call-stack overflow on large arrays
          let binary = '';
          for (let j = 0; j < imageBytes.length; j++) {
            binary += String.fromCharCode(imageBytes[j]);
          }
          return `data:${mime};base64,${btoa(binary)}`;
        }
      }

      // No recognizable format found
      return null;
    } catch {
      return null;
    }
  });

  /**
   * Computed background color class based on name
   * Uses consistent hash of name to pick color
   */
  protected readonly avatarColor = computed(() => {
    const colors = [
      'bg-primary',
      'bg-secondary',
      'bg-accent',
      'bg-info',
      'bg-success',
      'bg-warning',
      'bg-error',
    ];

    const name = `${this.firstName()}${this.lastName()}`;
    const hash = name.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    const colorIndex = hash % colors.length;

    return colors[colorIndex];
  });

  /**
   * Handle image load error - logs warning for monitoring broken URLs
   */
  protected onImageError(): void {
    this.imageLoadFailed.set(true);
    const url = this.photoUrl();
    if (url) {
      console.warn(
        `[StudentAvatar] Failed to load image for ${this.firstName()} ${this.lastName()}: ${url}`,
      );
    }
  }

  /**
   * Reset error state when URL changes
   */
  protected resetError(): void {
    this.imageLoadFailed.set(false);
  }
}
