import { useId, type ReactNode } from 'react'
import styles from './EmptyState.module.css'

export interface EmptyStateProps {
  title: string
  description?: string
  action?: ReactNode
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  const titleId = useId()
  return (
    <section className={styles.empty} aria-labelledby={titleId}>
      <h2 id={titleId} className={styles.title}>
        {title}
      </h2>
      {description && <p className={styles.description}>{description}</p>}
      {action && <div className={styles.action}>{action}</div>}
    </section>
  )
}
