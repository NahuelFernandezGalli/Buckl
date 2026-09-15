import {
  ArchivedGarmentIsReadOnlyError,
  GarmentAlreadyArchivedError,
  GarmentNotArchivedError,
  GarmentNotFoundError,
} from '../domain/errors'
import type { Garment } from '../domain/garment'
import type { GarmentChanges, GarmentRepository, NewGarment } from '../domain/garment-repository'
import type { Product } from '../domain/product'
import type { WardrobeFilter } from '../domain/wardrobe-filter'

export interface InMemoryGarmentRepositoryOptions {
  /** Products the search text can match by name and brand. */
  products?: ReadonlyArray<Product>
  now?: () => Date
  generateId?: () => string
}

/** Phase 3 stand-in for the API. Mirrors the domain rules the UI has to react to. */
export class InMemoryGarmentRepository implements GarmentRepository {
  private readonly garments = new Map<string, Garment>()
  private readonly products: ReadonlyMap<string, Product>
  private readonly now: () => Date
  private readonly generateId: () => string

  constructor(seed: ReadonlyArray<Garment> = [], options: InMemoryGarmentRepositoryOptions = {}) {
    for (const garment of seed) {
      this.garments.set(garment.id, copy(garment))
    }
    this.products = new Map((options.products ?? []).map((product) => [product.id, product]))
    this.now = options.now ?? (() => new Date())
    this.generateId = options.generateId ?? (() => crypto.randomUUID())
  }

  async list(filter: WardrobeFilter): Promise<Garment[]> {
    return [...this.garments.values()]
      .filter((garment) => this.matches(garment, filter))
      .sort((a, b) => b.createdAt.localeCompare(a.createdAt))
      .map(copy)
  }

  async getById(id: string): Promise<Garment | null> {
    const garment = this.garments.get(id)
    return garment ? copy(garment) : null
  }

  async create(input: NewGarment): Promise<Garment> {
    const timestamp = this.now().toISOString()
    const garment: Garment = {
      id: this.generateId(),
      productId: null,
      photoUrl: input.photoUrl,
      classification: input.classification,
      purchaseInfo: input.purchaseInfo,
      source: 'manual',
      status: 'active',
      notes: input.notes,
      createdAt: timestamp,
      updatedAt: timestamp,
      archivedAt: null,
    }
    this.garments.set(garment.id, garment)
    return copy(garment)
  }

  async update(id: string, changes: GarmentChanges): Promise<Garment> {
    const current = this.require(id)
    if (current.status === 'archived') {
      throw new ArchivedGarmentIsReadOnlyError(id)
    }
    const updated: Garment = {
      ...current,
      photoUrl: changes.photoUrl !== undefined ? changes.photoUrl : current.photoUrl,
      classification: changes.classification ?? current.classification,
      purchaseInfo:
        changes.purchaseInfo !== undefined ? changes.purchaseInfo : current.purchaseInfo,
      notes: changes.notes !== undefined ? changes.notes : current.notes,
      updatedAt: this.now().toISOString(),
    }
    this.garments.set(id, updated)
    return copy(updated)
  }

  async archive(id: string): Promise<Garment> {
    const current = this.require(id)
    if (current.status === 'archived') {
      throw new GarmentAlreadyArchivedError(id)
    }
    const timestamp = this.now().toISOString()
    const archived: Garment = {
      ...current,
      status: 'archived',
      archivedAt: timestamp,
      updatedAt: timestamp,
    }
    this.garments.set(id, archived)
    return copy(archived)
  }

  async restore(id: string): Promise<Garment> {
    const current = this.require(id)
    if (current.status !== 'archived') {
      throw new GarmentNotArchivedError(id)
    }
    const restored: Garment = {
      ...current,
      status: 'active',
      archivedAt: null,
      updatedAt: this.now().toISOString(),
    }
    this.garments.set(id, restored)
    return copy(restored)
  }

  private require(id: string): Garment {
    const garment = this.garments.get(id)
    if (!garment) {
      throw new GarmentNotFoundError(id)
    }
    return garment
  }

  private matches(garment: Garment, filter: WardrobeFilter): boolean {
    if (garment.status !== filter.status) return false
    if (filter.category && garment.classification.category !== filter.category) return false
    if (filter.color && garment.classification.color !== filter.color) return false
    if (filter.size && garment.classification.size?.toLowerCase() !== filter.size.toLowerCase()) {
      return false
    }
    if (filter.searchText) {
      const product = garment.productId ? this.products.get(garment.productId) : undefined
      const haystack = [garment.notes, garment.classification.size, product?.name, product?.brand]
        .filter((value): value is string => typeof value === 'string')
        .join(' ')
        .toLowerCase()
      if (!haystack.includes(filter.searchText.toLowerCase())) return false
    }
    return true
  }
}

function copy(garment: Garment): Garment {
  return {
    ...garment,
    classification: { ...garment.classification },
    purchaseInfo: garment.purchaseInfo
      ? { ...garment.purchaseInfo, price: { ...garment.purchaseInfo.price } }
      : null,
  }
}
