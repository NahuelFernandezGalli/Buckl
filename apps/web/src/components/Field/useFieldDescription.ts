import { useId } from 'react'

export interface FieldDescription {
  id: string
  hintId: string
  errorId: string
  describedBy: string | undefined
}

/**
 * Generates the id and aria-describedby wiring shared by every labelled form control
 * (Input, Select, TextArea): one id for the control, one for its hint and one for its error,
 * combined into a single describedBy value that only lists the messages actually rendered.
 */
export function useFieldDescription(
  hint: string | undefined,
  error: string | undefined,
): FieldDescription {
  const id = useId()
  const hintId = `${id}-hint`
  const errorId = `${id}-error`
  const describedBy = [hint ? hintId : null, error ? errorId : null].filter(Boolean).join(' ')

  return { id, hintId, errorId, describedBy: describedBy || undefined }
}
