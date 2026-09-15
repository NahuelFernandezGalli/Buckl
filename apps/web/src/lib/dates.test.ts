import { describe, expect, it } from 'vitest'
import { todayIsoDate } from './dates'

describe('todayIsoDate', () => {
  it('formats the local calendar date as YYYY-MM-DD', () => {
    expect(todayIsoDate(new Date(2026, 8, 15, 23, 30))).toBe('2026-09-15')
  })

  it('pads month and day', () => {
    expect(todayIsoDate(new Date(2026, 0, 5))).toBe('2026-01-05')
  })
})
