import type { ImportSource } from './garment'

export interface Product {
  id: string
  name: string
  brand: string | null
  referenceImageUrl: string | null
  sourceUrl: string | null
  source: ImportSource
  createdAt: string
}
