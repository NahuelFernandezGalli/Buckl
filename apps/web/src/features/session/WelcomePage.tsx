import { Navigate, useLocation } from 'react-router'
import { usePageTitle } from '../../app/usePageTitle'
import { Button } from '../../components/Button/Button'
import { safeReturnTo } from '../../session/return-to'
import { useSession } from '../../session/useSession'
import styles from './WelcomePage.module.css'

export function WelcomePage() {
  usePageTitle('Welcome')
  const session = useSession()
  const location = useLocation()
  const returnTo = safeReturnTo((location.state as { returnTo?: unknown } | null)?.returnTo)

  if (session.state.status === 'signedIn') {
    return <Navigate to={returnTo} replace />
  }

  const error = session.state.status === 'signedOut' ? session.state.error : undefined
  return (
    <main className={styles.welcome}>
      <h1>Buckl</h1>
      <p>Your closet, digitized. Upload once, dress smarter.</p>
      {error && <p role="alert">{error.message}</p>}
      <Button
        disabled={session.state.status === 'loading'}
        onClick={() => void session.signIn(returnTo)}
      >
        Log in
      </Button>
    </main>
  )
}
