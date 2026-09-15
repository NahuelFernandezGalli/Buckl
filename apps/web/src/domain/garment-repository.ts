import type { Classification } from './classification'
import type { Garment } from './garment'
import type { PurchaseInfo } from './purchase-info'
import type { WardrobeFilter } from './wardrobe-filter'

/** Photos cross the port as a Blob; the implementation decides how to store them. */
export interface NewGarment {
  photo: Blob | null
  classification: Classification
  purchaseInfo: PurchaseInfo | null
  notes: string | null
}

export interface GarmentChanges {
  /** Undefined keeps the current photo, null removes it, a Blob replaces it. */
  photo?: Blob | null
  classification?: Classification
  purchaseInfo?: PurchaseInfo | null
  notes?: string | null
}

/** Mirror of IGarmentRepository plus the phase 4 use cases the API will expose. */
export interface GarmentRepository {
  list(filter: WardrobeFilter): Promise<Garment[]>
  getById(id: string): Promise<Garment | null>
  create(garment: NewGarment): Promise<Garment>
  update(id: string, changes: GarmentChanges): Promise<Garment>
  archive(id: string): Promise<Garment>
  restore(id: string): Promise<Garment>
}
