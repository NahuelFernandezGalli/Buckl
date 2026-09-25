import { useState } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { Auth0SessionProvider } from '../session/Auth0SessionProvider'
import type { Auth0Settings } from './config'
import type { Repositories } from './RepositoriesContext'
import { RepositoriesProvider } from './RepositoriesProvider'
import { routes } from './routes'

export interface AppProps {
  auth0: Auth0Settings
  repositories: Repositories
}

export function App({ auth0, repositories }: AppProps) {
  const [router] = useState(() => createBrowserRouter(routes))
  return (
    <Auth0SessionProvider
      settings={auth0}
      onSignedIn={(returnTo) => void router.navigate(returnTo, { replace: true })}
    >
      <RepositoriesProvider repositories={repositories}>
        <RouterProvider router={router} />
      </RepositoriesProvider>
    </Auth0SessionProvider>
  )
}
