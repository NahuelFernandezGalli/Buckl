import { Button } from '../../components/Button/Button'
import { Input } from '../../components/Input/Input'
import { Select } from '../../components/Select/Select'
import {
  CATEGORIES,
  CATEGORY_LABELS,
  COLORS,
  COLOR_LABELS,
  MAX_SIZE_LENGTH,
} from '../../domain/classification'
import { MAX_SEARCH_TEXT_LENGTH } from '../../domain/wardrobe-filter'
import { FILTER_PARAMS } from './wardrobe-filter-params'
import styles from './WardrobeFilters.module.css'

export interface WardrobeFiltersProps {
  params: URLSearchParams
  onChange: (params: URLSearchParams) => void
}

const categoryOptions = CATEGORIES.map((value) => ({ value, label: CATEGORY_LABELS[value] }))
const colorOptions = COLORS.map((value) => ({ value, label: COLOR_LABELS[value] }))
const criteria = [
  FILTER_PARAMS.category,
  FILTER_PARAMS.color,
  FILTER_PARAMS.size,
  FILTER_PARAMS.searchText,
]

export function WardrobeFilters({ params, onChange }: WardrobeFiltersProps) {
  const set = (name: string, value: string) => {
    const next = new URLSearchParams(params)
    if (value) {
      next.set(name, value)
    } else {
      next.delete(name)
    }
    onChange(next)
  }

  const clear = () => {
    const next = new URLSearchParams(params)
    for (const name of criteria) next.delete(name)
    onChange(next)
  }

  const hasCriteria = criteria.some((name) => params.has(name))

  return (
    <form
      role="search"
      aria-label="Wardrobe filters"
      className={styles.filters}
      onSubmit={(event) => event.preventDefault()}
    >
      <Input
        label="Search"
        type="search"
        maxLength={MAX_SEARCH_TEXT_LENGTH}
        value={params.get(FILTER_PARAMS.searchText) ?? ''}
        onChange={(event) => set(FILTER_PARAMS.searchText, event.target.value)}
      />
      <Select
        label="Category"
        options={categoryOptions}
        placeholder="All categories"
        value={params.get(FILTER_PARAMS.category) ?? ''}
        onChange={(event) => set(FILTER_PARAMS.category, event.target.value)}
      />
      <Select
        label="Color"
        options={colorOptions}
        placeholder="All colors"
        value={params.get(FILTER_PARAMS.color) ?? ''}
        onChange={(event) => set(FILTER_PARAMS.color, event.target.value)}
      />
      <Input
        label="Size"
        maxLength={MAX_SIZE_LENGTH}
        value={params.get(FILTER_PARAMS.size) ?? ''}
        onChange={(event) => set(FILTER_PARAMS.size, event.target.value)}
      />
      {hasCriteria && (
        <Button variant="secondary" onClick={clear}>
          Clear filters
        </Button>
      )}
    </form>
  )
}
