import { beforeEach, describe, expect, it } from 'vitest'
import {
  ArchivedGarmentIsReadOnlyError,
  GarmentAlreadyArchivedError,
  GarmentNotArchivedError,
  GarmentNotFoundError,
} from '../domain/errors'
import type { Garment } from '../domain/garment'
import { DEFAULT_WARDROBE_FILTER } from '../domain/wardrobe-filter'
import { GarmentMother, ProductMother } from '../test/garment-mother'
import { InMemoryGarmentRepository } from './in-memory-garment-repository'

const NOW = new Date('2026-09-15T12:00:00.000Z')

const photoBlob = (content: string) => new Blob([content], { type: 'image/jpeg' })

function repositoryWith(...garments: Array<Partial<Garment>>) {
  let counter = 0
  return new InMemoryGarmentRepository(
    garments.map((overrides, index) => GarmentMother.active({ id: `g${index + 1}`, ...overrides })),
    {
      products: [ProductMother.fromUrl({ id: 'p1', name: 'Oxford shirt', brand: 'Uniqlo' })],
      now: () => NOW,
      generateId: () => `new-${++counter}`,
    },
  )
}

describe('InMemoryGarmentRepository', () => {
  describe('list', () => {
    it('returns only active garments by default', async () => {
      const repository = repositoryWith({}, { status: 'archived' })
      const garments = await repository.list(DEFAULT_WARDROBE_FILTER)
      expect(garments.map((g) => g.id)).toEqual(['g1'])
    })

    it('returns archived garments when the status asks for them', async () => {
      const repository = repositoryWith({}, { status: 'archived' })
      const garments = await repository.list({ ...DEFAULT_WARDROBE_FILTER, status: 'archived' })
      expect(garments.map((g) => g.id)).toEqual(['g2'])
    })

    it('filters by category and color', async () => {
      const repository = repositoryWith(
        { classification: { category: 'top', color: 'blue', size: null } },
        { classification: { category: 'bottom', color: 'black', size: null } },
      )
      expect(
        (await repository.list({ ...DEFAULT_WARDROBE_FILTER, category: 'bottom' })).map(
          (g) => g.id,
        ),
      ).toEqual(['g2'])
      expect(
        (await repository.list({ ...DEFAULT_WARDROBE_FILTER, color: 'blue' })).map((g) => g.id),
      ).toEqual(['g1'])
    })

    it('filters by size ignoring case', async () => {
      const repository = repositoryWith(
        { classification: { category: 'top', color: 'blue', size: 'M' } },
        { classification: { category: 'top', color: 'blue', size: 'L' } },
      )
      const garments = await repository.list({ ...DEFAULT_WARDROBE_FILTER, size: 'm' })
      expect(garments.map((g) => g.id)).toEqual(['g1'])
    })

    it('matches search text against notes, size and the linked product name and brand', async () => {
      const repository = repositoryWith(
        { notes: 'Bought in Madrid' },
        { classification: { category: 'top', color: 'blue', size: '32x32' } },
        { productId: 'p1' },
        {},
      )
      const search = (searchText: string) =>
        repository
          .list({ ...DEFAULT_WARDROBE_FILTER, searchText })
          .then((gs) => gs.map((g) => g.id))
      expect(await search('madrid')).toEqual(['g1'])
      expect(await search('32x')).toEqual(['g2'])
      expect(await search('oxford')).toEqual(['g3'])
      expect(await search('uniqlo')).toEqual(['g3'])
      expect(await search('nothing')).toEqual([])
    })

    it('lists newest first', async () => {
      const repository = repositoryWith(
        { createdAt: '2026-09-01T00:00:00.000Z' },
        { createdAt: '2026-09-05T00:00:00.000Z' },
      )
      const garments = await repository.list(DEFAULT_WARDROBE_FILTER)
      expect(garments.map((g) => g.id)).toEqual(['g2', 'g1'])
    })
  })

  describe('getById', () => {
    it('returns the garment or null', async () => {
      const repository = repositoryWith({})
      expect((await repository.getById('g1'))?.id).toBe('g1')
      expect(await repository.getById('missing')).toBeNull()
    })

    it('returns a copy, so callers cannot mutate the stored garment', async () => {
      const repository = repositoryWith({ notes: 'original' })
      const garment = await repository.getById('g1')
      garment!.notes = 'changed'
      expect((await repository.getById('g1'))!.notes).toBe('original')
    })
  })

  describe('create', () => {
    it('assigns an id, timestamps, manual source and active status', async () => {
      const repository = repositoryWith()
      const garment = await repository.create({
        photo: null,
        classification: { category: 'dress', color: 'red', size: 'S' },
        purchaseInfo: { price: { amount: 45000, currency: 'ARS' }, date: '2026-03-15' },
        notes: null,
      })
      expect(garment).toMatchObject({
        id: 'new-1',
        source: 'manual',
        status: 'active',
        createdAt: '2026-09-15T12:00:00.000Z',
        updatedAt: '2026-09-15T12:00:00.000Z',
        archivedAt: null,
        productId: null,
        photoUrl: null,
      })
      expect((await repository.list(DEFAULT_WARDROBE_FILTER)).map((g) => g.id)).toEqual(['new-1'])
    })

    it('stores a photo blob as a data URL', async () => {
      const repository = repositoryWith()
      const garment = await repository.create({
        photo: photoBlob('photo'),
        classification: { category: 'dress', color: 'red', size: 'S' },
        purchaseInfo: null,
        notes: null,
      })
      expect(garment.photoUrl).toBe('data:image/jpeg;base64,cGhvdG8=')
    })
  })

  describe('update', () => {
    let repository: InMemoryGarmentRepository

    beforeEach(() => {
      repository = repositoryWith({ notes: 'old' }, { status: 'archived' })
    })

    it('merges the changes and stamps updatedAt', async () => {
      const updated = await repository.update('g1', {
        classification: { category: 'top', color: 'navy', size: 'L' },
      })
      expect(updated.classification.color).toBe('navy')
      expect(updated.notes).toBe('old')
      expect(updated.updatedAt).toBe('2026-09-15T12:00:00.000Z')
    })

    it('allows clearing optional fields with null', async () => {
      const updated = await repository.update('g1', { notes: null })
      expect(updated.notes).toBeNull()
    })

    it('keeps the current photo when the changes leave it out', async () => {
      const current = await repository.getById('g1')
      const updated = await repository.update('g1', { notes: 'new' })
      expect(updated.photoUrl).toBe(current!.photoUrl)
    })

    it('removes the photo with null', async () => {
      const updated = await repository.update('g1', { photo: null })
      expect(updated.photoUrl).toBeNull()
    })

    it('replaces the photo with a blob, stored as a data URL', async () => {
      const updated = await repository.update('g1', { photo: photoBlob('new') })
      expect(updated.photoUrl).toBe('data:image/jpeg;base64,bmV3')
    })

    it('rejects an unknown garment', async () => {
      await expect(repository.update('missing', { notes: 'x' })).rejects.toBeInstanceOf(
        GarmentNotFoundError,
      )
    })

    it('rejects editing an archived garment', async () => {
      await expect(repository.update('g2', { notes: 'x' })).rejects.toBeInstanceOf(
        ArchivedGarmentIsReadOnlyError,
      )
    })
  })

  describe('archive and restore', () => {
    it('archives an active garment', async () => {
      const repository = repositoryWith({})
      const archived = await repository.archive('g1')
      expect(archived.status).toBe('archived')
      expect(archived.archivedAt).toBe('2026-09-15T12:00:00.000Z')
      expect(await repository.list(DEFAULT_WARDROBE_FILTER)).toEqual([])
    })

    it('rejects archiving twice', async () => {
      const repository = repositoryWith({ status: 'archived' })
      await expect(repository.archive('g1')).rejects.toBeInstanceOf(GarmentAlreadyArchivedError)
    })

    it('restores an archived garment', async () => {
      const repository = repositoryWith({
        status: 'archived',
        archivedAt: '2026-09-10T00:00:00.000Z',
      })
      const restored = await repository.restore('g1')
      expect(restored.status).toBe('active')
      expect(restored.archivedAt).toBeNull()
    })

    it('rejects restoring an active garment', async () => {
      const repository = repositoryWith({})
      await expect(repository.restore('g1')).rejects.toBeInstanceOf(GarmentNotArchivedError)
    })

    it('rejects an unknown garment', async () => {
      const repository = repositoryWith()
      await expect(repository.archive('missing')).rejects.toBeInstanceOf(GarmentNotFoundError)
    })
  })
})
