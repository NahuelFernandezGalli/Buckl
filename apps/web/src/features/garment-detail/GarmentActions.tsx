import { useRef, useState } from 'react'
import { useRepositories } from '../../app/useRepositories'
import { Button } from '../../components/Button/Button'
import { ButtonLink } from '../../components/Button/ButtonLink'
import { ConfirmDialog } from '../../components/ConfirmDialog/ConfirmDialog'
import type { Garment } from '../../domain/garment'

export interface GarmentActionsProps {
  garment: Garment
  onChanged: (garment: Garment) => void
}

export function GarmentActions({ garment, onChanged }: GarmentActionsProps) {
  const { garments } = useRepositories()
  const [confirming, setConfirming] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const archiveButton = useRef<HTMLButtonElement>(null)

  // The confirmation takes the focus when it opens; give it back to the button that opened it.
  const cancelArchive = () => {
    setConfirming(false)
    archiveButton.current?.focus()
  }

  const run = async (action: () => Promise<Garment>) => {
    setError(null)
    try {
      onChanged(await action())
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Could not update the garment.')
    } finally {
      setConfirming(false)
    }
  }

  if (garment.status === 'archived') {
    return (
      <>
        <Button onClick={() => run(() => garments.restore(garment.id))}>Restore</Button>
        {error && <p role="alert">{error}</p>}
      </>
    )
  }

  return (
    <>
      <ButtonLink variant="secondary" to={`/wardrobe/${garment.id}/edit`}>
        Edit
      </ButtonLink>
      <Button ref={archiveButton} variant="danger" onClick={() => setConfirming(true)}>
        Archive
      </Button>
      {confirming && (
        <ConfirmDialog
          title="Archive this garment?"
          description="It leaves your wardrobe but stays on record. You can restore it later."
          confirmLabel="Yes, archive it"
          danger
          onConfirm={() => run(() => garments.archive(garment.id))}
          onCancel={cancelArchive}
        />
      )}
      {error && <p role="alert">{error}</p>}
    </>
  )
}
