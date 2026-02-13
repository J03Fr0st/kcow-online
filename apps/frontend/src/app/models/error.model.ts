/**
 * Error severity levels
 */
export enum ErrorSeverity {
  INFO = 'info',
  WARNING = 'warning',
  ERROR = 'error',
  CRITICAL = 'critical',
}

/**
 * Error types for categorization
 */
export enum ErrorType {
  HTTP = 'http',
  VALIDATION = 'validation',
  RUNTIME = 'runtime',
  NETWORK = 'network',
  AUTHENTICATION = 'authentication',
  AUTHORIZATION = 'authorization',
  NOT_FOUND = 'not_found',
  SERVER = 'server',
  UNKNOWN = 'unknown',
}

/**
 * HTTP error status categories
 */
export enum HttpErrorStatus {
  BAD_REQUEST = 400,
  UNAUTHORIZED = 401,
  FORBIDDEN = 403,
  NOT_FOUND = 404,
  CONFLICT = 409,
  UNPROCESSABLE_ENTITY = 422,
  INTERNAL_SERVER_ERROR = 500,
  BAD_GATEWAY = 502,
  SERVICE_UNAVAILABLE = 503,
  GATEWAY_TIMEOUT = 504,
}

/**
 * Structured error information
 */
export interface AppError {
  id: string;
  timestamp: Date;
  type: ErrorType;
  severity: ErrorSeverity;
  message: string;
  userMessage?: string;
  statusCode?: number;
  url?: string;
  method?: string;
  stack?: string;
  context?: Record<string, unknown>;
  handled: boolean;
  retryable?: boolean;
}

/**
 * Error log entry for detailed tracking
 */
export interface ErrorLogEntry extends AppError {
  userAgent?: string;
  sessionId?: string;
  userId?: string;
  route?: string;
  previousRoute?: string;
}

/**
 * Error display configuration
 */
export interface ErrorDisplayConfig {
  showNotification: boolean;
  showDialog: boolean;
  autoClose?: boolean;
  duration?: number;
  allowRetry?: boolean;
  redirectUrl?: string;
}

/**
 * Error handler options
 */
export interface ErrorHandlerOptions {
  logToConsole?: boolean;
  logToServer?: boolean;
  showUserNotification?: boolean;
  severity?: ErrorSeverity;
  context?: Record<string, unknown>;
}
