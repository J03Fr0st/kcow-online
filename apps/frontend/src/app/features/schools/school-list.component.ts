import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed, type WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SchoolService, type School } from '@core/services/school.service';
import { ModalService } from '@core/services/modal.service';
import { NotificationService } from '@core/services/notification.service';

interface ColumnConfig {
    key: keyof School;
    label: string;
    visible: WritableSignal<boolean>;
}

@Component({
    selector: 'app-school-list',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './school-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchoolListComponent implements OnInit {
    protected schoolService = inject(SchoolService);
    private modalService = inject(ModalService);
    private notificationService = inject(NotificationService);

    // Column visibility state
    protected readonly columns: ColumnConfig[] = [
        { key: 'name', label: 'Name', visible: signal(true) },
        { key: 'shortName', label: 'Short Name', visible: signal(false) },
        { key: 'contactPerson', label: 'Contact', visible: signal(true) },
        { key: 'address', label: 'Address', visible: signal(true) },
        { key: 'phone', label: 'Phone', visible: signal(true) },
        { key: 'email', label: 'Email', visible: signal(false) },
        { key: 'language', label: 'Lang', visible: signal(true) },
        { key: 'visitDay', label: 'Visit Day', visible: signal(false) },
    ];

    protected visibleColumns = computed(() => this.columns.filter(col => col.visible()));

    // Column dropdown state
    protected showColumnMenu = signal<boolean>(false);

    // Local UI state
    protected sortAsc = signal<boolean>(true);
    protected processingIds = signal<Set<number>>(new Set());

    // Computed sorted schools
    protected sortedSchools = computed(() => {
        const schools = this.schoolService.schools();
        const asc = this.sortAsc();

        return [...schools].sort((a, b) => {
            const nameA = a.name.toLowerCase();
            const nameB = b.name.toLowerCase();

            if (nameA < nameB) return asc ? -1 : 1;
            if (nameA > nameB) return asc ? 1 : -1;
            return 0;
        });
    });

    ngOnInit(): void {
        this.schoolService.getSchools().subscribe();
    }

    protected toggleSort(): void {
        this.sortAsc.update(val => !val);
    }

    protected toggleColumnMenu(): void {
        this.showColumnMenu.update(val => !val);
    }

    protected toggleColumnVisibility(column: ColumnConfig): void {
        column.visible.update((v: boolean) => !v);
    }

    protected getCellValue(school: School, key: keyof School): string {
        const value = school[key];
        if (value === null || value === undefined) return '-';
        if (typeof value === 'boolean') return value ? '✓' : '✗';
        return String(value);
    }

    /**
     * Deactivate a school after confirmation
     */
    protected async onDeactivateSchool(school: School): Promise<void> {
        const confirmed = await this.modalService.confirm({
            title: 'Deactivate School',
            message: `Are you sure you want to deactivate "${school.name}"? It will no longer be available for new student or class assignments.`,
            confirmText: 'Deactivate',
            cancelText: 'Cancel',
            confirmClass: 'btn-error',
            size: 'sm'
        });

        if (confirmed) {
            this.setProcessing(school.id, true);
            this.schoolService.deactivateSchool(school.id, school).subscribe({
                next: () => {
                    this.notificationService.success('School deactivated successfully');
                    this.setProcessing(school.id, false);
                },
                error: (err) => {
                    this.notificationService.error(
                        err.detail || 'Failed to deactivate school',
                        'Error'
                    );
                    this.setProcessing(school.id, false);
                }
            });
        }
    }

    /**
     * Reactivate a school after confirmation
     */
    protected async onReactivateSchool(school: School): Promise<void> {
        const confirmed = await this.modalService.confirm({
            title: 'Reactivate School',
            message: `Are you sure you want to reactivate "${school.name}"? It will become available for new student or class assignments.`,
            confirmText: 'Reactivate',
            cancelText: 'Cancel',
            confirmClass: 'btn-success',
            size: 'sm'
        });

        if (confirmed) {
            this.setProcessing(school.id, true);
            this.schoolService.reactivateSchool(school.id, school).subscribe({
                next: () => {
                    this.notificationService.success('School reactivated successfully');
                    this.setProcessing(school.id, false);
                },
                error: (err) => {
                    this.notificationService.error(
                        err.detail || 'Failed to reactivate school',
                        'Error'
                    );
                    this.setProcessing(school.id, false);
                }
            });
        }
    }

    private setProcessing(id: number, isProcessing: boolean): void {
        this.processingIds.update(set => {
            const newSet = new Set(set);
            if (isProcessing) {
                newSet.add(id);
            } else {
                newSet.delete(id);
            }
            return newSet;
        });
    }
}