export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

let csrfTokenPromise: Promise<string> | null = null

// O token CSRF é vinculado à identidade autenticada no momento em que foi emitido.
// Precisa ser descartado sempre que essa identidade muda (login, registro, logout),
// senão a próxima chamada autenticada falha com 403 usando um token "de outro usuário".
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

// Chamadas que legitimamente podem devolver 401 sem que isso signifique
// "sessão expirada" (ex: senha errada no login) — não tentar refresh nelas.
const NO_REFRESH_RETRY_PATHS = new Set(['/auth/login', '/auth/register', '/auth/refresh'])

let refreshPromise: Promise<boolean> | null = null

async function tryRefresh(): Promise<boolean> {
  refreshPromise ??= rawFetch('/auth/refresh', { method: 'POST' })
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
