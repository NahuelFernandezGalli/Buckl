import type { InputHTMLAttributes } from 'react'
import { FieldMessages } from '../Field/FieldMessages'
import { useFieldDescription } from '../Field/useFieldDescription'
import styles from './Input.module.css'

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  label: string
  error?: string
  hint?: string
}

export function Input({ label, error, hint, className, ...rest }: InputProps) {
  const { id, hintId, errorId, describedBy } = useFieldDescription(hint, error)

  return (
    <div className={styles.field}>
      <label htmlFor={id} className={styles.label}>
        {label}
      </label>
      <input
        id={id}
        className={[styles.control, className].filter(Boolean).join(' ')}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        {...rest}
      />
      <FieldMessages hint={hint} hintId={hintId} error={error} errorId={errorId} />
    </div>
  )
}
