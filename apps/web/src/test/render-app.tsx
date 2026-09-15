import { render } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { routes } from '../app/routes'

export interface RenderAppOptions {
  route?: string
}

export function renderApp({ route = '/wardrobe' }: RenderAppOptions = {}) {
  const router = createMemoryRouter(routes, { initialEntries: [route] })
  return { router, ...render(<RouterProvider router={router} />) }
}
