import { useEffect, useState } from 'react'
import { useRepositories } from '../../app/useRepositories'
import type { Garment } from '../../domain/garment'
import type { WardrobeFilter } from '../../domain/wardrobe-filter'

export type WardrobeState =
  | { status: 'loading' }
  | { status: 'ready'; garments: Garment[] }
  | { status: 'error'; message: string }

interface Loaded {
  filter: WardrobeFilter
  state: WardrobeState
}

/** Lists the wardrobe for a filter. `filter` must keep its identity between renders (constant or useMemo). */
export function useWardrobe(filter: WardrobeFilter): WardrobeState {
  const { garments } = useRepositories()
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    let cancelled = false
    garments.list(filter).then(
      (result) => {
        if (!cancelled) setLoaded({ filter, state: { status: 'ready', garments: result } })
      },
      (error: unknown) => {
        if (!cancelled) setLoaded({ filter, state: { status: 'error', message: describe(error) } })
      },
    )
    return () => {
      cancelled = true
    }
  }, [garments, filter])

  if (!loaded || loaded.filter !== filter) {
    return { status: 'loading' }
  }
  return loaded.state
}

function describe(error: unknown): string {
  return error instanceof Error ? error.message : 'Could not load the wardrobe.'
}
