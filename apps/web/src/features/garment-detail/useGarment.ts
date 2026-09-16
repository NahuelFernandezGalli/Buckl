import { useCallback, useEffect, useState } from 'react'
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
}

interface Loaded {
  id: string
  state: GarmentState
}

export function useGarment(id: string): UseGarmentResult {
  const { garments } = useRepositories()
  const [loaded, setLoaded] = useState<Loaded | null>(null)

  useEffect(() => {
    let cancelled = false
    garments.getById(id).then(
      (garment) => {
        if (cancelled) return
        setLoaded({ id, state: garment ? { status: 'ready', garment } : { status: 'not-found' } })
      },
      (error: unknown) => {
        if (cancelled) return
        const message = error instanceof Error ? error.message : 'Could not load the garment.'
        setLoaded({ id, state: { status: 'error', message } })
      },
    )
    return () => {
      cancelled = true
    }
  }, [garments, id])

  const setGarment = useCallback(
    (garment: Garment) => setLoaded({ id: garment.id, state: { status: 'ready', garment } }),
    [],
  )

  const state: GarmentState = loaded && loaded.id === id ? loaded.state : { status: 'loading' }
  return { state, setGarment }
}
