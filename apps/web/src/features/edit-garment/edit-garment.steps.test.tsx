import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import type { Garment } from '../../domain/garment'
import { GarmentMother } from '../../test/garment-mother'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./edit-garment.feature')

const BLUE_TOP_ID = 'blue-top'

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  const user = userEvent.setup()
  let repository: InMemoryGarmentRepository

  const blueTop = (overrides: Partial<Garment> = {}) =>
    GarmentMother.active({
      id: BLUE_TOP_ID,
      classification: { category: 'top', color: 'blue', size: 'M' },
      ...overrides,
    })

  const seed = (...garments: Garment[]) => {
    repository = new InMemoryGarmentRepository(garments)
  }

  const open = (route: string) => {
    renderApp({ route, repositories: { garments: repository } })
  }
  const openEdit = () => open(`/wardrobe/${BLUE_TOP_ID}/edit`)

  Scenario("the form starts with the garment's current details", ({ Given, When, Then, And }) => {
    Given(
      'a blue top sized M bought for {int} {string} on {string} with the note {string}',
      (_ctx, amount: number, currency: string, date: string, note: string) => {
        seed(blueTop({ purchaseInfo: { price: { amount, currency }, date }, notes: note }))
      },
    )
    When('the user opens the edit screen of the blue top', openEdit)
    Then(
      'the form shows the category {string}, the color {string} and the size {string}',
      async (_ctx, category: string, color: string, size: string) => {
        expect(await screen.findByLabelText('Category')).toHaveDisplayValue(category)
        expect(screen.getByLabelText('Color')).toHaveDisplayValue(color)
        expect(screen.getByLabelText('Size')).toHaveValue(size)
      },
    )
    And(
      'the form shows the price {int} in {string} paid on {string}',
      (_ctx, amount: number, currency: string, date: string) => {
        expect(screen.getByLabelText('Price')).toHaveValue(String(amount))
        expect(screen.getByLabelText('Currency')).toHaveDisplayValue(currency)
        expect(screen.getByLabelText('Purchase date')).toHaveValue(date)
      },
    )
    And('the form shows the note {string}', (_ctx, note: string) => {
      expect(screen.getByLabelText('Notes')).toHaveValue(note)
    })
    And('the current photo is shown', () => {
      expect(screen.getByRole('img', { name: 'Garment photo preview' })).toHaveAttribute(
        'src',
        blueTop().photoUrl,
      )
    })
  })

  Scenario('changing the color updates the garment', ({ Given, When, And, Then }) => {
    Given('a blue top sized M', () => seed(blueTop()))
    When('the user opens the edit screen of the blue top', openEdit)
    And('changes the color to {string}', async (_ctx, color: string) => {
      await user.selectOptions(await screen.findByLabelText('Color'), color)
    })
    And('saves the changes', async () => {
      await user.click(screen.getByRole('button', { name: 'Save changes' }))
    })
    Then('the detail is titled {string}', async (_ctx, title: string) => {
      expect(await screen.findByRole('heading', { level: 1, name: title })).toBeVisible()
    })
    And('the wardrobe garment has the color {string}', async (_ctx, color: string) => {
      expect((await repository.getById(BLUE_TOP_ID))?.classification.color).toBe(color)
    })
  })

  Scenario('the detail offers to edit an active garment', ({ Given, When, Then }) => {
    Given('a blue top sized M', () => seed(blueTop()))
    When('the user opens the detail of the blue top', () => open(`/wardrobe/${BLUE_TOP_ID}`))
    Then('an edit link leads to the edit screen', async () => {
      expect(await screen.findByRole('link', { name: 'Edit' })).toHaveAttribute(
        'href',
        `/wardrobe/${BLUE_TOP_ID}/edit`,
      )
    })
  })

  Scenario('an archived garment cannot be edited', ({ Given, When, Then, And }) => {
    Given('an archived blue top', () => seed(GarmentMother.archived({ id: BLUE_TOP_ID })))
    When('the user opens the edit screen of the blue top', openEdit)
    Then('a message says the garment is archived and must be restored first', async () => {
      expect(
        await screen.findByText('This garment is archived. Restore it to edit it.'),
      ).toBeVisible()
      expect(screen.queryByRole('button', { name: 'Save changes' })).not.toBeInTheDocument()
    })
    And('a link back to the garment is offered', () => {
      expect(screen.getByRole('link', { name: 'Back to the garment' })).toHaveAttribute(
        'href',
        `/wardrobe/${BLUE_TOP_ID}`,
      )
    })
  })

  Scenario('a garment that does not exist cannot be edited', ({ Given, When, Then }) => {
    Given('a wardrobe with no garments', () => seed())
    When('the user opens the edit screen of a garment that does not exist', () =>
      open('/wardrobe/missing/edit'),
    )
    Then('a message says the garment is not in the wardrobe', async () => {
      expect(
        await screen.findByRole('heading', { level: 1, name: 'Garment not found' }),
      ).toBeVisible()
    })
  })
})
