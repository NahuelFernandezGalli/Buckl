import { useEffect, useState, type ChangeEvent } from 'react'
import { Button } from '../../components/Button/Button'
import styles from './PhotoCapture.module.css'

export interface PhotoSelection {
  /** The picked file, or null when the preview is an existing photo URL. */
  file: File | null
  previewUrl: string
}

export interface PhotoCaptureProps {
  value: PhotoSelection | null
  onChange: (selection: PhotoSelection | null) => void
}

export function PhotoCapture({ value, onChange }: PhotoCaptureProps) {
  const [error, setError] = useState<string | null>(null)

  // Release the object URL of a picked file once it is replaced or the component goes away.
  useEffect(() => {
    if (!value?.file) return
    const url = value.previewUrl
    return () => URL.revokeObjectURL(url)
  }, [value])

  const handleFile = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return
    if (!file.type.startsWith('image/')) {
      setError('Only images are accepted.')
      return
    }
    setError(null)
    onChange({ file, previewUrl: URL.createObjectURL(file) })
  }

  if (value) {
    return (
      <div className={styles.capture}>
        <img src={value.previewUrl} alt="Garment photo preview" className={styles.preview} />
        <Button variant="secondary" onClick={() => onChange(null)}>
          Retake photo
        </Button>
      </div>
    )
  }

  return (
    <div className={styles.capture}>
      <div className={styles.options}>
        <label className={styles.option}>
          Take photo
          <input
            type="file"
            accept="image/*"
            capture="environment"
            className={styles.input}
            onChange={handleFile}
          />
        </label>
        <label className={styles.option}>
          Choose from gallery
          <input type="file" accept="image/*" className={styles.input} onChange={handleFile} />
        </label>
      </div>
      {error && (
        <p role="alert" className={styles.error}>
          {error}
        </p>
      )}
    </div>
  )
}
