import { Link, type LinkProps } from 'react-router'
import type { ButtonVariant } from './Button'
import styles from './Button.module.css'

export interface ButtonLinkProps extends LinkProps {
  variant?: ButtonVariant
}

export function ButtonLink({ variant = 'primary', className, ...rest }: ButtonLinkProps) {
  const classes = [styles.button, styles[variant], className].filter(Boolean).join(' ')
  return <Link className={classes} {...rest} />
}
