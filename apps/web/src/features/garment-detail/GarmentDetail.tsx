import { useId, type ReactNode } from 'react'
import { placeholderPhoto } from '../../data/placeholder-photo'
import { CATEGORY_LABELS, COLOR_LABELS } from '../../domain/classification'
import type { Garment } from '../../domain/garment'
import { formatMoney, formatPurchaseDate } from '../../lib/format'
import { garmentTitle } from '../../lib/garment-title'
import styles from './GarmentDetail.module.css'
import { useProduct } from './useProduct'

export interface GarmentDetailProps {
  garment: Garment
  /** Edit, archive and restore controls, supplied by the page. */
  actions?: ReactNode
}

export function GarmentDetail({ garment, actions }: GarmentDetailProps) {
  const { category, color, size } = garment.classification
  const productState = useProduct(garment.productId)
  const purchaseId = useId()
  const productId = useId()
  const notesId = useId()
  const title = garmentTitle(garment)

  return (
    <article className={styles.detail}>
      <header className={styles.header}>
        <h1 className={styles.title}>{title}</h1>
        {garment.status === 'archived' && <span className={styles.badge}>Archived</span>}
      </header>
      {actions && <div className={styles.actions}>{actions}</div>}
      <img src={garment.photoUrl ?? placeholderPhoto(color)} alt={title} className={styles.photo} />

      <dl className={styles.facts}>
        <div>
          <dt>Category</dt>
          <dd>{CATEGORY_LABELS[category]}</dd>
        </div>
        <div>
          <dt>Color</dt>
          <dd>{COLOR_LABELS[color]}</dd>
        </div>
        <div>
          <dt>Size</dt>
          <dd>{size ?? 'Not set'}</dd>
        </div>
      </dl>

      <section aria-labelledby={purchaseId} className={styles.section}>
        <h2 id={purchaseId}>Purchase</h2>
        {garment.purchaseInfo ? (
          <dl className={styles.facts}>
            <div>
              <dt>Price</dt>
              <dd>{formatMoney(garment.purchaseInfo.price)}</dd>
            </div>
            <div>
              <dt>Purchased on</dt>
              <dd>{formatPurchaseDate(garment.purchaseInfo.date)}</dd>
            </div>
          </dl>
        ) : (
          <p className={styles.muted}>No purchase information.</p>
        )}
      </section>

      {garment.productId && (
        <section aria-labelledby={productId} className={styles.section}>
          <h2 id={productId}>Product</h2>
          {productState.status === 'ready' && (
            <>
              <p className={styles.productName}>{productState.product.name}</p>
              {productState.product.brand && (
                <p className={styles.muted}>{productState.product.brand}</p>
              )}
              {productState.product.sourceUrl && (
                <a href={productState.product.sourceUrl} target="_blank" rel="noopener noreferrer">
                  View product page
                </a>
              )}
            </>
          )}
          {productState.status === 'loading' && (
            <p className={styles.muted}>Loading the product…</p>
          )}
          {productState.status === 'missing' && (
            <p className={styles.muted}>This product is no longer available.</p>
          )}
        </section>
      )}

      {garment.notes && (
        <section aria-labelledby={notesId} className={styles.section}>
          <h2 id={notesId}>Notes</h2>
          <p>{garment.notes}</p>
        </section>
      )}
    </article>
  )
}
