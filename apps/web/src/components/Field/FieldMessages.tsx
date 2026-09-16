import styles from '../Input/Input.module.css'

export interface FieldMessagesProps {
  hint?: string
  hintId?: string
  error?: string
  errorId: string
}

/** Renders the hint and error paragraphs shared by every labelled form control. */
export function FieldMessages({ hint, hintId, error, errorId }: FieldMessagesProps) {
  return (
    <>
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className={styles.error}>
          {error}
        </p>
      )}
    </>
  )
}
