# Story 1.3: Frontend Auth Service & Login Page

Status: done

## Story

As an **admin**,
I want **a login page to authenticate with my credentials**,
so that **I can securely access the admin system**.

## Acceptance Criteria

1. **Given** I am on the login page (`/login`)
   **When** I enter valid admin credentials and submit
   **Then** I am redirected to the dashboard
   **And** my session is persisted (token stored)

2. **Given** I enter invalid credentials
   **When** I submit the form
   **Then** I see an inline error message
   **And** I remain on the login page

3. **Given** I am authenticated
   **When** I click logout
   **Then** my session is cleared
   **And** I am redirected to the login page

4. **And** AuthService uses Angular Signals for state

5. **And** HTTP interceptor attaches auth token to API requests

6. **And** Auth guard redirects unauthenticated users to login

## Tasks / Subtasks

- [x] Task 1: Create AuthService with Signals (AC: #4)
  - [x] Create AuthService in `core/services/`
  - [x] Implement `isAuthenticated$` signal
  - [x] Implement `currentUser$` signal
  - [x] Implement login method (calls API, stores token)
  - [x] Implement logout method (clears token, resets state)
- [x] Task 2: Create HTTP interceptor for auth tokens (AC: #5)
  - [x] Create AuthInterceptor in `core/interceptors/`
  - [x] Attach Authorization header to API requests
  - [x] Handle 401 responses (redirect to login)
  - [x] Register interceptor in app.config.ts
- [x] Task 3: Create auth guard (AC: #6)
  - [x] Create AuthGuard in `core/guards/`
  - [x] Check authentication state
  - [x] Redirect to `/login` if not authenticated
  - [x] Apply guard to protected routes
- [x] Task 4: Create login page component (AC: #1, #2)
  - [x] Create login component in `features/auth/`
  - [x] Create reactive form with email/password fields
  - [x] Style with DaisyUI dark theme
  - [x] Add loading state during submission
  - [x] Display inline error messages
- [x] Task 5: Handle login flow (AC: #1)
  - [x] Call AuthService.login() on form submit
  - [x] Store token on success
  - [x] Navigate to dashboard on success
- [x] Task 6: Handle error states (AC: #2)
  - [x] Display server error messages inline
  - [x] Keep user on login page on failure
  - [x] Clear password field on failure
- [x] Task 7: Implement logout (AC: #3)
  - [x] Add logout button (will be in layout later)
  - [x] Call AuthService.logout() on click
  - [x] Navigate to login page
- [x] Task 8: Configure routing (AC: #6)
  - [x] Add `/login` route
  - [x] Configure AuthGuard on protected routes

## Dev Notes

### Architecture Compliance

- **AuthService in `core/services/`** - Uses Angular Signals for UI state
- **Interceptor in `core/interceptors/`** - Shared HTTP interceptor for all API calls
- **Guard in `core/guards/`** - Functional guard for route protection
- **Login component in `features/auth/`** - Standalone component with OnPush

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Angular | 21 | Standalone components |
| Reactive Forms | - | Required for form handling |
| Angular Signals | - | For auth state management |
| DaisyUI | Latest | Dark theme styling |

### Critical Rules

- **OnPush change detection mandatory** for all components
- **Reactive Forms only** - no template-driven forms
- **No `any` type** - use proper types or `unknown`
- **Components are presentation-only** - services own state

### AuthService API

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  // Signals for state
  readonly isAuthenticated = signal(false);
  readonly currentUser = signal<User | null>(null);
  readonly isLoading = signal(false);
  
  // Methods
  login(credentials: LoginRequest): Observable<LoginResponse>;
  logout(): void;
  getToken(): string | null;
}
```

### AuthInterceptor Pattern

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};
```

### File Structure

```
apps/frontend/src/app/
├── core/
│   ├── services/
│   │   └── auth.service.ts
│   ├── guards/
│   │   └── auth.guard.ts
│   └── interceptors/
│       └── auth.interceptor.ts
├── features/
│   └── auth/
│       ├── login/
│       │   ├── login.component.ts
│       │   ├── login.component.html
│       │   └── login.component.spec.ts
│       └── models/
│           ├── login-request.model.ts
│           └── login-response.model.ts
└── app.config.ts (register interceptor)
```

### Previous Story Dependencies

- **Story 1.2** provides: Backend auth endpoints (`/api/auth/login`, `/api/auth/logout`)

### Testing Requirements

- Unit test: AuthService login/logout/token handling
- Unit test: AuthInterceptor header attachment
- Unit test: AuthGuard redirect behavior
- Component test: Login form validation and submission

### UI/UX Requirements

- DaisyUI dark theme
- Centered login card
- Loading spinner during submission
- Inline error messages (not toasts)
- Visible focus states for accessibility

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Frontend Architecture]
- [Source: docs/project_context.md#Framework-Specific Rules]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.3]

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

- ✅ Implemented `AuthService` with Signals, token storage (localStorage), and API integration
- ✅ Created `authInterceptor` to attach Bearer tokens and handle 401 errors automatically
- ✅ Implemented `authGuard` using `AuthService` logic for route protection
- ✅ Built `LoginComponent` with Reactive Forms, validation, and DaisyUI styling
- ✅ Configured proper routing with guard protection for main admin area
- ✅ Added comprehensive unit tests for Service, Interceptor, Guard, and Component

### File List

- apps/frontend/src/app/core/services/auth.service.ts
- apps/frontend/src/app/core/services/auth.service.spec.ts
- apps/frontend/src/app/core/interceptors/auth.interceptor.ts
- apps/frontend/src/app/core/interceptors/auth.interceptor.spec.ts
- apps/frontend/src/app/core/guards/auth.guard.ts
- apps/frontend/src/app/core/guards/auth.guard.spec.ts
- apps/frontend/src/app/features/auth/login.component.ts
- apps/frontend/src/app/features/auth/login.component.html
- apps/frontend/src/app/features/auth/login.component.scss
- apps/frontend/src/app/features/auth/login.component.spec.ts
- apps/frontend/src/app/features/auth/models/user.model.ts
- apps/frontend/src/app/features/auth/models/login-request.model.ts
- apps/frontend/src/app/features/auth/models/login-response.model.ts
- apps/frontend/src/app/app.config.ts
- apps/frontend/e2e/auth/login.spec.ts

### Code Review Fixes Applied

**Date:** 2026-01-03
**Reviewer:** Adversarial Code Review (AI)
**Issues Found:** 8 HIGH, 4 MEDIUM, 2 LOW
**Issues Fixed:** 12 (all HIGH and MEDIUM issues)

**Critical Fixes:**
1. ✅ Fixed auth interceptor 401 handling - now calls `clearSessionAndRedirect()` synchronously
2. ✅ Removed navigation from logout service method - follows SRP, components handle navigation
3. ✅ Fixed `isLoading` signal pattern - changed from computed to asReadonly()
4. ✅ Added JWT token validation - validates format and non-empty before storing
5. ✅ Implemented password clearing on login error - satisfies AC #2
6. ✅ Added OnPush change detection strategy - mandatory architecture requirement
7. ✅ Fixed interceptor test false positive - now mocks `clearSessionAndRedirect()`
8. ✅ Added missing files to File List - login.component.scss and E2E test

**Quality Improvements:**
9. ✅ Added aria-live accessibility - error alerts announced to screen readers
10. ✅ Replaced `any` type with `unknown` - proper type safety with guards
11. ✅ Refactored to use DaisyUI components - removed custom CSS, uses design system
12. ✅ Created comprehensive E2E test - covers full login flow with 11 test cases

**Service Pattern Update:**
- `logout()` - Returns Observable, clears session, NO navigation
- `clearSessionAndRedirect()` - New synchronous method for immediate logout + navigation
- Components call `logout().subscribe(() => router.navigate(...))` pattern
- Interceptor calls `clearSessionAndRedirect()` for 401 errors

### Change Log

- 2026-01-03: Implemented frontend authentication system with AuthService, Interceptor, Guard, and Login Page (Story 1.3)
- 2026-01-03: Code review fixes applied - 12 issues resolved (8 HIGH, 4 MEDIUM)