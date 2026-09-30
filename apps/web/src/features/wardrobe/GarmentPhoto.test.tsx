import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { placeholderPhoto } from '../../data/placeholder-photo'
import { GarmentPhoto } from './GarmentPhoto'

const URL_1 = 'https://storage.test/users/a/garments/1.jpg?sig=1'

describe('GarmentPhoto', () => {
  afterEach(() => cleanup())

  it('shows the photo', () => {
    render(<GarmentPhoto photoUrl={URL_1} color="blue" alt="Blue top" />)

    expect(screen.getByRole('img', { name: 'Blue top' })).toHaveAttribute('src', URL_1)
  })

  it('shows the placeholder when there is no photo', () => {
    render(<GarmentPhoto photoUrl={null} color="blue" alt="Blue top" />)

    expect(screen.getByRole('img', { name: 'Blue top' })).toHaveAttribute(
      'src',
      placeholderPhoto('blue'),
    )
  })

  it('falls back to the placeholder when the photo does not load, and tries a new URL again', () => {
    const { rerender } = render(<GarmentPhoto photoUrl={URL_1} color="blue" alt="Blue top" />)

    fireEvent.error(screen.getByRole('img', { name: 'Blue top' }))
    expect(screen.getByRole('img', { name: 'Blue top' })).toHaveAttribute(
      'src',
      placeholderPhoto('blue'),
    )

    rerender(<GarmentPhoto photoUrl={`${URL_1}&renewed`} color="blue" alt="Blue top" />)
    expect(screen.getByRole('img', { name: 'Blue top' })).toHaveAttribute('src', `${URL_1}&renewed`)
  })
})
