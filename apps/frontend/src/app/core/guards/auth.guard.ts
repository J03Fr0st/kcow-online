import { inject } from '@angular/core';
import {
  CanActivateFn,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';

/**
 * Authentication Guard
 *
 * Protects routes by checking if user is authenticated.
 * If not authenticated, redirects to login page with return URL.
 *
 * @example
 * ```typescript
 * export const appRoutes: Routes = [
 *   {
 *     path: 'dashboard',
 *     canActivate: [authGuard],
 *     loadComponent: () => import('./dashboard.component')
 *   }
 * ];
 * ```
 */
export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);

  // Check if user has auth token in session storage
  // This is a simple check - the AuthService will validate with the server
  const hasAuthToken = sessionStorage.getItem('auth_token') !== null;

  if (hasAuthToken) {
    return true;
  }

  // Not authenticated - store return URL and redirect to login
  const returnUrl = state.url;
  sessionStorage.setItem('returnUrl', returnUrl);

  router.navigate(['/login'], {
    queryParams: { returnUrl },
  });

  return false;
};
