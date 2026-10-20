import { Link } from 'react-router'
import { Card } from '../../components/Card/Card'
import { CATEGORY_LABELS, COLOR_LABELS } from '../../domain/classification'
import type { Garment } from '../../domain/garment'
import { garmentTitle } from '../../lib/garment-title'
import { GarmentPhoto } from './GarmentPhoto'
import styles from './GarmentCard.module.css'

export interface GarmentCardProps {
  garment: Garment
}

export function GarmentCard({ garment }: GarmentCardProps) {
  const { category, color, size } = garment.classification
  return (
    <li className={styles.item}>
      <Link to={`/wardrobe/${garment.id}`} className={styles.link}>
        <Card>
          <GarmentPhoto
            photoUrl={garment.photoUrl}
            color={color}
            alt={garmentTitle(garment)}
            className={styles.photo}
          />
          <div className={styles.meta}>
            <span className={styles.category}>{CATEGORY_LABELS[category]}</span>
            <span className={styles.details}>
              {COLOR_LABELS[color]}
              {size ? ` · ${size}` : ''}
            </span>
          </div>
        </Card>
      </Link>
    </li>
  )
}
