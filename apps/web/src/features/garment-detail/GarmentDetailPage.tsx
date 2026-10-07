import { useParams } from 'react-router'
import { LoadError } from '../../app/LoadError'
import { usePageTitle } from '../../app/usePageTitle'
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
        <LoadError message={state.message} onRetry={retry} />
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
