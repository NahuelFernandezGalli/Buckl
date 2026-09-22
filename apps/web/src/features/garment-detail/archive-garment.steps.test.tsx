import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { InMemoryGarmentRepository } from '../../data/in-memory-garment-repository'
import type { Garment } from '../../domain/garment'
import { DEFAULT_WARDROBE_FILTER } from '../../domain/wardrobe-filter'
import { GarmentMother } from '../../test/garment-mother'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./archive-garment.feature')

const BLUE_TOP_ID = 'blue-top'

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  const user = userEvent.setup()
  let repository: InMemoryGarmentRepository

  const seed = (garment: Garment) => {
    repository = new InMemoryGarmentRepository([garment])
  }
  const openDetail = () =>
    renderApp({ route: `/wardrobe/${BLUE_TOP_ID}`, repositories: { garments: repository } })

  const chooseArchive = async () => {
    openDetail()
    await user.click(await screen.findByRole('button', { name: 'Archive' }))
  }

  const expectConfirmation = () => {
    expect(screen.getByRole('alertdialog', { name: 'Archive this garment?' })).toBeVisible()
  }

  const activeIds = async () =>
    (await repository.list(DEFAULT_WARDROBE_FILTER)).map((garment) => garment.id)

  Scenario('archiving asks for confirmation', ({ Given, When, Then }) => {
    Given('a blue top in the wardrobe', () => seed(GarmentMother.active({ id: BLUE_TOP_ID })))
    When('the user chooses to archive it from its detail', chooseArchive)
    Then('a confirmation asks whether to archive the garment', expectConfirmation)
  })

  Scenario('confirming archives the garment', ({ Given, And, When, Then }) => {
    Given('a blue top in the wardrobe', () => seed(GarmentMother.active({ id: BLUE_TOP_ID })))
    And('the user chose to archive it from its detail', async () => {
      await chooseArchive()
      expectConfirmation()
    })
    When('the user confirms', async () => {
      await user.click(screen.getByRole('button', { name: 'Yes, archive it' }))
    })
    Then('the detail is marked as archived', async () => {
      expect(await screen.findByText('Archived')).toBeVisible()
    })
    And('the garment can no longer be edited from the detail', () => {
      expect(screen.queryByRole('link', { name: 'Edit' })).not.toBeInTheDocument()
    })
    And('the wardrobe no longer lists the blue top', async () => {
      expect(await activeIds()).toEqual([])
    })
  })

  Scenario('cancelling keeps the garment as it was', ({ Given, And, When, Then }) => {
    Given('a blue top in the wardrobe', () => seed(GarmentMother.active({ id: BLUE_TOP_ID })))
    And('the user chose to archive it from its detail', async () => {
      await chooseArchive()
      expectConfirmation()
    })
    When('the user cancels', async () => {
      await user.click(screen.getByRole('button', { name: 'Cancel' }))
    })
    Then('the detail is not marked as archived', async () => {
      await waitFor(() => expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument())
      expect(screen.queryByText('Archived')).not.toBeInTheDocument()
    })
    And('the blue top is still in the wardrobe', async () => {
      expect(await activeIds()).toEqual([BLUE_TOP_ID])
    })
  })

  Scenario('an archived garment can be restored', ({ Given, When, Then, And }) => {
    Given('an archived blue top', () => seed(GarmentMother.archived({ id: BLUE_TOP_ID })))
    When('the user restores it from its detail', async () => {
      openDetail()
      await user.click(await screen.findByRole('button', { name: 'Restore' }))
    })
    Then('the detail is not marked as archived', async () => {
      await waitFor(() => expect(screen.queryByText('Archived')).not.toBeInTheDocument())
      expect(screen.getByRole('link', { name: 'Edit' })).toBeVisible()
    })
    And('the blue top is back in the wardrobe', async () => {
      expect(await activeIds()).toEqual([BLUE_TOP_ID])
    })
  })

  Scenario('cancelling returns the user to the archive button', ({ Given, And, When, Then }) => {
    Given('a blue top in the wardrobe', () => seed(GarmentMother.active({ id: BLUE_TOP_ID })))
    And('the user chose to archive it from its detail', async () => {
      await chooseArchive()
      expectConfirmation()
    })
    When('the user cancels with the keyboard', async () => {
      await user.keyboard('{Escape}')
    })
    Then('the archive button has the focus', async () => {
      await waitFor(() => expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument())
      expect(screen.getByRole('button', { name: 'Archive' })).toHaveFocus()
    })
  })
})
