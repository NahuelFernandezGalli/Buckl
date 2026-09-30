import { SessionExpiredError } from '../../session/session'
import type { ApiClient } from './api-client'
import { ApiError, NetworkError } from './api-errors'

export interface JsonRequest {
  method?: 'GET' | 'POST' | 'PATCH'
  /** Sent as JSON; `undefined` sends no body. */
  body?: unknown
}

/**
 * The problem details of an error answer. A body that is not problem details (a proxy's error
 * page) still gives an error, with a code derived from the status.
 */
export async function readProblem(response: Response): Promise<ApiError> {
  let code = response.status >= 500 ? 'server.error' : 'request.rejected'
  let detail: string | undefined
  try {
    const body: unknown = await response.json()
    if (typeof body === 'object' && body !== null) {
      if ('code' in body && typeof body.code === 'string') code = body.code
      if ('detail' in body && typeof body.detail === 'string') detail = body.detail
    }
  } catch {
    // Not JSON: the status-derived code stays.
  }
  return new ApiError(response.status, code, detail)
}

/**
 * One JSON request to the API. Resolves with the parsed body of a successful answer. Rejects with
 * `SessionExpiredError` when there is no valid token (the session could not renew it, or the API
 * answered 401), `NetworkError` when no answer arrived, and `ApiError` for any other error answer.
 */
export async function requestJson<T>(
  api: ApiClient,
  path: string,
  { method = 'GET', body }: JsonRequest = {},
): Promise<T> {
  const init: RequestInit = { method }
  if (body !== undefined) {
    init.body = JSON.stringify(body)
    init.headers = { 'Content-Type': 'application/json' }
  }

  let response: Response
  try {
    response = await api.send(path, init)
  } catch (error) {
    // fetch rejects with a TypeError when no answer arrives. Anything else, such as an expired
    // session or a path the client refuses, passes through unchanged.
    if (error instanceof TypeError) throw new NetworkError(error)
    throw error
  }

  if (response.status === 401) throw new SessionExpiredError()
  if (!response.ok) throw await readProblem(response)
  return (await response.json()) as T
}
