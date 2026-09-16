import { Link, useParams } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { garmentTitle } from '../../lib/garment-title'
import { GarmentDetail } from './GarmentDetail'
import { useGarment } from './useGarment'

export function GarmentDetailPage() {
  const { garmentId = '' } = useParams()
  const { state } = useGarment(garmentId)
  usePageTitle(state.status === 'ready' ? garmentTitle(state.garment) : 'Garment')

  if (state.status === 'loading') {
    return <p role="status">Loading the garment…</p>
  }
  if (state.status === 'not-found') {
    return (
      <>
        <h1>Garment not found</h1>
        <p>This garment is not in your wardrobe.</p>
        <Link to="/wardrobe">Back to the wardrobe</Link>
      </>
    )
  }
  if (state.status === 'error') {
    return (
      <>
        <h1>Garment</h1>
        <p role="alert">{state.message}</p>
      </>
    )
  }
  return <GarmentDetail garment={state.garment} />
}
