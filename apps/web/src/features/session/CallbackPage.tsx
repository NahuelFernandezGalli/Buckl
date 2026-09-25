import { Navigate } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { useSession } from '../../session/useSession'

/**
 * Where Auth0 sends the user back. The session provider finishes the sign-in and navigates to the
 * page the user asked for; this screen only covers the wait, and the cases where nothing is left
 * to finish.
 */
export function CallbackPage() {
  usePageTitle('Signing in')
  const { state } = useSession()

  if (state.status === 'signedIn') return <Navigate to="/wardrobe" replace />
  if (state.status === 'signedOut') return <Navigate to="/welcome" replace />
  return <p role="status">Signing you in…</p>
}
