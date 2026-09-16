import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { Input } from './Input'

describe('Input', () => {
  afterEach(cleanup)

  it('is reachable by its label', () => {
    render(<Input label="Size" />)
    expect(screen.getByLabelText('Size')).toBeInTheDocument()
  })

  it('describes itself with the hint', () => {
    render(<Input label="Size" hint="As the store states it" />)
    expect(screen.getByLabelText('Size')).toHaveAccessibleDescription('As the store states it')
  })

  it('exposes the error as its description and marks itself invalid', () => {
    render(<Input label="Size" error="Size can have at most 20 characters" />)
    const input = screen.getByLabelText('Size')
    expect(input).toBeInvalid()
    expect(input).toHaveAccessibleDescription('Size can have at most 20 characters')
  })

  it('is valid when there is no error', () => {
    render(<Input label="Size" />)
    expect(screen.getByLabelText('Size')).toBeValid()
  })
})
