import type { PhotoCodec } from './prepare-photo'

/**
 * The browser's own image codec: `createImageBitmap` applies the EXIF orientation, a canvas crops
 * and scales, and `toBlob` writes a JPEG without metadata. Transparent pixels (a PNG) turn white
 * rather than black. jsdom has neither API, so this is checked by hand (docs/testing.md).
 */
export const browserPhotoCodec: PhotoCodec<ImageBitmap> = {
  decode: (file) => createImageBitmap(file, { imageOrientation: 'from-image' }),
  encode: (bitmap, { source, output }, quality) =>
    new Promise((resolve, reject) => {
      const canvas = document.createElement('canvas')
      canvas.width = output.width
      canvas.height = output.height
      const context = canvas.getContext('2d')
      if (!context) {
        reject(new Error('This browser cannot draw images.'))
        return
      }
      context.fillStyle = '#ffffff'
      context.fillRect(0, 0, output.width, output.height)
      context.imageSmoothingQuality = 'high'
      context.drawImage(
        bitmap,
        source.x,
        source.y,
        source.width,
        source.height,
        0,
        0,
        output.width,
        output.height,
      )
      canvas.toBlob(
        (blob) =>
          blob ? resolve(blob) : reject(new Error('The browser could not encode the photo.')),
        'image/jpeg',
        quality,
      )
    }),
}
