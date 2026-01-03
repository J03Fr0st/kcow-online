# Story 1.2: Better Auth Server Integration

Status: ready-for-dev

## Story

As a **developer**,
I want **Better Auth configured on the backend API**,
so that **the system can authenticate admin users securely**.

## Acceptance Criteria

1. **Given** the backend scaffold from Story 1.1
   **When** Better Auth is integrated
   **Then** the Admin role and user are seeded in the database

2. **And** `/api/auth/login` endpoint accepts credentials and returns a token

3. **And** `/api/auth/logout` endpoint invalidates the session

4. **And** protected endpoints return 401 for unauthenticated requests

5. **And** protected endpoints return 403 for unauthorized roles

## Tasks / Subtasks

- [ ] Task 1: Research Better Auth .NET integration (AC: #1-5)
  - [ ] Review Better Auth documentation for .NET backend
  - [ ] Determine session vs JWT strategy
  - [ ] Plan database schema for auth tables
- [ ] Task 2: Add Better Auth NuGet packages (AC: #1)
  - [ ] Add authentication packages to Api project
  - [ ] Configure services in Program.cs
- [ ] Task 3: Create auth database tables (AC: #1)
  - [ ] Create User entity in Domain
  - [ ] Create Role entity in Domain
  - [ ] Create EF configurations in Infrastructure
  - [ ] Add migration for auth tables
- [ ] Task 4: Seed Admin user (AC: #1)
  - [ ] Create database seeder in Infrastructure
  - [ ] Seed Admin role
  - [ ] Seed default admin user with hashed password
  - [ ] Run seeder on application startup (dev only)
- [ ] Task 5: Implement login endpoint (AC: #2)
  - [ ] Create AuthController in Api
  - [ ] Create LoginRequest/LoginResponse DTOs
  - [ ] Implement credential validation
  - [ ] Return auth token on success
- [ ] Task 6: Implement logout endpoint (AC: #3)
  - [ ] Add logout action to AuthController
  - [ ] Invalidate session/token
- [ ] Task 7: Configure authorization middleware (AC: #4, #5)
  - [ ] Add authentication middleware
  - [ ] Add authorization middleware
  - [ ] Configure [Authorize] attribute defaults
  - [ ] Test 401/403 responses

## Dev Notes

### Architecture Compliance

- **Auth integration in Infrastructure layer** - Better Auth setup in `Infrastructure/Auth/`
- **Controllers in Api layer** - AuthController in `Api/Controllers/`
- **DTOs in Application layer** - LoginRequest, LoginResponse in `Application/Auth/`
- **Entities in Domain layer** - User, Role in `Domain/Entities/`

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Better Auth | Latest | TypeScript auth framework adapted for .NET |
| HTTPS | Required | All auth endpoints must be HTTPS |

### Better Auth Configuration

Better Auth is a TypeScript-first authentication framework. For .NET integration:
- May need to use JWT tokens for cross-platform compatibility
- Consider session-based auth if frontend and backend on same domain
- Implement email/password authentication with Admin role

### Security Requirements

- **Password hashing**: Use BCrypt or Argon2
- **HTTPS only**: No HTTP endpoints for auth
- **Secure token storage**: HttpOnly cookies or secure storage
- **PII protection**: POPIA compliance for user data

### API Endpoints

| Endpoint | Method | Auth Required | Description |
|----------|--------|---------------|-------------|
| `/api/auth/login` | POST | No | Authenticate user |
| `/api/auth/logout` | POST | Yes | Invalidate session |
| `/api/auth/me` | GET | Yes | Get current user info |

### File Structure

```
apps/backend/
├── src/
│   ├── Api/
│   │   └── Controllers/
│   │       └── AuthController.cs
│   ├── Application/
│   │   └── Auth/
│   │       ├── LoginRequest.cs
│   │       ├── LoginResponse.cs
│   │       └── IAuthService.cs
│   ├── Domain/
│   │   └── Entities/
│   │       ├── User.cs
│   │       └── Role.cs
│   └── Infrastructure/
│       ├── Auth/
│       │   ├── AuthService.cs
│       │   └── PasswordHasher.cs
│       └── Data/
│           ├── Configurations/
│           │   ├── UserConfiguration.cs
│           │   └── RoleConfiguration.cs
│           └── Seeders/
│               └── AuthSeeder.cs
```

### Previous Story Dependencies

- **Story 1.1** provides: Backend scaffold with EF Core + SQLite configured

### Testing Requirements

- Unit test: Password hashing, token generation
- Integration test: Login/logout flows with valid/invalid credentials
- Test 401/403 responses for protected endpoints

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Authentication & Security]
- [Source: docs/project_context.md#Technology Stack]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.2]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
