import { Navigate, Outlet, useLocation } from 'react-router'
import { useSession } from '../session/useSession'

/** Layout route in front of every screen that shows a user's data. */
export function RequireSession() {
  const { state } = useSession()
  const location = useLocation()

  if (state.status === 'loading') {
    return <p role="status">Opening your wardrobe…</p>
  }
  if (state.status === 'signedOut') {
    const returnTo = `${location.pathname}${location.search}`
    return <Navigate to="/welcome" replace state={{ returnTo }} />
  }
  return <Outlet />
}
