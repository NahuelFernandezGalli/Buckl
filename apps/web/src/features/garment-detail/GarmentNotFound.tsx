import { Link } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'

export function GarmentNotFound() {
  usePageTitle('Garment not found')
  return (
    <>
      <h1>Garment not found</h1>
      <p>This garment is not in your wardrobe.</p>
      <Link to="/wardrobe">Back to the wardrobe</Link>
    </>
  )
}
