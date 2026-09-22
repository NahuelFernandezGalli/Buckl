import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ConfirmDialog } from './ConfirmDialog'

describe('ConfirmDialog', () => {
  afterEach(cleanup)

  const renderDialog = () => {
    const onConfirm = vi.fn()
    const onCancel = vi.fn()
    render(
      <ConfirmDialog
        title="Archive this garment?"
        description="It stays on record."
        confirmLabel="Yes, archive it"
        onConfirm={onConfirm}
        onCancel={onCancel}
      />,
    )
    return { onConfirm, onCancel }
  }

  it('is an alert dialog named by its title and described by its text', () => {
    renderDialog()
    const dialog = screen.getByRole('alertdialog', { name: 'Archive this garment?' })
    expect(dialog).toHaveAccessibleDescription('It stays on record.')
  })

  it('focuses the confirm button so the keyboard can answer right away', () => {
    renderDialog()
    expect(screen.getByRole('button', { name: 'Yes, archive it' })).toHaveFocus()
  })

  it('confirms and cancels through its buttons', async () => {
    const { onConfirm, onCancel } = renderDialog()
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Yes, archive it' }))
    await user.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(onConfirm).toHaveBeenCalledTimes(1)
    expect(onCancel).toHaveBeenCalledTimes(1)
  })

  it('cancels with Escape', async () => {
    const { onCancel } = renderDialog()
    await userEvent.setup().keyboard('{Escape}')
    expect(onCancel).toHaveBeenCalledTimes(1)
  })
})
