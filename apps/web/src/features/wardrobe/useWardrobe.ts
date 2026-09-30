import { useCallback, useEffect, useState } from 'react'
import { describeError } from '../../app/describe-error'
import { useRepositories } from '../../app/useRepositories'
import type { Garment } from '../../domain/garment'
import type { WardrobeFilter } from '../../domain/wardrobe-filter'

export type WardrobeState =
  | { status: 'loading' }
  | { status: 'ready'; garments: Garment[] }
  | { status: 'error'; message: string }

export interface UseWardrobeResult {
  state: WardrobeState
  /** Loads the same filter again, after a failure. */
  retry: () => void
}

interface Loaded {
  filter: WardrobeFilter
  attempt: number
  state: WardrobeState
}

/** Lists the wardrobe for a filter. `filter` must keep its identity between renders (constant or useMemo). */
export function useWardrobe(filter: WardrobeFilter): UseWardrobeResult {
  const { garments } = useRepositories()
  const [attempt, setAttempt] = useState(0)
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    let cancelled = false
    garments.list(filter).then(
      (result) => {
        if (!cancelled) setLoaded({ filter, attempt, state: { status: 'ready', garments: result } })
      },
      (error: unknown) => {
        if (cancelled) return
        const message = describeError(error, 'Could not load the wardrobe.')
        setLoaded({ filter, attempt, state: { status: 'error', message } })
      },
    )
    return () => {
      cancelled = true
    }
  }, [garments, filter, attempt])

  const retry = useCallback(() => setAttempt((current) => current + 1), [])
  const current = loaded && loaded.filter === filter && loaded.attempt === attempt
  return { state: current ? loaded.state : { status: 'loading' }, retry }
}
