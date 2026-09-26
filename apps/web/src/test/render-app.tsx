import { render } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import type { Repositories } from '../app/RepositoriesContext'
import { RepositoriesProvider } from '../app/RepositoriesProvider'
import { routes } from '../app/routes'
import { InMemoryGarmentRepository } from '../data/in-memory-garment-repository'
import { InMemoryProductRepository } from '../data/in-memory-product-repository'
import type { Session } from '../session/session'
import { SessionContext } from '../session/SessionContext'
import { fakeSession } from './fake-session'

export interface RenderAppOptions {
  route?: string
  repositories?: Partial<Repositories>
  /** Signed in as Alice unless the scenario says otherwise. */
  session?: Session
}

export function renderApp({
  route = '/wardrobe',
  repositories = {},
  session = fakeSession(),
}: RenderAppOptions = {}) {
  const value: Repositories = {
    garments: repositories.garments ?? new InMemoryGarmentRepository(),
    products: repositories.products ?? new InMemoryProductRepository(),
  }
  const router = createMemoryRouter(routes, { initialEntries: [route] })
  return {
    router,
    repositories: value,
    session,
    ...render(
      <SessionContext.Provider value={session}>
        <RepositoriesProvider repositories={value}>
          <RouterProvider router={router} />
        </RepositoriesProvider>
      </SessionContext.Provider>,
    ),
  }
}
