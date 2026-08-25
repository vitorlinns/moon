export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

let csrfTokenPromise: Promise<string> | null = null

// Endpoint de CSRF é compartilhado de propósito com a loja (mesmo IAntiforgery global no
// backend) — só o token de acesso/refresh do admin é isolado, não o CSRF.
export function invalidateCsrfToken() {
  csrfTokenPromise = null
}

async function getCsrfToken(): Promise<string> {
  csrfTokenPromise ??= fetch(`${API_URL}/api/auth/csrf-token`, { credentials: 'include' })
    .then((response) => response.json())
    .then((data) => data.token as string)
    .catch((err) => {
      csrfTokenPromise = null
      throw err
    })

  return csrfTokenPromise
}

async function rawFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const method = (options.method ?? 'GET').toUpperCase()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> | undefined),
  }

  if (method !== 'GET') {
    headers['X-CSRF-TOKEN'] = await getCsrfToken()
  }

  return fetch(`${API_URL}/api${path}`, {
    credentials: 'include',
    headers,
    ...options,
  })
}

const NO_REFRESH_RETRY_PATHS = new Set(['/admin/auth/login', '/admin/auth/refresh'])

let refreshPromise: Promise<boolean> | null = null

async function tryRefresh(): Promise<boolean> {
  refreshPromise ??= rawFetch('/admin/auth/refresh', { method: 'POST' })
    .then((response) => response.ok)
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

async function toResult<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json().catch(() => null)
    throw new ApiError(response.status, body?.message ?? 'Não foi possível completar a solicitação.')
  }

  if (response.status === 204) return undefined as T

  return response.json()
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await rawFetch(path, options)

  if (response.status === 401 && !NO_REFRESH_RETRY_PATHS.has(path)) {
    const refreshed = await tryRefresh()
    if (refreshed) {
      return toResult<T>(await rawFetch(path, options))
    }
  }

  return toResult<T>(response)
}
