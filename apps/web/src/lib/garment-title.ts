import { CATEGORY_LABELS, COLOR_LABELS } from '../domain/classification'
import type { Garment } from '../domain/garment'

/** Human title of a garment ("Blue top"): the photo's alt text and the detail heading. */
export function garmentTitle(garment: Garment): string {
  const { category, color } = garment.classification
  return `${COLOR_LABELS[color]} ${CATEGORY_LABELS[category].toLowerCase()}`
}
