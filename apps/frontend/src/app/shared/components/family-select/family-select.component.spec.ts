import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FamilySelectComponent } from './family-select.component';
import { FamilyService, type Family } from '@core/services/family.service';
import { of } from 'rxjs';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';

describe('FamilySelectComponent', () => {
    let component: FamilySelectComponent;
    let fixture: ComponentFixture<FamilySelectComponent>;
    let mockFamilyService: {
        families: ReturnType<typeof signal<Family[]>>;
        getActiveFamilies: jest.Mock;
        getFamilyById: jest.Mock;
    };

    const mockFamilies: Family[] = [
        {
            id: 1,
            familyName: 'Smith Family',
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
            updatedAt: '2024-01-01T00:00:00Z',
            guardians: [
                {
                    id: 1,
                    familyId: 1,
                    firstName: 'John',
                    lastName: 'Smith',
                    relationship: 'Father',
                    phone: '555-1234',
                    email: 'john@example.com',
                    isPrimaryContact: true,
                    createdAt: '2024-01-01T00:00:00Z',
                    updatedAt: '2024-01-01T00:00:00Z',
                }
            ],
            studentCount: 2,
        },
        {
            id: 2,
            familyName: 'Johnson Family',
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
            updatedAt: '2024-01-01T00:00:00Z',
            guardians: [
                {
                    id: 2,
                    familyId: 2,
                    firstName: 'Jane',
                    lastName: 'Johnson',
                    relationship: 'Mother',
                    phone: '555-5678',
                    email: 'jane@example.com',
                    isPrimaryContact: true,
                    createdAt: '2024-01-01T00:00:00Z',
                    updatedAt: '2024-01-01T00:00:00Z',
                }
            ],
            studentCount: 0,
        },
        {
            id: 3,
            familyName: 'Williams Family',
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
            updatedAt: '2024-01-01T00:00:00Z',
            studentCount: 1,
        },
    ];

    beforeEach(async () => {
        mockFamilyService = {
            families: signal<Family[]>([]),
            getActiveFamilies: jest.fn().mockImplementation((search) => {
                if (!search) return of(mockFamilies);
                const query = search.toLowerCase();
                return of(mockFamilies.filter(f => 
                    f.familyName.toLowerCase().includes(query) ||
                    f.guardians?.some(g => g.firstName.toLowerCase().includes(query) || g.lastName.toLowerCase().includes(query))
                ));
            }),
            getFamilyById: jest.fn().mockImplementation((id) => {
                const family = mockFamilies.find(f => f.id === id);
                return of(family || mockFamilies[0]);
            }),
        };

        await TestBed.configureTestingModule({
            imports: [FamilySelectComponent, ReactiveFormsModule],
            providers: [
                { provide: FamilyService, useValue: mockFamilyService },
            ],
        }).compileComponents();

        fixture = TestBed.createComponent(FamilySelectComponent);
        component = fixture.componentInstance;
    });

    describe('Component Creation', () => {
        it('should create', () => {
            fixture.detectChanges();
            expect(component).toBeTruthy();
        });

        it('should load families on init', () => {
            fixture.detectChanges();
            expect(mockFamilyService.getActiveFamilies).toHaveBeenCalled();
        });

        it('should render family label with optional indicator', () => {
            fixture.detectChanges();
            const label = fixture.debugElement.query(By.css('.label-text'));
            const optionalIndicator = fixture.debugElement.query(By.css('.label-text-alt'));

            expect(label.nativeElement.textContent).toContain('Family');
            expect(optionalIndicator.nativeElement.textContent).toContain('Optional');
        });

        it('should render search input', () => {
            fixture.detectChanges();
            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input).toBeTruthy();
            expect(input.nativeElement.placeholder).toBe('Search families...');
        });
    });

    describe('Search Functionality', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should display No Family option, all families plus "Create New" option when no search query', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a'));
            // 1 "No Family" + 3 families + 1 "Create New Family" option
            expect(items.length).toBe(5);
        });

        it('should filter families based on family name search query with debounce', fakeAsync(() => {
            const input = fixture.debugElement.query(By.css('input[type="text"]'));

            input.nativeElement.value = 'Smith';
            input.nativeElement.dispatchEvent(new Event('input'));

            tick(300);
            fixture.detectChanges();

            expect(mockFamilyService.getActiveFamilies).toHaveBeenCalledWith('Smith');
            
            const familyItems = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            // Should show "No Family" + 1 Smith family + "Create New"
            expect(familyItems.length).toBe(3);
        }));

        it('should filter families based on guardian name search query with debounce', fakeAsync(() => {
            const input = fixture.debugElement.query(By.css('input[type="text"]'));

            input.nativeElement.value = 'John';
            input.nativeElement.dispatchEvent(new Event('input'));

            tick(300);
            fixture.detectChanges();

            expect(mockFamilyService.getActiveFamilies).toHaveBeenCalledWith('John');
            
            const familyItems = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            // Should match Smith family by guardian name
            expect(familyItems.length).toBe(3); // No Family + Smith family + Create New
        }));
    });

    describe('Enhanced Display Features', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should display primary contact name in dropdown', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            // First item should be "No Family", second should be Smith Family with guardian info
            const smithFamilyItem = items[1];
            expect(smithFamilyItem.nativeElement.textContent).toContain('Smith Family');
            expect(smithFamilyItem.nativeElement.textContent).toContain('John Smith');
        });

        it('should display student count badge', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            // Smith family has 2 students
            const smithFamilyItem = items[1];
            expect(smithFamilyItem.nativeElement.textContent).toContain('2 students');
        });
    });

    describe('Selection Functionality', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should select a family when clicked', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            // Skip "No Family" option (index 0), select first real family (index 1)
            items[1].triggerEventHandler('click', null);
            fixture.detectChanges();

            expect(component.familyId()).toBe(1);
        });

        it('should display selected family name in input', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.text-primary):not(.disabled)'));
            items[1].triggerEventHandler('click', null);
            fixture.detectChanges();

            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.value).toBe('Smith Family');
        });
    });

    describe('ControlValueAccessor', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should implement writeValue', () => {
            component.writeValue(2);
            fixture.detectChanges();

            expect(component.familyId()).toBe(2);
        });

        it('should update display name when writeValue is called', () => {
            component.writeValue(2);
            fixture.detectChanges();

            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.value).toBe('Johnson Family');
        });

        it('should handle null writeValue', () => {
            component.writeValue(null);
            fixture.detectChanges();

            expect(component.familyId()).toBeNull();
            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.value).toBe('');
        });
    });
    
    describe('Pre-selection in Edit Mode', () => {
        it('should fetch family if not in list', fakeAsync(() => {
            // Setup mock to simulate family not in initial list (if initial list was limited, but our mock returns all)
            // But we can verify getFamilyById is called if we clear filteredFamilies?
            // Actually writeValue handles it.
            
            // If we write a value:
            component.writeValue(99); // ID not in mockFamilies
            // It calls getFamilyById
            expect(mockFamilyService.getFamilyById).toHaveBeenCalledWith(99);
        }));
    });
});