import { render } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import type { Repositories } from '../app/RepositoriesContext'
import { RepositoriesProvider } from '../app/RepositoriesProvider'
import { routes } from '../app/routes'
import { InMemoryGarmentRepository } from '../data/in-memory-garment-repository'
import { InMemoryProductRepository } from '../data/in-memory-product-repository'

export interface RenderAppOptions {
  route?: string
  repositories?: Partial<Repositories>
}

export function renderApp({ route = '/wardrobe', repositories = {} }: RenderAppOptions = {}) {
  const value: Repositories = {
    garments: repositories.garments ?? new InMemoryGarmentRepository(),
    products: repositories.products ?? new InMemoryProductRepository(),
  }
  const router = createMemoryRouter(routes, { initialEntries: [route] })
  return {
    router,
    repositories: value,
    ...render(
      <RepositoriesProvider repositories={value}>
        <RouterProvider router={router} />
      </RepositoriesProvider>,
    ),
  }
}
