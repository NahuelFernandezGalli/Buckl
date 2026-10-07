import { useState } from 'react'
import { placeholderPhoto } from '../../data/placeholder-photo'
import type { Color } from '../../domain/classification'

export interface GarmentPhotoProps {
  photoUrl: string | null
  color: Color
  alt: string
  className?: string
}

/**
 * A garment's photo, or its color when there is none or it does not load: signed URLs expire after
 * an hour (ADR-0032), and storage can be unreachable. A new URL is tried again.
 */
export function GarmentPhoto({ photoUrl, color, alt, className }: GarmentPhotoProps) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null)
  const usable = photoUrl !== null && photoUrl !== failedUrl
  return (
    <img
      src={usable ? photoUrl : placeholderPhoto(color)}
      alt={alt}
      className={className}
      onError={usable ? () => setFailedUrl(photoUrl) : undefined}
    />
  )
}
