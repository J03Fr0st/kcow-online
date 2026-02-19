import { HttpClient } from '@angular/common/http';
import { computed, Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, type Observable, of, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { LoginRequest } from '../../features/auth/models/login-request.model';
import type { LoginResponse } from '../../features/auth/models/login-response.model';
import type { User } from '../../features/auth/models/user.model';

/**
 * Authentication Service
 *
 * Handles user authentication, session management, and auth state.
 * Uses Angular Signals for reactive state management.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly apiUrl = environment.apiUrl;
  private readonly tokenKey = 'auth_token';
  private readonly returnUrlKey = 'returnUrl';

  // Signals for auth state
  private userSignal = signal<User | null>(null);
  private isLoadingSignal = signal<boolean>(false);

  // Public readonly signals
  readonly isAuthenticated = computed(() => this.userSignal() !== null);
  readonly currentUser = computed(() => this.userSignal());
  readonly isLoading = this.isLoadingSignal.asReadonly();

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

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials).pipe(
      tap((response) => {
        // Validate token exists and is not empty
        if (!response.token || response.token.trim() === '') {
          throw new Error('Invalid token received from server');
        }

        // Basic JWT format validation (xxx.yyy.zzz)
        const tokenParts = response.token.split('.');
        if (tokenParts.length !== 3) {
          throw new Error('Malformed JWT token received');
        }

        // Store auth token
        localStorage.setItem(this.tokenKey, response.token);

        // Update user state
        this.userSignal.set(response.user);

        this.isLoadingSignal.set(false);
      }),
      catchError((error) => {
        this.isLoadingSignal.set(false);
        throw error;
      }),
    );
  }

  /**
   * Logout current user and call logout API
   * Note: Caller is responsible for navigation after logout completes
   * @returns Observable<void>
   */
  logout(): Observable<void> {
    this.isLoadingSignal.set(true);

    return this.http.post<void>(`${this.apiUrl}/auth/logout`, {}).pipe(
      tap(() => {
        this.clearSession();
        this.isLoadingSignal.set(false);
      }),
      catchError((error) => {
        // Even if API call fails, clear local session
        this.clearSession();
        this.isLoadingSignal.set(false);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Clear session data synchronously without API call
   * Use this when you need immediate logout (e.g., on 401 error)
   */
  clearSessionAndRedirect(): void {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  /**
   * Get authentication token
   */
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  /**
   * Get current user from server
   * @returns Observable<User | null>
   */
  getCurrentUser(): Observable<User | null> {
    return this.http.get<User>(`${this.apiUrl}/auth/me`).pipe(
      tap((user) => {
        this.userSignal.set(user);
      }),
      catchError((error) => {
        // Only clear session on 401 Unauthorized (token is actually invalid/expired)
        // For other errors (network, 500, etc.), keep the token and let user continue
        if (error.status === 401) {
          this.clearSession();
        }
        return of(null);
      }),
    );
  }

  /**
   * Check for existing session on service initialization
   */
  private checkSession(): void {
    const token = this.getToken();

    if (token) {
      // Validate session with server and get user details
      this.getCurrentUser().subscribe();
    }
  }

  /**
   * Clear session data
   */
  private clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.returnUrlKey);
    this.userSignal.set(null);
  }

  /**
   * Store return URL for post-login redirect
   * @param url URL to return to
   */
  setReturnUrl(url: string): void {
    localStorage.setItem(this.returnUrlKey, url);
  }

  /**
   * Get stored return URL for post-login redirect
   */
  getReturnUrl(): string {
    const returnUrl = localStorage.getItem(this.returnUrlKey);
    localStorage.removeItem(this.returnUrlKey);
    return returnUrl || '/dashboard';
  }
}
