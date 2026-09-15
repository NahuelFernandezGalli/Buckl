import type { Color } from '../domain/classification'

const FILL: Record<Color, string> = {
  black: '#1c1b1f',
  white: '#f4f2ee',
  grey: '#8d8a94',
  navy: '#1f2a5c',
  blue: '#2f6fde',
  red: '#c9302c',
  green: '#2e8b57',
  yellow: '#e6b800',
  brown: '#7b4b2a',
  beige: '#d9c4a3',
  pink: '#e58fb0',
  purple: '#7a4dd6',
  orange: '#e8772e',
  multicolor: '#9c6ade',
}

/** A flat SVG in the garment's color, so sample garments and garments without a photo still show something. */
export function placeholderPhoto(color: Color): string {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 4 5"><rect width="4" height="5" fill="${FILL[color]}"/></svg>`
  return `data:image/svg+xml,${encodeURIComponent(svg)}`
}
