import { describe, expect, it } from 'vitest'
import { fakeFetch, jsonResponse, problemResponse, testApiClient } from '../test/fake-fetch'
import { ProductMother } from '../test/garment-mother'
import { HttpProductRepository } from './http-product-repository'

describe('HttpProductRepository', () => {
  it('finds a product by id', async () => {
    const http = fakeFetch()
    const product = ProductMother.fromUrl({ id: 'p-1' })
    http.on('GET /products/p-1', () => jsonResponse(product))

    await expect(
      new HttpProductRepository({ api: testApiClient(http) }).getById('p-1'),
    ).resolves.toEqual(product)
  })

  it('answers null for a product the API cannot find', async () => {
    const http = fakeFetch()
    http.on('GET /products/p-2', () => problemResponse(404, 'product.not_found'))

    await expect(
      new HttpProductRepository({ api: testApiClient(http) }).getById('p-2'),
    ).resolves.toBeNull()
  })
})
