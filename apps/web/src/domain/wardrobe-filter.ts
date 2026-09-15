import type { Category, Color } from './classification'
import type { GarmentStatus } from './garment'

export const MAX_SEARCH_TEXT_LENGTH = 100

/** Mirror of the domain's WardrobeFilter. Status always has a value; it never counts as a criterion. */
export interface WardrobeFilter {
  category: Category | null
  color: Color | null
  size: string | null
  searchText: string | null
  status: GarmentStatus
}

export const DEFAULT_WARDROBE_FILTER: WardrobeFilter = {
  category: null,
  color: null,
  size: null,
  searchText: null,
  status: 'active',
}

export function hasCriteria(filter: WardrobeFilter): boolean {
  return (
    filter.category !== null ||
    filter.color !== null ||
    filter.size !== null ||
    filter.searchText !== null
  )
}

/** Trims, turns blank into null and caps the length. The API rejects longer text; the UI never sends it. */
export function normalizeSearchText(value: string | null | undefined): string | null {
  const trimmed = value?.trim() ?? ''
  if (trimmed === '') {
    return null
  }
  return trimmed.slice(0, MAX_SEARCH_TEXT_LENGTH)
}
