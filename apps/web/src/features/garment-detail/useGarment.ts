import { useCallback, useEffect, useState } from 'react'
import { describeError } from '../../app/describe-error'
import { useRepositories } from '../../app/useRepositories'
import type { Garment } from '../../domain/garment'

export type GarmentState =
  | { status: 'loading' }
  | { status: 'ready'; garment: Garment }
  | { status: 'not-found' }
  | { status: 'error'; message: string }

export interface UseGarmentResult {
  state: GarmentState
  /** Replaces the loaded garment after a mutation, without reloading. */
  setGarment: (garment: Garment) => void
  /** Loads the same garment again, after a failure. */
  retry: () => void
}

interface Loaded {
  id: string
  attempt: number
  state: GarmentState
}

export function useGarment(id: string): UseGarmentResult {
  const { garments } = useRepositories()
  const [attempt, setAttempt] = useState(0)
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    let cancelled = false
    garments.getById(id).then(
      (garment) => {
        if (cancelled) return
        const state: GarmentState = garment ? { status: 'ready', garment } : { status: 'not-found' }
        setLoaded({ id, attempt, state })
      },
      (error: unknown) => {
        if (cancelled) return
        const message = describeError(error, 'Could not load the garment.')
        setLoaded({ id, attempt, state: { status: 'error', message } })
      },
    )
    return () => {
      cancelled = true
    }
  }, [garments, id, attempt])

  const setGarment = useCallback(
    (garment: Garment) =>
      setLoaded({ id: garment.id, attempt, state: { status: 'ready', garment } }),
    [attempt],
  )
  const retry = useCallback(() => setAttempt((current) => current + 1), [])

  const current = loaded && loaded.id === id && loaded.attempt === attempt
  const state: GarmentState = current ? loaded.state : { status: 'loading' }
  return { state, setGarment, retry }
}
