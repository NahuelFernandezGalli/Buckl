import { cleanup, renderHook } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { InMemoryGarmentRepository } from '../data/in-memory-garment-repository'
import { InMemoryProductRepository } from '../data/in-memory-product-repository'
import { RepositoriesProvider } from './RepositoriesProvider'
import { useRepositories } from './useRepositories'

describe('useRepositories', () => {
  afterEach(cleanup)

  it('returns the repositories given to the provider', () => {
    const repositories = {
      garments: new InMemoryGarmentRepository(),
      products: new InMemoryProductRepository(),
    }
    const { result } = renderHook(() => useRepositories(), {
      wrapper: ({ children }) => (
        <RepositoriesProvider repositories={repositories}>{children}</RepositoriesProvider>
      ),
    })
    expect(result.current).toBe(repositories)
  })

  it('fails loudly outside a provider', () => {
    expect(() => renderHook(() => useRepositories())).toThrow(/RepositoriesProvider/)
  })
})
