import { Component, input, computed, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

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
            console.warn(`[StudentAvatar] Failed to load image for ${this.firstName()} ${this.lastName()}: ${url}`);
        }
    }

    /**
     * Reset error state when URL changes
     */
    protected resetError(): void {
        this.imageLoadFailed.set(false);
    }
}
