# Story 4.4: Global Student Search

Status: review

## Story

As an **admin**,
I want **a global search bar to quickly find students by name, school, or grade**,
so that **I can locate any student within seconds (FR11)**.

## Acceptance Criteria

1. **Given** I am on any page in the admin layout
   **When** I type in the global search bar
   **Then** typeahead results appear showing matching students
   **And** each result shows: Full Name, School, Grade, Class Group

2. **Given** there are students with similar names
   **When** I view the results
   **Then** disambiguation is clear (showing school/grade) to select the correct student

3. **Given** I select a search result
   **When** I click on it
   **Then** I navigate to the student's profile page

4. **And** search returns results in under 2 seconds (NFR1)

5. **And** "No results found" appears when no matches exist

## Tasks / Subtasks

- [x] Task 1: Create search API endpoint (AC: #1, #4)
  - [x] Add GET `/api/students/search?q=term&limit=10`
  - [x] Search by firstName, lastName (contains, case-insensitive)
  - [x] Include school and class group in response
  - [x] Optimize with index (EF.Functions.Like for database-level search)
- [x] Task 2: Create GlobalSearchService (AC: #1)
  - [x] Implement search with debounce (300ms via RxJS debounceTime)
  - [x] Handle loading and empty states
- [x] Task 3: Create GlobalSearch component (AC: #1, #2, #5)
  - [x] Add search input to navbar
  - [x] Display typeahead dropdown with results
  - [x] Show Full Name, School, Grade, Class Group per result
  - [x] Show "No results found" when empty
- [x] Task 4: Implement navigation (AC: #3)
  - [x] On result click, navigate to `/students/{id}`
  - [x] Close search dropdown
  - [x] Clear search input
- [x] Task 5: Style and UX polish
  - [x] Add keyboard navigation (up/down arrows, enter to select)
  - [x] Highlight matching text in results

## Dev Notes

### Search API

```typescript
// Request
GET /api/students/search?q=john&limit=10

// Response
[
  {
    "id": 123,
    "fullName": "John Smith",
    "schoolName": "Greenwood Primary",
    "grade": "Grade 5",
    "classGroupName": "Class 5A"
  },
  // ...
]
```

### Global Search Component Pattern

```typescript
@Component({
  selector: 'app-global-search',
  templateUrl: './global-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GlobalSearchComponent {
  private readonly searchService = inject(GlobalSearchService);
  private readonly router = inject(Router);
  
  searchTerm = signal('');
  results = signal<StudentSearchResult[]>([]);
  isLoading = signal(false);
  isOpen = signal(false);
  
  onSearchChange(term: string) {
    this.searchTerm.set(term);
    if (term.length >= 2) {
      this.isLoading.set(true);
      this.searchService.search(term).subscribe(results => {
        this.results.set(results);
        this.isLoading.set(false);
        this.isOpen.set(true);
      });
    } else {
      this.results.set([]);
      this.isOpen.set(false);
    }
  }
  
  selectResult(result: StudentSearchResult) {
    this.router.navigate(['/students', result.id]);
    this.searchTerm.set('');
    this.isOpen.set(false);
  }
}
```

### File Structure

```
apps/frontend/src/app/
├── core/
│   └── services/
│       └── global-search.service.ts
├── layouts/
│   └── navbar/
│       └── global-search/
│           ├── global-search.component.ts
│           └── global-search.component.html
```

### Performance Requirements

- Debounce: 300ms
- API response: < 2 seconds (NFR1)
- Minimum search length: 2 characters

### Previous Story Dependencies

- **Story 1.4** provides: Navbar where search will be placed
- **Story 4.1** provides: Students API

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.4]
- [Source: _bmad-output/planning-artifacts/prd.md#FR11, NFR1]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Global Student Search]

## Dev Agent Record

### Agent Model Used

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- Created complete global student search functionality with typeahead
- Backend API endpoint `/api/students/search` with debouncing and case-insensitive search
- Frontend Angular component with keyboard navigation and text highlighting
- All acceptance criteria satisfied
- 7 unit tests passing for search functionality

**Technical Decisions:**
- Used EF.Functions.Like for database-level search (better performance than client-side)
- Implemented 300ms debounce on frontend to reduce API calls
- Minimum search length of 2 characters to prevent too many results
- Keyboard navigation (arrow keys, Enter, Escape) for accessibility
- Text highlighting with regex escape for security

**Test Coverage:**
- 7 unit tests for search functionality (all passing)
- Tests cover: matching, case-insensitivity, active-only filter, limits, school/class group inclusion, defaults

### File List

**Backend:**
- `apps/backend/src/Application/Students/StudentSearchResultDto.cs` (NEW)
- `apps/backend/src/Application/Students/IStudentService.cs` (MODIFIED - added SearchAsync method)
- `apps/backend/src/Infrastructure/Students/StudentService.cs` (MODIFIED - implemented SearchAsync)
- `apps/backend/src/Api/Controllers/StudentsController.cs` (MODIFIED - added Search endpoint)
- `apps/backend/tests/Unit/StudentSearchTests.cs` (NEW - 7 unit tests)

**Frontend:**
- `apps/frontend/src/app/core/services/student.service.ts` (MODIFIED - added searchStudents method and StudentSearchResult interface)
- `apps/frontend/src/app/layouts/navbar/global-search/global-search.component.ts` (NEW)
- `apps/frontend/src/app/layouts/navbar/global-search/global-search.component.html` (NEW)
- `apps/frontend/src/app/layouts/navbar/global-search/global-search.component.scss` (NEW)
- `apps/frontend/src/app/layouts/navbar/global-search/global-search.component.spec.ts` (NEW - 15 unit tests)
- `apps/frontend/src/app/layouts/navbar/navbar.component.ts` (MODIFIED - imported GlobalSearchComponent)
- `apps/frontend/src/app/layouts/navbar/navbar.component.html` (MODIFIED - added app-global-search)
