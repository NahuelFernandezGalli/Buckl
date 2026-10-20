import type { Garment } from '../domain/garment'
import type { GarmentChanges, GarmentRepository, NewGarment } from '../domain/garment-repository'
import type { WardrobeFilter } from '../domain/wardrobe-filter'
import type { ApiClient } from './api/api-client'
import { ApiError } from './api/api-errors'
import { requestJson } from './api/responses'
import type { PhotoUploader } from './photos/photo-uploader'
import { wardrobeQuery } from './wardrobe-query'

export interface HttpGarmentRepositoryOptions {
  api: ApiClient
  uploadPhoto: PhotoUploader
}

/**
 * The wardrobe over the Buckl API (ADR-0019, ADR-0034). The API answers in the shape of `Garment`
 * already; photos are uploaded first and travel as the id of their upload (ADR-0032).
 */
export class HttpGarmentRepository implements GarmentRepository {
  private readonly api: ApiClient
  private readonly uploadPhoto: PhotoUploader

  constructor({ api, uploadPhoto }: HttpGarmentRepositoryOptions) {
    this.api = api
    this.uploadPhoto = uploadPhoto
  }

  list(filter: WardrobeFilter): Promise<Garment[]> {
    return requestJson<Garment[]>(this.api, `/garments?${wardrobeQuery(filter)}`)
  }

  async getById(id: string): Promise<Garment | null> {
    try {
      return await requestJson<Garment>(this.api, garmentPath(id))
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) return null
      throw error
    }
  }

  async create({ photo, classification, purchaseInfo, notes }: NewGarment): Promise<Garment> {
    const upload = photo ? { uploadId: await this.uploadPhoto(photo) } : undefined
    return requestJson<Garment>(this.api, '/garments', {
      method: 'POST',
      body: { classification, purchaseInfo, notes, photo: upload },
    })
  }

  async update(id: string, changes: GarmentChanges): Promise<Garment> {
    const body: Record<string, unknown> = {}
    if (changes.classification !== undefined) body.classification = changes.classification
    if (changes.purchaseInfo !== undefined) body.purchaseInfo = changes.purchaseInfo
    if (changes.notes !== undefined) body.notes = changes.notes
    if (changes.photo === null) body.photo = null
    else if (changes.photo) body.photo = { uploadId: await this.uploadPhoto(changes.photo) }
    return requestJson<Garment>(this.api, garmentPath(id), { method: 'PATCH', body })
  }

  archive(id: string): Promise<Garment> {
    return requestJson<Garment>(this.api, `${garmentPath(id)}/archive`, { method: 'POST' })
  }

  restore(id: string): Promise<Garment> {
    return requestJson<Garment>(this.api, `${garmentPath(id)}/restore`, { method: 'POST' })
  }
}

/** An id is data from the address bar: encoded, it can never leave its path segment. */
function garmentPath(id: string): string {
  return `/garments/${encodeURIComponent(id)}`
}
