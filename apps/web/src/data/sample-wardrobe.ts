import type { Garment } from '../domain/garment'
import type { Product } from '../domain/product'
import { placeholderPhoto } from './placeholder-photo'

export const sampleProducts: Product[] = [
  {
    id: 'product-oxford-shirt',
    name: 'Oxford shirt',
    brand: 'Uniqlo',
    referenceImageUrl: null,
    sourceUrl: 'https://www.uniqlo.com/oxford-shirt',
    source: 'url',
    createdAt: '2026-08-20T09:00:00.000Z',
  },
  {
    id: 'product-classic-jeans',
    name: '501 Original jeans',
    brand: "Levi's",
    referenceImageUrl: null,
    sourceUrl: 'https://www.levi.com/501-original',
    source: 'email',
    createdAt: '2026-08-22T09:00:00.000Z',
  },
]

function garment(
  id: string,
  createdAt: string,
  overrides: Partial<Garment> & Pick<Garment, 'classification'>,
): Garment {
  return {
    id,
    productId: null,
    photoUrl: placeholderPhoto(overrides.classification.color),
    purchaseInfo: null,
    source: 'manual',
    status: 'active',
    notes: null,
    createdAt,
    updatedAt: createdAt,
    archivedAt: null,
    ...overrides,
  }
}

export const sampleGarments: Garment[] = [
  garment('garment-blue-oxford', '2026-08-20T09:05:00.000Z', {
    classification: { category: 'top', color: 'blue', size: 'M' },
    productId: 'product-oxford-shirt',
    source: 'url',
    purchaseInfo: { price: { amount: 39.9, currency: 'USD' }, date: '2026-08-19' },
  }),
  garment('garment-black-jeans', '2026-08-22T09:05:00.000Z', {
    classification: { category: 'bottom', color: 'black', size: '32x32' },
    productId: 'product-classic-jeans',
    source: 'email',
    purchaseInfo: { price: { amount: 89000, currency: 'ARS' }, date: '2026-08-21' },
  }),
  garment('garment-white-tee', '2026-08-25T18:00:00.000Z', {
    classification: { category: 'top', color: 'white', size: 'L' },
    notes: 'Everyday tee, slightly oversized.',
  }),
  garment('garment-navy-coat', '2026-08-28T18:00:00.000Z', {
    classification: { category: 'outerwear', color: 'navy', size: 'M' },
    purchaseInfo: { price: { amount: 120, currency: 'EUR' }, date: '2025-11-02' },
    notes: 'Wool coat bought in Madrid.',
  }),
  garment('garment-red-dress', '2026-09-01T18:00:00.000Z', {
    classification: { category: 'dress', color: 'red', size: 'S' },
  }),
  garment('garment-white-sneakers', '2026-09-03T18:00:00.000Z', {
    classification: { category: 'footwear', color: 'white', size: '42' },
    purchaseInfo: { price: { amount: 150000, currency: 'ARS' }, date: '2026-09-02' },
  }),
  garment('garment-brown-belt', '2026-09-05T18:00:00.000Z', {
    classification: { category: 'accessory', color: 'brown', size: null },
  }),
  garment('garment-grey-hoodie', '2026-07-10T18:00:00.000Z', {
    classification: { category: 'top', color: 'grey', size: 'L' },
    status: 'archived',
    archivedAt: '2026-09-08T10:00:00.000Z',
    updatedAt: '2026-09-08T10:00:00.000Z',
    notes: 'Worn out; kept on record.',
  }),
]
