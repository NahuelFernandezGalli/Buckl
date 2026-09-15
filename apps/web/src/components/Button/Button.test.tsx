import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createRef } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Button } from './Button'

describe('Button', () => {
  afterEach(cleanup)

  it('calls onClick when pressed', async () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Save</Button>)
    await userEvent.setup().click(screen.getByRole('button', { name: 'Save' }))
    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('does not call onClick when disabled', async () => {
    const onClick = vi.fn()
    render(
      <Button onClick={onClick} disabled>
        Save
      </Button>,
    )
    await userEvent.setup().click(screen.getByRole('button', { name: 'Save' }))
    expect(onClick).not.toHaveBeenCalled()
  })

  it('is a plain button by default so it never submits a form by accident', () => {
    render(<Button>Save</Button>)
    expect(screen.getByRole('button')).toHaveAttribute('type', 'button')
  })

  it('can be a submit button when asked', () => {
    render(<Button type="submit">Save</Button>)
    expect(screen.getByRole('button')).toHaveAttribute('type', 'submit')
  })

  it('forwards its ref to the button element', () => {
    const ref = createRef<HTMLButtonElement>()
    render(<Button ref={ref}>Save</Button>)
    expect(ref.current).toBe(screen.getByRole('button', { name: 'Save' }))
  })
})
