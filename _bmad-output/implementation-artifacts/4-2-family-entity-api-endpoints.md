# Story 4.2: Family Entity & API Endpoints

Status: done

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

- [x] Task 1: Create Family entity (AC: #1)
  - [x] Create Family.cs with properties
  - [x] Add navigation to StudentFamily join
- [x] Task 2: Create StudentFamily join entity (AC: #2)
  - [x] Create StudentFamily.cs with StudentId, FamilyId, RelationshipType
  - [x] RelationshipType enum: Parent, Guardian, Sibling, Other
- [x] Task 3: Configure EF Core (AC: #2)
  - [x] Configure families table
  - [x] Configure student_families join table
  - [x] Set up many-to-many relationship
- [x] Task 4: Apply migration (AC: #2)
  - [x] Create and apply AddFamilies migration
- [x] Task 5: Create Family DTOs (AC: #3, #4)
  - [x] FamilyDto, CreateFamilyRequest, UpdateFamilyRequest
  - [x] StudentFamilyDto with relationship type
- [x] Task 6: Create FamilyService (AC: #3)
  - [x] CRUD for families
  - [x] Link/unlink family to student
- [x] Task 7: Create endpoints (AC: #3, #4, #5)
  - [x] FamiliesController for CRUD
  - [x] Add GET/POST to StudentsController for family links

## Dev Notes

### Family Entity

```csharp
// ... (code omitted for brevity, matches implemented Family.cs)
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

gemini-2.0-flash-exp

### Debug Log References

- Created `Domain/Enums/RelationshipType.cs`.
- Created `Domain/Entities/Family.cs` and `StudentFamily.cs`.
- Updated `Student.cs` with navigation property.
- Added `FamilyConfiguration` and `StudentFamilyConfiguration`.
- Updated `AppDbContext` with new `DbSet`s.
- Created and applied `AddFamilies` migration.
- Created Family DTOs.
- Implemented `FamilyService` with student linking logic.
- Implemented `FamiliesController` and added link endpoints to `StudentsController`.
- Verified with unit tests and integration tests.

### Completion Notes List

- ✅ All Acceptance Criteria met.
- ✅ Many-to-many relationship between Students and Families implemented with join entity.
- ✅ CRUD for families and linking functionality working as expected.
- ✅ Tests passing (Unit & Integration).

### File List
- apps/backend/src/Domain/Enums/RelationshipType.cs
- apps/backend/src/Domain/Entities/Family.cs
- apps/backend/src/Domain/Entities/StudentFamily.cs
- apps/backend/src/Infrastructure/Data/Configurations/FamilyConfiguration.cs
- apps/backend/src/Infrastructure/Data/Configurations/StudentFamilyConfiguration.cs
- apps/backend/src/Infrastructure/Migrations/20260107121215_AddFamilies.cs
- apps/backend/src/Application/Families/FamilyDto.cs
- apps/backend/src/Application/Families/CreateFamilyRequest.cs
- apps/backend/src/Application/Families/UpdateFamilyRequest.cs
- apps/backend/src/Application/Families/LinkFamilyRequest.cs
- apps/backend/src/Application/Families/IFamilyService.cs
- apps/backend/src/Infrastructure/Families/FamilyService.cs
- apps/backend/src/Api/Controllers/FamiliesController.cs
- apps/backend/tests/Unit/FamilyServiceTests.cs
- apps/backend/tests/Integration/Families/FamiliesControllerTests.cs
