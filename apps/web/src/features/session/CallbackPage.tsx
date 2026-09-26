import { Navigate, useLocation } from 'react-router'
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
  const location = useLocation()

  if (state.status === 'signedOut') return <Navigate to="/welcome" replace />
  if (state.status === 'signedIn' && !carriesAuth0Answer(location.search)) {
    return <Navigate to="/wardrobe" replace />
  }
  // Either still loading, or just signed in with Auth0's answer still on the URL: the router
  // update that carries the session state can render before the session provider's own
  // navigation to the requested page (React Router applies it inside startTransition), so a
  // `signedIn` state here does not yet mean the redirect has happened. Keep showing the same
  // message and let that navigation land.
  return <p role="status">Signing you in…</p>
}

/** Auth0 answers `/callback` with `code`/`state` on success or `error`/`state` on cancellation. */
function carriesAuth0Answer(search: string): boolean {
  const params = new URLSearchParams(search)
  return params.has('code') || params.has('state') || params.has('error')
}
