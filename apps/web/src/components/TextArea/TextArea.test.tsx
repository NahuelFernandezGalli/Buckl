import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { TextArea } from './TextArea'

describe('TextArea', () => {
  afterEach(cleanup)

  it('is reachable by its label', () => {
    render(<TextArea label="Notes" />)
    expect(screen.getByLabelText('Notes')).toBeInTheDocument()
  })

  it('exposes the error as its description and marks itself invalid', () => {
    render(<TextArea label="Notes" error="Notes can have at most 500 characters" />)
    const field = screen.getByLabelText('Notes')
    expect(field).toBeInvalid()
    expect(field).toHaveAccessibleDescription('Notes can have at most 500 characters')
  })
})
