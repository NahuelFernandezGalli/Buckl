import { InMemoryGarmentRepository } from '../data/in-memory-garment-repository'
import type { Garment } from '../domain/garment'
import type { NewGarment } from '../domain/garment-repository'
import type { WardrobeFilter } from '../domain/wardrobe-filter'

/**
 * The in-memory repository with failures queued per method, to play what the network does to the
 * HTTP one. Each queued error fails one call, in order; after that, calls behave normally. Plain
 * arrays rather than mocks: vitest-cucumber runs every step as its own test.
 */
export class ScriptedGarmentRepository extends InMemoryGarmentRepository {
  readonly failures: { list: Error[]; create: Error[] } = { list: [], create: [] }

  override async list(filter: WardrobeFilter): Promise<Garment[]> {
    const failure = this.failures.list.shift()
    if (failure) throw failure
    return super.list(filter)
  }

  override async create(garment: NewGarment): Promise<Garment> {
    const failure = this.failures.create.shift()
    if (failure) throw failure
    return super.create(garment)
  }
}
