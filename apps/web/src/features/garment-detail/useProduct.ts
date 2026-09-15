import { useEffect, useState } from 'react'
import { useRepositories } from '../../app/useRepositories'
import type { Product } from '../../domain/product'

export type ProductState =
  { status: 'loading' } | { status: 'ready'; product: Product } | { status: 'missing' }

interface Loaded {
  id: string
  state: ProductState
}

/**
 * The linked product's load state: `loading` while fetching, `ready` once it resolves, and
 * `missing` when there is no link, the product cannot be found, or the fetch fails. Kept apart
 * from `loading` so a dangling `productId` does not show "Loading the product…" forever.
 */
export function useProduct(id: string | null): ProductState {
  const { products } = useRepositories()
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    if (id === null) return
    let cancelled = false
    products.getById(id).then(
      (product) => {
        if (cancelled) return
        setLoaded({ id, state: product ? { status: 'ready', product } : { status: 'missing' } })
      },
      () => {
        if (cancelled) return
        setLoaded({ id, state: { status: 'missing' } })
      },
    )
    return () => {
      cancelled = true
    }
  }, [products, id])

  if (id === null) return { status: 'missing' }
  return loaded && loaded.id === id ? loaded.state : { status: 'loading' }
}
