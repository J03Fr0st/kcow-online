import { type ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { type StudentSearchResult, StudentService } from '../../../core/services/student.service';
import { GlobalSearchComponent } from './global-search.component';

describe('GlobalSearchComponent', () => {
  let component: GlobalSearchComponent;
  let fixture: ComponentFixture<GlobalSearchComponent>;
  let mockStudentService: jasmine.SpyObj<StudentService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockSearchResults: StudentSearchResult[] = [
    {
      id: 1,
      fullName: 'John Smith',
      schoolName: 'Greenwood Primary',
      grade: 'Grade 5',
      classGroupName: 'Class 5A',
    },
    {
      id: 2,
      fullName: 'Jane Doe',
      schoolName: 'Oakwood Elementary',
      grade: 'Grade 3',
      classGroupName: 'Class 3B',
    },
  ];

  beforeEach(async () => {
    mockStudentService = jasmine.createSpyObj('StudentService', [], {
      searchStudents: of(mockSearchResults),
    });

    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [GlobalSearchComponent],
      providers: [
        { provide: StudentService, useValue: mockStudentService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GlobalSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty state', () => {
    expect(component.searchTerm()).toBe('');
    expect(component.results()).toEqual([]);
    expect(component.isLoading()).toBe(false);
    expect(component.isOpen()).toBe(false);
  });

  it('should not search for terms less than 2 characters', fakeAsync(() => {
    component.onSearchChange('a');
    tick(300);

    expect(component.results()).toEqual([]);
    expect(component.isOpen()).toBe(false);
    expect(mockStudentService.searchStudents).not.toHaveBeenCalled();
  }));

  it('should search for terms with 2 or more characters after debounce', fakeAsync(() => {
    component.onSearchChange('Jo');
    tick(300);

    expect(component.isLoading()).toBe(true);
    expect(component.isOpen()).toBe(true);

    tick();

    expect(mockStudentService.searchStudents).toHaveBeenCalledWith('Jo', 10);
    expect(component.results()).toEqual(mockSearchResults);
    expect(component.isLoading()).toBe(false);
  }));

  it('should clear search and close dropdown on clearSearch', () => {
    component.onSearchChange('John');
    component.performSearch('John'); // Simulate search completion
    component.results.set(mockSearchResults);

    component.clearSearch();

    expect(component.searchTerm()).toBe('');
    expect(component.results()).toEqual([]);
    expect(component.isOpen()).be(false);
    expect(component.highlightedIndex()).toBe(-1);
  });

  it('should navigate to student profile on selectResult', () => {
    const result = mockSearchResults[0];
    component.selectResult(result);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/students', result.id]);
    expect(component.searchTerm()).toBe('');
    expect(component.isOpen()).toBe(false);
  });

  it('should handle keyboard navigation - arrow down', () => {
    component.results.set(mockSearchResults);
    component.onKeyDown(new KeyboardEvent('keydown', { key: 'ArrowDown' }));

    expect(component.highlightedIndex()).toBe(0);
  });

  it('should handle keyboard navigation - arrow up', () => {
    component.results.set(mockSearchResults);
    component.highlightedIndex.set(1);
    component.onKeyDown(new KeyboardEvent('keydown', { key: 'ArrowUp' }));

    expect(component.highlightedIndex()).toBe(0);
  });

  it('should select result on Enter key', () => {
    component.results.set(mockSearchResults);
    component.highlightedIndex.set(0);

    const event = new KeyboardEvent('keydown', { key: 'Enter' });
    component.onKeyDown(event);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/students', mockSearchResults[0].id]);
  });

  it('should close dropdown on Escape key', () => {
    component.searchTerm.set('John');
    component.isOpen.set(true);

    component.onKeyDown(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(component.searchTerm()).toBe('');
    expect(component.isOpen()).toBe(false);
  });

  it('should highlight matching text', () => {
    const text = 'John Smith';
    const query = 'John';

    const highlighted = component.highlightMatch(text, query);

    expect(highlighted).toContain('<mark>John</mark>');
    expect(highlighted).toContain('Smith');
  });

  it('should escape special regex characters in highlightMatch', () => {
    const text = 'John+Smith';
    const query = 'John+';

    const highlighted = component.highlightMatch(text, query);

    expect(highlighted).toContain('<mark>John+</mark>');
  });

  it('should handle search errors gracefully', fakeAsync(() => {
    (mockStudentService as any).searchStudents = throwError(() => new Error('Search failed'));

    component.onSearchChange('John');
    tick(300);
    tick();

    expect(component.results()).toEqual([]);
    expect(component.isLoading()).toBe(false);
  }));

  it('should open dropdown on focus if search term is valid', () => {
    component.searchTerm.set('Jo');
    component.onFocus();

    expect(component.isOpen()).toBe(true);
  });
});
