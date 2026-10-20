import { useContext } from 'react'
import { useLocation } from 'react-router'
import { Button } from '../components/Button/Button'
import { useSession } from '../session/useSession'
import styles from './SessionExpiredNotice.module.css'
import { SessionExpiryContext } from './SessionExpiryContext'

/** Asks to log in again, and comes back to the same address, once the API refused the session. */
export function SessionExpiredNotice() {
  const { expired } = useContext(SessionExpiryContext)
  const session = useSession()
  const location = useLocation()
  if (!expired) return null
  return (
    <div role="alert" className={styles.notice}>
      <p className={styles.message}>Your session expired. Log in again to keep going.</p>
      <Button onClick={() => void session.signIn(`${location.pathname}${location.search}`)}>
        Log in again
      </Button>
    </div>
  )
}
