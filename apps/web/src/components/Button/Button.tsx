import type { ButtonHTMLAttributes, Ref } from 'react'
import styles from './Button.module.css'

export type ButtonVariant = 'primary' | 'secondary' | 'danger'

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant
  /** React 19 passes refs as a plain prop; it reaches the underlying button. */
  ref?: Ref<HTMLButtonElement>
}

export function Button({
  variant = 'primary',
  type = 'button',
  className,
  ref,
  ...rest
}: ButtonProps) {
  const classes = [styles.button, styles[variant], className].filter(Boolean).join(' ')
  return <button ref={ref} type={type} className={classes} {...rest} />
}
