# Story 1.3: Frontend Auth Service & Login Page

Status: ready-for-dev

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

- [ ] Task 1: Create AuthService with Signals (AC: #4)
  - [ ] Create AuthService in `core/services/`
  - [ ] Implement `isAuthenticated$` signal
  - [ ] Implement `currentUser$` signal
  - [ ] Implement login method (calls API, stores token)
  - [ ] Implement logout method (clears token, resets state)
- [ ] Task 2: Create HTTP interceptor for auth tokens (AC: #5)
  - [ ] Create AuthInterceptor in `core/interceptors/`
  - [ ] Attach Authorization header to API requests
  - [ ] Handle 401 responses (redirect to login)
  - [ ] Register interceptor in app.config.ts
- [ ] Task 3: Create auth guard (AC: #6)
  - [ ] Create AuthGuard in `core/guards/`
  - [ ] Check authentication state
  - [ ] Redirect to `/login` if not authenticated
  - [ ] Apply guard to protected routes
- [ ] Task 4: Create login page component (AC: #1, #2)
  - [ ] Create login component in `features/auth/`
  - [ ] Create reactive form with email/password fields
  - [ ] Style with DaisyUI dark theme
  - [ ] Add loading state during submission
  - [ ] Display inline error messages
- [ ] Task 5: Handle login flow (AC: #1)
  - [ ] Call AuthService.login() on form submit
  - [ ] Store token on success
  - [ ] Navigate to dashboard on success
- [ ] Task 6: Handle error states (AC: #2)
  - [ ] Display server error messages inline
  - [ ] Keep user on login page on failure
  - [ ] Clear password field on failure
- [ ] Task 7: Implement logout (AC: #3)
  - [ ] Add logout button (will be in layout later)
  - [ ] Call AuthService.logout() on click
  - [ ] Navigate to login page
- [ ] Task 8: Configure routing (AC: #6)
  - [ ] Add `/login` route
  - [ ] Configure AuthGuard on protected routes

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
