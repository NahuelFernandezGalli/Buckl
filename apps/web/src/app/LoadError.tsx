import { useContext } from 'react'
import { Button } from '../components/Button/Button'
import { SessionExpiryContext } from './SessionExpiryContext'

interface LoadErrorProps {
  message: string
  onRetry: () => void
}

/**
 * Why a screen could not load, with a way to try again. Once the session expired the layout's
 * notice is the only place that says so, and trying again cannot succeed until the user logs in,
 * so nothing is shown here.
 */
export function LoadError({ message, onRetry }: LoadErrorProps) {
  const { expired } = useContext(SessionExpiryContext)
  if (expired) return null
  return (
    <>
      <p role="alert">{message}</p>
      <Button variant="secondary" onClick={onRetry}>
        Try again
      </Button>
    </>
  )
}
