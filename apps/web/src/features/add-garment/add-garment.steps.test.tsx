import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import { DEFAULT_WARDROBE_FILTER } from '../../domain/wardrobe-filter'
import { todayIsoDate } from '../../lib/dates'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./add-garment.feature')

const imageFile = () => new File(['photo'], 'shirt.jpg', { type: 'image/jpeg' })

const tomorrow = () => {
  const date = new Date()
  date.setDate(date.getDate() + 1)
  return todayIsoDate(date)
}

describeFeature(feature, ({ Background, Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  const user = userEvent.setup()
  let repository: InMemoryGarmentRepository

  const chooseCategoryAndColor = async (category: string, color: string) => {
    await user.selectOptions(screen.getByLabelText('Category'), category)
    await user.selectOptions(screen.getByLabelText('Color'), color)
  }

  const enterPrice = async (amount: number, currency: string, date: string | null) => {
    await user.type(screen.getByLabelText('Price'), String(amount))
    await user.selectOptions(screen.getByLabelText('Currency'), currency)
    if (date !== null) {
      await user.type(screen.getByLabelText('Purchase date'), date)
    }
  }

  const save = () => user.click(screen.getByRole('button', { name: 'Save garment' }))

  const expectDetailTitled = async (title: string) => {
    expect(await screen.findByRole('heading', { level: 1, name: title })).toBeVisible()
  }

  const expectNothingAdded = async () => {
    expect(await repository.list(DEFAULT_WARDROBE_FILTER)).toEqual([])
  }

  Background(({ Given }) => {
    Given('the user is on the add garment screen', () => {
      repository = new InMemoryGarmentRepository()
      renderApp({ route: '/garments/new', repositories: { garments: repository } })
    })
  })

  Scenario('a garment with a photo and its classification is saved', ({ When, And, Then }) => {
    When('the user takes a photo of the garment', async () => {
      await user.upload(screen.getByLabelText('Take photo'), imageFile())
    })
    And(
      'chooses the category {string} and the color {string}',
      (_ctx, category: string, color: string) => chooseCategoryAndColor(category, color),
    )
    And('saves the garment', save)
    Then('the detail of the new garment is titled {string}', (_ctx, title: string) =>
      expectDetailTitled(title),
    )
    And('the wardrobe contains one garment with that photo', async () => {
      const garments = await repository.list(DEFAULT_WARDROBE_FILTER)
      expect(garments).toHaveLength(1)
      expect(garments[0].photoUrl).toMatch(/^data:image\/jpeg;base64,/)
    })
  })

  Scenario('category and color are required', ({ When, Then, And }) => {
    When('the user saves the garment without choosing a category or a color', save)
    Then('the form asks to choose a category', async () => {
      expect(await screen.findByLabelText('Category')).toHaveAccessibleDescription(
        'Choose a category',
      )
    })
    And('the form asks to choose a color', () => {
      expect(screen.getByLabelText('Color')).toHaveAccessibleDescription('Choose a color')
    })
    And('nothing was added to the wardrobe', expectNothingAdded)
  })

  Scenario('purchase information is saved with the garment', ({ When, And, Then }) => {
    When(
      'the user chooses the category {string} and the color {string}',
      (_ctx, category: string, color: string) => chooseCategoryAndColor(category, color),
    )
    And(
      'enters a price of {int} {string} paid on {string}',
      (_ctx, amount: number, currency: string, date: string) => enterPrice(amount, currency, date),
    )
    And('saves the garment', save)
    Then('the detail of the new garment is titled {string}', (_ctx, title: string) =>
      expectDetailTitled(title),
    )
    And(
      'the price {string} and the date {string} are shown',
      (_ctx, price: string, date: string) => {
        expect(screen.getByText(price)).toBeVisible()
        expect(screen.getByText(date)).toBeVisible()
      },
    )
  })

  Scenario('a purchase date in the future is rejected', ({ When, And, Then }) => {
    When(
      'the user chooses the category {string} and the color {string}',
      (_ctx, category: string, color: string) => chooseCategoryAndColor(category, color),
    )
    And(
      'enters a price of {int} {string} paid tomorrow',
      (_ctx, amount: number, currency: string) => enterPrice(amount, currency, tomorrow()),
    )
    And('saves the garment', save)
    Then('the form says the purchase date cannot be in the future', async () => {
      expect(await screen.findByLabelText('Purchase date')).toHaveAccessibleDescription(
        'The purchase date cannot be in the future',
      )
    })
    And('nothing was added to the wardrobe', expectNothingAdded)
  })

  Scenario('a price without a date is incomplete', ({ When, And, Then }) => {
    When(
      'the user chooses the category {string} and the color {string}',
      (_ctx, category: string, color: string) => chooseCategoryAndColor(category, color),
    )
    And(
      'enters a price of {int} {string} without a date',
      (_ctx, amount: number, currency: string) => enterPrice(amount, currency, null),
    )
    And('saves the garment', save)
    Then('the form asks for the purchase date', async () => {
      expect(await screen.findByLabelText('Purchase date')).toHaveAccessibleDescription(
        'Enter the purchase date',
      )
    })
  })

  Scenario('a garment can be saved without a photo', ({ When, And, Then }) => {
    When(
      'the user chooses the category {string} and the color {string}',
      (_ctx, category: string, color: string) => chooseCategoryAndColor(category, color),
    )
    And('saves the garment', save)
    Then('the detail of the new garment is titled {string}', (_ctx, title: string) =>
      expectDetailTitled(title),
    )
  })
})
