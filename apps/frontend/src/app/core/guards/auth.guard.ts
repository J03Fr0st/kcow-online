import { inject } from '@angular/core';
import {
  type ActivatedRouteSnapshot,
  type CanActivateFn,
  Router,
  type RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Authentication Guard
 *
 * Protects routes by checking if user is authenticated via AuthService.
 * If not authenticated, redirects to login page.
 */
export const authGuard: CanActivateFn = (
  _route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const token = authService.getToken();

  if (token) {
    return true;
  }

  // Not authenticated - store return URL and redirect to login
  const returnUrl = state.url;
  authService.setReturnUrl(returnUrl);

  router.navigate(['/login']);

  return false;
};
