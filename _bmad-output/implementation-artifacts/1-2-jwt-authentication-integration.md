# Story 1.2: ASP.NET Core Authentication Integration

Status: ready-for-dev

## Story

As a **developer**,
I want **JWT authentication configured on the backend API**,
so that **the system can authenticate admin users securely**.

## Acceptance Criteria

1. **Given** the backend scaffold from Story 1.1
   **When** authentication is integrated
   **Then** the Admin role and user are seeded in the database

2. **And** `/api/auth/login` endpoint accepts credentials and returns a JWT token

3. **And** `/api/auth/logout` endpoint invalidates the session

4. **And** protected endpoints return 401 for unauthenticated requests

5. **And** protected endpoints return 403 for unauthorized roles

## Tasks / Subtasks

- [ ] Task 1: Plan authentication strategy (AC: #1-5)
  - [ ] Design JWT token structure with claims
  - [ ] Plan refresh token strategy (optional)
  - [ ] Plan database schema for auth tables
- [ ] Task 2: Add authentication NuGet packages (AC: #1)
  - [ ] Add Microsoft.AspNetCore.Authentication.JwtBearer to Api
  - [ ] Add BCrypt.Net-Next for password hashing
  - [ ] Configure JWT services in Program.cs
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
  - [ ] Implement credential validation with BCrypt
  - [ ] Generate and return JWT token on success
- [ ] Task 6: Implement logout endpoint (AC: #3)
  - [ ] Add logout action to AuthController
  - [ ] Client-side token removal (stateless JWT)
  - [ ] Optional: Add token to blacklist for revocation
- [ ] Task 7: Configure authorization middleware (AC: #4, #5)
  - [ ] Add JWT authentication middleware
  - [ ] Add authorization middleware with policies
  - [ ] Configure [Authorize] attribute defaults
  - [ ] Test 401/403 responses

## Dev Notes

### Architecture Compliance

- **Auth services in Infrastructure layer** - JwtService, PasswordHasher in `Infrastructure/Auth/`
- **Controllers in Api layer** - AuthController in `Api/Controllers/`
- **DTOs in Application layer** - LoginRequest, LoginResponse in `Application/Auth/`
- **Entities in Domain layer** - User, Role in `Domain/Entities/`

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| JWT Bearer | Built-in | Microsoft.AspNetCore.Authentication.JwtBearer |
| BCrypt.Net | Latest | Password hashing |
| HTTPS | Required | All auth endpoints must be HTTPS |

### JWT Configuration

```json
// appsettings.json
{
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-minimum-32-chars",
    "Issuer": "kcow-api",
    "Audience": "kcow-frontend",
    "ExpirationMinutes": 60
  }
}
```

### Security Requirements

- **Password hashing**: BCrypt with work factor 12
- **JWT signing**: HMAC-SHA256 minimum
- **Token expiration**: 1 hour access token
- **HTTPS only**: No HTTP endpoints for auth
- **Secure token storage**: HttpOnly cookies or localStorage with XSS protection
- **PII protection**: POPIA compliance for user data

### API Endpoints

| Endpoint | Method | Auth Required | Description |
|----------|--------|---------------|-------------|
| `/api/auth/login` | POST | No | Authenticate user, return JWT |
| `/api/auth/logout` | POST | Yes | Invalidate session (client-side) |
| `/api/auth/me` | GET | Yes | Get current user info from token |

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
│   │       ├── UserDto.cs
│   │       └── IAuthService.cs
│   ├── Domain/
│   │   └── Entities/
│   │       ├── User.cs
│   │       └── Role.cs
│   └── Infrastructure/
│       ├── Auth/
│       │   ├── AuthService.cs
│       │   ├── JwtService.cs
│       │   └── PasswordHasher.cs
│       └── Data/
│           ├── Configurations/
│           │   ├── UserConfiguration.cs
│           │   └── RoleConfiguration.cs
│           └── Seeders/
│               └── AuthSeeder.cs
```

### Default Admin Credentials (Dev Only)

```
Email: admin@kcow.local
Password: Admin123!
Role: Admin
```

### Previous Story Dependencies

- **Story 1.1** provides: Backend scaffold with EF Core + SQLite configured

### Testing Requirements

- Unit test: Password hashing, JWT generation/validation
- Integration test: Login/logout flows with valid/invalid credentials
- Test 401 response for unauthenticated requests
- Test 403 response for unauthorized roles

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
