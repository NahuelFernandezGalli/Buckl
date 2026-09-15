import { useState } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router'
import type { Repositories } from './RepositoriesContext'
import { RepositoriesProvider } from './RepositoriesProvider'
import { routes } from './routes'

export interface AppProps {
  repositories: Repositories
}

export function App({ repositories }: AppProps) {
  const [router] = useState(() => createBrowserRouter(routes))
  return (
    <RepositoriesProvider repositories={repositories}>
      <RouterProvider router={router} />
    </RepositoriesProvider>
  )
}
