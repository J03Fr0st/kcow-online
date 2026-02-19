/**
 * ProblemDetails interface matching backend error format
 * From: Microsoft.AspNetCore.Mvc.ProblemDetails
 */
export interface ProblemDetails {
  type?: string; // URI reference to error type
  title?: string; // Human-readable title
  status?: number; // HTTP status code
  detail?: string; // Human-readable detail
  instance?: string; // URI reference to specific occurrence
  [key: string]: unknown; // Additional properties
}
