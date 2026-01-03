# Story 4.4: Global Student Search

Status: ready-for-dev

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

- [ ] Task 1: Create search API endpoint (AC: #1, #4)
  - [ ] Add GET `/api/students/search?q=term&limit=10`
  - [ ] Search by firstName, lastName (contains, case-insensitive)
  - [ ] Include school and class group in response
  - [ ] Optimize with index
- [ ] Task 2: Create GlobalSearchService (AC: #1)
  - [ ] Implement search with debounce
  - [ ] Handle loading and empty states
- [ ] Task 3: Create GlobalSearch component (AC: #1, #2, #5)
  - [ ] Add search input to navbar
  - [ ] Display typeahead dropdown with results
  - [ ] Show Full Name, School, Grade, Class Group per result
  - [ ] Show "No results found" when empty
- [ ] Task 4: Implement navigation (AC: #3)
  - [ ] On result click, navigate to `/students/{id}`
  - [ ] Close search dropdown
  - [ ] Clear search input
- [ ] Task 5: Style and UX polish
  - [ ] Add keyboard navigation (up/down arrows, enter to select)
  - [ ] Highlight matching text in results

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
