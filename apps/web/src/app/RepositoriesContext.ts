import { createContext } from 'react'
import type { GarmentRepository } from '../domain/garment-repository'
import type { ProductRepository } from '../domain/product-repository'

export interface Repositories {
  garments: GarmentRepository
  products: ProductRepository
}

export const RepositoriesContext = createContext<Repositories | null>(null)
