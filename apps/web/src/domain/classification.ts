export const CATEGORIES = ['top', 'bottom', 'dress', 'outerwear', 'footwear', 'accessory'] as const
export type Category = (typeof CATEGORIES)[number]

export const COLORS = [
  'black',
  'white',
  'grey',
  'navy',
  'blue',
  'red',
  'green',
  'yellow',
  'brown',
  'beige',
  'pink',
  'purple',
  'orange',
  'multicolor',
] as const
export type Color = (typeof COLORS)[number]

export const CATEGORY_LABELS: Record<Category, string> = {
  top: 'Top',
  bottom: 'Bottom',
  dress: 'Dress',
  outerwear: 'Outerwear',
  footwear: 'Footwear',
  accessory: 'Accessory',
}

export const COLOR_LABELS: Record<Color, string> = {
  black: 'Black',
  white: 'White',
  grey: 'Grey',
  navy: 'Navy',
  blue: 'Blue',
  red: 'Red',
  green: 'Green',
  yellow: 'Yellow',
  brown: 'Brown',
  beige: 'Beige',
  pink: 'Pink',
  purple: 'Purple',
  orange: 'Orange',
  multicolor: 'Multicolor',
}

export const MAX_SIZE_LENGTH = 20

export interface Classification {
  category: Category
  color: Color
  size: string | null
}

export function isCategory(value: string): value is Category {
  return (CATEGORIES as readonly string[]).includes(value)
}

export function isColor(value: string): value is Color {
  return (COLORS as readonly string[]).includes(value)
}
