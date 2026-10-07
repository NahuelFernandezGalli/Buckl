import type { WardrobeFilter } from '../domain/wardrobe-filter'

/** The query string of `GET /garments`, named like the API's `WardrobeQuery`. */
export function wardrobeQuery(filter: WardrobeFilter): string {
  const params = new URLSearchParams()
  if (filter.category) params.set('category', filter.category)
  if (filter.color) params.set('color', filter.color)
  if (filter.size) params.set('size', filter.size)
  if (filter.searchText) params.set('q', filter.searchText)
  params.set('status', filter.status)
  return params.toString()
}
