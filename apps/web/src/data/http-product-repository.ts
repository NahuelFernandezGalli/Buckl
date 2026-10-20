import type { Product } from '../domain/product'
import type { ProductRepository } from '../domain/product-repository'
import type { ApiClient } from './api/api-client'
import { ApiError } from './api/api-errors'
import { requestJson } from './api/responses'

/** Catalog products over the Buckl API. Read only: products are created by imports (phase 7). */
export class HttpProductRepository implements ProductRepository {
  private readonly api: ApiClient

  constructor({ api }: { api: ApiClient }) {
    this.api = api
  }

  async getById(id: string): Promise<Product | null> {
    try {
      return await requestJson<Product>(this.api, `/products/${encodeURIComponent(id)}`)
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) return null
      throw error
    }
  }
}
