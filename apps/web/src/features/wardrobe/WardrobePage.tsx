import { useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { Button } from '../../components/Button/Button'
import { ButtonLink } from '../../components/Button/ButtonLink'
import { EmptyState } from '../../components/EmptyState/EmptyState'
import { hasCriteria } from '../../domain/wardrobe-filter'
import { GarmentCard } from './GarmentCard'
import { useWardrobe } from './useWardrobe'
import { FILTER_PARAMS, parseWardrobeFilter } from './wardrobe-filter-params'
import { WardrobeFilters } from './WardrobeFilters'
import styles from './WardrobePage.module.css'

export function WardrobePage() {
  usePageTitle('Wardrobe')
  const [searchParams, setSearchParams] = useSearchParams()
  const filter = useMemo(() => parseWardrobeFilter(searchParams), [searchParams])
  const wardrobe = useWardrobe(filter)
  const filtering = hasCriteria(filter)

  const showWholeWardrobe = () => {
    const next = new URLSearchParams(searchParams)
    for (const name of [
      FILTER_PARAMS.category,
      FILTER_PARAMS.color,
      FILTER_PARAMS.size,
      FILTER_PARAMS.searchText,
    ]) {
      next.delete(name)
    }
    setSearchParams(next, { replace: true })
  }

  return (
    <>
      <h1>Wardrobe</h1>
      <WardrobeFilters
        params={searchParams}
        onChange={(next) => setSearchParams(next, { replace: true })}
      />
      {wardrobe.status === 'loading' && <p role="status">Loading your wardrobe…</p>}
      {wardrobe.status === 'error' && <p role="alert">{wardrobe.message}</p>}
      {wardrobe.status === 'ready' && wardrobe.garments.length === 0 && !filtering && (
        <EmptyState
          title="Your wardrobe is empty"
          description="Photograph a garment to get started."
          action={<ButtonLink to="/garments/new">Add your first garment</ButtonLink>}
        />
      )}
      {wardrobe.status === 'ready' && wardrobe.garments.length === 0 && filtering && (
        <EmptyState
          title="No garment matches these filters"
          action={
            <Button variant="secondary" onClick={showWholeWardrobe}>
              Show the whole wardrobe
            </Button>
          }
        />
      )}
      {wardrobe.status === 'ready' && wardrobe.garments.length > 0 && (
        <ul aria-label="Garments" className={styles.grid}>
          {wardrobe.garments.map((garment) => (
            <GarmentCard key={garment.id} garment={garment} />
          ))}
        </ul>
      )}
    </>
  )
}
