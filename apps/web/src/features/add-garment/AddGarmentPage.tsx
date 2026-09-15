import { useNavigate } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { useRepositories } from '../../app/useRepositories'
import type { NewGarment } from '../../domain/garment-repository'
import { GarmentForm } from './GarmentForm'

export function AddGarmentPage() {
  usePageTitle('Add garment')
  const { garments } = useRepositories()
  const navigate = useNavigate()

  const save = async (garment: NewGarment) => {
    const created = await garments.create(garment)
    navigate(`/wardrobe/${created.id}`)
  }

  return (
    <>
      <h1>Add garment</h1>
      <GarmentForm submitLabel="Save garment" onSubmit={save} />
    </>
  )
}
