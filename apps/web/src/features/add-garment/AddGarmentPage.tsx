import { useNavigate } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { useRepositories } from '../../app/useRepositories'
import type { GarmentSubmission } from './garment-form'
import { GarmentForm } from './GarmentForm'

export function AddGarmentPage() {
  usePageTitle('Add garment')
  const { garments } = useRepositories()
  const navigate = useNavigate()

  const save = async ({ photo, ...details }: GarmentSubmission) => {
    const created = await garments.create({ ...details, photo: photo ?? null })
    navigate(`/wardrobe/${created.id}`)
  }

  return (
    <>
      <h1>Add garment</h1>
      <GarmentForm submitLabel="Save garment" onSubmit={save} />
    </>
  )
}
