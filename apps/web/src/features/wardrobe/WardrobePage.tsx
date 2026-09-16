import { usePageTitle } from '../../app/usePageTitle'
import { ButtonLink } from '../../components/Button/ButtonLink'
import { EmptyState } from '../../components/EmptyState/EmptyState'
import { DEFAULT_WARDROBE_FILTER } from '../../domain/wardrobe-filter'
import { GarmentCard } from './GarmentCard'
import { useWardrobe } from './useWardrobe'
import styles from './WardrobePage.module.css'

export function WardrobePage() {
  usePageTitle('Wardrobe')
  const wardrobe = useWardrobe(DEFAULT_WARDROBE_FILTER)

  return (
    <>
      <h1>Wardrobe</h1>
      {wardrobe.status === 'loading' && <p role="status">Loading your wardrobe…</p>}
      {wardrobe.status === 'error' && <p role="alert">{wardrobe.message}</p>}
      {wardrobe.status === 'ready' && wardrobe.garments.length === 0 && (
        <EmptyState
          title="Your wardrobe is empty"
          description="Photograph a garment to get started."
          action={<ButtonLink to="/garments/new">Add your first garment</ButtonLink>}
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
