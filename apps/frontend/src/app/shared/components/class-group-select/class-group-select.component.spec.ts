import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ClassGroupService } from '@core/services/class-group.service';
import { of } from 'rxjs';
import { ClassGroupSelectComponent } from './class-group-select.component';

describe('ClassGroupSelectComponent', () => {
  let component: ClassGroupSelectComponent;
  let fixture: ComponentFixture<ClassGroupSelectComponent>;
  let loadClassGroups: jest.Mock;

  beforeEach(async () => {
    loadClassGroups = jest.fn();
    const service = { loadClassGroups, classGroups: of([]) };

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
});
