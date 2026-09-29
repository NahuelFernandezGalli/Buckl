import { planPhotoCrop, type CropPlan } from '../../lib/photo-crop'

/** A decoded image, as wide and tall as it is displayed (EXIF orientation applied). */
export interface DecodedPhoto {
  readonly width: number
  readonly height: number
  close(): void
}

/** How the browser reads and writes images. Tests pass a fake: jsdom has no canvas. */
export interface PhotoCodec<Photo extends DecodedPhoto = DecodedPhoto> {
  decode(file: Blob): Promise<Photo>
  /** Draws `plan.source` of the photo at `plan.output` size, as a JPEG of the given quality. */
  encode(photo: Photo, plan: CropPlan, quality: number): Promise<Blob>
}

/** Visually close to the original at 1080×1350, at a few hundred kilobytes. */
export const PHOTO_QUALITY = 0.82

export class PhotoUnreadableError extends Error {
  readonly code = 'photo.unreadable'

  constructor(cause: unknown) {
    super('The browser could not read this photo.', { cause })
    this.name = 'PhotoUnreadableError'
  }
}

/**
 * What the app stores of a picked photo (ADR-0033): its centered 4:5 part, at most 1080×1350, as
 * a JPEG. Re-encoding drops the file's metadata, GPS location included.
 */
export async function preparePhoto<Photo extends DecodedPhoto>(
  file: Blob,
  codec: PhotoCodec<Photo>,
): Promise<Blob> {
  let photo: Photo
  try {
    photo = await codec.decode(file)
  } catch (cause) {
    throw new PhotoUnreadableError(cause)
  }
  try {
    return await codec.encode(photo, planPhotoCrop(photo), PHOTO_QUALITY)
  } finally {
    photo.close()
  }
}
