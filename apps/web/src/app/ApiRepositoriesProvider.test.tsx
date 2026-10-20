import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { useContext, useEffect } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import { DEFAULT_WARDROBE_FILTER } from '../domain/wardrobe-filter'
import { SessionExpiredError, type Session } from '../session/session'
import { SessionContext } from '../session/SessionContext'
import { fakeFetch, jsonResponse, problemResponse } from '../test/fake-fetch'
import { fakeSession } from '../test/fake-session'
import { ApiRepositoriesProvider } from './ApiRepositoriesProvider'
import type { Repositories } from './RepositoriesContext'
import { SessionExpiryContext } from './SessionExpiryContext'
import { useRepositories } from './useRepositories'

function Probe({ seen }: { seen: Repositories[] }) {
  const repositories = useRepositories()
  const { expired } = useContext(SessionExpiryContext)
  useEffect(() => {
    seen.push(repositories)
  }, [repositories, seen])
  return <p>{expired ? 'expired' : 'fresh'}</p>
}

function renderProvider(session: Session, http = fakeFetch()) {
  const seen: Repositories[] = []
  const ui = (current: Session) => (
    <SessionContext.Provider value={current}>
      <ApiRepositoriesProvider apiBaseUrl="http://api.test" fetch={http.fetch}>
        <Probe seen={seen} />
      </ApiRepositoriesProvider>
    </SessionContext.Provider>
  )
  const view = render(ui(session))
  return {
    http,
    seen,
    garments: () => seen[seen.length - 1].garments,
    rerender: (next: Session) => view.rerender(ui(next)),
  }
}

const withToken = (token: string): Session => ({
  ...fakeSession(),
  getAccessToken: async () => token,
})

describe('ApiRepositoriesProvider', () => {
  afterEach(cleanup)

  it('calls the API with the access token of the session', async () => {
    const { http, garments } = renderProvider(withToken('token-1'))
    http.on('GET /garments?status=active', () => jsonResponse([]))

    await garments().list(DEFAULT_WARDROBE_FILTER)

    expect(http.requests[0].headers.get('Authorization')).toBe('Bearer token-1')
  })

  it('keeps the same repositories while the session changes, and asks the latest one for tokens', async () => {
    const { http, seen, garments, rerender } = renderProvider(withToken('token-1'))
    http.on('GET /garments?status=active', () => jsonResponse([]))

    rerender(withToken('token-2'))
    await garments().list(DEFAULT_WARDROBE_FILTER)

    expect(new Set(seen).size).toBe(1)
    expect(http.requests[0].headers.get('Authorization')).toBe('Bearer token-2')
  })

  it('flags the session as expired when the API rejects the token', async () => {
    const { http, garments } = renderProvider(withToken('token-1'))
    http.on('GET /garments?status=active', () => problemResponse(401, 'request.unauthenticated'))

    await expect(garments().list(DEFAULT_WARDROBE_FILTER)).rejects.toBeInstanceOf(
      SessionExpiredError,
    )

    await waitFor(() => expect(screen.getByText('expired')).toBeInTheDocument())
  })

  it('flags the session as expired when the session cannot renew the token', async () => {
    const session: Session = {
      ...fakeSession(),
      getAccessToken: async () => {
        throw new SessionExpiredError()
      },
    }
    const { garments } = renderProvider(session)

    await expect(garments().list(DEFAULT_WARDROBE_FILTER)).rejects.toBeInstanceOf(
      SessionExpiredError,
    )

    await waitFor(() => expect(screen.getByText('expired')).toBeInTheDocument())
  })

  it('does not flag anything for other failures', async () => {
    const { http, garments } = renderProvider(withToken('token-1'))
    http.on('GET /garments?status=active', () => problemResponse(500, 'server.error'))

    await expect(garments().list(DEFAULT_WARDROBE_FILTER)).rejects.toMatchObject({ status: 500 })

    expect(screen.getByText('fresh')).toBeInTheDocument()
  })
})
