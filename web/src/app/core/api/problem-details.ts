export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function extractFieldErrors(err: unknown): Record<string, string[]> {
  if (err && typeof err === 'object' && 'error' in err) {
    const e = (err as { error: ProblemDetails }).error;
    if (e?.errors) return e.errors;
  }
  return {};
}

export function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'error' in err) {
    const e = (err as { error: ProblemDetails }).error;
    if (e?.detail) return e.detail;
    if (e?.title) return e.title;
  }
  if (err && typeof err === 'object' && 'message' in err) {
    return (err as { message: string }).message;
  }
  return 'An unexpected error occurred.';
}
