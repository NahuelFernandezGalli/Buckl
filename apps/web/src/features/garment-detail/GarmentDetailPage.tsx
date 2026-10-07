import { useParams } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { Button } from '../../components/Button/Button'
import { garmentTitle } from '../../lib/garment-title'
import { GarmentActions } from './GarmentActions'
import { GarmentDetail } from './GarmentDetail'
import { GarmentNotFound } from './GarmentNotFound'
import { useGarment } from './useGarment'

export function GarmentDetailPage() {
  const { garmentId = '' } = useParams()
  const { state, setGarment, retry } = useGarment(garmentId)
  usePageTitle(state.status === 'ready' ? garmentTitle(state.garment) : 'Garment')

  if (state.status === 'loading') {
    return <p role="status">Loading the garment…</p>
  }
  if (state.status === 'not-found') {
    return <GarmentNotFound />
  }
  if (state.status === 'error') {
    return (
      <>
        <h1>Garment</h1>
        <p role="alert">{state.message}</p>
        <Button variant="secondary" onClick={retry}>
          Try again
        </Button>
      </>
    )
  }
  const garment = state.garment
  return (
    <GarmentDetail
      garment={garment}
      actions={<GarmentActions garment={garment} onChanged={setGarment} />}
    />
  )
}
