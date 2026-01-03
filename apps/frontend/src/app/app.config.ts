import {
  provideHttpClient,
  withInterceptors,
  withXsrfConfiguration,
} from '@angular/common/http';
import { type ApplicationConfig, ErrorHandler } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { authInterceptor } from '@core/interceptors/auth.interceptor';
import { errorInterceptor } from '@core/interceptors/error.interceptor';
import { httpMonitoringInterceptor } from '@core/interceptors/http-monitoring.interceptor';
import { GlobalErrorHandler } from '@core/services/global-error-handler.service';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        authInterceptor,
        httpMonitoringInterceptor,
        errorInterceptor, // Global error interceptor
      ]),
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN',
      }),
    ),
    provideAnimations(),
    // Global error handler for uncaught errors
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
