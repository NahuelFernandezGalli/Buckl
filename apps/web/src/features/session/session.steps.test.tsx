import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { cleanup, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect } from 'vitest'
import { fakeSession, type FakeSession } from '../../test/fake-session'
import { renderApp } from '../../test/render-app'

const feature = await loadFeature('./session.feature')

describeFeature(feature, ({ Scenario, AfterEachScenario }) => {
  AfterEachScenario(() => cleanup())

  let session: FakeSession

  const nobodySignedIn = () => {
    session = fakeSession({ status: 'signedOut' })
  }
  const aliceSignedIn = () => {
    session = fakeSession()
  }
  const restoring = () => {
    session = fakeSession({ status: 'loading' })
  }
  const open = (path: string) => () => {
    renderApp({ route: path, session })
  }
  const welcomeShown = async () => {
    expect(await screen.findByRole('heading', { level: 1, name: 'Buckl' })).toBeVisible()
  }
  const logInOffered = () => {
    expect(screen.getByRole('button', { name: 'Log in' })).toBeEnabled()
  }
  const failedSignIn = () => {
    session = fakeSession({
      status: 'signedOut',
      error: { message: 'We could not sign you in. Please try again.' },
    })
  }
  const failureExplained = async () => {
    expect(await screen.findByRole('alert')).toHaveTextContent(
      'We could not sign you in. Please try again.',
    )
  }
  const wardrobeShown = async () => {
    expect(await screen.findByRole('heading', { level: 1, name: 'Wardrobe' })).toBeVisible()
  }

  Scenario(
    'a visitor who is not signed in is welcomed instead of seeing a wardrobe',
    ({ Given, When, Then, And }) => {
      Given('nobody is signed in', nobodySignedIn)
      When('the visitor opens the wardrobe', open('/wardrobe'))
      Then('the welcome screen is shown', welcomeShown)
      And('a way to log in is offered', logInOffered)
    },
  )

  Scenario(
    'logging in comes back to the page the visitor asked for',
    ({ Given, And, When, Then }) => {
      Given('nobody is signed in', nobodySignedIn)
      And('the visitor opened the address {string}', (_ctx, address: string) => {
        renderApp({ route: address, session })
      })
      When('the visitor chooses {string}', async (_ctx, label: string) => {
        await userEvent.setup().click(await screen.findByRole('button', { name: label }))
      })
      Then('the sign-in starts, asking to come back to {string}', (_ctx, address: string) => {
        expect(session.signIns).toEqual([address])
      })
    },
  )

  Scenario('a failed sign-in is explained on the welcome screen', ({ Given, When, Then, And }) => {
    Given('the last sign-in attempt failed', failedSignIn)
    When('the visitor opens the wardrobe', open('/wardrobe'))
    Then('the welcome screen explains that the sign-in did not work', failureExplained)
    And('a way to log in is offered', logInOffered)
  })

  Scenario(
    'the app waits for the session instead of showing the welcome screen',
    ({ Given, When, Then, And }) => {
      Given('the session is still being restored', restoring)
      When('the visitor opens the wardrobe', open('/wardrobe'))
      Then('a loading message is shown', () => {
        expect(screen.getByRole('status')).toHaveTextContent('Opening your wardrobe…')
      })
      And('the welcome screen is not shown', () => {
        expect(screen.queryByRole('button', { name: 'Log in' })).not.toBeInTheDocument()
      })
    },
  )

  Scenario('a signed-in user sees the wardrobe and who they are', ({ Given, When, Then, And }) => {
    Given('Alice is signed in', aliceSignedIn)
    When('she opens the wardrobe', open('/wardrobe'))
    Then('the wardrobe is shown', wardrobeShown)
    And('her name is shown in the header', () => {
      expect(screen.getByRole('banner')).toHaveTextContent('Alice')
    })
  })

  Scenario(
    'a signed-in user who opens the welcome screen goes to the wardrobe',
    ({ Given, When, Then }) => {
      Given('Alice is signed in', aliceSignedIn)
      When('she opens the welcome screen', open('/welcome'))
      Then('the wardrobe is shown', wardrobeShown)
    },
  )

  Scenario('logging out ends the session', ({ Given, And, When, Then }) => {
    Given('Alice is signed in', aliceSignedIn)
    And('she is on the wardrobe', async () => {
      renderApp({ route: '/wardrobe', session })
      await wardrobeShown()
    })
    When('she chooses {string}', async (_ctx, label: string) => {
      await userEvent.setup().click(screen.getByRole('button', { name: label }))
    })
    Then('the session is ended', () => {
      expect(session.signOuts.count).toBe(1)
    })
  })

  Scenario('the sign-in callback waits for the session', ({ Given, When, Then }) => {
    Given('the session is still being restored', restoring)
    When('Auth0 sends the visitor back to the app', open('/callback?code=abc&state=xyz'))
    Then('a message says the sign-in is being completed', () => {
      expect(screen.getByRole('status')).toHaveTextContent('Signing you in…')
    })
  })

  Scenario(
    'a sign-in that failed at Auth0 comes back to the welcome screen',
    ({ Given, When, Then }) => {
      Given('the last sign-in attempt failed', failedSignIn)
      When(
        'Auth0 sends the visitor back to the app',
        open('/callback?error=access_denied&state=xyz'),
      )
      Then('the welcome screen explains that the sign-in did not work', failureExplained)
    },
  )
})
