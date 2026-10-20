import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { createApiClient, type ApiClient } from '../data/api/api-client'
import { HttpGarmentRepository } from '../data/http-garment-repository'
import { HttpProductRepository } from '../data/http-product-repository'
import { browserPhotoCodec } from '../data/photos/browser-photo-codec'
import { createPhotoUploader } from '../data/photos/photo-uploader'
import { preparePhoto, type DecodedPhoto, type PhotoCodec } from '../data/photos/prepare-photo'
import { SessionExpiredError } from '../session/session'
import { useSession } from '../session/useSession'
import type { Repositories } from './RepositoriesContext'
import { RepositoriesProvider } from './RepositoriesProvider'
import { SessionExpiryContext } from './SessionExpiryContext'

export interface ApiRepositoriesProviderProps {
  apiBaseUrl: string
  children: ReactNode
  /** Tests pass a fake; the browser's otherwise. */
  fetch?: typeof fetch
  photoCodec?: PhotoCodec<DecodedPhoto>
}

/**
 * The HTTP repositories, built once for the life of the app with the session's tokens (ADR-0019,
 * ADR-0029). Tokens are asked of the latest session, so a session that changes does not rebuild
 * the repositories, which would make every screen load again. A 401 or a session that cannot
 * renew its token turns `SessionExpiryContext.expired` on.
 */
export function ApiRepositoriesProvider({
  apiBaseUrl,
  children,
  fetch = globalThis.fetch,
  photoCodec = browserPhotoCodec as PhotoCodec<DecodedPhoto>,
}: ApiRepositoriesProviderProps) {
  const session = useSession()
  const latestSession = useRef(session)
  useEffect(() => {
    latestSession.current = session
  }, [session])

  const [expired, setExpired] = useState(false)
  const [repositories, setRepositories] = useState<Repositories | null>(null)
  // Built in an effect rather than in the `useState` initializer: `react-hooks/refs` rejects
  // reading `latestSession` from a callback created while rendering. `current ?? ...` keeps the
  // first repositories when StrictMode runs the effect twice.
  useEffect(() => {
    const build = (): Repositories => {
      const api = watchForExpiry(
        createApiClient({
          baseUrl: apiBaseUrl,
          getAccessToken: () => latestSession.current.getAccessToken(),
          fetch,
        }),
        () => setExpired(true),
      )
      const uploadPhoto = createPhotoUploader({
        api,
        prepare: (file) => preparePhoto(file, photoCodec),
        fetch,
      })
      return {
        garments: new HttpGarmentRepository({ api, uploadPhoto }),
        products: new HttpProductRepository({ api }),
      }
    }
    setRepositories((current) => current ?? build())
  }, [apiBaseUrl, fetch, photoCodec])
  const expiry = useMemo(() => ({ expired }), [expired])

  return (
    <SessionExpiryContext.Provider value={expiry}>
      {repositories && (
        <RepositoriesProvider repositories={repositories}>{children}</RepositoriesProvider>
      )}
    </SessionExpiryContext.Provider>
  )
}

function watchForExpiry(api: ApiClient, onExpired: () => void): ApiClient {
  return {
    async send(path, init) {
      try {
        const response = await api.send(path, init)
        if (response.status === 401) onExpired()
        return response
      } catch (error) {
        if (error instanceof SessionExpiredError) onExpired()
        throw error
      }
    },
  }
}
