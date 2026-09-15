import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Select } from './Select'

const options = [
  { value: 'top', label: 'Top' },
  { value: 'bottom', label: 'Bottom' },
]

describe('Select', () => {
  afterEach(cleanup)

  it('offers a placeholder option with an empty value', () => {
    render(<Select label="Category" options={options} placeholder="Choose a category" />)
    expect(screen.getByRole('option', { name: 'Choose a category' })).toHaveValue('')
  })

  it('reports the chosen value', async () => {
    const onChange = vi.fn()
    render(<Select label="Category" options={options} onChange={onChange} />)
    await userEvent.setup().selectOptions(screen.getByLabelText('Category'), 'bottom')
    expect(onChange).toHaveBeenCalled()
    expect(screen.getByLabelText('Category')).toHaveValue('bottom')
  })

  it('exposes the error as its description', () => {
    render(<Select label="Category" options={options} error="Choose a category" />)
    expect(screen.getByLabelText('Category')).toHaveAccessibleDescription('Choose a category')
  })
})
