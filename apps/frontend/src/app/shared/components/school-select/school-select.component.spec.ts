import { ComponentFixture, TestBed, fakeAsync, tick, flush } from '@angular/core/testing';
import { SchoolSelectComponent } from './school-select.component';
import { SchoolService, type School } from '@core/services/school.service';
import { of, Subject } from 'rxjs';
import { signal, ChangeDetectionStrategy } from '@angular/core';
import { By } from '@angular/platform-browser';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

describe('SchoolSelectComponent', () => {
    let component: SchoolSelectComponent;
    let fixture: ComponentFixture<SchoolSelectComponent>;
    let mockSchoolService: {
        schools: ReturnType<typeof signal<School[]>>;
        getActiveSchools: jest.Mock;
    };
    let schoolsSubject: Subject<School[]>;

    const mockSchools: School[] = [
        {
            id: 1,
            name: 'Primary School',
            shortName: 'PS',
            isActive: true,
            printInvoice: true,
            importFlag: false,
        },
        {
            id: 2,
            name: 'High School',
            shortName: 'HS',
            isActive: true,
            printInvoice: true,
            importFlag: false,
        },
        {
            id: 3,
            name: 'Academy of Excellence',
            shortName: 'AoE',
            isActive: true,
            printInvoice: true,
            importFlag: false,
        },
    ];

    beforeEach(async () => {
        schoolsSubject = new Subject<School[]>();
        mockSchoolService = {
            schools: signal<School[]>(mockSchools),
            getActiveSchools: jest.fn().mockReturnValue(schoolsSubject.asObservable()),
        };

        await TestBed.configureTestingModule({
            imports: [SchoolSelectComponent, ReactiveFormsModule],
            providers: [
                { provide: SchoolService, useValue: mockSchoolService },
            ],
        }).overrideComponent(SchoolSelectComponent, {
            set: { changeDetection: ChangeDetectionStrategy.Default }
        }).compileComponents();

        fixture = TestBed.createComponent(SchoolSelectComponent);
        component = fixture.componentInstance;
    });

    describe('Component Creation', () => {
        it('should create', () => {
            fixture.detectChanges();
            expect(component).toBeTruthy();
        });

        it('should load schools on init', () => {
            fixture.detectChanges();
            expect(mockSchoolService.getActiveSchools).toHaveBeenCalled();
        });

        it('should render school label with required indicator', () => {
            fixture.detectChanges();
            const label = fixture.debugElement.query(By.css('.label-text'));
            const requiredIndicator = fixture.debugElement.query(By.css('.text-error'));

            expect(label.nativeElement.textContent).toContain('School');
            expect(requiredIndicator.nativeElement.textContent).toContain('*');
        });

        it('should render search input', () => {
            fixture.detectChanges();
            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input).toBeTruthy();
            expect(input.nativeElement.placeholder).toBe('Search schools...');
        });
    });

    describe('Search Functionality', () => {
        beforeEach(() => {
            fixture.detectChanges();
            schoolsSubject.next(mockSchools);
            fixture.detectChanges();
        });

        it('should display all schools when no search query', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            expect(items.length).toBe(3);
        });

        it('should filter schools based on search query with debounce', fakeAsync(() => {
            const input = fixture.debugElement.query(By.css('input[type="text"]'));

            // Simulate typing
            input.nativeElement.value = 'Primary';
            input.nativeElement.dispatchEvent(new Event('input'));

            // After 300ms debounce
            tick(300);
            fixture.detectChanges();

            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            expect(items.length).toBe(1);
            expect(items[0].nativeElement.textContent).toContain('Primary School');
        }));

        it('should filter by school shortName', fakeAsync(() => {
            const input = fixture.debugElement.query(By.css('input[type="text"]'));

            input.nativeElement.value = 'HS';
            input.nativeElement.dispatchEvent(new Event('input'));

            tick(300);
            fixture.detectChanges();

            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            expect(items.length).toBe(1);
            expect(items[0].nativeElement.textContent).toContain('High School');
        }));

        it('should show "No schools found" when no matches', fakeAsync(() => {
            const input = fixture.debugElement.query(By.css('input[type="text"]'));

            input.nativeElement.value = 'xyz123';
            input.nativeElement.dispatchEvent(new Event('input'));

            tick(300);
            fixture.detectChanges();

            const noResults = fixture.debugElement.query(By.css('.disabled'));
            expect(noResults).toBeTruthy();
            expect(noResults.nativeElement.textContent).toContain('No schools found');
        }));
    });

    describe('Selection Functionality', () => {
        beforeEach(() => {
            fixture.detectChanges();
            schoolsSubject.next(mockSchools);
            fixture.detectChanges();
        });

        it('should select a school when clicked', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            items[0].triggerEventHandler('click', null);
            fixture.detectChanges();

            expect(component.schoolId()).toBe(1);
        });

        it('should display selected school name in input', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            items[0].triggerEventHandler('click', null);
            fixture.detectChanges();

            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.value).toBe('Primary School');
        });

        it('should highlight selected school in dropdown', () => {
            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            items[0].triggerEventHandler('click', null);
            fixture.detectChanges();

            const selectedItem = fixture.debugElement.query(By.css('.dropdown-content li a.active'));
            expect(selectedItem).toBeTruthy();
        });
    });

    describe('ControlValueAccessor', () => {
        beforeEach(() => {
            fixture.detectChanges();
            schoolsSubject.next(mockSchools);
            fixture.detectChanges();
        });

        it('should implement writeValue', () => {
            component.writeValue(2);
            fixture.detectChanges();

            expect(component.schoolId()).toBe(2);
        });

        it('should update display name when writeValue is called', () => {
            component.writeValue(2);
            fixture.detectChanges();

            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.value).toBe('High School');
        });

        it('should call onChange when school is selected', () => {
            const onChangeSpy = jest.fn();
            component.registerOnChange(onChangeSpy);

            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            items[0].triggerEventHandler('click', null);

            expect(onChangeSpy).toHaveBeenCalledWith(1);
        });

        it('should call onTouched when school is selected', () => {
            const onTouchedSpy = jest.fn();
            component.registerOnTouched(onTouchedSpy);

            const items = fixture.debugElement.queryAll(By.css('.dropdown-content li a:not(.disabled)'));
            items[0].triggerEventHandler('click', null);

            expect(onTouchedSpy).toHaveBeenCalled();
        });

        it('should disable input when setDisabledState is called with true', () => {
            component.setDisabledState(true);
            fixture.detectChanges();

            const input = fixture.debugElement.query(By.css('input[type="text"]'));
            expect(input.nativeElement.disabled).toBe(true);
        });
    });

    describe('Loading State', () => {
        it('should show loading message while fetching schools', () => {
            // override the mock for this specific test to not return immediately
            mockSchoolService.getActiveSchools.mockReturnValue(new Subject().asObservable());
            
            // Re-create component to trigger ngOnInit with new mock
            fixture = TestBed.createComponent(SchoolSelectComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();

            const loadingItem = fixture.debugElement.query(By.css('.disabled'));
            expect(loadingItem.nativeElement.textContent).toContain('Loading schools...');
        });
    });
});
