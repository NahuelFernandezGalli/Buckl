import type { Product } from './product'

export interface ProductRepository {
  getById(id: string): Promise<Product | null>
}
