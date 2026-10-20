import { describe, expect, it } from 'vitest'
import type { CropPlan } from '../../lib/photo-crop'
import { PHOTO_QUALITY, PhotoUnreadableError, preparePhoto, type PhotoCodec } from './prepare-photo'

function fakeCodec(size: { width: number; height: number }, { failEncoding = false } = {}) {
  const log = {
    encoded: [] as Array<{ plan: CropPlan; quality: number }>,
    closed: 0,
  }
  const jpeg = new Blob(['jpeg'], { type: 'image/jpeg' })
  const codec: PhotoCodec = {
    decode: async () => ({
      ...size,
      close: () => {
        log.closed += 1
      },
    }),
    encode: async (_photo, plan, quality) => {
      if (failEncoding) throw new Error('No canvas')
      log.encoded.push({ plan, quality })
      return jpeg
    },
  }
  return { codec, log, jpeg }
}

const original = () => new File(['original'], 'IMG_0001.jpg', { type: 'image/jpeg' })

describe('preparePhoto', () => {
  it('keeps the centered 4:5 part of a phone photo as a JPEG of at most 1080×1350', async () => {
    const { codec, log, jpeg } = fakeCodec({ width: 3024, height: 4032 })

    const prepared = await preparePhoto(original(), codec)

    expect(prepared).toBe(jpeg)
    expect(log.encoded).toEqual([
      {
        plan: {
          source: { x: 0, y: 126, width: 3024, height: 3780 },
          output: { width: 1080, height: 1350 },
        },
        quality: PHOTO_QUALITY,
      },
    ])
    expect(log.closed).toBe(1)
  })

  it('releases the decoded photo even when encoding fails', async () => {
    const { codec, log } = fakeCodec({ width: 100, height: 100 }, { failEncoding: true })

    await expect(preparePhoto(original(), codec)).rejects.toThrow('No canvas')
    expect(log.closed).toBe(1)
  })

  it('reports a file the browser cannot read as an unreadable photo', async () => {
    const cause = new DOMException('The source image could not be decoded.', 'InvalidStateError')
    const codec: PhotoCodec = {
      decode: async () => {
        throw cause
      },
      encode: async () => new Blob(),
    }

    const result = preparePhoto(new File(['heic'], 'IMG_0002.HEIC', { type: 'image/heic' }), codec)

    await expect(result).rejects.toBeInstanceOf(PhotoUnreadableError)
    await expect(result).rejects.toMatchObject({ code: 'photo.unreadable', cause })
  })
})
