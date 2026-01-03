import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FamilyListComponent } from './family-list.component';
import { FamilyService } from '@core/services/family.service';
import { ModalService } from '@core/services/modal.service';
import { NotificationService } from '@core/services/notification.service';
import { of, throwError, Subject } from 'rxjs';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';

describe('FamilyListComponent', () => {
    let component: FamilyListComponent;
    let fixture: ComponentFixture<FamilyListComponent>;
    let mockFamilyService: any;
    let mockModalService: any;
    let mockNotificationService: any;

    const mockFamilies = [
        {
            id: 1,
            familyName: 'Smith Family',
            primaryBillingContactId: 1,
            notes: 'Some notes',
            isActive: true,
            createdAt: '2024-01-01',
            updatedAt: '2024-01-01',
            guardians: [
                { id: 1, firstName: 'John', lastName: 'Smith', isPrimaryContact: true }
            ]
        },
        {
            id: 2,
            familyName: 'Johnson Family',
            primaryBillingContactId: null,
            notes: null,
            isActive: false,
            createdAt: '2024-01-01',
            updatedAt: '2024-01-01',
            guardians: []
        },
    ];

    beforeEach(async () => {
        mockFamilyService = {
            families: signal([]),
            isLoading: signal(false),
            error: signal(null),
            getFamilies: jest.fn().mockReturnValue(of([])),
            deactivateFamily: jest.fn().mockReturnValue(of({ ...mockFamilies[0], isActive: false })),
            reactivateFamily: jest.fn().mockReturnValue(of({ ...mockFamilies[1], isActive: true })),
        };

        mockModalService = {
            confirm: jest.fn(),
        };

        mockNotificationService = {
            success: jest.fn(),
            error: jest.fn(),
        };

        await TestBed.configureTestingModule({
            imports: [FamilyListComponent],
            providers: [
                { provide: FamilyService, useValue: mockFamilyService },
                { provide: ModalService, useValue: mockModalService },
                { provide: NotificationService, useValue: mockNotificationService },
                provideRouter([]),
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(FamilyListComponent);
        component = fixture.componentInstance;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call getFamilies on init', () => {
        fixture.detectChanges();
        expect(mockFamilyService.getFamilies).toHaveBeenCalled();
    });

    it('should render table rows when families are present', () => {
        mockFamilyService.families.set(mockFamilies);
        fixture.detectChanges();

        const rows = fixture.debugElement.queryAll(By.css('tbody tr'));
        expect(rows.length).toBe(2);
        expect(rows[0].nativeElement.textContent).toContain('Smith Family');
        expect(rows[1].nativeElement.textContent).toContain('Johnson Family');
    });

    it('should render empty state when no families', () => {
        mockFamilyService.families.set([]);
        fixture.detectChanges();

        const emptyState = fixture.debugElement.query(By.css('.empty-state'));
        expect(emptyState).toBeTruthy();
        expect(emptyState.nativeElement.textContent).toContain('No families found');
    });

    it('should show loading spinner when loading', () => {
        mockFamilyService.isLoading.set(true);
        fixture.detectChanges();

        const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
        expect(spinner).toBeTruthy();
    });

    it('should display primary contact name when available', () => {
        mockFamilyService.families.set([mockFamilies[0]]);
        fixture.detectChanges();

        const rowText = fixture.debugElement.query(By.css('tbody tr')).nativeElement.textContent;
        expect(rowText).toContain('John Smith');
    });

    it('should display - when no primary contact', () => {
        mockFamilyService.families.set([mockFamilies[1]]);
        fixture.detectChanges();

        const row = fixture.debugElement.query(By.css('tbody tr'));
        const cells = row.queryAll(By.css('td'));
        // Primary contact column (second column)
        expect(cells[1].nativeElement.textContent).toContain('-');
    });

    it('should toggle sort direction when header is clicked', () => {
        mockFamilyService.families.set(mockFamilies);
        fixture.detectChanges();

        const nameHeader = fixture.debugElement.query(By.css('thead th'));

        // Initial state: Ascending (Johnson Family, then Smith Family alphabetically)
        let firstRowName = fixture.debugElement.query(By.css('tbody tr td')).nativeElement.textContent;
        expect(firstRowName).toContain('Johnson Family');

        // Click to toggle to Descending
        nameHeader.triggerEventHandler('click', null);
        fixture.detectChanges();

        firstRowName = fixture.debugElement.query(By.css('tbody tr td')).nativeElement.textContent;
        expect(firstRowName).toContain('Smith Family');
    });

    it('should show error alert when error occurs', () => {
        mockFamilyService.error.set({ title: 'Server Error', detail: 'Failed to fetch data' });
        fixture.detectChanges();

        const alert = fixture.debugElement.query(By.css('[role="alert"]'));
        expect(alert).toBeTruthy();
        expect(alert.nativeElement.textContent).toContain('Server Error');
        expect(alert.nativeElement.textContent).toContain('Failed to fetch data');
    });

    // Deactivate/Reactivate tests
    it('should show deactivate button for active families only', () => {
        mockFamilyService.families.set(mockFamilies);
        fixture.detectChanges();

        // Smith Family is active - should have deactivate button
        const familyARow = fixture.debugElement.queryAll(By.css('tbody tr'))[0];
        const deactivateButtonA = familyARow.query(By.css('button[aria-label="Deactivate family"]'));
        expect(deactivateButtonA).toBeTruthy();

        // Smith Family should NOT have reactivate button
        const reactivateButtonA = familyARow.query(By.css('button[aria-label="Reactivate family"]'));
        expect(reactivateButtonA).toBeFalsy();

        // Johnson Family is inactive - should have reactivate button
        const familyBRow = fixture.debugElement.queryAll(By.css('tbody tr'))[1];
        const reactivateButtonB = familyBRow.query(By.css('button[aria-label="Reactivate family"]'));
        expect(reactivateButtonB).toBeTruthy();
    });

    it('should open confirmation dialog when deactivate button is clicked', async () => {
        mockModalService.confirm.mockResolvedValue(false);
        mockFamilyService.families.set([mockFamilies[0]]);
        fixture.detectChanges();

        const deactivateButton = fixture.debugElement.query(By.css('button[aria-label="Deactivate family"]'));
        deactivateButton.triggerEventHandler('click', null);

        await fixture.whenStable();

        expect(mockModalService.confirm).toHaveBeenCalledWith({
            title: 'Deactivate Family',
            message: 'Are you sure you want to deactivate "Smith Family"? It will no longer be available for new student assignments.',
            confirmText: 'Deactivate',
            cancelText: 'Cancel',
            confirmClass: 'btn-error',
            size: 'sm'
        });
    });

    it('should call deactivateFamily API when confirmed', async () => {
        mockModalService.confirm.mockResolvedValue(true);
        mockFamilyService.families.set([mockFamilies[0]]);
        fixture.detectChanges();

        const deactivateButton = fixture.debugElement.query(By.css('button[aria-label="Deactivate family"]'));
        deactivateButton.triggerEventHandler('click', null);

        await fixture.whenStable();

        expect(mockFamilyService.deactivateFamily).toHaveBeenCalledWith(1, mockFamilies[0]);
        expect(mockNotificationService.success).toHaveBeenCalledWith('Family deactivated successfully');
    });

    it('should call reactivateFamily API when confirmed', async () => {
        mockModalService.confirm.mockResolvedValue(true);
        mockFamilyService.families.set([mockFamilies[1]]);
        fixture.detectChanges();

        const reactivateButton = fixture.debugElement.query(By.css('button[aria-label="Reactivate family"]'));
        reactivateButton.triggerEventHandler('click', null);

        await fixture.whenStable();

        expect(mockFamilyService.reactivateFamily).toHaveBeenCalledWith(2, mockFamilies[1]);
        expect(mockNotificationService.success).toHaveBeenCalledWith('Family reactivated successfully');
    });

    it('should show error notification when deactivate fails', async () => {
        mockFamilyService.deactivateFamily.mockReturnValue(throwError(() => ({ detail: 'Network error' })));
        mockModalService.confirm.mockResolvedValue(true);
        mockFamilyService.families.set([mockFamilies[0]]);
        fixture.detectChanges();

        const deactivateButton = fixture.debugElement.query(By.css('button[aria-label="Deactivate family"]'));
        deactivateButton.triggerEventHandler('click', null);

        await fixture.whenStable();

        expect(mockNotificationService.error).toHaveBeenCalledWith('Network error', 'Error');
    });
});
