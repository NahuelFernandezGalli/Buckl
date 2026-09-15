import '@testing-library/jest-dom/vitest'
import { vi } from 'vitest'

// jsdom does not implement object URLs; PhotoCapture needs them for previews.
let objectUrlCounter = 0
URL.createObjectURL = vi.fn(() => `blob:preview-${++objectUrlCounter}`)
URL.revokeObjectURL = vi.fn()
