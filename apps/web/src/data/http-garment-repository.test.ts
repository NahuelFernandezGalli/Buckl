import { describe, expect, it } from 'vitest'
import { DEFAULT_WARDROBE_FILTER } from '../domain/wardrobe-filter'
import { fakeFetch, jsonResponse, problemResponse, testApiClient } from '../test/fake-fetch'
import { GarmentMother } from '../test/garment-mother'
import { ApiError } from './api/api-errors'
import { HttpGarmentRepository } from './http-garment-repository'

const ID = '0b6f5a86-3c1f-4a4b-9a57-2f6de4a1c0d1'

function setup() {
  const http = fakeFetch()
  const uploaded: Blob[] = []
  const repository = new HttpGarmentRepository({
    api: testApiClient(http),
    uploadPhoto: async (photo) => {
      uploaded.push(photo)
      return 'upload-1'
    },
  })
  return { http, uploaded, repository }
}

const shirt = GarmentMother.active({
  id: ID,
  photoUrl: 'https://storage.test/users/a/garments/b.jpg?sig',
})
const photo = () => new File(['photo'], 'shirt.jpg', { type: 'image/jpeg' })

describe('HttpGarmentRepository', () => {
  it('lists the wardrobe for a filter', async () => {
    const { http, repository } = setup()
    http.on('GET /garments?category=top&status=active', () => jsonResponse([shirt]))

    await expect(repository.list({ ...DEFAULT_WARDROBE_FILTER, category: 'top' })).resolves.toEqual(
      [shirt],
    )
  })

  it('finds a garment by id', async () => {
    const { http, repository } = setup()
    http.on(`GET /garments/${ID}`, () => jsonResponse(shirt))

    await expect(repository.getById(ID)).resolves.toEqual(shirt)
  })

  it.each([
    ['one the API cannot find', 'garment.not_found'],
    ['an id that is not a garment id', 'resource.not_found'],
  ])('answers null for %s', async (_case, code) => {
    const { http, repository } = setup()
    http.on('GET /garments/g-42', () => problemResponse(404, code))

    await expect(repository.getById('g-42')).resolves.toBeNull()
  })

  it('lets any other failure of a lookup through', async () => {
    const { http, repository } = setup()
    http.on(`GET /garments/${ID}`, () => problemResponse(500, 'server.error'))

    await expect(repository.getById(ID)).rejects.toBeInstanceOf(ApiError)
  })

  it('adds a garment without a photo, sending no photo at all', async () => {
    const { http, uploaded, repository } = setup()
    http.on('POST /garments', () => jsonResponse(shirt, 201))

    await repository.create({
      photo: null,
      classification: { category: 'top', color: 'blue', size: 'M' },
      purchaseInfo: { price: { amount: 45000, currency: 'ARS' }, date: '2026-03-15' },
      notes: 'Linen',
    })

    expect(uploaded).toEqual([])
    expect(http.requests[0].body).toEqual({
      classification: { category: 'top', color: 'blue', size: 'M' },
      purchaseInfo: { price: { amount: 45000, currency: 'ARS' }, date: '2026-03-15' },
      notes: 'Linen',
    })
  })

  it('uploads the photo first and adds the garment with its upload id', async () => {
    const { http, uploaded, repository } = setup()
    http.on('POST /garments', () => jsonResponse(shirt, 201))
    const picked = photo()

    const created = await repository.create({
      photo: picked,
      classification: { category: 'top', color: 'blue', size: null },
      purchaseInfo: null,
      notes: null,
    })

    expect(uploaded).toEqual([picked])
    expect(http.requests[0].body).toMatchObject({ photo: { uploadId: 'upload-1' } })
    expect(created).toEqual(shirt)
  })

  it('does not add the garment when its photo does not upload', async () => {
    const http = fakeFetch()
    const repository = new HttpGarmentRepository({
      api: testApiClient(http),
      uploadPhoto: async () => {
        throw new Error('upload failed')
      },
    })

    await expect(
      repository.create({
        photo: photo(),
        classification: { category: 'top', color: 'blue', size: null },
        purchaseInfo: null,
        notes: null,
      }),
    ).rejects.toThrow('upload failed')
    expect(http.requests).toEqual([])
  })

  it('edits only what changed', async () => {
    const { http, uploaded, repository } = setup()
    http.on(`PATCH /garments/${ID}`, () => jsonResponse(shirt))

    await repository.update(ID, { notes: null })

    expect(uploaded).toEqual([])
    expect(http.requests[0].body).toEqual({ notes: null })
  })

  it('removes the photo when it is set to null', async () => {
    const { http, repository } = setup()
    http.on(`PATCH /garments/${ID}`, () => jsonResponse(shirt))

    await repository.update(ID, { photo: null })

    expect(http.requests[0].body).toEqual({ photo: null })
  })

  it('replaces the photo with a new upload', async () => {
    const { http, uploaded, repository } = setup()
    http.on(`PATCH /garments/${ID}`, () => jsonResponse(shirt))
    const picked = photo()

    await repository.update(ID, { photo: picked, notes: 'Ironed' })

    expect(uploaded).toEqual([picked])
    expect(http.requests[0].body).toEqual({ photo: { uploadId: 'upload-1' }, notes: 'Ironed' })
  })

  it('archives and restores through their own endpoints', async () => {
    const { http, repository } = setup()
    http.on(`POST /garments/${ID}/archive`, () => jsonResponse({ ...shirt, status: 'archived' }))
    http.on(`POST /garments/${ID}/restore`, () => jsonResponse(shirt))

    await expect(repository.archive(ID)).resolves.toMatchObject({ status: 'archived' })
    await expect(repository.restore(ID)).resolves.toMatchObject({ status: 'active' })
  })

  it('reports a rule of the API with its code', async () => {
    const { http, repository } = setup()
    http.on(`POST /garments/${ID}/archive`, () => problemResponse(409, 'garment.already_archived'))

    await expect(repository.archive(ID)).rejects.toMatchObject({ code: 'garment.already_archived' })
  })

  it('keeps an id inside its path segment', async () => {
    const { http, repository } = setup()
    http.on('GET /garments/a%2F..%2Fproducts', () => problemResponse(404, 'resource.not_found'))

    await expect(repository.getById('a/../products')).resolves.toBeNull()
  })
})
