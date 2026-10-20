import { useState } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { Auth0SessionProvider } from '../session/Auth0SessionProvider'
import { ApiRepositoriesProvider } from './ApiRepositoriesProvider'
import type { Auth0Settings } from './config'
import { routes } from './routes'

export interface AppProps {
  auth0: Auth0Settings
  apiBaseUrl: string
}

export function App({ auth0, apiBaseUrl }: AppProps) {
  const [router] = useState(() => createBrowserRouter(routes))
  return (
    <Auth0SessionProvider
      settings={auth0}
      onSignedIn={(returnTo) => void router.navigate(returnTo, { replace: true })}
    >
      <ApiRepositoriesProvider apiBaseUrl={apiBaseUrl}>
        <RouterProvider router={router} />
      </ApiRepositoriesProvider>
    </Auth0SessionProvider>
  )
}
