import { Link } from 'react-router'
import { usePageTitle } from './usePageTitle'

export function NotFoundPage() {
  usePageTitle('Page not found')
  return (
    <>
      <h1>Page not found</h1>
      <p>There is nothing at this address.</p>
      <Link to="/wardrobe">Back to the wardrobe</Link>
    </>
  )
}
