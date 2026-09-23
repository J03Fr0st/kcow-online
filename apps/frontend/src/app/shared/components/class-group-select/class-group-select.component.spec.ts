import { signal } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ClassGroupService } from '@core/services/class-group.service';
import type { ClassGroup } from '@features/class-groups/models/class-group.model';
import { ClassGroupSelectComponent } from './class-group-select.component';

describe('ClassGroupSelectComponent', () => {
  let component: ClassGroupSelectComponent;
  let fixture: ComponentFixture<ClassGroupSelectComponent>;
  let loadClassGroups: jest.Mock;
  const classGroups = signal<ClassGroup[]>([]);

  beforeEach(async () => {
    loadClassGroups = jest.fn();
    classGroups.set([]);
    const service = { loadClassGroups, classGroups, loading: signal(false) };

    await TestBed.configureTestingModule({
      imports: [ClassGroupSelectComponent, ReactiveFormsModule, NoopAnimationsModule],
      providers: [{ provide: ClassGroupService, useValue: service }],
    }).compileComponents();

    fixture = TestBed.createComponent(ClassGroupSelectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have empty class groups when no school is selected', () => {
    fixture.componentRef.setInput('schoolId', null);
    fixture.detectChanges();
    expect(component['filteredClassGroups']).toEqual([]);
  });

  it('should call loadClassGroups when schoolId is set', () => {
    fixture.componentRef.setInput('schoolId', 1);
    fixture.detectChanges();
    expect(loadClassGroups).toHaveBeenCalledWith(1);
  });

  it('should render groups from the service signal for the selected school', () => {
    fixture.componentRef.setInput('schoolId', 1);
    fixture.detectChanges();
    classGroups.set([
      { id: 3, schoolId: 1, name: 'Blue' },
      { id: 4, schoolId: 2, name: 'Red' },
    ] as ClassGroup[]);
    fixture.detectChanges();

    expect(component['filteredClassGroups'].map((group) => group.id)).toEqual([3]);
  });
});
