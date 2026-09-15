import { useEffect, useState } from 'react'
import { useRepositories } from '../../app/useRepositories'
import type { Product } from '../../domain/product'

interface Loaded {
  id: string
  product: Product | null
}

/** The linked product, or null while loading, when there is no link, or when it cannot be found. */
export function useProduct(id: string | null): Product | null {
  const { products } = useRepositories()
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    if (id === null) return
    let cancelled = false
    products.getById(id).then((product) => {
      if (!cancelled) setLoaded({ id, product })
    })
    return () => {
      cancelled = true
    }
  }, [products, id])

  return id !== null && loaded?.id === id ? loaded.product : null
}
