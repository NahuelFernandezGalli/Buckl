import { describe, expect, it } from 'vitest'
import { ProductMother } from '../test/garment-mother'
import { InMemoryProductRepository } from './in-memory-product-repository'

describe('InMemoryProductRepository', () => {
  it('returns the product or null', async () => {
    const repository = new InMemoryProductRepository([ProductMother.fromUrl({ id: 'p1' })])
    expect((await repository.getById('p1'))?.name).toBe('Oxford shirt')
    expect(await repository.getById('missing')).toBeNull()
  })
})
