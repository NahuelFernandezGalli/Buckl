# Testing strategy

How Buckl is tested, layer by layer, and the cycle each one follows. The decisions behind this
document are [ADR-0013](adr/0013-use-bdd-for-front-end-features.md) for testing style and
[ADR-0011](adr/0011-gate-merges-with-local-hooks-and-ci.md) for the checks that enforce it.

| Layer                          | Style                       | Tools                                        | Cycle                                   |
| ------------------------------ | --------------------------- | -------------------------------------------- | --------------------------------------- |
| Domain and application (.NET)  | Test-driven development     | xUnit                                        | red, green, refactor                    |
| Infrastructure adapters (.NET) | Integration tests           | xUnit, Postgres in a container, test doubles | written with the adapter                |
| Front-end features             | Behavior-driven development | Gherkin, vitest-cucumber, Testing Library    | scenario, automate, implement, refactor |
| Front-end helpers with no UI   | Unit tests                  | Vitest                                       | red, green, refactor                    |

Everything runs under `npm run test` for the front end and the .NET test job for the API. Both are
required checks on `main`, so a red test blocks the merge.

## The front-end cycle

Every feature that changes what a user can see or do goes through four steps, in this order. The
order is the point: the scenario is written before the component exists, so it describes the
behavior wanted rather than the markup that happens to be there.

### 1. Formulate the scenario

Write the scenario in a `.feature` file, in Gherkin, in the language of someone using the app.
This happens before any component code. If you cannot describe the behavior without naming a React
component, the behavior is not understood yet.

```gherkin
Feature: Wardrobe list

  As someone who owns clothes
  I want to see everything in my wardrobe at a glance
  So that I can decide what to wear without emptying the closet

  Scenario: an empty wardrobe invites the first garment
    Given a wardrobe with no garments
    When the user opens the wardrobe
    Then an invitation to add the first garment is shown

  Scenario: garments are listed with their photo and category
    Given a wardrobe with a blue shirt and black jeans
    When the user opens the wardrobe
    Then both garments are listed
    And each one shows its photo and its category
```

### 2. Automate it

Bind each step to code and run the suite. The scenario must fail, and it must fail for the reason
you expect: the behavior is missing, not the test file is broken.

```tsx
import { describeFeature, loadFeature } from '@amiceli/vitest-cucumber'
import { render, screen } from '@testing-library/react'
import { expect } from 'vitest'

const feature = await loadFeature('src/features/wardrobe/wardrobe-list.feature')

describeFeature(feature, ({ Scenario }) => {
  Scenario('an empty wardrobe invites the first garment', ({ Given, When, Then }) => {
    let repository: GarmentRepository

    Given('a wardrobe with no garments', () => {
      repository = new InMemoryGarmentRepository([])
    })
    When('the user opens the wardrobe', () => {
      render(<WardrobeList repository={repository} />)
    })
    Then('an invitation to add the first garment is shown', async () => {
      expect(await screen.findByRole('button', { name: /add your first garment/i })).toBeVisible()
    })
  })
})
```

`vitest-cucumber` fails the run when a scenario in the feature file has no implementation, or when
a step is missing or named differently. That is what keeps the cycle honest: you cannot quietly
skip a scenario you wrote.

### 3. Implement

Write the least code that makes the scenario pass. Not the screen you imagine, the scenario in
front of you. The next scenario will ask for the next piece.

### 4. Refactor

With the scenario green, improve names, extract components, remove duplication. The scenario is
the safety net, so this step is where design happens rather than in step 3.

## Writing scenarios

- **Describe outcomes, not clicks.** `Then an invitation to add the first garment is shown`, not
  `Then the EmptyState component renders`. A scenario that names components breaks on every
  refactor and stops being readable.
- **One behavior per scenario.** If a scenario needs two `When` steps, it is probably two
  scenarios.
- **`Given` sets up state, `When` is the single action under test, `Then` asserts what the user
  can observe.** Use `And` to extend any of them.
- **Use `Background`** for a `Given` shared by every scenario in the file, so it is written once.
- **Use `Scenario Outline` with an `Examples` table** when the same behavior holds for several
  values, such as filtering by each category.
- **Write in the vocabulary of the [glossary](glossary.md).** A scenario says garment, wardrobe and
  archive, never "item" or "clothing piece".
- **Avoid implementation detail in step names.** Steps are prose that a person who does not read
  code can check for truth.

## What does not get a scenario

- Pure helpers with no user-visible behavior of their own, such as image compression, currency
  formatting or a URL parser. These get plain Vitest unit tests, driven by the usual red, green,
  refactor cycle.
- Design tokens, layout and styling. Appearance is reviewed by looking at it, not asserted in
  tests.
- Third-party behavior. We test that we call the Auth0 SDK and how we react to its answers, not
  that Auth0 works.

## File layout

Scenarios live next to the feature they describe, inside `apps/web/src/features/<feature>/`:

```
src/features/wardrobe/
├── wardrobe-list.feature         the scenarios, in Gherkin
├── wardrobe-list.steps.test.tsx  the step definitions, run by Vitest
├── WardrobeList.tsx              the component
└── ...
```

The steps file ends in `.test.tsx` so the existing Vitest configuration picks it up with no change,
and the extension allows the JSX that rendering a component needs.

Prettier does not format `.feature` files, so their layout is up to the author. Keep the steps
indented under their scenario, as in the example above.

## Definition of done for a front-end pull request

- Every new user-visible behavior has at least one scenario, and the scenarios were written before
  the implementation.
- The scenarios read as plain language and use glossary terms.
- `npm run test` is green, and no scenario in a `.feature` file is left unimplemented.
- Helpers introduced along the way have their own unit tests.

## Tooling

[`@amiceli/vitest-cucumber`](https://www.npmjs.com/package/@amiceli/vitest-cucumber) reads the
`.feature` files and runs the steps as Vitest tests, so there is no second runner, no extra
configuration file and no new CI check. It is added as a development dependency in phase 3,
alongside the first screen that needs it.

If the library ever stops being maintained, the `.feature` files remain plain Gherkin and can be
run by another tool; only the step definition wrappers would change.
