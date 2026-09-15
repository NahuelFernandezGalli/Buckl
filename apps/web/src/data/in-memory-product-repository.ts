import type { Product } from '../domain/product'
import type { ProductRepository } from '../domain/product-repository'

export class InMemoryProductRepository implements ProductRepository {
  private readonly products: ReadonlyMap<string, Product>

  constructor(seed: ReadonlyArray<Product> = []) {
    this.products = new Map(seed.map((product) => [product.id, product]))
  }

  async getById(id: string): Promise<Product | null> {
    const product = this.products.get(id)
    return product ? { ...product } : null
  }
}
