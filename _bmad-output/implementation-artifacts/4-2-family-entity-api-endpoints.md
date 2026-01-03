# Story 4.2: Family Entity & API Endpoints

Status: ready-for-dev

## Story

As a **developer**,
I want **the Family domain entity and relationship endpoints**,
so that **guardian/contact data can be managed and linked to students (FR9)**.

## Acceptance Criteria

1. **Given** the backend project
   **When** the Family entity is created
   **Then** the `Family` entity exists with properties:
   - Id, FamilyName, PrimaryContactName, Phone, Email, Address, Notes

2. **And** a `StudentFamily` join table links students to families with a relationship type (parent, guardian, sibling)

3. **And** `/api/families` endpoints support CRUD

4. **And** `/api/students/:id/families` returns families linked to a student

5. **And** POST `/api/students/:id/families` links a family to a student

## Tasks / Subtasks

- [ ] Task 1: Create Family entity (AC: #1)
  - [ ] Create Family.cs with properties
  - [ ] Add navigation to StudentFamily join
- [ ] Task 2: Create StudentFamily join entity (AC: #2)
  - [ ] Create StudentFamily.cs with StudentId, FamilyId, RelationshipType
  - [ ] RelationshipType enum: Parent, Guardian, Sibling, Other
- [ ] Task 3: Configure EF Core (AC: #2)
  - [ ] Configure families table
  - [ ] Configure student_families join table
  - [ ] Set up many-to-many relationship
- [ ] Task 4: Apply migration
  - [ ] Create and apply AddFamilies migration
- [ ] Task 5: Create Family DTOs (AC: #3, #4)
  - [ ] FamilyDto, CreateFamilyRequest, UpdateFamilyRequest
  - [ ] StudentFamilyDto with relationship type
- [ ] Task 6: Create FamilyService (AC: #3)
  - [ ] CRUD for families
  - [ ] Link/unlink family to student
- [ ] Task 7: Create endpoints (AC: #3, #4, #5)
  - [ ] FamiliesController for CRUD
  - [ ] Add GET/POST to StudentsController for family links

## Dev Notes

### Family Entity

```csharp
public class Family
{
    public int Id { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public string PrimaryContactName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<StudentFamily> StudentFamilies { get; set; } = new List<StudentFamily>();
}

public class StudentFamily
{
    public int StudentId { get; set; }
    public int FamilyId { get; set; }
    public RelationshipType RelationshipType { get; set; }
    
    public Student Student { get; set; } = null!;
    public Family Family { get; set; } = null!;
}

public enum RelationshipType
{
    Parent,
    Guardian,
    Sibling,
    Other
}
```

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/families` | List all families |
| GET | `/api/families/{id}` | Single family with linked students |
| POST | `/api/families` | Create family |
| PUT | `/api/families/{id}` | Update family |
| DELETE | `/api/families/{id}` | Archive family |
| GET | `/api/students/{id}/families` | Get families for student |
| POST | `/api/students/{id}/families` | Link family to student |
| DELETE | `/api/students/{id}/families/{familyId}` | Unlink family |

### Previous Story Dependencies

- **Story 4.1** provides: Student entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.2]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
