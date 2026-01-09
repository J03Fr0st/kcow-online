import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';
import { ClassGroupSelectComponent } from './class-group-select.component';
import { ClassGroupService } from '@core/services/class-group.service';

describe('ClassGroupSelectComponent', () => {
    let component: ClassGroupSelectComponent;
    let fixture: ComponentFixture<ClassGroupSelectComponent>;
    let classGroupServiceSpy: jasmine.SpyObj<ClassGroupService>;

    beforeEach(async () => {
        const spy = jasmine.createSpyObj('ClassGroupService', ['loadClassGroups']);
        spy.classGroups = signal([]); // Use signal instead of Observable

        await TestBed.configureTestingModule({
            imports: [ClassGroupSelectComponent, ReactiveFormsModule, NoopAnimationsModule],
            providers: [
                { provide: ClassGroupService, useValue: spy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ClassGroupSelectComponent);
        component = fixture.componentInstance;
        classGroupServiceSpy = TestBed.inject(ClassGroupService) as jasmine.SpyObj<ClassGroupService>;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have empty class groups when no school is selected', () => {
        component.schoolId = signal(null);
        fixture.detectChanges();
        expect(component['filteredClassGroups']).toEqual([]);
    });

    it('should call loadClassGroups when schoolId is set', () => {
        component.schoolId = signal(1);
        fixture.detectChanges();
        expect(classGroupServiceSpy.loadClassGroups).toHaveBeenCalledWith(1);
    });
});
