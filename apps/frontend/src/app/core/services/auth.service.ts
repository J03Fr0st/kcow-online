import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, tap, catchError, of, map } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * User interface matching backend User model
 */
export interface User {
  id: string;
  email: string;
  createdAt: string;
}

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Login response from backend
 */
export interface LoginResponse {
  user: User;
  message: string;
}

/**
 * Auth state for signals
 */
interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

/**
 * Authentication Service
 *
 * Handles user authentication, session management, and auth state.
 * Uses Angular Signals for reactive state management.
 *
 * @example
 * ```typescript
 * constructor(private authService: AuthService) {
 *   // Subscribe to auth state changes
 *   effect(() => {
 *     if (this.authService.isAuthenticated()) {
 *       console.log('User logged in:', this.authService.currentUser());
 *     }
 *   });
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly apiUrl = environment.apiUrl;

  // Signals for auth state
  private userSignal = signal<User | null>(null);
  private isLoadingSignal = signal<boolean>(false);

  // Computed signals
  readonly isAuthenticated = computed(() => this.userSignal() !== null);
  readonly currentUser = computed(() => this.userSignal());
  readonly isLoading = computed(() => this.isLoadingSignal());

  constructor() {
    // Check for existing session on init
    this.checkSession();
  }

  /**
   * Login with email and password
   * @param credentials LoginRequest with email and password
   * @returns Observable<LoginResponse>
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    this.isLoadingSignal.set(true);

    return this.http
      .post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials, {
        withCredentials: true, // Send/receive cookies
      })
      .pipe(
        tap((response) => {
          // Store auth token in session storage
          sessionStorage.setItem('auth_token', 'authenticated');

          // Update user state
          this.userSignal.set(response.user);

          this.isLoadingSignal.set(false);
        }),
        catchError((error) => {
          this.isLoadingSignal.set(false);
          throw error;
        })
      );
  }

  /**
   * Logout current user
   * @returns Observable<void>
   */
  logout(): Observable<void> {
    this.isLoadingSignal.set(true);

    return this.http.post<void>(`${this.apiUrl}/auth/logout`, {}, {
      withCredentials: true,
    }).pipe(
      tap(() => {
        // Clear session
        this.clearSession();

        // Redirect to login
        this.router.navigate(['/login']);

        this.isLoadingSignal.set(false);
      }),
      catchError((error) => {
        // Even if API call fails, clear local session
        this.clearSession();
        this.router.navigate(['/login']);

        this.isLoadingSignal.set(false);
        throw error;
      })
    );
  }

  /**
   * Get current user from server
   * @returns Observable<User | null>
   */
  getCurrentUser(): Observable<User | null> {
    return this.http.get<User>(`${this.apiUrl}/auth/me`, {
      withCredentials: true,
    }).pipe(
      tap((user) => {
        sessionStorage.setItem('auth_token', 'authenticated');
        this.userSignal.set(user);
      }),
      map((user) => user),
      catchError(() => {
        // User not authenticated or error occurred
        this.clearSession();
        return of(null);
      })
    );
  }

  /**
   * Check for existing session on service initialization
   */
  private checkSession(): void {
    const hasToken = sessionStorage.getItem('auth_token') !== null;

    if (hasToken) {
      // Validate session with server
      this.getCurrentUser().subscribe();
    }
  }

  /**
   * Clear session data
   */
  private clearSession(): void {
    sessionStorage.removeItem('auth_token');
    sessionStorage.removeItem('returnUrl');
    this.userSignal.set(null);
  }

  /**
   * Get stored return URL for post-login redirect
   */
  getReturnUrl(): string {
    const returnUrl = sessionStorage.getItem('returnUrl');
    sessionStorage.removeItem('returnUrl');
    return returnUrl || '/dashboard';
  }

  /**
   * Manual method to refresh auth state from server
   * Useful after page refresh or when怀疑 session state is stale
   */
  refreshAuthState(): Observable<User | null> {
    return this.getCurrentUser();
  }
}
