export interface Size {
  width: number
  height: number
}

export interface CropPlan {
  /** The part of the source photo to keep, in source pixels. */
  source: { x: number; y: number; width: number; height: number }
  /** The size it is drawn at. */
  output: Size
}

/** The shape every garment photo is shown in: card, detail and preview use `aspect-ratio: 4 / 5`. */
export const PHOTO_ASPECT_RATIO = 4 / 5

/** Enough for the detail screen on a phone at 3x; more only costs upload time and storage. */
export const MAX_PHOTO_WIDTH = 1080

/**
 * The largest centered 4:5 region of a photo, scaled down (never up) to at most 1080 pixels wide.
 * What `object-fit: cover` shows is exactly what is kept (ADR-0033).
 */
export function planPhotoCrop({ width, height }: Size): CropPlan {
  if (!(width > 0 && height > 0)) {
    throw new RangeError('A photo has a positive width and height.')
  }
  const wide = width / height > PHOTO_ASPECT_RATIO
  const cropWidth = wide ? Math.round(height * PHOTO_ASPECT_RATIO) : width
  const cropHeight = wide ? height : Math.round(width / PHOTO_ASPECT_RATIO)
  const outputWidth = Math.min(cropWidth, MAX_PHOTO_WIDTH)
  return {
    source: {
      x: Math.floor((width - cropWidth) / 2),
      y: Math.floor((height - cropHeight) / 2),
      width: cropWidth,
      height: cropHeight,
    },
    output: {
      width: outputWidth,
      height: Math.round(outputWidth / PHOTO_ASPECT_RATIO),
    },
  }
}
