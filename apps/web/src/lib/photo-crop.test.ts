import { describe, expect, it } from 'vitest'
import { planPhotoCrop } from './photo-crop'

describe('planPhotoCrop', () => {
  it.each([
    {
      photo: 'a portrait phone photo',
      size: { width: 3024, height: 4032 },
      source: { x: 0, y: 126, width: 3024, height: 3780 },
      output: { width: 1080, height: 1350 },
    },
    {
      photo: 'a landscape phone photo',
      size: { width: 4032, height: 3024 },
      source: { x: 806, y: 0, width: 2419, height: 3024 },
      output: { width: 1080, height: 1350 },
    },
    {
      photo: 'a square photo',
      size: { width: 1000, height: 1000 },
      source: { x: 100, y: 0, width: 800, height: 1000 },
      output: { width: 800, height: 1000 },
    },
    {
      photo: 'a small photo already in 4:5',
      size: { width: 400, height: 500 },
      source: { x: 0, y: 0, width: 400, height: 500 },
      output: { width: 400, height: 500 },
    },
  ])('keeps the centered 4:5 part of $photo, never wider than 1080', ({ size, source, output }) => {
    expect(planPhotoCrop(size)).toEqual({ source, output })
  })

  it('never scales a photo up', () => {
    expect(planPhotoCrop({ width: 600, height: 900 }).output).toEqual({
      width: 600,
      height: 750,
    })
  })

  it('handles an extremely narrow photo', () => {
    expect(planPhotoCrop({ width: 3, height: 1000 })).toEqual({
      source: { x: 0, y: 498, width: 3, height: 4 },
      output: { width: 3, height: 4 },
    })
  })

  it.each([
    { width: 0, height: 100 },
    { width: 100, height: -1 },
    { width: Number.NaN, height: 100 },
  ])('refuses a photo without a positive size: $width×$height', (size) => {
    expect(() => planPhotoCrop(size)).toThrow(RangeError)
  })
})
