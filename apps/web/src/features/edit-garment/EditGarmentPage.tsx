import { Link, useNavigate, useParams } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { useRepositories } from '../../app/useRepositories'
import type { NewGarment } from '../../domain/garment-repository'
import { fromGarment } from '../add-garment/garment-form'
import { GarmentForm } from '../add-garment/GarmentForm'
import { GarmentNotFound } from '../garment-detail/GarmentNotFound'
import { useGarment } from '../garment-detail/useGarment'

export function EditGarmentPage() {
  usePageTitle('Edit garment')
  const { garmentId = '' } = useParams()
  const { garments } = useRepositories()
  const navigate = useNavigate()
  const { state } = useGarment(garmentId)

  if (state.status === 'loading') {
    return <p role="status">Loading the garment…</p>
  }
  if (state.status === 'not-found') {
    return <GarmentNotFound />
  }
  if (state.status === 'error') {
    return (
      <>
        <h1>Edit garment</h1>
        <p role="alert">{state.message}</p>
      </>
    )
  }

  const garment = state.garment

  if (garment.status === 'archived') {
    return (
      <>
        <h1>Edit garment</h1>
        <p>This garment is archived. Restore it to edit it.</p>
        <Link to={`/wardrobe/${garment.id}`}>Back to the garment</Link>
      </>
    )
  }

  const save = async (changes: NewGarment) => {
    await garments.update(garment.id, changes)
    navigate(`/wardrobe/${garment.id}`)
  }

  return (
    <>
      <h1>Edit garment</h1>
      <GarmentForm
        initialValues={fromGarment(garment)}
        initialPhotoUrl={garment.photoUrl}
        submitLabel="Save changes"
        onSubmit={save}
      />
    </>
  )
}
