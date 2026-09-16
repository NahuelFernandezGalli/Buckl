import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { EmptyState } from './EmptyState'

describe('EmptyState', () => {
  afterEach(cleanup)

  it('is a region named by its title', () => {
    render(<EmptyState title="Your wardrobe is empty" />)
    expect(screen.getByRole('region', { name: 'Your wardrobe is empty' })).toBeInTheDocument()
  })

  it('shows the description and the action', () => {
    render(
      <EmptyState
        title="Your wardrobe is empty"
        description="Photograph a garment to get started."
        action={<button type="button">Add your first garment</button>}
      />,
    )
    expect(screen.getByText('Photograph a garment to get started.')).toBeVisible()
    expect(screen.getByRole('button', { name: 'Add your first garment' })).toBeVisible()
  })
})
