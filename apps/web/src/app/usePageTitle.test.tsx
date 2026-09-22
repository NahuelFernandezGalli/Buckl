import { cleanup, renderHook } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { usePageTitle } from './usePageTitle'

describe('usePageTitle', () => {
  afterEach(cleanup)

  it('sets the document title with the app name as suffix', () => {
    renderHook(() => usePageTitle('Wardrobe'))
    expect(document.title).toBe('Wardrobe · Buckl')
  })

  it('updates the title when it changes', () => {
    const { rerender } = renderHook(({ title }) => usePageTitle(title), {
      initialProps: { title: 'Wardrobe' },
    })
    rerender({ title: 'Add garment' })
    expect(document.title).toBe('Add garment · Buckl')
  })
})
