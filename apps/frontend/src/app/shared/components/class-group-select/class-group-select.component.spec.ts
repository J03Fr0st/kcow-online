import { signal } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ClassGroupService } from '@features/class-groups/data-access/class-group.service';
import { ClassGroupSelectComponent } from './class-group-select.component';

describe('ClassGroupSelectComponent', () => {
  let component: ClassGroupSelectComponent;
  let fixture: ComponentFixture<ClassGroupSelectComponent>;
  let classGroupServiceSpy: ClassGroupService;

  beforeEach(async () => {
    const spy = { loadClassGroups: jest.fn(), classGroups: signal([]), loading: signal(false) };

    await TestBed.configureTestingModule({
      imports: [ClassGroupSelectComponent, ReactiveFormsModule, NoopAnimationsModule],
      providers: [{ provide: ClassGroupService, useValue: spy }],
    }).compileComponents();

    fixture = TestBed.createComponent(ClassGroupSelectComponent);
    component = fixture.componentInstance;
    classGroupServiceSpy = TestBed.inject(ClassGroupService) as ClassGroupService;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have empty class groups when no school is selected', () => {
    fixture.componentRef.setInput('schoolId', null);
    fixture.detectChanges();
    expect(component.filteredClassGroups).toEqual([]);
  });

  it('should call loadClassGroups when schoolId is set', () => {
    fixture.componentRef.setInput('schoolId', 1);
    fixture.detectChanges();
    expect(classGroupServiceSpy.loadClassGroups).toHaveBeenCalledWith(1);
  });
  it('clears the form value when the placeholder is selected', () => {
    fixture.componentRef.setInput('schoolId', 1);
    fixture.detectChanges();
    const changed = jest.fn();
    component.registerOnChange(changed);
    component.writeValue(3);
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('select');
    select.value = '';
    select.dispatchEvent(new Event('change'));
    expect(changed).toHaveBeenCalledWith(null);
    expect(component.classGroupId()).toBeNull();
  });

});
