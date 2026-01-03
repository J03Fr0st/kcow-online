# Story 3.1: Class Group Entity & API Endpoints

Status: ready-for-dev

## Story

As a **developer**,
I want **the ClassGroup domain entity and REST API endpoints**,
so that **class group data can be managed through the API**.

## Acceptance Criteria

1. **Given** the backend project with School and Truck entities
   **When** the ClassGroup entity is created
   **Then** the `ClassGroup` entity exists with properties:
   - Id, Name, SchoolId (FK), TruckId (FK), DayOfWeek, StartTime, EndTime, Sequence, IsActive, Notes

2. **And** EF Core configuration with foreign key relationships to School and Truck

3. **And** migration creates the `class_groups` table

4. **And** `/api/class-groups` endpoints support:
   - GET (list class groups with optional school/truck filters)
   - GET `/:id` (single class group with school and truck details)
   - POST (create class group)
   - PUT `/:id` (update class group)
   - DELETE `/:id` (archive class group)

5. **And** endpoints require authentication

6. **And** ProblemDetails errors for validation failures

## Tasks / Subtasks

- [ ] Task 1: Create ClassGroup entity (AC: #1)
  - [ ] Create ClassGroup.cs in Domain/Entities
  - [ ] Add properties and foreign keys
  - [ ] Add navigation properties to School and Truck
- [ ] Task 2: Create EF Core configuration (AC: #2, #3)
  - [ ] Create ClassGroupConfiguration.cs
  - [ ] Configure table name as `class_groups`
  - [ ] Configure FK relationships
  - [ ] Add composite index on SchoolId + DayOfWeek + StartTime
- [ ] Task 3: Apply migration (AC: #3)
  - [ ] Create and apply AddClassGroups migration
- [ ] Task 4: Create DTOs (AC: #4)
  - [ ] Create ClassGroupDto with nested School/Truck info
  - [ ] Create CreateClassGroupRequest
  - [ ] Create UpdateClassGroupRequest
- [ ] Task 5: Create ClassGroupService (AC: #4)
  - [ ] Implement CRUD with filtering
  - [ ] Include School and Truck in queries
- [ ] Task 6: Create ClassGroupsController (AC: #4, #5, #6)
  - [ ] Implement endpoints with [Authorize]
  - [ ] Add query params for filtering
  - [ ] Return ProblemDetails on errors

## Dev Notes

### ClassGroup Entity

```csharp
public class ClassGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SchoolId { get; set; }
    public int? TruckId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Sequence { get; set; } = 1;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public School School { get; set; } = null!;
    public Truck? Truck { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
```

### API Endpoints with Filtering

| Method | Path | Query Params | Description |
|--------|------|--------------|-------------|
| GET | `/api/class-groups` | `?schoolId=&truckId=` | List with filters |
| GET | `/api/class-groups/{id}` | - | Single with details |
| POST | `/api/class-groups` | - | Create |
| PUT | `/api/class-groups/{id}` | - | Update |
| DELETE | `/api/class-groups/{id}` | - | Archive |

### Previous Story Dependencies

- **Story 2.1** provides: Truck entity
- **Story 2.3** provides: School entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.1]
- [Source: docs/legacy/2_Class_Group/Class Group.xsd]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
