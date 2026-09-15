import type { TextareaHTMLAttributes } from 'react'
import { FieldMessages } from '../Field/FieldMessages'
import { useFieldDescription } from '../Field/useFieldDescription'
import styles from '../Input/Input.module.css'

export interface TextAreaProps extends Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, 'id'> {
  label: string
  error?: string
  hint?: string
}

export function TextArea({ label, error, hint, className, ...rest }: TextAreaProps) {
  const { id, hintId, errorId, describedBy } = useFieldDescription(hint, error)

  return (
    <div className={styles.field}>
      <label htmlFor={id} className={styles.label}>
        {label}
      </label>
      <textarea
        id={id}
        rows={3}
        className={[styles.control, className].filter(Boolean).join(' ')}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...rest}
      />
      <FieldMessages hint={hint} hintId={hintId} error={error} errorId={errorId} />
    </div>
  )
}
