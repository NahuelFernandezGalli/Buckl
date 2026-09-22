import { placeholderPhoto } from '../data/placeholder-photo'
import type { Garment } from '../domain/garment'
import type { Product } from '../domain/product'

const CREATED_AT = '2026-09-01T10:00:00.000Z'

export const GarmentMother = {
  active(overrides: Partial<Garment> = {}): Garment {
    return {
      id: 'garment-1',
      productId: null,
      photoUrl: placeholderPhoto('blue'),
      classification: { category: 'top', color: 'blue', size: 'M' },
      purchaseInfo: null,
      source: 'manual',
      status: 'active',
      notes: null,
      createdAt: CREATED_AT,
      updatedAt: CREATED_AT,
      archivedAt: null,
      ...overrides,
    }
  },

  archived(overrides: Partial<Garment> = {}): Garment {
    return GarmentMother.active({
      id: 'garment-2',
      status: 'archived',
      archivedAt: '2026-09-10T10:00:00.000Z',
      updatedAt: '2026-09-10T10:00:00.000Z',
      ...overrides,
    })
  },
}

export const ProductMother = {
  fromUrl(overrides: Partial<Product> = {}): Product {
    return {
      id: 'product-1',
      name: 'Oxford shirt',
      brand: 'Uniqlo',
      referenceImageUrl: null,
      sourceUrl: 'https://example.com/oxford-shirt',
      source: 'url',
      createdAt: CREATED_AT,
      ...overrides,
    }
  },
}
