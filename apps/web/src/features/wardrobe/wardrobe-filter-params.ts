import { isCategory, isColor, MAX_SIZE_LENGTH } from '../../domain/classification'
import { normalizeSearchText, type WardrobeFilter } from '../../domain/wardrobe-filter'

/** Query string parameters of /wardrobe. Unknown values fall back to the default. */
export const FILTER_PARAMS = {
  category: 'category',
  color: 'color',
  size: 'size',
  searchText: 'q',
  status: 'status',
} as const

/** The parameters that narrow the wardrobe. Status is not one of them: every listing has one. */
export const CRITERION_PARAMS = [
  FILTER_PARAMS.category,
  FILTER_PARAMS.color,
  FILTER_PARAMS.size,
  FILTER_PARAMS.searchText,
] as const

/** A copy of the parameters without any criterion, keeping the status. */
export function clearCriteria(params: URLSearchParams): URLSearchParams {
  const next = new URLSearchParams(params)
  for (const name of CRITERION_PARAMS) next.delete(name)
  return next
}

export function parseWardrobeFilter(params: URLSearchParams): WardrobeFilter {
  const category = params.get(FILTER_PARAMS.category) ?? ''
  const color = params.get(FILTER_PARAMS.color) ?? ''
  const size = params.get(FILTER_PARAMS.size)?.trim() ?? ''

  return {
    category: isCategory(category) ? category : null,
    color: isColor(color) ? color : null,
    size: size === '' ? null : size.slice(0, MAX_SIZE_LENGTH),
    searchText: normalizeSearchText(params.get(FILTER_PARAMS.searchText)),
    status: params.get(FILTER_PARAMS.status) === 'archived' ? 'archived' : 'active',
  }
}
