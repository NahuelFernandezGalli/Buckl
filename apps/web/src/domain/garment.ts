import type { Classification } from './classification'
import type { PurchaseInfo } from './purchase-info'

export const IMPORT_SOURCES = ['manual', 'url', 'email'] as const
export type ImportSource = (typeof IMPORT_SOURCES)[number]

export const GARMENT_STATUSES = ['active', 'archived'] as const
export type GarmentStatus = (typeof GARMENT_STATUSES)[number]

export const MAX_NOTES_LENGTH = 500

export interface Garment {
  id: string
  productId: string | null
  /** Where the photo can be loaded from: a signed URL from the API, or a data URL in phase 3. */
  photoUrl: string | null
  classification: Classification
  purchaseInfo: PurchaseInfo | null
  source: ImportSource
  status: GarmentStatus
  notes: string | null
  /** ISO 8601 in UTC. Converted to local time only when rendered. */
  createdAt: string
  updatedAt: string
  archivedAt: string | null
}
