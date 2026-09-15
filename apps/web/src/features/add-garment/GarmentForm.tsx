import { useState, type FormEvent } from 'react'
import { Button } from '../../components/Button/Button'
import { Input } from '../../components/Input/Input'
import { Select } from '../../components/Select/Select'
import { TextArea } from '../../components/TextArea/TextArea'
import {
  CATEGORIES,
  CATEGORY_LABELS,
  COLORS,
  COLOR_LABELS,
  isCategory,
  isColor,
  MAX_SIZE_LENGTH,
} from '../../domain/classification'
import { MAX_NOTES_LENGTH } from '../../domain/garment'
import type { NewGarment } from '../../domain/garment-repository'
import { CURRENCIES } from '../../domain/money'
import { todayIsoDate } from '../../lib/dates'
import { readFileAsDataUrl } from '../../lib/files'
import { PhotoCapture, type PhotoSelection } from '../photo-capture/PhotoCapture'
import {
  EMPTY_GARMENT_FORM,
  validateGarmentForm,
  type GarmentFormErrors,
  type GarmentFormValues,
} from './garment-form'
import styles from './GarmentForm.module.css'

export interface GarmentFormProps {
  initialValues?: GarmentFormValues
  initialPhotoUrl?: string | null
  submitLabel: string
  /** Injected by tests; defaults to the local date at submit time. */
  today?: string
  onSubmit: (garment: NewGarment) => Promise<void>
}

const categoryOptions = CATEGORIES.map((value) => ({ value, label: CATEGORY_LABELS[value] }))
const colorOptions = COLORS.map((value) => ({ value, label: COLOR_LABELS[value] }))
const currencyOptions = CURRENCIES.map((value) => ({ value, label: value }))

export function GarmentForm({
  initialValues = EMPTY_GARMENT_FORM,
  initialPhotoUrl = null,
  submitLabel,
  today,
  onSubmit,
}: GarmentFormProps) {
  const [values, setValues] = useState<GarmentFormValues>(initialValues)
  const [photo, setPhoto] = useState<PhotoSelection | null>(
    initialPhotoUrl ? { file: null, previewUrl: initialPhotoUrl } : null,
  )
  const [errors, setErrors] = useState<GarmentFormErrors>({})
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const update = <K extends keyof GarmentFormValues>(field: K, value: GarmentFormValues[K]) =>
    setValues((current) => ({ ...current, [field]: value }))

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const result = validateGarmentForm(values, today ?? todayIsoDate())
    if (!result.ok) {
      setErrors(result.errors)
      return
    }
    setErrors({})
    setSubmitting(true)
    setSubmitError(null)
    try {
      const photoUrl = photo
        ? photo.file
          ? await readFileAsDataUrl(photo.file)
          : photo.previewUrl
        : null
      await onSubmit({ photoUrl, ...result.output })
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : 'Could not save the garment.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate className={styles.form}>
      <fieldset className={styles.fieldset}>
        <legend className={styles.legend}>Photo</legend>
        <PhotoCapture value={photo} onChange={setPhoto} />
      </fieldset>

      <Select
        label="Category"
        options={categoryOptions}
        placeholder="Choose a category"
        value={values.category}
        error={errors.category}
        onChange={(event) =>
          update('category', isCategory(event.target.value) ? event.target.value : '')
        }
      />
      <Select
        label="Color"
        options={colorOptions}
        placeholder="Choose a color"
        value={values.color}
        error={errors.color}
        onChange={(event) => update('color', isColor(event.target.value) ? event.target.value : '')}
      />
      <Input
        label="Size"
        hint="As the store states it, for example M, 42 or 32x32"
        maxLength={MAX_SIZE_LENGTH}
        value={values.size}
        error={errors.size}
        onChange={(event) => update('size', event.target.value)}
      />

      <fieldset className={styles.fieldset}>
        <legend className={styles.legend}>Purchase</legend>
        <div className={styles.price}>
          <Input
            label="Price"
            type="text"
            inputMode="decimal"
            value={values.amount}
            error={errors.amount}
            onChange={(event) => update('amount', event.target.value)}
          />
          <Select
            label="Currency"
            options={currencyOptions}
            value={values.currency}
            onChange={(event) => update('currency', event.target.value)}
          />
        </div>
        <Input
          label="Purchase date"
          type="date"
          value={values.purchaseDate}
          error={errors.purchaseDate}
          onChange={(event) => update('purchaseDate', event.target.value)}
        />
      </fieldset>

      <TextArea
        label="Notes"
        maxLength={MAX_NOTES_LENGTH}
        value={values.notes}
        error={errors.notes}
        onChange={(event) => update('notes', event.target.value)}
      />

      {submitError && <p role="alert">{submitError}</p>}
      <Button type="submit" disabled={submitting}>
        {submitLabel}
      </Button>
    </form>
  )
}
