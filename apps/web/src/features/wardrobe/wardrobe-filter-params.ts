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
